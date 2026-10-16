using System.Globalization;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Storage;
using SACD.Infrastructure.Text;

namespace SACD.Infrastructure.Services;

// Additive application workflow. Existing dossiers/documents remain the source file identities.
public sealed class CorrespondenceService(SqliteDatabaseContext db, ActorContext actor)
{
    private readonly LocalFileStore store = new(db.WorkspaceDirectory);
    private static readonly JsonSerializerOptions Json = new() { WriteIndented = true };
    public IReadOnlyList<CorrespondenceCase> List()
    {
        using var c = db.CreateConnection();
        using var q = c.CreateCommand();
        q.CommandText = "SELECT payloadJson FROM CorrespondenceCase ORDER BY rowid DESC";
        using var r = q.ExecuteReader();
        var rows = new List<CorrespondenceCase>();
        while (r.Read()) rows.Add(JsonSerializer.Deserialize<CorrespondenceCase>(r.GetString(0))!);
        return rows;
    }

    public CorrespondenceCase Get(string id) => List().Single(x => x.Id == id);

    public async Task RecordAssistanceAsync(CorrespondenceCase row,string action,string note)
    {
        RequireRole("Management");
        using var c=db.CreateConnection();using var tx=c.BeginTransaction();
        await Event(c,tx,row.Id,action,note);tx.Commit();
    }

    public IReadOnlyList<CaseHistory> History(string id)
    {
        using var c = db.CreateConnection(); using var q = c.CreateCommand();
        q.CommandText = "SELECT action,actor,atUtc,note FROM CorrespondenceCaseEvent WHERE caseId=@id ORDER BY rowid DESC";
        q.Parameters.AddWithValue("@id", id); using var r = q.ExecuteReader();
        var rows = new List<CaseHistory>();
        while (r.Read()) rows.Add(new(r.GetString(0),r.GetString(1),r.GetString(2),r.GetString(3)));
        return rows;
    }

    private void RequireRole(string role)
    {
        using var c = db.CreateConnection(); using var q = c.CreateCommand();
        q.CommandText = "SELECT machineProfile FROM Workspace LIMIT 1";
        if ((string?)q.ExecuteScalar() != role) throw new InvalidOperationException(role == "Intake"
            ? "Thao tác này dành cho kho Máy 1 — Tiếp nhận." : "Thao tác này dành cho kho Máy 2 — Quản lý.");
    }

    public async Task<CorrespondenceCase> ReceiveAsync(string file)
    {
        RequireRole("Intake");
        var ext = Path.GetExtension(file).ToLowerInvariant();
        if (ext is not (".txt" or ".docx" or ".pdf")) throw new InvalidOperationException("Chọn TXT, DOCX hoặc PDF. PDF/scan cần đối chiếu thủ công; OCR chưa tích hợp.");
        using (var c = db.CreateConnection()) using (var q = c.CreateCommand())
        {
            q.CommandText = "SELECT cc.payloadJson FROM CorrespondenceCase cc JOIN Document d ON d.id=cc.documentId WHERE d.objectHash=@hash LIMIT 1";
            await using var input = File.OpenRead(file);
            q.Parameters.AddWithValue("@hash", Convert.ToHexString(await SHA256.HashDataAsync(input)).ToLowerInvariant());
            if (q.ExecuteScalar() is string existing) return JsonSerializer.Deserialize<CorrespondenceCase>(existing)!;
        }
        var imported = await new IntakeService(db, store).ImportAsync(new(new[]{file},null,"File","Email/tệp do cán bộ chọn",1),actor);
        if (!imported.Success) throw new InvalidOperationException(imported.Message);
        var item = imported.Value!.Items.Single();
        var extracted = await new TextService(db,store,new DocumentTextExtractor()).ExtractAsync(new(item.DocumentId,"BuiltIn"),actor);
        if (!extracted.Success) throw new InvalidOperationException(extracted.Message);
        var row = new CorrespondenceCase { DocumentId=item.DocumentId,DossierId=imported.Value.DossierId,
            Code=imported.Value.BusinessCode,OriginalName=Path.GetFileName(file),ObjectHash=item.ObjectHash };
        using var conn = db.CreateConnection(); using var tx = conn.BeginTransaction();
        Insert(conn,tx,row); await Event(conn,tx,row.Id,"Tiếp nhận tệp",row.OriginalName); tx.Commit();
        return row;
    }

