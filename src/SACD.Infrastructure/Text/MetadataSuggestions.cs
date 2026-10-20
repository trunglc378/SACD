using System.Globalization;
using System.Text.RegularExpressions;
using SACD.Application.Contracts;

namespace SACD.Infrastructure.Text;

/// <summary>Conservative, labelled-field suggestions only; never confirms metadata or invents a deadline.</summary>
public static class MetadataSuggestions
{
    public static CorrespondenceCase Apply(CorrespondenceCase row,string text)
    {
        string Field(string label)=>Regex.Match(text,@"(?im)^\s*(?:"+label+@")\s*:\s*([^\r\n]+)").Groups[1].Value.Trim();
        var number=Field("Số(?: văn bản)?");var issuer=Field("Đơn vị ban hành|Cơ quan ban hành");var signer=Field("Người ký");
        var summary=Field("Trích yếu");if(summary.Length==0)summary=Regex.Match(text,@"(?im)^\s*V/v\s+([^\r\n]+)").Groups[1].Value.Trim();
        var dateText=Field("Ngày ban hành");var date="";
        if(DateOnly.TryParseExact(dateText,new[]{"yyyy-MM-dd","dd/MM/yyyy","d/M/yyyy"},CultureInfo.InvariantCulture,DateTimeStyles.None,out var issued))date=issued.ToString("yyyy-MM-dd");
        return row with {Number=row.Number.Length==0?number:row.Number,Issuer=row.Issuer.Length==0?issuer:row.Issuer,
            Signer=row.Signer.Length==0?signer:row.Signer,Summary=row.Summary.Length==0?summary:row.Summary,IssuedDate=row.IssuedDate.Length==0?date:row.IssuedDate};
    }
}
