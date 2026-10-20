using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using SACD.Application.Common;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;

namespace SACD.Infrastructure.Services;

internal sealed class VerifiedAggregation(SqliteDatabaseContext database)
{
    private sealed record Fact(string Id,string Logical,string Value,string Start,string End,string Scope,string? DedupId,string? Role,string? Canonical);
    public async Task<OperationResult<AggregateResult>> AggregateAsync(AggregateRequest request,ActorContext actor,CancellationToken ct)
    {
        OperationResult<AggregateResult> Fail(string message) => OperationResult<AggregateResult>.Fail(ErrorCodes.NeedsReview,message,actor.CorrelationId);
        if(!DateOnly.TryParseExact(request.PeriodStart,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var start) ||
           !DateOnly.TryParseExact(request.PeriodEnd,"yyyy-MM-dd",CultureInfo.InvariantCulture,DateTimeStyles.None,out var end) || start>end)
            return Fail("Kỳ tổng hợp không hợp lệ.");
        if(request.GroupBy!="None")return Fail("Chưa hỗ trợ tổng hợp nhiều nhóm. Chọn một phạm vi đã xác nhận.");
        if(request.ConfirmedFactVersionIds.Count==0 || request.ConfirmedFactVersionIds.Distinct().Count()!=request.ConfirmedFactVersionIds.Count)
            return Fail("Chọn tập số liệu không rỗng và không lặp ID.");
        using var connection=database.CreateConnection();using var tx=connection.BeginTransaction();
        using var metric=connection.CreateCommand();metric.Transaction=tx;metric.CommandText="SELECT kind FROM MetricDefinitionVersion WHERE id=@id;";metric.Parameters.AddWithValue("@id",request.MetricVersionId);
        if(await metric.ExecuteScalarAsync(ct) is not string kind)return Fail("Chỉ tiêu không tồn tại.");
        var facts=new List<Fact>();
        foreach(var id in request.ConfirmedFactVersionIds)
        {
            ct.ThrowIfCancellationRequested();using var query=connection.CreateCommand();query.Transaction=tx;
            query.CommandText=@"SELECT f.logicalId,f.valueDecimalText,f.periodStart,f.periodEnd,f.scopeJson,
                dm.decisionVersionId,dm.role,dd.canonicalFactVersionId FROM FactVersion f
                LEFT JOIN DedupMember dm ON dm.factVersionId=f.id
                LEFT JOIN DedupDecisionVersion dd ON dd.id=dm.decisionVersionId
                WHERE f.id=@id AND f.metricVersionId=@metric AND f.status='Confirmed' AND f.valueStatus='Known';";
            query.Parameters.AddWithValue("@id",id);query.Parameters.AddWithValue("@metric",request.MetricVersionId);
            using(var reader=await query.ExecuteReaderAsync(ct))
            {
                if(!await reader.ReadAsync(ct))return Fail("Số liệu thiếu, chưa xác nhận hoặc khác chỉ tiêu: "+id);
                var fact=new Fact(id,reader.GetString(0),reader.GetString(1),reader.GetString(2),reader.GetString(3),reader.GetString(4),
                    reader.IsDBNull(5)?null:reader.GetString(5),reader.IsDBNull(6)?null:reader.GetString(6),reader.IsDBNull(7)?null:reader.GetString(7));
                if(await reader.ReadAsync(ct))return Fail("Số liệu thuộc nhiều quyết định trùng; cần đối soát: "+id);
                if(string.CompareOrdinal(fact.Start,request.PeriodStart)<0 || string.CompareOrdinal(fact.End,request.PeriodEnd)>0)return Fail("Số liệu nằm ngoài kỳ được chọn: "+id);
                if(fact.Role=="Unresolved")return Fail("Chưa giải quyết trùng lặp: "+id);
                if(fact.Role is "Equivalent" or "Excluded")
                {
                    if(fact.Canonical==null || !request.ConfirmedFactVersionIds.Contains(fact.Canonical))return Fail("Thiếu số liệu đại diện cho nhóm trùng: "+id);
                    continue;
                }
                facts.Add(fact);
            }
        }
        if(facts.Count==0 || facts.Select(f=>f.Scope).Distinct().Count()!=1 || facts.Select(f=>f.Logical).Distinct().Count()!=facts.Count)
            return Fail("Tập số liệu không cùng phạm vi hoặc chứa nhiều phiên bản của cùng số liệu.");
        var values=new Dictionary<string,decimal>();
        foreach(var fact in facts)
        {
            using var evidence=connection.CreateCommand();evidence.Transaction=tx;
            evidence.CommandText="SELECT e.evidenceId,f.sourceRevisionId FROM FactVersion f JOIN FactEvidence e ON e.factVersionId=f.id WHERE f.id=@id;";
            evidence.Parameters.AddWithValue("@id",fact.Id);
            var references=new List<(string Id,string Revision)>();
            using(var reader=await evidence.ExecuteReaderAsync(ct))while(await reader.ReadAsync(ct))references.Add((reader.GetString(0),reader.GetString(1)));
            if(references.Count==0)return Fail("Số liệu xác nhận cũ thiếu bằng chứng: "+fact.Id);
            foreach(var reference in references)
            {
                var error=await ReportReadiness.ValidateEvidenceAsync(connection,tx,reference.Id,reference.Revision,ct);
                if(error!=null)return Fail(error);
            }
            if(!InvariantDecimal.TryParseExact(fact.Value,out var number))
                return Fail("Giá trị không phải decimal invariant: "+fact.Id);
            values.Add(fact.Id,number);
        }
        decimal total;string display;decimal? numerator=null,denominator=null;List<Fact> contributions;
        try
        {
            checked
            {
                if(kind=="Ratio")
                {
                    var nums=request.NumeratorFactIds;var dens=request.DenominatorFactIds;
                    if(nums==null || dens==null || nums.Count==0 || dens.Count==0 || nums.Distinct().Count()!=nums.Count || dens.Distinct().Count()!=dens.Count || nums.Intersect(dens).Any() ||
                        !nums.Concat(dens).ToHashSet().SetEquals(values.Keys))return Fail("Tỷ lệ cần khai rõ tập tử số và mẫu số, không suy theo thứ tự đầu vào.");
                    numerator=nums.Aggregate(0m,(sum,id)=>checked(sum+values[id]));denominator=dens.Aggregate(0m,(sum,id)=>checked(sum+values[id]));
                    if(denominator==0)return Fail("Mẫu số bằng 0: không áp dụng, không trả tổng bằng 0.");
                    total=numerator.Value/denominator.Value;
                    display=new ExactRatio(numerator.Value,denominator.Value).ToPercentageString(2);contributions=facts;
                }
                else if(kind is "Stock" or "Cumulative")
                {
                    var ordered=facts.OrderBy(f=>f.End,StringComparer.Ordinal).ToList();
                    if(ordered.Select(f=>f.End).Distinct().Count()!=ordered.Count)return Fail("Nhiều quan sát cùng thời điểm, cần chọn căn cứ đại diện.");
                    if(kind=="Cumulative" && (facts.Select(f=>f.Start).Distinct().Count()!=1 || ordered.Zip(ordered.Skip(1),(a,b)=>values[b.Id]<values[a.Id]).Any(x=>x)))
                        return Fail("Lũy kế khác mốc bắt đầu hoặc có điều chỉnh giảm; cần quy tắc đối soát.");
                    contributions=new(){ordered[^1]};total=values[ordered[^1].Id];display=total.ToString("0.############################",CultureInfo.InvariantCulture);
                }
                else
                {
                    var ordered=facts.OrderBy(f=>f.Start,StringComparer.Ordinal).ToList();
                    if(ordered.Zip(ordered.Skip(1),(a,b)=>string.CompareOrdinal(a.End,b.Start)>=0).Any(x=>x))return Fail("Các khoảng phát sinh chồng nhau; chưa được cộng tổng.");
                    contributions=facts;total=values.Values.Aggregate(0m,(sum,value)=>checked(sum+value));display=total.ToString("0.############################",CultureInfo.InvariantCulture);
                }
            }
        }
        catch(OverflowException){return OperationResult<AggregateResult>.Fail(ErrorCodes.NumericOverflow,"Số liệu vượt giới hạn decimal.",actor.CorrelationId);}
        string runId=Guid.NewGuid().ToString("N");
        string snapshot=JsonSerializer.Serialize(new{request,kind,facts});
        string hash=Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(snapshot))).ToLowerInvariant();
        using var insert=connection.CreateCommand();insert.Transaction=tx;
        insert.CommandText="INSERT INTO AggregationRun(id,metricVersionId,queryJson,resultJson,snapshotHash,state,createdUtc) VALUES(@id,@metric,@query,@result,@hash,'Valid',@now);";
        insert.Parameters.AddWithValue("@id",runId);insert.Parameters.AddWithValue("@metric",request.MetricVersionId);insert.Parameters.AddWithValue("@query",snapshot);
        insert.Parameters.AddWithValue("@result",JsonSerializer.Serialize(new{total,numerator,denominator,count=contributions.Count,kind}));insert.Parameters.AddWithValue("@hash",hash);insert.Parameters.AddWithValue("@now",DateTime.UtcNow.ToString("o"));
        await insert.ExecuteNonQueryAsync(ct);
        foreach(var fact in contributions)
        {
            using var item=connection.CreateCommand();item.Transaction=tx;
            item.CommandText="INSERT INTO AggregationContribution(aggregationId,factVersionId,groupKey,operation,contributionDecimalText,dedupDecisionVersionId) VALUES(@run,@fact,@scope,@op,@value,@dedup);";
            item.Parameters.AddWithValue("@run",runId);item.Parameters.AddWithValue("@fact",fact.Id);item.Parameters.AddWithValue("@scope",fact.Scope);item.Parameters.AddWithValue("@op",kind);
            item.Parameters.AddWithValue("@value",fact.Value);item.Parameters.AddWithValue("@dedup",(object?)fact.DedupId??DBNull.Value);await item.ExecuteNonQueryAsync(ct);
        }
        tx.Commit();return OperationResult<AggregateResult>.Ok(new(runId,request.MetricVersionId,total,display,contributions.Count,"Valid"),actor.CorrelationId);
    }
}
