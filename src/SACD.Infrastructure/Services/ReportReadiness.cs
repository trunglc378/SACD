using Microsoft.Data.Sqlite;
using SACD.Domain.Entities;
using SACD.Domain.Rules;

namespace SACD.Infrastructure.Services;

internal static class ReportReadiness
{
    public static async Task<string?> ValidateEvidenceAsync(SqliteConnection connection,SqliteTransaction? tx,string? evidenceId,string? revisionId,CancellationToken ct)
    {
        if(string.IsNullOrWhiteSpace(evidenceId))return "Thiếu bằng chứng nguồn.";
        using var query=connection.CreateCommand();query.Transaction=tx;
        query.CommandText=@"SELECT e.revisionId,e.startOffset,e.endOffset,e.quoteHash,b.rawTextNfc
            FROM EvidenceRef e JOIN TextBlock b ON b.id=e.blockId AND b.revisionId=e.revisionId
            JOIN TextRevision t ON t.id=e.revisionId JOIN Document d ON d.id=t.documentId
            WHERE e.id=@id AND t.reviewState='Reviewed' AND b.reviewed=1 AND d.currentRevisionId=t.id;";
        query.Parameters.AddWithValue("@id",evidenceId);using var reader=await query.ExecuteReaderAsync(ct);
        if(!await reader.ReadAsync(ct))return "Bằng chứng không tồn tại, chưa đối chiếu hoặc không thuộc phiên bản hiện hành.";
        if(revisionId!=null && reader.GetString(0)!=revisionId)return "Bằng chứng không thuộc phiên bản được chọn.";
        try
        {
            BusinessRuleValidator.ValidateEvidenceInterval(new EvidenceRefEntity{RevisionId=reader.GetString(0),StartOffset=reader.GetInt32(1),EndOffset=reader.GetInt32(2),QuoteHash=reader.GetString(3)},reader.GetString(4));
        }
        catch(DomainValidationException ex){return ex.Message;}
        return null;
    }

    public static async Task<string?> ValidateAsync(SqliteConnection connection,SqliteTransaction? tx,string reportId,int expectedVersion,IReadOnlyList<string> checkedIds,CancellationToken ct)
    {
        using var report=connection.CreateCommand();report.Transaction=tx;
        report.CommandText="SELECT versionNo,isStale FROM ReportRevision WHERE id=@id;";report.Parameters.AddWithValue("@id",reportId);
        using(var reader=await report.ExecuteReaderAsync(ct))
        {
            if(!await reader.ReadAsync(ct))return "Không có phiên bản báo cáo.";
            if(reader.GetInt32(0)!=expectedVersion)return "Phiên bản báo cáo đã thay đổi.";
            if(reader.GetInt32(1)!=0)return "Nguồn báo cáo đã thay đổi, cần tạo bản nháp mới.";
        }
        using var query=connection.CreateCommand();query.Transaction=tx;
        query.CommandText=@"SELECT c.id,c.kind,c.checked,c.commentaryReason FROM Claim c
            JOIN ReportSection s ON s.id=c.sectionId WHERE s.reportRevisionId=@id;";
        query.Parameters.AddWithValue("@id",reportId);
        var claims=new List<(string Id,string Kind,bool Checked,string? Reason)>();
        using(var reader=await query.ExecuteReaderAsync(ct))
            while(await reader.ReadAsync(ct))claims.Add((reader.GetString(0),reader.GetString(1),reader.GetInt32(2)==1,reader.IsDBNull(3)?null:reader.GetString(3)));
        if(claims.Count==0)return "Báo cáo chưa có nhận định để rà soát.";
        if(checkedIds.Distinct().Count()!=checkedIds.Count || checkedIds.Any(id=>claims.All(c=>c.Id!=id)))return "Danh sách nhận định có ID lạ hoặc bị trùng.";
        foreach(var claim in claims)
        {
            if(!claim.Checked && !checkedIds.Contains(claim.Id))return "Còn nhận định chưa được đối chiếu.";
            if(claim.Kind=="Commentary")
            {
                if(string.IsNullOrWhiteSpace(claim.Reason))return "Bình luận chưa có lý do nghiệp vụ.";
                continue;
            }
            using var evidence=connection.CreateCommand();evidence.Transaction=tx;evidence.CommandText="SELECT evidenceId FROM ClaimEvidence WHERE claimId=@id;";evidence.Parameters.AddWithValue("@id",claim.Id);
            var ids=new List<string>();using(var reader=await evidence.ExecuteReaderAsync(ct))while(await reader.ReadAsync(ct))ids.Add(reader.GetString(0));
            if(ids.Count==0)return "Nhận định thực tế chưa gắn bằng chứng.";
            foreach(var id in ids)
            {
                var error=await ValidateEvidenceAsync(connection,tx,id,null,ct);
                if(error!=null)return error;
            }
        }
        using var sources=connection.CreateCommand();sources.Transaction=tx;
        sources.CommandText=@"SELECT COUNT(*) FROM ReportSource s JOIN TextRevision t ON t.id=s.sourceRevisionId
            JOIN Document d ON d.id=t.documentId WHERE s.reportRevisionId=@id AND
            (t.reviewState<>'Reviewed' OR d.currentRevisionId<>t.id OR s.sourceHash<>d.objectHash);";
        sources.Parameters.AddWithValue("@id",reportId);
        if(Convert.ToInt64(await sources.ExecuteScalarAsync(ct))>0)return "Nguồn báo cáo chưa được xác nhận hoặc không còn hiện hành.";
        return null;
    }
}
