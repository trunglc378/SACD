using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Rpa;
using SACD.Infrastructure.Services;
using SACD.Infrastructure.Storage;
using SACD.Infrastructure.Text;
using SACD.Infrastructure.Word;
using UglyToad.PdfPig.Core;
using UglyToad.PdfPig.Fonts.Standard14Fonts;
using UglyToad.PdfPig.Writer;

namespace SACD.Tests;

public sealed class SafetyRegressionTests : IDisposable
{
    private readonly string root=Path.Combine(Path.GetTempPath(),"SACD_Regression_"+Guid.NewGuid().ToString("N"));
    private readonly ActorContext actor=ActorContext.Create("S-REGRESSION","test");
    private SqliteDatabaseContext? context;
    private async Task<SqliteDatabaseContext> Open()
    {
        context=new(Path.Combine(root,"workspace"));
        var opened=await new WorkspaceService(context).OpenAsync(new(context.WorkspaceDirectory,"Intake",1),actor);
        Assert.True(opened.Success,opened.Message);return context;
    }
    private static void Sql(SqliteDatabaseContext db,string sql)
    {using var c=db.CreateConnection();using var q=c.CreateCommand();q.CommandText=sql;q.ExecuteNonQuery();}
    private async Task<(CorrespondenceCase Row,TextService Text)> Receive(SqliteDatabaseContext db,string content="Thông tin nguồn để kiểm tra bằng chứng.")
    {
        string file=Path.Combine(root,"source.txt");await File.WriteAllTextAsync(file,content);
        var row=await new CorrespondenceService(db,actor).ReceiveAsync(file);
        return(row,new TextService(db,new LocalFileStore(db.WorkspaceDirectory),new DocumentTextExtractor()));
    }

    [Fact]
    public async Task UnconfiguredExternalServices_DoNotCreateSuccessOrIdentifiers()
    {
        var db=await Open();var external=await new ExternalEntryService(db).FillAsync(new("none","none","none"),actor);
        var print=await new PrintService(db).SubmitAsync("none",actor);
        var ocr=await new TextService(db,new LocalFileStore(db.WorkspaceDirectory),new DocumentTextExtractor()).OcrAsync(new("none",null,"vie"),actor);
        Assert.False(external.Success);Assert.Equal("NotConfigured",external.Code);Assert.False(print.Success);Assert.False(ocr.Success);
        using var c=db.CreateConnection();using var q=c.CreateCommand();q.CommandText="SELECT COUNT(*) FROM ExternalEntryAttempt;";Assert.Equal(0L,q.ExecuteScalar());
    }

    [Fact]
    public async Task DifferentOwner_IsRejectedWithoutChangingWorkspace()
    {
        var db=await Open();string path=db.WorkspaceDirectory;db.Dispose();
        using var other=new SqliteDatabaseContext(path);
        var result=await new WorkspaceService(other).OpenAsync(new(path,"Management",1),ActorContext.Create("OTHER","other"));
        Assert.False(result.Success);Assert.Equal("AccessDenied",result.Code);
    }

    [Fact]
    public async Task Search_ReturnsOnlyCurrentRevision()
    {
        var db=await Open();var (row,text)=await Receive(db,"obsoleteword");
        await text.ExtractAsync(new(row.DocumentId,"BuiltIn"),actor);
        var result=await new SearchService(db).SearchAsync(new("obsoleteword",null,null,null,1,20),actor);
        Assert.True(result.Success);Assert.Single(result.Value!.Results);Assert.Equal(2,result.Value.Results[0].VersionNo);
    }

