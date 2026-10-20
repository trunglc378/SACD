# SACD — hỗ trợ xử lý văn bản trên hai mạng nội bộ tách biệt

Windows .NET 10/WPF, SQLite cục bộ. Mỗi máy truy cập website trong mạng nội bộ của mình, không dùng Internet và không liên thông với máy kia. Chuyển hồ sơ bằng USB an toàn theo quy trình đơn vị.

## Bản sửa ngày 20/10/2026

- PDF có text được đọc thật theo trang; trang scan đánh dấu cần OCR, chặn xác nhận toàn văn. DOCX đọc thêm bảng/header/footer.
- Gợi ý số/ngày/đơn vị/người ký/trích yếu từ trường có nhãn; cán bộ đối chiếu, không tự đặt hạn/độ mật.
- Tìm phiên bản hiện hành; khóa sửa text đã được tham chiếu. Tra case trực tiếp theo ID.
- Sửa Stock, Ratio thập phân và kiểm tra kỳ/phạm vi/ID lặp/khoảng chồng; lưu contribution và snapshot hash thật. Tỷ lệ cần tập tử/mẫu rõ ràng.
- Kiểm tra nhận định/bằng chứng trước Reviewed/Submission tại service. UI chỉ xuất nháp; mẫu Word tùy chỉnh chưa hỗ trợ sẽ trả lỗi rõ ràng.
- Phục hồi vào kho mới sau kiểm tra chủ/schema/SQLite/FK/hash. Mở kho kiểm tra SID, không tự chuyển chủ.
- Có bước riêng đối soát chỉ đạo đã cập nhật QLVB sau Approved, trước giao giấy: xác nhận thủ công có căn cứ, chưa phải connector web.
- Các service chưa tích hợp in/QLVB/OCR/recorder trả NotConfigured, không sinh mã thành công giả. RPA mô phỏng vẫn là thử trạng thái.
- Báo cáo đếm hồ sơ/nhiệm vụ từ DB theo kỳ giờ Việt Nam. Nhiệm vụ chưa rõ hạn giữ Unknown.

Xem [bản sửa và chạy thử](docs/09_SACD_Ban_sua_va_chay_thu_2026-10-20.md), [quy trình](docs/08_SACD_Quy_trinh_hop_nhat_v1.2.md) và [đánh giá trước sửa](docs/analysis/2026-10-20-review-and-proposal.md).

## Giới hạn

Chưa có connector website đơn vị, OCR scan thực, AI local, recorder tổng quát, mẫu phiếu chính thức, nhiều đính kèm/case và luồng trả lại/gia hạn/sửa sai đầy đủ. Không tuyên bố hoàn thành 117 FR/24 NFR. UIA bốn trường cần mapping đã khảo sát, đọc lại và dừng trước Lưu. SHA256 không thay kiểm tra mã độc/chữ ký/mã hóa.

## Build và chạy

Cần .NET 10 SDK. Trong phiên phát triển này có SDK cục bộ `.tools/dotnet`, không thay PATH hệ thống. Dùng lệnh `dotnet` nếu đã cài SDK, hoặc `& ./.tools/dotnet/dotnet.exe` với `$env:DOTNET_ROOT = "$PWD/.tools/dotnet"`.

```powershell
dotnet build SACD.sln
dotnet test tests/SACD.Tests/SACD.Tests.csproj
dotnet run --project tests/SACD.Desktop.Smoke -- TestResults/workflow-ui-current.png
dotnet run --project src/SACD.Desktop
dotnet publish src/SACD.Desktop/SACD.Desktop.csproj -c Release -r win-x64 --self-contained true -o dist/SACD
```

Mở `dist/SACD/SACD.Desktop.exe` từ **toàn bộ thư mục publish** khi thử offline; không cần SDK. Kho dữ liệu ở ngoài thư mục chương trình. Chỉ công bố tương thích sau khi thử đúng OS/trình duyệt/máy in/USB. Docker chỉ là CLI thử kỹ thuật, không chạy WPF.

Kết quả mới lưu riêng tại `TestResults/2026-10-20-fixed.trx` và `TestResults/2026-10-20-workflow-ui.png`. Giữ TRX 16/10 làm lịch sử, không quy đổi số test thành phần trăm yêu cầu hoàn thành.
