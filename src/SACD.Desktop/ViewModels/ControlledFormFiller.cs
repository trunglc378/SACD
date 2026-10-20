using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Windows.Automation;
using SACD.Application.Contracts;

namespace SACD.Desktop.ViewModels;

// Windows UIA adapter, opt-in mapping per target application. Deliberately no Save/Submit action.
public static class ControlledFormFiller
{
    public record Mapping(string ExecutablePath,string WindowTitle,Dictionary<string,string> Fields);
    public static void Fill(string mappingPath,CorrespondenceCase row)
    {
        var map=JsonSerializer.Deserialize<Mapping>(File.ReadAllText(mappingPath))??throw new InvalidOperationException("Cấu hình ánh xạ rỗng.");
        if(!Path.IsPathFullyQualified(map.ExecutablePath)||!File.Exists(map.ExecutablePath)||string.IsNullOrWhiteSpace(map.WindowTitle))
            throw new InvalidOperationException("Cấu hình ánh xạ cần đường dẫn chương trình và tiêu đề cửa sổ chính xác.");
        var windows=AutomationElement.RootElement.FindAll(TreeScope.Children,new PropertyCondition(AutomationElement.NameProperty,map.WindowTitle));
        if(windows.Count!=1)throw new InvalidOperationException("Không tìm thấy duy nhất một cửa sổ QLVB. Mở đúng biểu mẫu trước khi chạy.");
        var window=windows[0];using var process=Process.GetProcessById(window.Current.ProcessId);
        if(!string.Equals(Path.GetFullPath(process.MainModule!.FileName),Path.GetFullPath(map.ExecutablePath),StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("Cửa sổ không thuộc chương trình đã cấu hình.");
        var values=new Dictionary<string,string>{{"Number",row.Number},{"Summary",row.Summary},{"Issuer",row.Issuer},{"DueDate",row.DueDate}};
        if(map.Fields==null || !values.Keys.ToHashSet().SetEquals(map.Fields.Keys) || map.Fields.Values.Any(string.IsNullOrWhiteSpace)
            || map.Fields.Values.Distinct().Count()!=4)throw new InvalidOperationException("Cấu hình phải có định danh riêng cho 4 trường: số văn bản, trích yếu, đơn vị ban hành và hạn giải quyết.");
        var targets=new List<(ValuePattern Pattern,string Value)>();
        foreach(var field in map.Fields)
        {
            var matches=window.FindAll(TreeScope.Descendants,new PropertyCondition(AutomationElement.AutomationIdProperty,field.Value));
            if(matches.Count!=1||!matches[0].Current.IsEnabled||!matches[0].TryGetCurrentPattern(ValuePattern.Pattern,out var pattern)
                || ((ValuePattern)pattern).Current.IsReadOnly)throw new InvalidOperationException("Trường không duy nhất hoặc không cho nhập: "+(field.Key switch { "Number"=>"số văn bản", "Summary"=>"trích yếu", "Issuer"=>"đơn vị ban hành", "DueDate"=>"hạn giải quyết", _=>"chưa xác định" }));
            targets.Add(((ValuePattern)pattern,values[field.Key]));
        }
        // Resolve and validate every selector before the first side effect. Never retry automatically.
        foreach(var (pattern,value) in targets)
        {
            pattern.SetValue(value);
            if(pattern.Current.Value!=value)throw new InvalidOperationException("Giá trị đọc lại không khớp. Dừng để cán bộ đối soát; chưa bấm Lưu.");
        }
    }
}