    public (string RevisionId, IReadOnlyList<TextBlockDto> Blocks) ReadText(CorrespondenceCase row)
    {
        using var c = db.CreateConnection(); using var q = c.CreateCommand();
        q.CommandText = "SELECT currentRevisionId FROM Document WHERE id=@id"; q.Parameters.AddWithValue("@id",row.DocumentId);
        var rev = q.ExecuteScalar() as string ?? throw new InvalidOperationException("Văn bản chưa có nội dung trích xuất.");
        using var b = c.CreateCommand(); b.CommandText = "SELECT id,ordinal,locatorJson,extractedText,rawTextNfc,reviewed FROM TextBlock WHERE revisionId=@id ORDER BY ordinal";
        b.Parameters.AddWithValue("@id",rev); using var r = b.ExecuteReader(); var blocks = new List<TextBlockDto>();
        while(r.Read()) blocks.Add(new(r.GetString(0),r.GetInt32(1),r.GetString(2),r.GetString(3),r.GetString(4),r.GetInt32(5)));
        return (rev,blocks);
    }

    public async Task<CorrespondenceCase> SaveAsync(CorrespondenceCase edited)
    {
        ValidateDate(edited.DueDate); ValidateDate(edited.IssuedDate);
        var old = Get(edited.Id);
        if (old.RowVersion != edited.RowVersion) throw new InvalidOperationException("Hồ sơ đã thay đổi. Tải lại trước khi sửa.");
        if (old.State != "Received" && old.State != "Imported")
            throw new InvalidOperationException("Thông tin đã xác nhận; không sửa đè sau khi in/trình duyệt. Chỉ cập nhật ở bước nghiệp vụ tương ứng.");
        RequireRole(old.State == "Received" ? "Intake" : "Management");
        var next = edited with { DocumentId=old.DocumentId,DossierId=old.DossierId,Code=old.Code,OriginalName=old.OriginalName,
            ObjectHash=old.ObjectHash,State=old.State,RowVersion=old.RowVersion+1 };
        await Persist(old,next,"Lưu thông tin","Cán bộ cập nhật biểu mẫu"); return next;
    }

