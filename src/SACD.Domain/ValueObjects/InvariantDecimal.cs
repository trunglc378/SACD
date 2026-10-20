using System.Globalization;
using System.Text.RegularExpressions;

namespace SACD.Domain.ValueObjects;

public static class InvariantDecimal
{
    public static bool TryParseExact(string text,out decimal value)
    {
        value=0;
        if(text.Length>128 || !Regex.IsMatch(text,@"^[+-]?\d+(?:\.\d+)?$",RegexOptions.CultureInvariant))return false;
        if(!decimal.TryParse(text,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out value))return false;
        string Normalize(string input)
        {
            bool negative=input.StartsWith('-');input=input.TrimStart('+','-');var parts=input.Split('.');
            string whole=parts[0].TrimStart('0');if(whole.Length==0)whole="0";
            string fraction=parts.Length>1?parts[1].TrimEnd('0'):"";
            return (negative && (whole!="0" || fraction.Length>0)?"-":"")+whole+(fraction.Length==0?"":"."+fraction);
        }
        return Normalize(text)==Normalize(value.ToString("0.############################",CultureInfo.InvariantCulture));
    }
}
