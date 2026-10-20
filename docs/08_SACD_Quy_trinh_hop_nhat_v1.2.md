> **Bổ sung 20/10:** dùng .NET 10; PDF text đọc thật theo trang, DOCX có bảng/header/footer; scan chưa OCR không được xác nhận toàn văn. Sau Approved cần đối soát chỉ đạo đã cập nhật QLVB trước Deliver. Xem [bản sửa](09_SACD_Ban_sua_va_chay_thu_2026-10-20.md). Những mô tả PDF fallback bên dưới là lịch sử đã được thay thế.

# SACD — Quy trình văn thư hai máy và hỗ trợ RPA

Phiên bản: 1.2 · Ngày: 16/10/2026

## 1. Phương án đã thống nhất

Kết hợp **ứng dụng Trợ lý văn thư** với **RPA hỗ trợ phần mềm quản lý văn bản (QLVB) hiện có**. SACD lưu hồ sơ trung gian, thông tin văn bản, tiến độ và lịch sử. QLVB vẫn là phần mềm bên ngoài; mã/số đến trong QLVB được cán bộ đối soát và ghi vào hồ sơ SACD. Hai máy trao đổi bằng gói tệp qua USB, không đồng bộ mạng.

Máy 1 dùng kho `Intake`. Máy 2 dùng kho `Management`. Mỗi kho có một phiên ghi. Khi thử trên cùng máy tính, tạo hai thư mục kho riêng và mở lần lượt. Vai trò của kho đã tạo được giữ nguyên khi mở lại.

## 2. Đối chiếu với 12 bước thực tế

| Bước thực tế | Thao tác trong SACD / bên ngoài | Điều kiện và kết quả |
|---|---|---|
| 1. Kiểm tra email nội bộ | Cán bộ mở email bằng ứng dụng nội bộ | Chưa có bộ kết nối email tự động. Không có email thì không tạo hồ sơ |
| 2. Đọc, phân loại, tải văn bản | Tải tệp về thư mục cán bộ chọn; tại tab **Quy trình văn thư**, chọn **M1 · Tiếp nhận tệp thật** | Chọn TXT/DOCX/PDF, lưu bản gốc theo SHA256; hồ sơ `Received` |
| 2. Kiểm tra thông tin | Mở tệp gốc, đối chiếu nội dung, điền loại/số/ngày/đơn vị ban hành/nhận/người ký/trích yếu/công việc liên quan/độ mật/căn cứ giải mật/hạn/đề xuất | Bấm **Lưu thông tin** trước. Ngày `yyyy-MM-dd`; hạn chưa rõ để trống, không tự đặt hạn |
| 2. Xác nhận | Nhập ghi chú xác nhận đã đối chiếu; bấm **M1 · Xác nhận đã đối chiếu** | Chỉ xác nhận đủ trường bắt buộc; dùng đúng revision hiện tại; chuyển `Reviewed` |
| 3. In tài liệu | **Mở tệp gốc / In** mở bản sao trong ứng dụng đọc. Cán bộ in và kiểm tra giấy | Nhập căn cứ, bấm **M1 · Xác nhận đã in giấy**; chuyển `Printed`. Mở tệp không có nghĩa đã in |
| 4–5. USB và chuyển Máy 2 | **M1 · Xuất gói USB**, chọn đường dẫn ZIP trên phương tiện đã được phép | Gói chứa tệp gốc, metadata/nội dung đã đối chiếu và checksum. Cán bộ chuyển USB vật lý và tài liệu giấy |
| 6. Mở QLVB, nhận hồ sơ | Trên kho `Management`, kiểm tra gói bằng công cụ của đơn vị, tích xác nhận kiểm tra mã độc, bấm **M2 · Nhập gói USB** | Kiểm tra SHA256; tạo hồ sơ `Imported`, giữ mã hồ sơ và thông tin Máy 1, ánh xạ ID nội bộ mới |
| 6.1.1. Kiểm tra hạn | Danh sách có cột hạn và thông tin cần chú ý | Tính lúc mở/tải lại và mỗi phút. Cảnh báo sắp hạn trong 3 ngày; không có dòng cảnh báo quá hạn khi số quá hạn bằng 0. Không phát popup định kỳ |
| 6.1.2. Nhập QLVB | Nhập thủ công hoặc **RPA · Điền QLVB theo cấu hình** | RPA thật điền số, trích yếu, đơn vị ban hành, hạn; đọc lại giá trị rồi dừng trước Lưu |
| 6.2. Đề xuất người xử lý | Điền đề xuất lãnh đạo/chỉ huy/đội, bấm Lưu thông tin khi hồ sơ còn `Imported` | Đề xuất của cán bộ được giữ riêng với người nhận theo quyết định lãnh đạo |
| 6.3. Lưu và in phiếu | Cán bộ kiểm tra và bấm Lưu ở QLVB, nhập mã/số đến vào SACD; nhập ghi chú rồi **Đối soát đã lưu QLVB** | `Registered`. Sau đó **Tạo phiếu Word + QR để in**, kiểm tra mẫu và in trong Word |
| 7. Trình lãnh đạo | Ghép phiếu với tài liệu giấy; ghi chú và **Xác nhận đã trình phiếu + tài liệu** | `Submitted`. SACD ghi nhận cán bộ đã trình, không ký thay lãnh đạo |
| 8–9. Nhận lại phiếu đã ký | Tra mã/QR; nhập người duyệt, người/đội thực tế được giao, hạn theo chỉ đạo, căn cứ phiếu ký | Nhập ghi chú, **Cập nhật đã duyệt** → `Approved`. Không chấp nhận duyệt trước khi trình |
| 10. Giao giấy | Chuyển đúng người/đội được giao, ghi chú, **Đã giao bản giấy** | `Delivered`, chưa coi là đã ký nhận |
| 11–12. Ký nhận và cập nhật | Người nhận ký thực tế. Nhập căn cứ sổ/bản ký, ghi chú, **Xác nhận ký nhận** | `Acknowledged`. Không coi ký nhận là hoàn thành công việc |
| Theo dõi đến hoàn thành | Tra cứu, kiểm tra hạn; có kết quả thì ghi căn cứ và **Xác nhận hoàn thành** | `Completed`; lưu thời điểm để phân biệt hoàn thành đúng hạn/quá hạn |