    public async Task<CorrespondenceCase> AdvanceAsync(CorrespondenceCase input, string action, string note)
    {
        var old = Get(input.Id);
        if (old.RowVersion != input.RowVersion) throw new InvalidOperationException("Hồ sơ đã thay đổi. Hãy tải lại.");
        var allowed = new Dictionary<string,(string From,string To,string Role)>
        {
            ["Confirm"]=("Received","Reviewed","Intake"),["Printed"]=("Reviewed","Printed","Intake"),
            ["Register"]=("Imported","Registered","Management"),["Submit"]=("Registered","Submitted","Management"),
            ["Approve"]=("Submitted","Approved","Management"),["Deliver"]=("Approved","Delivered","Management"),
            ["Receipt"]=("Delivered","Acknowledged","Management"),["Complete"]=("Acknowledged","Completed","Management")
        };
        if (!allowed.TryGetValue(action,out var rule) || old.State != rule.From)
            throw new InvalidOperationException("Chưa đúng bước nghiệp vụ. Hãy hoàn thành bước trước theo hướng dẫn.");
        RequireRole(rule.Role);
        if (string.IsNullOrWhiteSpace(note)) throw new InvalidOperationException("Nhập căn cứ/xác nhận cho thao tác này.");
        if (action == "Confirm")
        {
            if (new[]{old.Number,old.IssuedDate,old.Issuer,old.Recipient,old.DocumentType,old.Summary,old.Classification}.Any(string.IsNullOrWhiteSpace)
                || old.Classification == "Chưa xác định") throw new InvalidOperationException("Điền và lưu số, loại, ngày, đơn vị ban hành/nhận, trích yếu và độ mật trước khi xác nhận.");
            var text = ReadText(old);
            if (text.Blocks.Count == 0) throw new InvalidOperationException("Chưa có nội dung đối chiếu.");
            var reviewed = await new TextService(db,store,new DocumentTextExtractor()).SaveReviewAsync(
                new(old.DocumentId,text.RevisionId,text.Blocks,new[]{1},note),actor);
            if (!reviewed.Success) throw new InvalidOperationException(reviewed.Message);
        }
        if (action == "Register" && string.IsNullOrWhiteSpace(input.ExternalRef)) throw new InvalidOperationException("Nhập mã/số đến sau khi đã kiểm tra hồ sơ thực tế trong QLVB.");
        if (action == "Submit" && string.IsNullOrWhiteSpace(old.ProposedReceiver)) throw new InvalidOperationException("Lưu đề xuất lãnh đạo/đội xử lý trước khi trình.");
        if (action == "Approve")
        {
            if (new[]{input.Leader,input.ApprovedReceiver,input.DecisionEvidence}.Any(string.IsNullOrWhiteSpace))
                throw new InvalidOperationException("Nhập người duyệt, người/đội được giao và căn cứ phiếu đã ký.");
            ValidateDate(input.DueDate);
        }
        if (action == "Receipt" && string.IsNullOrWhiteSpace(input.ReceiptEvidence)) throw new InvalidOperationException("Nhập căn cứ chữ ký nhận thực tế.");
        var next = old with { State=rule.To,RowVersion=old.RowVersion+1,
            ExternalRef=action=="Register"?input.ExternalRef:old.ExternalRef,
            Leader=action=="Approve"?input.Leader:old.Leader,
            ApprovedReceiver=action=="Approve"?input.ApprovedReceiver:old.ApprovedReceiver,
            DueDate=action=="Approve"?input.DueDate:old.DueDate,
            DecisionEvidence=action=="Approve"?input.DecisionEvidence:old.DecisionEvidence,
            ReceiptEvidence=action=="Receipt"?input.ReceiptEvidence:old.ReceiptEvidence,
            CompletedUtc=action=="Complete"?DateTime.UtcNow.ToString("o"):old.CompletedUtc };
        await Persist(old,next,action,note); return next;
    }

    private async Task Persist(CorrespondenceCase old, CorrespondenceCase next, string action, string note)
    {
        using var c=db.CreateConnection(); using var tx=c.BeginTransaction(); using var q=c.CreateCommand(); q.Transaction=tx;
        q.CommandText="UPDATE CorrespondenceCase SET payloadJson=@json,rowVersion=@ver WHERE id=@id AND rowVersion=@expected";
        q.Parameters.AddWithValue("@json",JsonSerializer.Serialize(next,Json));q.Parameters.AddWithValue("@ver",next.RowVersion);
        q.Parameters.AddWithValue("@id",old.Id);q.Parameters.AddWithValue("@expected",old.RowVersion);
        if(q.ExecuteNonQuery()!=1) throw new InvalidOperationException("Xung đột phiên bản hồ sơ.");
        await Event(c,tx,next.Id,action,note);tx.Commit();
    }
    private static void Insert(SqliteConnection c,SqliteTransaction tx,CorrespondenceCase row)
    {
        using var q=c.CreateCommand();q.Transaction=tx;q.CommandText="INSERT INTO CorrespondenceCase VALUES(@id,@doc,@json,@ver)";
        q.Parameters.AddWithValue("@id",row.Id);q.Parameters.AddWithValue("@doc",row.DocumentId);
        q.Parameters.AddWithValue("@json",JsonSerializer.Serialize(row,Json));q.Parameters.AddWithValue("@ver",row.RowVersion);q.ExecuteNonQuery();
    }
    private async Task Event(SqliteConnection c,SqliteTransaction tx,string id,string action,string note)
    {
        using var q=c.CreateCommand();q.Transaction=tx;
        q.CommandText="INSERT INTO CorrespondenceCaseEvent VALUES(@id,@case,@action,@actor,@time,@note)";
        q.Parameters.AddWithValue("@id",Guid.NewGuid().ToString("N"));q.Parameters.AddWithValue("@case",id);
        q.Parameters.AddWithValue("@action",action);q.Parameters.AddWithValue("@actor",actor.WindowsSid);
        q.Parameters.AddWithValue("@time",DateTime.UtcNow.ToString("o"));q.Parameters.AddWithValue("@note",note);q.ExecuteNonQuery();
        await new AuditLogger().LogAsync(c,tx,actor,action,"CorrespondenceCase",id);
    }
    public static void ValidateDate(string value)
    {
        if(value.Length>0 && !DateOnly.TryParseExact(value,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out _))
            throw new InvalidOperationException("Ngày phải theo định dạng yyyy-MM-dd; để trống nếu chưa xác định hạn.");
    }
    public static string Deadline(CorrespondenceCase row,DateTime nowUtc) => DeadlineCalculator.Evaluate(
        row.DueDate.Length==0?"Unknown":"Date",row.DueDate,null,row.State=="Completed"?nowUtc.ToString("o"):null,nowUtc,TimeSpan.FromDays(3)).DisplayText;

