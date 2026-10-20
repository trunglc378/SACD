# SACD — bản sửa và chạy thử ngày 20/10/2026

Phạm vi: sửa các lỗi đã xác minh trong luồng cục bộ, cập nhật tài liệu và kiểm chứng trên Windows. Đây là đợt hiện thực nền tảng đầu tiên sau [đánh giá commit 293323c](analysis/2026-10-20-review-and-proposal.md), chưa hoàn thành toàn bộ roadmap tự động hóa.

## 1. Những phần đã sửa

| Nhóm | Hành vi mới |
|---|---|
| Nền tảng | Tất cả project chuyển sang .NET 10; SDK cục bộ 10.0.402, runtime 10.0.13; thêm global.json và implicit usings thống nhất |
| PDF/DOCX | PdfPig 0.1.16 đọc PDF theo trang; trang không có text mang needsOcr và revision Partial; không xác nhận toàn văn khi còn trang cần OCR. DOCX lấy thêm bảng/header/footer |
| Metadata | Gợi ý từ các trường có nhãn cho số/ngày/đơn vị/người ký/trích yếu; chỉ điền trường còn trống, chưa tự Confirm; không đặt hạn/độ mật |
| Nguồn và tìm kiếm | Không sửa text của revision đã được EvidenceRef/Fact/ReportSource tham chiếu; tìm kiếm mặc định chỉ currentRevision; Get case dùng truy vấn theo ID |
| Số liệu | Decimal invariant kiểm tra round-trip để tránh làm tròn âm thầm. Stock chọn theo thời điểm, không theo thứ tự request. Ratio cần tập tử/mẫu rõ, giữ decimal; mẫu 0 trả NeedsReview. Kiểm tra chỉ tiêu/kỳ/scope/ID lặp/khoảng chồng và nguồn hiện hành Reviewed có evidence; lưu contribution/hash snapshot thật |
| Lũy kế | Quan sát cuối trong cùng phạm vi/mốc bắt đầu, không giảm; trường hợp khác dừng đối soát thay vì cộng chồng |
| Báo cáo | Service chặn Reviewed/Submission khi chưa kiểm tra claim, thiếu/sai evidence, source cũ hoặc sai expected version. Lưu sourceHash thật và ReportAggregation. Mẫu tùy chỉnh đăng ký được object/FK nhưng export trả TemplateUnsupported nếu chưa có renderer phù hợp |
| Phục hồi | Chỉ đích mới; staging; giới hạn ZIP 10.000 entry/1 GiB bytes thực; whitelist đường dẫn; kiểm tra SID/schema, SQLite/FK và hash/size tất cả FileObject rồi mới publish kho |
| Quyền/kết nối DB | Mở kho đối chiếu SID trước khởi tạo schema; không tự chuyển chủ. Tắt connection pooling để giải phóng handle SQLite khi đóng, giúp cleanup/restore trên Windows |
| Trạng thái nghiệp vụ | Approved khác với DecisionExternalStatus. Cán bộ cập nhật QLVB, ghi căn cứ rồi đối soát chỉ đạo; chỉ sau Verified mới Deliver. Import máy 2 không tin các trường chỉ đạo/mã đích do gói máy 1 cung cấp |
| Dịch vụ chưa tích hợp | In, QLVB, OCR, recorder thật và điều khiển worker chưa triển khai trả NotConfigured; không tạo mã/thành công giả. Giữ RPA mô phỏng có nhãn riêng |
| Thống kê | Đếm hồ sơ/nhiệm vụ trong DB theo kỳ ngày Việt Nam, thay danh sách UI của phiên. Nhiệm vụ mới không tự gán hạn +3 ngày |
| CLI/Docker | Diagnostic SQLite read-only, không tranh writer lock; Dockerfile chuyển .NET 10. Chỉ CLI trên Windows được thử, chưa chạy Docker |
| Tài liệu | Cập nhật README/chỉ mục/cài đặt; ghi rõ báo cáo cũ là lịch sử; NFR-01 cho phép website nội bộ đúng mạng máy đang dùng, cấm Internet/cầu nối hai mạng |

