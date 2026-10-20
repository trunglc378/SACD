using System.Globalization;
using System.Text.Json;
using System.Windows.Data;

namespace SACD.Desktop;

public sealed record MachineProfileOption(string Code, string Label);

/// <summary>Translate presentation values without changing persisted workflow codes.</summary>
public sealed class VietnameseDisplay : IValueConverter
{
    public static string Label(string? value) => value switch
    {
        "Intake" => "Máy 1 — Tiếp nhận", "Management" => "Máy 2 — Quản lý",
        "Received" => "Đã tiếp nhận", "Reviewed" => "Đã đối chiếu", "Printed" => "Đã in giấy",
        "Imported" => "Chờ nhập quản lý văn bản", "Registered" => "Đã lưu quản lý văn bản",
        "Submitted" => "Đã trình lãnh đạo", "Approved" => "Đã phê duyệt", "Delivered" => "Đã giao",
        "Acknowledged" => "Đã ký nhận", "Completed" => "Đã hoàn thành",
        "Open" or "Pending" => "Chờ xử lý", "InProgress" => "Đang xử lý",
        "Assigned" => "Đã phân công", "Reopened" => "Mở lại xử lý", "Checking" => "Đang kiểm tra",
        "AwaitingDecision" => "Chờ phê duyệt", "Returned" or "ReturnedForEdit" => "Trả lại để sửa",
        "Rejected" => "Không chấp thuận", "Archived" => "Đã lưu trữ", "Confirmed" => "Đã xác nhận",
        "Proposed" => "Đã đề xuất", "NeedsReview" => "Cần kiểm tra", "Partial" => "Chưa đầy đủ",
        "Queued" => "Đang chờ chạy", "Running" or "Executing" => "Đang thực hiện",
        "Paused" => "Đã tạm dừng", "Succeeded" => "Đã hoàn tất", "Interrupted" => "Đã gián đoạn",
        "Quarantined" => "Đã cách ly", "Exported" => "Đã xuất gói", "ConfirmedPrinted" => "Đã xác nhận in",
        "NotUpdated" => "Chưa cập nhật", "Verified" => "Đã đối soát", "Draft" => "Bản nháp",
        "Submission" => "Bản trình lãnh đạo", "Prepared" => "Đã chuẩn bị", "Dispatched" => "Đã thực hiện",
        "AwaitingUser" or "PauseForUser" => "Chờ cán bộ kiểm tra", "NeedsReconciliation" => "Cần đối soát",
        "Failed" => "Không thành công", "Cancelled" => "Đã hủy", "Unknown" => "Chưa xác định",
        "Confirm" => "Xác nhận đối chiếu", "Register" => "Đối soát đã lưu", "Submit" => "Trình lãnh đạo",
        "Approve" => "Ghi nhận phê duyệt", "Deliver" => "Giao bản giấy", "Receipt" => "Xác nhận ký nhận",
        "Complete" => "Xác nhận hoàn thành", "Save" => "Lưu thông tin", "Export" => "Xuất gói chuyển",
        "Import" => "Nhập gói chuyển", "Receive" => "Tiếp nhận tệp",
        null or "" => "", _ => "Chưa xác định"
    };

    public static string Message(string? value)
    {
        var text = value ?? "";
        foreach (var (source, target) in new[] {
            ("TransferConflict:", "Xung đột gói chuyển:"), ("checksum", "mã kiểm tra toàn vẹn"),
            ("Mapping SHA256:", "Mã kiểm tra cấu hình:"), ("Mapping", "Cấu hình ánh xạ"),
            ("mapping", "cấu hình ánh xạ"), ("connector website", "kết nối trang nội bộ"),
            ("snapshot", "bản chụp dữ liệu"), ("PDF/scan", "PDF hoặc bản quét"),
            ("decimal invariant", "số thập phân theo định dạng chuẩn"),
            ("RowVersion", "phiên bản dữ liệu"), ("Task", "nhiệm vụ"),
            ("Quarantined", "đã cách ly"),
            ("RPA", "Tự động hóa"), ("OCR", "nhận dạng chữ"), ("worker", "tiến trình xử lý"),
            ("Spool", "hàng đợi in"), ("Workspace", "Kho làm việc"), ("schema", "cấu trúc dữ liệu") })
            text = text.Replace(source, target, StringComparison.Ordinal);
        return text;
    }

    public static string Error(Exception exception)
    {
        System.Diagnostics.Trace.TraceError(exception.ToString());
        return exception switch {
            UnauthorizedAccessException => "Không có quyền truy cập. Kiểm tra quyền của tài khoản đối với thư mục hoặc tệp đã chọn.",
            System.IO.FileNotFoundException or System.IO.DirectoryNotFoundException => "Không tìm thấy tệp hoặc thư mục. Kiểm tra lại đường dẫn.",
            System.IO.IOException => "Không thể đọc hoặc ghi tệp. Kiểm tra dung lượng trống và đóng ứng dụng đang sử dụng tệp.",
            JsonException => "Tệp dữ liệu hoặc cấu hình không đúng định dạng. Kiểm tra lại tệp đã chọn.",
            Microsoft.Data.Sqlite.SqliteException => "Không thể truy cập dữ liệu trong kho. Kiểm tra kho đang mở và quyền truy cập thư mục.",
            System.ComponentModel.Win32Exception => "Không mở được ứng dụng đọc tệp. Kiểm tra phần mềm được cài cho loại tệp này.",
            _ => LocalizedError(exception.Message)
        };
    }

    public static string LocalizedError(string? message)
    {
        var text = System.Text.RegularExpressions.Regex.Replace(Message(message), @"\s*\(Parameter '[^']*'\)", "");
        // Library diagnostics stay in the trace; never expose an English system error as product copy.
        return text.Any(c => c is >= '\u00c0' and <= '\u1ef9')
            ? text : "Không thể hoàn tất thao tác. Kiểm tra dữ liệu, cấu hình và quyền truy cập rồi thử lại.";
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        string text = value?.ToString() ?? "";
        return parameter?.ToString() switch {
            "Boolean" => value is true ? "Có" : "Không",
            "Locator" => Locator(text),
            "Message" => Message(text),
            "Note" => text.StartsWith("Mapping SHA256:", StringComparison.Ordinal)
                ? "Mã kiểm tra cấu hình:" + text[15..] : text,
            "Action" => Label(text) is var label && label != "Chưa xác định" ? label : Message(text),
            "Time" => DateTimeOffset.TryParse(text, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var at)
                ? at.ToOffset(TimeSpan.FromHours(7)).ToString("dd/MM/yyyy HH:mm", CultureInfo.GetCultureInfo("vi-VN")) : "",
            _ => Label(text)
        };
    }

    private static string Locator(string text)
    {
        try {
            using var json = JsonDocument.Parse(text);
            var root = json.RootElement;
            string result = root.GetProperty("type").GetString() switch {
                "page" => "Trang " + root.GetProperty("pageNumber").GetInt32(),
                "line" => "Dòng " + root.GetProperty("lineNumber").GetInt32(),
                "paragraph" => "Đoạn " + (root.GetProperty("index").GetInt32() + 1),
                "document" => "Văn bản", "file" => "Tệp", _ => "Vị trí nguồn"
            };
            return root.TryGetProperty("needsOcr", out var needs) && needs.ValueKind == JsonValueKind.True
                ? result + " — cần nhận dạng chữ" : result;
        } catch (Exception ex) when (ex is JsonException or InvalidOperationException or KeyNotFoundException or FormatException) {
            return "Vị trí nguồn chưa xác định";
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
}