    private record Package(int Version,string OriginWorkspaceId,CorrespondenceCase Case,IReadOnlyList<TextBlockDto> Blocks);
    private static string Hash(byte[] bytes)=>Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();
    public async Task ExportAsync(CorrespondenceCase input,string destination)
    {
        RequireRole("Intake");var row=Get(input.Id);
        if(row.State!="Printed") throw new InvalidOperationException("Xác nhận thông tin và đã in bản giấy trước khi xuất USB.");
        using var c=db.CreateConnection();using var q=c.CreateCommand();q.CommandText="SELECT id FROM Workspace LIMIT 1";
        var origin=(string)q.ExecuteScalar()!;
        byte[] metadata=Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new Package(1,origin,row,ReadText(row).Blocks),Json));
        var original=await File.ReadAllBytesAsync(store.GetObjectAbsolutePath(row.ObjectHash));
        if(Hash(original)!=row.ObjectHash) throw new InvalidDataException("Tệp gốc không khớp SHA256.");
        using(var stream=new FileStream(destination,FileMode.CreateNew)) using(var zip=new ZipArchive(stream,ZipArchiveMode.Create))
        {
            foreach(var pair in new[]{("metadata.json",metadata),("original",original),("checksum.sha256",Encoding.UTF8.GetBytes($"{Hash(metadata)} metadata.json\n{row.ObjectHash} original\n"))})
            { using var output=zip.CreateEntry(pair.Item1).Open();await output.WriteAsync(pair.Item2); }
        }
        using var tx=c.BeginTransaction();await Event(c,tx,row.Id,"Xuất USB",Path.GetFileName(destination));tx.Commit();
    }
    public async Task<CorrespondenceCase> ImportAsync(string packagePath,bool scanned)
    {
        RequireRole("Management");
        if(!scanned) throw new InvalidOperationException("Cán bộ cần xác nhận gói đã được kiểm tra mã độc theo quy trình của đơn vị.");
        using var zip=ZipFile.OpenRead(packagePath);
        if(zip.Entries.Count!=3 || zip.Entries.Any(e=>e.Length>64*1024*1024) || zip.Entries.Select(e=>e.FullName).Distinct().Count()!=3)
            throw new InvalidDataException("Gói không hợp lệ hoặc vượt giới hạn 64 MiB mỗi tệp.");
        async Task<byte[]> Read(string name){var e=zip.GetEntry(name)??throw new InvalidDataException("Thiếu "+name);using var s=e.Open();using var m=new MemoryStream();await s.CopyToAsync(m);return m.ToArray();}
        var meta=await Read("metadata.json");var bytes=await Read("original");var checksum=Encoding.UTF8.GetString(await Read("checksum.sha256"));
        var envelope=JsonSerializer.Deserialize<Package>(meta)??throw new InvalidDataException("Metadata rỗng.");
        if(envelope.Version!=1 || Hash(bytes)!=envelope.Case.ObjectHash || checksum!=$"{Hash(meta)} metadata.json\n{Hash(bytes)} original\n")
            throw new InvalidDataException("Sai checksum hoặc phiên bản gói.");
        if(envelope.Case.State!="Printed") throw new InvalidDataException("Gói chưa qua bước xác nhận và in.");
        using var c=db.CreateConnection();using var tx=c.BeginTransaction();
        using(var check=c.CreateCommand())
        {
            check.Transaction=tx;check.CommandText="SELECT contentHash,localCaseId FROM CorrespondenceCaseImport WHERE originWorkspaceId=@ws AND originCaseId=@id";
            check.Parameters.AddWithValue("@ws",envelope.OriginWorkspaceId);check.Parameters.AddWithValue("@id",envelope.Case.Id);
            using var r=check.ExecuteReader();
            if(r.Read()) { var hash=r.GetString(0);var id=r.GetString(1);r.Close();if(hash!=Hash(meta))throw new InvalidDataException("TransferConflict: hồ sơ nguồn đã có nhưng nội dung/phiên bản khác. Cần đối soát.");tx.Rollback();return Get(id); }
        }
        using var content=new MemoryStream(bytes);var fo=await store.StoreStreamAsync(content,Path.GetExtension(envelope.Case.OriginalName));
        var row=envelope.Case with {Id=Guid.NewGuid().ToString("N"),DocumentId=Guid.NewGuid().ToString("N"),DossierId=Guid.NewGuid().ToString("N"),State="Imported",RowVersion=1};
        string rev=Guid.NewGuid().ToString("N"),now=DateTime.UtcNow.ToString("o");
        using(var insert=c.CreateCommand())
        {
            insert.Transaction=tx;insert.CommandText=@"
INSERT OR IGNORE INTO FileObject VALUES(@hash,@path,@size,@media,@now);
INSERT INTO Dossier(id,businessCode,state,receivedUtc) VALUES(@dos,@code,'Received',@now);
INSERT INTO Document(id,dossierId,objectHash,originalName,sourceKind,status,createdUtc) VALUES(@doc,@dos,@hash,@name,'Transfer','Imported',@now);
INSERT INTO TextRevision(id,documentId,versionNo,method,reviewState,createdUtc) VALUES(@rev,@doc,1,'Transfer','NeedsReview',@now);
UPDATE Document SET currentRevisionId=@rev WHERE id=@doc;";
            foreach(var p in new (string,object)[]{("@hash",fo.Hash),("@path",fo.RelativePath),("@size",fo.SizeBytes),("@media",fo.MediaType),("@now",now),("@dos",row.DossierId),("@code",row.Code+"-"+row.Id[..6]),("@doc",row.DocumentId),("@name",row.OriginalName),("@rev",rev)})insert.Parameters.AddWithValue(p.Item1,p.Item2);
            insert.ExecuteNonQuery();
        }
        foreach(var block in envelope.Blocks)
        {
            using var b=c.CreateCommand();b.Transaction=tx;string id=Guid.NewGuid().ToString("N");
            b.CommandText=@"INSERT INTO TextBlock(id,revisionId,ordinal,locatorJson,extractedText,rawTextNfc,searchText,reviewed) VALUES(@id,@rev,@ord,@loc,@raw,@raw,@search,0);
INSERT INTO TextSearch VALUES(@doc,@rev,@id,@raw,@search);";
            foreach(var p in new (string,object)[]{("@id",id),("@rev",rev),("@ord",block.Ordinal),("@loc",block.LocatorJson),("@raw",block.RawTextNfc),("@search",VietnameseTextNormalizer.PrepareSearchText(block.RawTextNfc)),("@doc",row.DocumentId)})b.Parameters.AddWithValue(p.Item1,p.Item2);
            b.ExecuteNonQuery();
        }
        Insert(c,tx,row);
        using(var map=c.CreateCommand()) {map.Transaction=tx;map.CommandText="INSERT INTO CorrespondenceCaseImport VALUES(@ws,@origin,@ver,@hash,@local)";
            foreach(var p in new (string,object)[]{("@ws",envelope.OriginWorkspaceId),("@origin",envelope.Case.Id),("@ver",envelope.Case.RowVersion),("@hash",Hash(meta)),("@local",row.Id)})map.Parameters.AddWithValue(p.Item1,p.Item2);map.ExecuteNonQuery();}
        await Event(c,tx,row.Id,"Nhập USB","Đã kiểm tra checksum; cán bộ xác nhận đã kiểm tra mã độc. Nguồn: "+envelope.OriginWorkspaceId);
        tx.Commit();return row;
    }
}