Schema vẫn là 1 và 53 bảng nghiệp vụ; hai trường đối soát mới nằm trong payload case, có giá trị mặc định cho JSON cũ. Đây không phải migration tổng quát. Kho Approved cũ chưa có căn cứ đối soát phải thực hiện bước mới trước Deliver. Không sửa DB để vượt bước.

## 2. Kết quả kiểm chứng

Ngày chạy: 20/10/2026, Windows x64 build 26200, SDK 10.0.402. Không dùng dữ liệu nghiệp vụ thật.

| Kiểm tra | Kết quả/bằng chứng |
|---|---|
| Build solution | Thành công, 0 warning / 0 error ở lần build kiểm tra |
| xUnit | **41/41 đạt**, không bỏ qua; [TRX mới](../TestResults/2026-10-20-fixed.trx) |
| WPF/UIA smoke | Đạt: mở kho, tiếp nhận, kiểm tra revision, RPA mô phỏng lặp và UIA thật trên form giả; đọc lại giá trị, selector sai bị chặn, không bấm Lưu; [ảnh render](../TestResults/2026-10-20-workflow-ui.png) |
| Luồng hai máy ở service | Test chuyển ZIP/metadata, đăng ký tham chiếu thử, trình/duyệt; chặn Deliver khi chưa đối soát chỉ đạo; ghi đối soát rồi giao/ký nhận/hoàn thành |
| CLI read-only diagnostic | quick_check=ok khi daemon cùng kho vẫn giữ writer lock; daemon thử đã dừng |
| Publish Windows | Bản Release win-x64 self-contained tại dist/SACD; đã khởi chạy exe và mở kho thử riêng trên giao diện |
| Tích hợp bên ngoài | Chưa thử website nội bộ, giấy in thật, USB vật lý, OCR engine hoặc AI; không suy từ test giả lập thành nghiệm thu các phần này |

TRX 16/10 vẫn giữ nguyên như lịch sử. Script trong docs/analysis tái hiện **commit nền** bằng git show, không phải regression test code hiện tại. Code hiện tại kiểm bằng xUnit. Dữ liệu lần chạy giao diện ở `.runs/interactive-2026-10-20/May1`, tệp giả `CongVan_Thu.txt`; không phải kho nghiệp vụ.

## 3. Chạy thử từ mã nguồn

Với SDK cục bộ của phiên làm việc:

```powershell
$env:DOTNET_ROOT = "$PWD/.tools/dotnet"
$env:DOTNET_CLI_TELEMETRY_OPTOUT = '1'
& ./.tools/dotnet/dotnet.exe build SACD.sln
& ./.tools/dotnet/dotnet.exe test tests/SACD.Tests/SACD.Tests.csproj
& ./.tools/dotnet/dotnet.exe run --project tests/SACD.Desktop.Smoke -- TestResults/workflow-ui-current.png
& ./.tools/dotnet/dotnet.exe run --project src/SACD.Desktop
```

Nếu đã có .NET 10 SDK trong PATH thì dùng `dotnet` thông thường. Máy nghiệp vụ không chạy restore/download Internet; chuyển toàn bộ bản publish đã chuẩn bị và kiểm tra vào môi trường được phép.

## 4. Chạy bản đóng gói

Mở `dist/SACD/SACD.Desktop.exe`. Chuyển toàn bộ thư mục, không chỉ exe; bản này chứa runtime nên không cần cài SDK. Đường dẫn bản hiện tại: `D:\Project\SACD\dist\SACD\SACD.Desktop.exe`.