    [Fact]
    public async Task ReferencedRevision_CannotBeEditedInPlace()
    {
        var db=await Open();var (row,text)=await Receive(db);var source=new CorrespondenceService(db,actor).ReadText(row);
        string hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source.Blocks[0].RawTextNfc))).ToLowerInvariant();
        Sql(db,$"INSERT INTO EvidenceRef VALUES('e','{source.RevisionId}','{source.Blocks[0].Id}',0,{source.Blocks[0].RawTextNfc.Length},'{hash}','2026-10-20');");
        var result=await text.SaveReviewAsync(new(row.DocumentId,source.RevisionId,source.Blocks,new[]{1},"Edited"),actor);
        Assert.False(result.Success);Assert.Equal("RevisionConflict",result.Code);
    }

    [Fact]
    public async Task PdfReader_ExtractsRealTextAndMarksBlankPagesForOcr()
    {
        Directory.CreateDirectory(root);string file=Path.Combine(root,"real.pdf");
        var builder=new PdfDocumentBuilder();var page=builder.AddPage(300,300);
        var font=builder.AddStandard14Font(Standard14Font.Helvetica);page.AddText("Real document 123",12,new PdfPoint(20,200),font);builder.AddPage(300,300);
        await File.WriteAllBytesAsync(file,builder.Build());
        var blocks=await new DocumentTextExtractor().ExtractBlocksAsync(file,"rev");
        Assert.Equal(2,blocks.Count);Assert.Contains("Real document 123",blocks[0].RawTextNfc);Assert.Contains("needsOcr",blocks[1].LocatorJson);
    }

    [Fact]
    public async Task ScanOnlyPdf_CannotBeConfirmedAsReviewed()
    {
        var db=await Open();string file=Path.Combine(root,"scan.pdf");var builder=new PdfDocumentBuilder();builder.AddPage(300,300);await File.WriteAllBytesAsync(file,builder.Build());
        var row=await new CorrespondenceService(db,actor).ReceiveAsync(file);var source=new CorrespondenceService(db,actor).ReadText(row);
        var result=await new TextService(db,new LocalFileStore(db.WorkspaceDirectory),new DocumentTextExtractor()).SaveReviewAsync(new(row.DocumentId,source.RevisionId,source.Blocks,new[]{1},"Checked"),actor);
        Assert.False(result.Success);Assert.Equal("NeedsReview",result.Code);
    }

    [Fact]
    public async Task DocxReader_IncludesTableAndHeaderText()
    {
        Directory.CreateDirectory(root);string file=Path.Combine(root,"table.docx");
        using(var word=WordprocessingDocument.Create(file,DocumentFormat.OpenXml.WordprocessingDocumentType.Document))
        {
            var main=word.AddMainDocumentPart();
            var cell=new TableCell(new Paragraph(new Run(new Text("Inside table"))));
            main.Document=new Document(new Body(new Table(new TableRow(cell))));
            var header=main.AddNewPart<HeaderPart>();header.Header=new Header(new Paragraph(new Run(new Text("Inside header"))));main.Document.Save();
        }
        var blocks=await new DocumentTextExtractor().ExtractBlocksAsync(file,"rev");
        Assert.Contains(blocks,b=>b.RawTextNfc=="Inside table");Assert.Contains(blocks,b=>b.RawTextNfc=="Inside header");
    }

    [Fact]
    public async Task Restore_VerifiesObjectsAndRejectsOverwriteAndTampering()
    {
        var db=await Open();await Receive(db);var operations=new OperationsService(db);
        var backup=await operations.BackupAsync(new(Path.Combine(root,"backup"),"Regression"),actor);Assert.True(backup.Success);
        string destination=Path.Combine(root,"restored");var restored=await operations.RestoreAsync(new(backup.Value!.BackupPath,destination),actor);
        Assert.True(restored.Success,restored.Message);Assert.True(restored.Value!.IntegrityOk);
        Assert.False((await operations.RestoreAsync(new(backup.Value.BackupPath,destination),actor)).Success);
        using(var zip=ZipFile.Open(backup.Value.BackupPath,ZipArchiveMode.Update))
        {
            var entry=zip.Entries.Single(e=>e.FullName.StartsWith("objects/") && e.Name.Length>0);string name=entry.FullName;entry.Delete();
            using var writer=new StreamWriter(zip.CreateEntry(name).Open());writer.Write("tampered");
        }
        string broken=Path.Combine(root,"broken");Assert.False((await operations.RestoreAsync(new(backup.Value.BackupPath,broken),actor)).Success);Assert.False(Directory.Exists(broken));
    }

    [Fact]
    public async Task Restore_RejectsTraversalWithoutPublishingTarget()
    {
        var db=await Open();string file=Path.Combine(root,"bad.zip");
        using(var zip=ZipFile.Open(file,ZipArchiveMode.Create)){using var writer=new StreamWriter(zip.CreateEntry("../escape.txt").Open());writer.Write("bad");}
        string target=Path.Combine(root,"target");var restored=await new OperationsService(db).RestoreAsync(new(file,target),actor);
        Assert.False(restored.Success);Assert.False(Directory.Exists(target));Assert.False(File.Exists(Path.Combine(root,"escape.txt")));
    }

    [Fact]
    public async Task Review_RejectsUncheckedOrUnprovenClaimsAtServiceBoundary()
    {
        var db=await Open();await Receive(db);var reports=new ReportService(db,new LocalFileStore(db.WorkspaceDirectory),new WordReportGenerator());
        var draft=await reports.CreateDraftAsync(new(null,"Test","TPL-DEFAULT","2026-10-01","2026-10-31",Array.Empty<string>(),Array.Empty<string>(),new Dictionary<string,string>{{"I","Test content"}}),actor);
        Assert.True(draft.Success);
        var result=await reports.ReviewAsync(new(draft.Value!.RevisionId,1,Array.Empty<string>(),"{}",""),actor);
        Assert.False(result.Success);
        using var c=db.CreateConnection();using var q=c.CreateCommand();q.CommandText="SELECT id FROM Claim";string claim=(string)q.ExecuteScalar()!;
        Assert.False((await reports.ReviewAsync(new(draft.Value.RevisionId,1,new[]{claim},"{}",""),actor)).Success);
    }

    private static void SeedFacts(SqliteDatabaseContext db,CorrespondenceCase row,string kind,string value1="10",string value2="25")
    {
        var source=new CorrespondenceService(db,ActorContext.Create("S-REGRESSION","test")).ReadText(row);
        string hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source.Blocks[0].RawTextNfc))).ToLowerInvariant();
        Sql(db,$@"UPDATE TextRevision SET reviewState='Reviewed',reviewerSid='TEST',reviewedUtc='2026-10-20' WHERE id='{source.RevisionId}';
            UPDATE TextBlock SET reviewed=1 WHERE revisionId='{source.RevisionId}';
            INSERT INTO EvidenceRef VALUES('fact-evidence','{source.RevisionId}','{source.Blocks[0].Id}',0,{source.Blocks[0].RawTextNfc.Length},'{hash}','2026-10-20');");
        Sql(db,$@"
        INSERT INTO MetricDefinitionVersion(id,logicalId,versionNo,name,unitCode,kind,ruleJson,createdUtc) VALUES('m','M',1,'Test','DOC','{kind}','{{}}','2026-10-20');
        INSERT INTO FactVersion(id,logicalId,versionNo,metricVersionId,sourceRevisionId,valueStatus,valueDecimalText,periodStart,periodEnd,periodKind,scopeJson,status,reviewerSid,reviewedUtc,createdUtc)
        SELECT 'a','A',1,'m',currentRevisionId,'Known','{value1}','2026-10-01','2026-10-10','Month','{{}}','Confirmed','TEST','2026-10-20','2026-10-20' FROM Document WHERE id='{row.DocumentId}';
        INSERT INTO FactVersion(id,logicalId,versionNo,metricVersionId,sourceRevisionId,valueStatus,valueDecimalText,periodStart,periodEnd,periodKind,scopeJson,status,reviewerSid,reviewedUtc,createdUtc)
        SELECT 'b','B',1,'m',currentRevisionId,'Known','{value2}','2026-10-11','2026-10-20','Month','{{}}','Confirmed','TEST','2026-10-20','2026-10-20' FROM Document WHERE id='{row.DocumentId}';
        INSERT INTO FactEvidence VALUES('a','fact-evidence'),('b','fact-evidence');
        ");
    }

    [Fact]
    public async Task Stock_SelectsLatestRegardlessOfInputOrderAndPersistsContributions()
    {
        var db=await Open();var (row,_)=await Receive(db);SeedFacts(db,row,"Stock");var facts=new FactService(db);
        var first=await facts.AggregateAsync(new("m","2026-10-01","2026-10-31","None",new[]{"a","b"}),actor);
        var second=await facts.AggregateAsync(new("m","2026-10-01","2026-10-31","None",new[]{"b","a"}),actor);
        Assert.True(first.Success,first.Message);Assert.Equal(25m,first.Value!.TotalValue);Assert.Equal(first.Value.TotalValue,second.Value!.TotalValue);
        using var c=db.CreateConnection();using var q=c.CreateCommand();q.CommandText="SELECT COUNT(*) FROM AggregationContribution";Assert.Equal(2L,q.ExecuteScalar());
    }

    [Fact]
    public async Task Ratio_PreservesDecimalsAndRequiresExplicitRoles()
    {
        var db=await Open();var (row,_)=await Receive(db);SeedFacts(db,row,"Ratio","1.5","2.5");var facts=new FactService(db);
        Assert.False((await facts.AggregateAsync(new("m","2026-10-01","2026-10-31","None",new[]{"a","b"}),actor)).Success);
        var result=await facts.AggregateAsync(new("m","2026-10-01","2026-10-31","None",new[]{"b","a"},new[]{"a"},new[]{"b"}),actor);
        Assert.True(result.Success,result.Message);Assert.Equal(0.6m,result.Value!.TotalValue);Assert.Equal("60.00%",result.Value.FormattedTotal);
    }

    [Fact]
    public async Task Aggregate_RejectsDuplicateIdsAndOutOfPeriodFacts()
    {
        var db=await Open();var (row,_)=await Receive(db);SeedFacts(db,row,"Flow");var facts=new FactService(db);
        Assert.False((await facts.AggregateAsync(new("m","2026-10-01","2026-10-31","None",new[]{"a","a"}),actor)).Success);
        Assert.False((await facts.AggregateAsync(new("m","2026-10-15","2026-10-31","None",new[]{"a","b"}),actor)).Success);
    }

    [Fact]
    public void MetadataSuggestions_DoNotInventDeadlineOrClassification()
    {
        var row=MetadataSuggestions.Apply(new(),"Số: 123/CV\nNgày ban hành: 20/10/2026\nNgười ký: Người thử\nTrích yếu: Nội dung thử");
        Assert.Equal("123/CV",row.Number);Assert.Equal("2026-10-20",row.IssuedDate);Assert.Empty(row.DueDate);Assert.Equal("Chưa xác định",row.Classification);
    }

    [Fact]
    public void DecimalParser_RejectsSilentRoundingAndLocaleDependentNumbers()
    {
        Assert.True(InvariantDecimal.TryParseExact("+001.5000",out var value));Assert.Equal(1.5m,value);
        Assert.False(InvariantDecimal.TryParseExact("1,5",out _));
        Assert.False(InvariantDecimal.TryParseExact("0.12345678901234567890123456789",out _));
    }

    [Fact]
    public async Task ReviewedReport_WithCurrentEvidenceCanBeExportedForSubmission()
    {
        var db=await Open();var (row,text)=await Receive(db);var source=new CorrespondenceService(db,actor).ReadText(row);
        Assert.True((await text.SaveReviewAsync(new(row.DocumentId,source.RevisionId,source.Blocks,new[]{1},"Đối chiếu nguồn thử"),actor)).Success);
        string hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(source.Blocks[0].RawTextNfc))).ToLowerInvariant();
        Sql(db,$"INSERT INTO EvidenceRef VALUES('e','{source.RevisionId}','{source.Blocks[0].Id}',0,{source.Blocks[0].RawTextNfc.Length},'{hash}','2026-10-20');");
        var reports=new ReportService(db,new LocalFileStore(db.WorkspaceDirectory),new WordReportGenerator());
        var draft=await reports.CreateDraftAsync(new(null,"Test","TPL-DEFAULT","2026-10-01","2026-10-31",new[]{source.RevisionId},Array.Empty<string>(),new Dictionary<string,string>{{"I","Nội dung đã kiểm tra"}}),actor);
        Assert.True(draft.Success);
        Sql(db,"INSERT INTO ClaimEvidence SELECT id,'e' FROM Claim;");
        using var c=db.CreateConnection();using var q=c.CreateCommand();q.CommandText="SELECT id FROM Claim";string claim=(string)q.ExecuteScalar()!;
        var review=await reports.ReviewAsync(new(draft.Value!.RevisionId,1,new[]{claim},"{}","Kiểm tra dữ liệu giả"),actor);
        Assert.True(review.Success,review.Message);
        var exported=await reports.ExportAsync(new(draft.Value.RevisionId,"Submission",Path.Combine(root,"export"),false),actor);
        Assert.True(exported.Success,exported.Message);Assert.True(File.Exists(exported.Value!.OutputPath));
    }

    [Fact]
    public async Task CustomTemplate_RegistersItsObjectButUnsupportedRenderingIsExplicit()
    {
        var db=await Open();string file=Path.Combine(root,"template.docx");ProcessingSlipGenerator.Generate(file,new(){Code="TEST"});
        var reports=new ReportService(db,new LocalFileStore(db.WorkspaceDirectory),new WordReportGenerator());
        var template=await reports.ValidateTemplateAsync(new("Test",file,"{}"),actor);Assert.True(template.Success,template.Message);
        var draft=await reports.CreateDraftAsync(new(null,"Test",template.Value!.TemplateVersionId,"2026-10-01","2026-10-31",Array.Empty<string>(),Array.Empty<string>(),new Dictionary<string,string>{{"I","Content"}}),actor);
        Assert.True(draft.Success);
        var exported=await reports.ExportAsync(new(draft.Value!.RevisionId,"Draft",root,false),actor);
        Assert.False(exported.Success);Assert.Equal("TemplateUnsupported",exported.Code);
    }

    [Fact]
    public async Task Aggregation_RejectsLegacyConfirmedFactsWithoutEvidence()
    {
        var db=await Open();var (row,_)=await Receive(db);SeedFacts(db,row,"Stock");Sql(db,"DELETE FROM FactEvidence;");
        var result=await new FactService(db).AggregateAsync(new("m","2026-10-01","2026-10-31","None",new[]{"a","b"}),actor);
        Assert.False(result.Success);Assert.Equal("NeedsReview",result.Code);
    }

    public void Dispose(){context?.Dispose();if(Directory.Exists(root))Directory.Delete(root,true);}
}