## 3. Cách thử nhanh trên một máy

1. Đóng cửa sổ SACD cũ. Tại PowerShell ở thư mục dự án, chạy `dotnet run --project src/SACD.Desktop`.
2. Tab **1. Chọn máy / Mở kho**: chọn `Intake`, đường dẫn ví dụ `D:\SACD_Test\May1`; bấm mở kho.
3. Tab **2. Quy trình văn thư**: tiếp nhận tệp dữ liệu giả trong `docs/examples/VanBan_ThuNghiem.txt`. Điền các thông tin theo tệp, bấm Lưu. Nhập ghi chú cho từng lần xác nhận.
4. Mở tệp, đối chiếu, xác nhận; in thử nếu có máy in, chỉ xác nhận đã in khi có bản giấy; xuất ZIP.
5. Đóng ứng dụng hoặc đổi kho tại tab 1 sang `D:\SACD_Test\May2`, chọn `Management` khi tạo kho mới.
6. Sau bước kiểm tra gói theo quy trình đơn vị, nhập ZIP. Thông tin phải còn nguyên. Nhập cùng gói lần hai phải trở về hồ sơ đã nhập, không tạo bản sao.
7. Nếu chưa có QLVB thử nghiệm, dừng ở bước `Imported`. Chỉ thử bước Đối soát với một hồ sơ thực sự đã lưu trong QLVB thử nghiệm, không tạo bằng chứng giả cho hệ thống thật.
8. Thử tạo/in phiếu, trình, cập nhật duyệt, giao, ký nhận và hoàn thành theo bảng trên. Mỗi bước cần căn cứ riêng.

Đầu đọc QR dạng bàn phím: đặt con trỏ vào ô tra mã, quét QR rồi bấm **Tra cứu / Tải lại danh sách**. Có thể nhập mã bằng tay. QR chỉ chứa mã hồ sơ; không chứa nội dung văn bản và không mở website. Phiếu hiện là mẫu tham khảo, chưa phải mẫu chính thức của đơn vị.

## 4. Cấu hình RPA QLVB thực tế

Mẫu: [qlvb.mapping.example.json](examples/qlvb.mapping.example.json).

1. Khảo sát tên chương trình, tiêu đề cửa sổ và `AutomationId` của 4 ô nhập trên biểu mẫu QLVB thử nghiệm.
2. Tạo bản sao mapping, điền đúng đường dẫn EXE, tiêu đề cửa sổ và các ID; mở sẵn biểu mẫu QLVB cần nhập.
3. Chọn hồ sơ `Imported`, bấm RPA và chọn mapping. Adapter kiểm tra đúng tiến trình, đúng một cửa sổ, mỗi ID đúng một trường cho phép ghi trước khi điền.
4. RPA dùng Windows UI Automation, không dò tọa độ và không tự bấm Lưu. Sau khi điền, cán bộ kiểm tra, bấm Lưu ở QLVB, lấy mã/số đến và đối soát trong SACD.
5. Nếu lỗi giữa chừng, biểu mẫu có thể đã được điền một phần. Xem lại trước khi chạy tiếp; không có retry tự động. Nhật ký ghi sự kiện cần đối soát.

Adapter hiện cần QLVB hỗ trợ `ValuePattern` cho ô nhập. Chưa có mapping được kiểm chứng cho phần mềm của đơn vị vì chưa có thông tin ứng dụng/biểu mẫu. Bản web, ô chọn ngày, combobox hoặc trường có hành vi riêng có thể cần adapter khác. Tab **RPA mô phỏng** chỉ thử trạng thái, không nhập dữ liệu ra ngoài.

