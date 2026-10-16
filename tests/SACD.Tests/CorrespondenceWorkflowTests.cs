using System.IO.Compression;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Validation;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Services;
using SACD.Infrastructure.Storage;
using SACD.Infrastructure.Text;
using SACD.Infrastructure.Word;

namespace SACD.Tests;

public class CorrespondenceWorkflowTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"SACD_Workflow_"+Guid.NewGuid().ToString("N"));
    private readonly ActorContext actor=ActorContext.Create("S-1-TEST","test");
    public CorrespondenceWorkflowTests()=>Directory.CreateDirectory(root);
    private async Task<SqliteDatabaseContext> Open(string role,string name)
    {
        var db=new SqliteDatabaseContext(Path.Combine(root,name));
        var result=await new WorkspaceService(db).OpenAsync(new(db.WorkspaceDirectory,role,1),actor);
        Assert.True(result.Success);Assert.Equal(role,result.Value!.MachineProfile);return db;
    }
    private async Task<CorrespondenceCase> Receive(CorrespondenceService service)
    {
        string file=Path.Combine(root,"CongVan.txt");await File.WriteAllTextAsync(file,"Công văn yêu cầu tổng hợp báo cáo.\nSố: 123/CV",Encoding.UTF8);
        var row=await service.ReceiveAsync(file);
        return await service.SaveAsync(row with {Number="123/CV",DocumentType="Công văn",IssuedDate="2026-10-16",Issuer="Đơn vị A",
            Recipient="Đơn vị B",Signer="Người ký",Summary="Tổng hợp báo cáo",Classification="Không mật",DueDate="2026-10-20",ProposedReceiver="Đội 1"});
    }
    private async Task<CorrespondenceCase> Print(CorrespondenceService s,CorrespondenceCase row)
    {
        row=await s.AdvanceAsync(row,"Confirm","Đã mở tệp gốc, đối chiếu toàn văn và thông tin");
        return await s.AdvanceAsync(row,"Printed","Đã kiểm tra bản giấy");
    }

    [Fact]
    public async Task TwoMachines_ShouldPreserveMetadata_AndRequireApprovalAndReceiptBeforeCompletion()
    {
        using var a=await Open("Intake","A");using var b=await Open("Management","B");
        var sa=new CorrespondenceService(a,actor);var sb=new CorrespondenceService(b,actor);
        var source=await Print(sa,await Receive(sa));string zip=Path.Combine(root,"transfer.zip");await sa.ExportAsync(source,zip);
        var row=await sb.ImportAsync(zip,true);
        Assert.Equal(source.Code,row.Code);Assert.Equal(source.Summary,row.Summary);Assert.Equal(source.DueDate,row.DueDate);
        Assert.NotEqual(source.DocumentId,row.DocumentId);Assert.Equal("Imported",row.State);
        Assert.Contains("123/CV",string.Join("\n",sb.ReadText(row).Blocks.Select(x=>x.RawTextNfc)));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sb.AdvanceAsync(row,"Complete","Kết quả"));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sb.AdvanceAsync(row,"Register","Đã lưu"));
        row=await sb.AdvanceAsync(row with {ExternalRef="DEN-001"},"Register","Đối chiếu trong QLVB");
        string slip=Path.Combine(root,"phieu.docx");ProcessingSlipGenerator.Generate(slip,row);
        using(var doc=WordprocessingDocument.Open(slip,false))
        { Assert.Single(doc.MainDocumentPart!.ImageParts);Assert.Contains(row.Code,doc.MainDocumentPart!.Document!.InnerText);
          var errors=new OpenXmlValidator().Validate(doc).ToList();Assert.True(errors.Count==0,string.Join("\n",errors.Select(e=>e.Description))); }
        row=await sb.AdvanceAsync(row,"Submit","Đã in phiếu và trình kèm bản giấy");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sb.AdvanceAsync(row,"Approve","Ký duyệt"));
        row=await sb.AdvanceAsync(row with {Leader="Trưởng phòng",ApprovedReceiver="Đội 2",DecisionEvidence="Phiếu ký 16/10/2026"},"Approve","Đã nhận lại phiếu ký");
        row=await sb.AdvanceAsync(row,"Deliver","Giao đúng người được chỉ đạo");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sb.AdvanceAsync(row,"Complete","Chưa có ký nhận"));
        row=await sb.AdvanceAsync(row with {ReceiptEvidence="Sổ giao nhận trang 12, người nhận đã ký"},"Receipt","Đã kiểm tra chữ ký");
        row=await sb.AdvanceAsync(row,"Complete","Đã nhận kết quả xử lý");
        Assert.Equal("Completed",sb.Get(row.Id).State);Assert.Equal("Đã hoàn thành",CorrespondenceService.Deadline(row,DateTime.UtcNow));
        Assert.True(sb.History(row.Id).Count>=7);
    }

    [Fact]
    public async Task Usb_ShouldRejectUnscannedAndTamperedPackages_AndDeduplicateReplay()
    {
        using var a=await Open("Intake","A");using var b=await Open("Management","B");
        var sa=new CorrespondenceService(a,actor);var sb=new CorrespondenceService(b,actor);
        var row=await Print(sa,await Receive(sa));string zip=Path.Combine(root,"transfer.zip");await sa.ExportAsync(row,zip);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sb.ImportAsync(zip,false));
        var first=await sb.ImportAsync(zip,true);var again=await sb.ImportAsync(zip,true);
        Assert.Equal(first.Id,again.Id);Assert.Single(sb.List());
        using(var archive=ZipFile.Open(zip,ZipArchiveMode.Update))
        {
            var metadata=archive.GetEntry("metadata.json")!;string json;
            using(var reader=new StreamReader(metadata.Open()))json=reader.ReadToEnd();
            var node=System.Text.Json.Nodes.JsonNode.Parse(json)!;node["Case"]!["Summary"]="Metadata nguồn đã thay đổi";
            byte[] modified=Encoding.UTF8.GetBytes(node.ToJsonString());metadata.Delete();
            using(var stream=archive.CreateEntry("metadata.json").Open())stream.Write(modified);
            archive.GetEntry("checksum.sha256")!.Delete();
            string hash=Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(modified)).ToLowerInvariant();
            using var writer=new StreamWriter(archive.CreateEntry("checksum.sha256").Open(),new UTF8Encoding(false));
            writer.Write($"{hash} metadata.json\n{row.ObjectHash} original\n");
        }
        var conflict=await Assert.ThrowsAsync<InvalidDataException>(()=>sb.ImportAsync(zip,true));
        Assert.Contains("TransferConflict",conflict.Message);Assert.Equal(first.Summary,sb.Get(first.Id).Summary);
        using(var archive=ZipFile.Open(zip,ZipArchiveMode.Update))
        { archive.GetEntry("original")!.Delete();using var writer=new StreamWriter(archive.CreateEntry("original").Open());writer.Write("Thay đổi nội dung"); }
        await Assert.ThrowsAsync<InvalidDataException>(()=>sb.ImportAsync(zip,true));Assert.Single(sb.List());
    }

    [Fact]
    public async Task Cases_ShouldRejectWrongMachine_StaleEdits_AndOutOfOrderTransitions()
    {
        using var a=await Open("Intake","A");using var b=await Open("Management","B");
        var sa=new CorrespondenceService(a,actor);var sb=new CorrespondenceService(b,actor);var row=await Receive(sa);
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sa.ExportAsync(row,Path.Combine(root,"early.zip")));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sa.AdvanceAsync(row,"Deliver","Sai thứ tự"));
        await sa.SaveAsync(row with {Summary="Bản cập nhật"});
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sa.SaveAsync(row with {Summary="Bản cũ"}));
        await Assert.ThrowsAsync<InvalidOperationException>(()=>sb.ReceiveAsync(Path.Combine(root,"CongVan.txt")));
        Assert.Throws<InvalidOperationException>(()=>CorrespondenceService.ValidateDate("31/02/2026"));
    }

    [Fact]
    public async Task RepeatedReceive_ShouldReturnSameCase_AndReviewedMetadataCannotBeOverwritten()
    {
        using var db=await Open("Intake","A");var s=new CorrespondenceService(db,actor);var row=await Receive(s);
        var duplicate=await s.ReceiveAsync(Path.Combine(root,"CongVan.txt"));Assert.Equal(row.Id,duplicate.Id);Assert.Single(s.List());
        row=await s.AdvanceAsync(row,"Confirm","Đã đối chiếu");
        await Assert.ThrowsAsync<InvalidOperationException>(()=>s.SaveAsync(row with {Number="Sửa sau xác nhận"}));
    }

    [Fact]
    public async Task TextReview_ShouldUseCurrentRevision_AndRejectForeignBlocksAndStaleRevision()
    {
        using var db=await Open("Intake","A");var s=new CorrespondenceService(db,actor);var row=await Receive(s);
        var text=s.ReadText(row);var service=new TextService(db,new LocalFileStore(db.WorkspaceDirectory),new DocumentTextExtractor());
        var wrong=await service.SaveReviewAsync(new(row.DocumentId,row.DocumentId,text.Blocks,new[]{1},"Sai mã"),actor);Assert.False(wrong.Success);
        var foreign=await service.SaveReviewAsync(new(row.DocumentId,text.RevisionId,new[]{text.Blocks[0] with {Id="foreign"}},new[]{1},"Sai block"),actor);Assert.False(foreign.Success);
        var right=await service.SaveReviewAsync(new(row.DocumentId,text.RevisionId,text.Blocks,new[]{1},"Đã kiểm tra"),actor);Assert.True(right.Success);
        await service.ExtractAsync(new(row.DocumentId,"BuiltIn"),actor);
        var stale=await service.SaveReviewAsync(new(row.DocumentId,text.RevisionId,text.Blocks,new[]{1},"Cũ"),actor);Assert.False(stale.Success);
    }

    [Fact]
    public async Task HashedDocx_ShouldExtractParagraphsUsingOriginalExtension()
    {
        using var db=await Open("Intake","A");var service=new CorrespondenceService(db,actor);
        string file=Path.Combine(root,"source.docx");
        ProcessingSlipGenerator.Generate(file,new CorrespondenceCase {Code="HS-QR",Number="456/CV",Summary="Nội dung tiếng Việt"});
        var row=await service.ReceiveAsync(file);var text=service.ReadText(row);
        Assert.Contains(text.Blocks,b=>b.RawTextNfc.Contains("456/CV"));
        Assert.All(text.Blocks,b=>Assert.Contains("paragraph",b.LocatorJson));
    }

    [Fact]
    public async Task PublishScript_ShouldIncreaseVersionOnRepeatedRuns_AndUseRealContentHash()
    {
        using var db=await Open("Intake","A");var service=new RecorderService(db);
        var request=new PublishScriptRequest("SCRIPT-QLVB-01","Test",new[]{new ScriptStepDto(1,"PauseForUser",null,"{}",1000,"None")},"{}");
        var a=await service.PublishScriptAsync(request,actor);var b=await service.PublishScriptAsync(request,actor);
        Assert.True(a.Success);Assert.True(b.Success);Assert.Equal(1,a.Value!.VersionNo);Assert.Equal(2,b.Value!.VersionNo);
        Assert.Equal(64,a.Value.ContentHash.Length);Assert.Equal(a.Value.ContentHash,b.Value.ContentHash);
        var c=await service.PublishScriptAsync(request with {ProfileJson="{\"name\":\"changed\"}"},actor);Assert.NotEqual(a.Value.ContentHash,c.Value!.ContentHash);
    }

    [Fact]
    public async Task DraftReportAndBackup_ShouldWorkOnFreshWorkspace_WithoutReadingWriterLock()
    {
        using var db=await Open("Intake","A");var service=new CorrespondenceService(db,actor);await Receive(service);
        var reports=new ReportService(db,new LocalFileStore(db.WorkspaceDirectory),new WordReportGenerator());
        var draft=await reports.CreateDraftAsync(new(null,"Báo cáo thử","TPL-DEFAULT","2026-10-01","2026-10-31",Array.Empty<string>(),Array.Empty<string>(),new Dictionary<string,string>{{"I","Nội dung thử"}}),actor);
        Assert.True(draft.Success);
        var exported=await reports.ExportAsync(new(draft.Value!.RevisionId,"Draft",root,true),actor);Assert.True(exported.Success);Assert.True(File.Exists(exported.Value!.OutputPath));
        var backup=await new OperationsService(db).BackupAsync(new(Path.Combine(root,"backups"),"Thử snapshot"),actor);
        Assert.True(backup.Success);using var zip=ZipFile.OpenRead(backup.Value!.BackupPath);
        Assert.NotNull(zip.GetEntry("sacd_workspace.db"));Assert.Null(zip.GetEntry("sacd.lock"));Assert.Contains(zip.Entries,e=>e.FullName.StartsWith("objects/"));
    }

    [Fact]
    public void Completion_ShouldDistinguishLateAndOnTime_AndKeepUnknownDeadline()
    {
        var row=new CorrespondenceCase{State="Completed",DueDate="2026-10-16",CompletedUtc="2026-10-16T16:59:00Z"};
        Assert.Equal("Hoàn thành đúng hạn",row.DeadlineLabel);
        Assert.Equal("Hoàn thành quá hạn",(row with {CompletedUtc="2026-10-16T17:00:00Z"}).DeadlineLabel);
        Assert.Contains("chưa xác định hạn",(row with {DueDate=""}).DeadlineLabel);
    }
    public void Dispose(){try{Directory.Delete(root,true);}catch{}}
}