1. Chọn **Máy 1 — Tiếp nhận** và kho thử Máy 1 riêng; tiếp nhận TXT/DOCX/PDF có chữ, kiểm tra gợi ý, lưu thông tin và đối chiếu bản gốc. Trang quét phải dừng chờ nhận dạng chữ.
2. In/kiểm tra giấy thật nếu có máy in; chỉ xác nhận đã in khi đã kiểm tra bản giấy. Xuất USB; dùng kho **Máy 2 — Quản lý** riêng trên Máy 2 để nhận.
3. Nhập QLVB bên ngoài, kiểm tra mã/số đến rồi Register. UIA chỉ dùng mapping đã khảo sát; chưa có mapping của đơn vị trong bản này.
4. Tạo phiếu Word tham khảo; kiểm tra mẫu, in/trình. Cập nhật người duyệt/người được giao/hạn theo phiếu ký.
5. Cập nhật chỉ đạo trên QLVB bên ngoài, nhập căn cứ và bấm Đối soát chỉ đạo đã cập nhật QLVB; sau đó giao/ký nhận/hoàn thành.

Khi thử không có hệ thống ngoài/máy in, dừng tại ranh giới tương ứng; không ghi bằng chứng giả vào kho nghiệp vụ.

## 5. Phần chưa làm xong và đầu vào cần thiết

- Connector hai website, chọn danh mục/ngày/đính kèm, đọc mã đích và đối soát tự động: cần cấu trúc giao diện/API và môi trường thử của từng mạng.
- OCR thật cho scan, renderer/checkpoint/worker và AI local: chưa tích hợp; không hứa tốc độ trên i3/8 GB từ máy phát triển.
- Nhiều đính kèm trong case, nguồn tiếp nhận lặp riêng, phân trang/tổng hợp cảnh báo tối ưu, hợp nhất hai mô hình case/task và chuẩn gói USB tiếp theo: còn backlog. Không tự đổi định dạng gói đã dùng.
- Trả lại/sửa sai/gia hạn/điều chỉnh quyết định, mẫu phiếu chính thức và Word template renderer: còn cần phát triển/đầu vào đơn vị.
- Chính sách ACL theo nhóm/chuyển chủ máy/SID, migration schema và rollback tổng quát: chưa có công cụ đầy đủ. Kiểm tra SID hiện tại không thay việc quản trị NTFS.
- Khảo sát và UAT trên đúng OS/trình duyệt/máy in/USB, kiểm tra traffic không Internet, tải giờ cao điểm và diễn tập phục hồi: còn phải thực hiện tại môi trường đích.

Các phần thiếu được ghi đúng trạng thái thay vì báo đã hoàn thành. Hướng phát triển tiếp theo là OCR và connector website sau khi có môi trường khảo sát, giữ lõi độc lập với AI.

## 6. Cập nhật giao diện tiếng Việt

- Việt hóa tiêu đề, các mục chức năng, vai trò máy, hướng dẫn, thông báo, trạng thái nhiệm vụ, đối soát và lịch sử thao tác. Mã nghiệp vụ và dữ liệu lưu trong kho được giữ nguyên; tên tệp và nội dung văn bản của người dùng không dịch tự động.
- Vị trí trích xuất hiển thị thành Trang/Dòng/Đoạn; giá trị đúng/sai thành Có/Không. Lịch sử hiển thị thời điểm theo giờ Việt Nam, định dạng ngày/tháng/năm giờ:phút.
- Hộp chọn/lưu tệp và trình đơn sao chép/dán của SACD dùng tiếng Việt, độc lập ngôn ngữ Windows. Hộp lưu yêu cầu tên mới khi tệp đã tồn tại. Phần mềm đọc tệp, trình duyệt, hộp in và thanh tiêu đề do Windows quản lý vẫn theo ngôn ngữ của các ứng dụng đó.
- Kiểm tra: 41/41 kiểm thử nghiệp vụ đạt; kiểm tra giao diện thực tế xác nhận vai trò hiển thị tiếng Việt nhưng vẫn lưu đúng mã máy, lịch sử và trạng thái được dịch, hộp chọn/lưu tệp hoạt động, chặn sai loại tệp và ghi đè. Biên dịch không có cảnh báo hoặc lỗi.
- Bằng chứng mới: [kết quả kiểm thử](../TestResults/2026-10-20-vietnamese.trx), [ảnh giao diện](../TestResults/2026-10-20-vietnamese-ui.png). Bản chạy đã cập nhật tại `dist/SACD`.