## 5. Dữ liệu và kiểm soát thay đổi

- Thêm 3 bảng tương thích kho cũ: `CorrespondenceCase` (thông tin hồ sơ và rowVersion), `CorrespondenceCaseEvent` (lịch sử và căn cứ), `CorrespondenceCaseImport` (nguồn và chống nhập lặp). Không xóa bảng hoặc bản gốc cũ. `schemaVersion` hiện vẫn là 1; đây là mở rộng additive, chưa phải cơ chế migration tổng quát.
- Dossier/Document/FileObject/TextRevision/TextBlock/FTS5 tiếp tục lưu định danh hồ sơ, tệp và nội dung. Dòng công việc hợp nhất và hạn của nó được theo dõi tại tab Quy trình văn thư; tab Nhiệm vụ bổ sung là chức năng riêng.
- Mỗi lần thay đổi kiểm tra rowVersion, ghi trạng thái và audit trong cùng giao dịch. Thông tin được khóa sau xác nhận/in/trình; chưa có luồng sửa sai sau xác nhận, trả lại hoặc gia hạn sau duyệt. Không sửa trực tiếp DB để bỏ qua bước.
- Gói USB v1 của luồng mới có `metadata.json`, `original`, `checksum.sha256`; khác gói transfer kỹ thuật cũ. Metadata gồm thông tin và các đoạn đã đối chiếu; không cần OCR lại ở Máy 2. Giới hạn 64 MiB mỗi entry, đúng 3 entry; không giải nén theo đường dẫn nhận từ gói.
- Cùng nguồn/cùng hồ sơ/cùng nội dung: trả hồ sơ đã nhập. Nội dung hoặc phiên bản nguồn khác: báo `TransferConflict` để đối soát, không ghi đè tiến trình Máy 2.
- SHA256 kiểm tra toàn vẹn, không phải chữ ký số, mã hóa hay xác nhận sạch mã độc. Tích “đã kiểm tra mã độc” là xác nhận của cán bộ, SACD chưa tích hợp máy quét. Cần xác nhận tính hợp lệ của quy trình USB và việc chuyển tài liệu theo quy định thực tế trước vận hành.
- Nội dung trong các kho/demo cũ vẫn còn; danh sách hồ sơ hợp nhất hiển thị các hồ sơ tiếp nhận bằng luồng mới, chưa tự chuyển dữ liệu demo cũ sang hồ sơ nghiệp vụ.

## 6. Phần đã hiện thực và phần còn phụ thuộc

Đã hiện thực: chọn tệp thật; TXT/DOCX; lưu biểu mẫu; kiểm soát thứ tự; ghi căn cứ; in qua ứng dụng đọc; phiếu DOCX có QR; gói USB có metadata và checksum; chống nhập lặp/xung đột; lịch sử; tra mã; tính hạn; RPA Windows UIA có mapping và dừng trước Lưu.

Chưa hiện thực tự động: kết nối email nội bộ, tải/đặt tên tệp từ email, nhận dạng OCR scan/PDF đúng bố cục, trích xuất/tóm tắt bằng AI, tự phân loại độ mật/giải mật, khảo sát và đóng gói mapping QLVB của đơn vị, tự gửi máy in và kiểm chứng spool, camera quét QR, chữ ký điện tử. PDF hiện dùng fallback thử nghiệm; phải mở tệp gốc và nhập/đối chiếu thủ công. AI vẫn tắt, không gửi tài liệu sang dịch vụ AI công cộng.

Luồng này chưa đủ để tuyên bố nghiệm thu toàn bộ SRS 117 FR/24 NFR. Kiểm thử bằng dữ liệu giả không chứng minh tương thích email/QLVB/máy in hoặc sẵn sàng xử lý dữ liệu nghiệp vụ thật.

## 7. Kiểm tra bản sửa

```powershell
dotnet build SACD.sln
dotnet test tests/SACD.Tests/SACD.Tests.csproj
dotnet run --project tests/SACD.Desktop.Smoke -- D:\Project\SACD\TestResults\workflow-ui.png
```

Bộ test mới kiểm tra: luồng hai máy đến hoàn thành; bắt buộc duyệt/giao/ký nhận đúng thứ tự; metadata/nội dung qua USB; gói chưa kiểm tra/tamper/nhập lặp; rowVersion; đúng vai trò máy; khóa metadata đã xác nhận; revision và block đúng văn bản; DOCX lưu bằng hash vẫn trích xuất đúng; tăng phiên bản script; phiếu Word/QR hợp lệ; báo cáo nháp và snapshot; hoàn thành đúng/quá hạn. Smoke test WPF chạy tiếp nhận/duyệt, RPA mô phỏng lặp lại, dựng giao diện và thử UIA thật trên biểu mẫu giả lập; không thao tác QLVB của đơn vị.
