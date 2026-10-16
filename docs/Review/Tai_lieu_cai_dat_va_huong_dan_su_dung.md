# Cài đặt và sử dụng SACD — luồng hợp nhất 1.2

Cập nhật ngày 16/10/2026. Phương án: Trợ lý văn thư + RPA hỗ trợ QLVB hiện có.

## 1. Môi trường

Dùng Windows có hỗ trợ .NET 8, ưu tiên Windows 11 x64, và .NET 8 SDK khi chạy từ mã nguồn. Không dùng hướng dẫn Windows 7 trong tài liệu cũ. SQLite được nhúng, không cần máy chủ cơ sở dữ liệu. Cần ứng dụng đọc tài liệu để mở/in tệp; tạo DOCX không cần cài Microsoft Word, nhưng mở và in DOCX cần ứng dụng tương thích.

```powershell
winget install --id Microsoft.DotNet.SDK.8 --exact --source winget
```

Mở lại PowerShell sau khi cài, rồi chạy lần lượt:

```powershell
Set-Location D:\Project\SACD
dotnet --list-sdks
dotnet restore SACD.sln
dotnet build SACD.sln
dotnet test tests\SACD.Tests\SACD.Tests.csproj
dotnet run --project src\SACD.Desktop\SACD.Desktop.csproj
```

## 2. Bắt đầu đúng luồng

1. Tab **Chọn máy / Mở kho**: chọn `Intake` cho Máy 1 hoặc `Management` cho Máy 2 khi tạo kho mới. Hai máy phải dùng hai kho riêng.
2. Mở kho, chuyển sang tab **Quy trình văn thư**. Đây là luồng nghiệp vụ chính. Các tab thử text và RPA mô phỏng phục vụ kiểm tra kỹ thuật.
3. Máy 1: kiểm tra email, tải tệp, tiếp nhận tệp thật, nhập và lưu thông tin, đối chiếu, xác nhận, mở/in, xác nhận bản giấy rồi xuất gói USB.
4. Máy 2: kiểm tra gói theo quy trình đơn vị, nhập gói, nhập QLVB thủ công hoặc bằng RPA có mapping; tự kiểm tra/bấm Lưu trên QLVB, ghi mã/số đến để đối soát.
5. Tạo phiếu Word có QR, kiểm tra/in và trình cùng tài liệu giấy. Sau khi lãnh đạo ký, cập nhật chỉ đạo, giao, ghi nhận ký nhận và theo dõi đến khi có kết quả hoàn thành.
6. Mỗi thao tác xác nhận cần nhập ghi chú/căn cứ. Mở tệp không đồng nghĩa đã in; tạo phiếu không đồng nghĩa đã trình; giao không đồng nghĩa đã ký nhận; ký nhận không đồng nghĩa hoàn thành.

Xem [bảng đối chiếu đầy đủ 12 bước, hướng dẫn thử hai máy và cấu hình RPA](../08_SACD_Quy_trinh_hop_nhat_v1.2.md).

## 3. Các lỗi đã sửa

- Khởi động thoát do ổ E: không sẵn sàng: bỏ đường dẫn cố định và chỉ khởi tạo kho sau khi cán bộ chọn mở.
- `TextRevision not found`: giao diện đọc revision hiện tại, không truyền DocumentId thay RevisionId; việc chọn văn bản không tạo revision mới.
- `UNIQUE constraint failed: ScriptVersion.scriptId, ScriptVersion.versionNo`: tăng versionNo theo từng script trong giao dịch và tính contentHash SHA256.
- Nhiệm vụ bổ sung: lấy DossierId từ Document được chọn, không dùng nhầm DocumentId.
- Trích xuất DOCX/TXT trong kho hash: dùng phần mở rộng của tên gốc.
- Báo cáo nháp: tự đăng ký mẫu dựng sẵn ở kho mới. Không tự đánh dấu đã rà soát để xuất bản trình.
- Sao lưu: chụp SQLite bằng Backup API, chỉ sao chép object có hash hợp lệ, không nén writer lock; thư mục sao lưu phải ở ngoài kho. Chưa tự diễn tập phục hồi.

## 4. Kiểm tra

```powershell
dotnet test tests\SACD.Tests\SACD.Tests.csproj --logger "trx;LogFileName=SACD.Tests.trx" --results-directory D:\Project\SACD\TestResults
dotnet run --project tests\SACD.Desktop.Smoke -- D:\Project\SACD\TestResults\workflow-ui.png
```

Bộ test hiện có 23 ca. Smoke test chạy trên Windows với dữ liệu và biểu mẫu QLVB giả lập; chưa thay UAT trên email, QLVB, máy in và USB của đơn vị.

## 5. Giới hạn cần phân biệt

Chưa có kết nối email tự động, OCR thực cho scan/PDF, AI tóm tắt, camera QR hay mapping QLVB đã khảo sát tại đơn vị. Nhập và đối chiếu thủ công là luồng hiện dùng cho các phần này. RPA thật dùng Windows UI Automation, chỉ điền bốn trường và dừng trước Lưu. Nếu ứng dụng không hỗ trợ các ô nhập theo mapping, cần khảo sát adapter tương thích. Phiếu Word là mẫu tham khảo, phải đối chiếu mẫu chính thức trước sử dụng.

SHA256 không thay chữ ký số, mã hóa hoặc kiểm tra mã độc. SACD không quyết định độ mật, giải mật hoặc phê duyệt thay người có thẩm quyền. AI hiện vẫn tắt. Docker chỉ chạy CLI, không hiển thị WPF.
