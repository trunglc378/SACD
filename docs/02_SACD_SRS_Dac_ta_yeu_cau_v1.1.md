# SACD SRS đặc tả yêu cầu phần mềm

**Mã tài liệu:** SACD-DOC-02  
**Phiên bản:** 1.1  
**Ngày:** 15/10/2026  
**Người biên soạn:** Đàm Quang Trung  
**Trạng thái:** Dự thảo chi tiết để rà soát; chưa phê duyệt baseline triển khai.

**Căn cứ:** Phiếu đăng ký và Report 1 SACD v1.0 trong `references/`. Các đặc tả bổ sung ở bản 1.1 là thiết kế đề xuất để hiện thực phạm vi đã nêu, không phải bằng chứng phần mềm đã có. AI mặc định tắt. Các mục DEC còn mở không được coi là quyết định đã được đơn vị chấp thuận.

## 1 Mục đích nguồn và quy ước

SRS chuyển 21 nhóm FE của Report 1 thành các yêu cầu kiểm tra được. Mã **FR-001–FR-117** thuộc baseline tài liệu 1.1, không trộn với FR-01–FR-20 của bản SRS cũ. Ghi rõ phạm vi C (lõi mục tiêu) hoặc E (AI mở rộng). C là mục tiêu cần phát triển/kiểm thử, không tự đồng nghĩa đã có trong prototype. Nhánh E chưa được kích hoạt nếu DEC-09 chưa chốt.

Nguồn v1.0, giới hạn transcript ở DEC-01, các giả định triển khai và đề xuất đo hiệu năng phải được giữ khi lập kế hoạch. Nếu thay đổi yêu cầu, tạo Change Request và cập nhật RTM; không xóa mã cũ để che thay đổi.

## 2 Bối cảnh tác nhân và biên hệ thống

| Mã | Tác nhân | Quyền và trách nhiệm |
|---|---|---|
| ACT-01 | Cán bộ tiếp nhận/văn thư/tổng hợp | Thao tác dữ liệu được phép; kiểm tra nguồn, hạn, đề xuất và kết quả |
| ACT-02 | Người quản lý máy/kho | Cấp ACL, kiểm tra công cụ, cấu hình và tổ chức phục hồi |
| ACT-03 | Lãnh đạo/trưởng phòng | Quyết định nghiệp vụ ngoài SACD; không tự là tài khoản mạng |
| ACT-04 | Người nhận/đội xử lý | Giao nhận/ký nhận/thực hiện ngoài SACD; cán bộ ghi nhận căn cứ |
| EXT-01 | Email nội bộ/QLVB/máy in | Hệ thống đích chỉ giao tiếp bằng phương thức được phép |
| EXT-02 | Công cụ OCR/AI | Tiến trình phụ trợ không có thẩm quyền xác nhận nghiệp vụ |

Máy A nhận thư trong môi trường được phép; máy B quản lý/tổng hợp; kho riêng. SACD không có backend web công cộng, không tự kết nối A/B, không dùng cloud. Việc chuyển USB và kiểm tra phương tiện vẫn do con người thực hiện. Tài khoản Windows và ACL quyết định quyền tệp; không giả một cờ UI là ranh giới bảo mật.

## 3 Phạm vi và ưu tiên

| FE | Nhóm chức năng | Phạm vi |
|---|---|---|
| FE-01 | Không gian làm việc và quyền truy cập | Cốt lõi |
| FE-02 | Tiếp nhận văn bản từ email nội bộ | Cốt lõi có điều kiện tích hợp |
| FE-03 | Nhập tài liệu và bảo toàn bản gốc | Cốt lõi |
| FE-04 | Đọc văn bản và OCR bản scan | Cốt lõi có điều kiện công cụ |
| FE-05 | Trích xuất và xác nhận thông tin văn bản | Cốt lõi |
| FE-06 | Phân loại nghiệp vụ và theo dõi thông tin mật | Cốt lõi |
| FE-07 | In văn bản và phiếu xử lý | Cốt lõi |
| FE-08 | Đóng gói và chuyển nhận qua USB được phép | Cốt lõi |
| FE-09 | Theo dõi công việc và cảnh báo thời hạn | Cốt lõi |
| FE-10 | Hỗ trợ nhập vào phần mềm quản lý văn bản hiện có | Cốt lõi có điều kiện tương thích |
| FE-11 | Lập đề xuất lãnh đạo và đội xử lý | Cốt lõi |
| FE-12 | Ghi nhận trình duyệt và kết quả chỉ đạo | Cốt lõi |
| FE-13 | Giao văn bản và theo dõi ký nhận | Cốt lõi |
| FE-14 | Tra cứu và quản lý lịch sử tài liệu | Cốt lõi |
| FE-15 | Quản lý số liệu và tổng hợp có kiểm chứng | Cốt lõi |
| FE-16 | Mẫu báo cáo và xuất tài liệu tổng hợp | Cốt lõi |
| FE-17 | Ghi và biên tập thao tác chuột bàn phím | Cốt lõi |
| FE-18 | Phát lại kịch bản và điểm người dùng can thiệp | Cốt lõi có điều kiện tương thích |
| FE-19 | Cấu hình nhật ký và sao lưu phục hồi | Cốt lõi |
| FE-20 | Điểm tích hợp AI cục bộ tùy chọn | Mở rộng chưa bắt buộc |
| FE-21 | Danh mục tổ chức người xử lý và quy tắc ánh xạ | Cốt lõi |

## 4 Yêu cầu chức năng

Mỗi yêu cầu có kiểm tra chấp nhận, dữ liệu, thành phần và ca kiểm thử. Luồng lỗi phải hiển thị code/correlationId và chỉ rõ dữ liệu nào đã commit; không chỉ hiện “thử lại” với tác dụng phụ chưa rõ. Các số test trong RTM là thiết kế test, chưa là kết quả.

### FR-001 Mở kho và kiểm tra quyền — luồng chính

- **Nguồn và mục tiêu:** FE-01 / UC-001; pha C.
- **Yêu cầu:** SACD phải hỗ trợ mở kho và kiểm tra quyền với đầu vào Đường dẫn kho, SID hệ điều hành. Các bước bắt buộc: Chuẩn hóa đường dẫn và từ chối UNC/reparse không được phép; Kiểm tra ACL thực tế và chủ kho; Lấy khóa một phiên ghi; Kiểm tra schema, integrity và trạng thái job cũ; Hiển thị profile máy và kho đang mở.
- **Tiền điều kiện:** Thư mục cục bộ được cấp quyền; schema được hỗ trợ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Kho đúng SID, một phiên ghi; không tự tạo kết nối mạng
- **Dữ liệu kiểm tra cụ thể:** Mở kho A ở hai tiến trình; tiến trình thứ hai bị chặn.
- **Ràng buộc:** BR-01, BR-02; kiểm soát CMP-01, dữ liệu Workspace, AuditEvent, Job.
- **Kiểm chứng dự kiến:** TC-F-001; trạng thái Chưa thực hiện.

### FR-002 Mở kho và kiểm tra quyền — nhánh thay thế

- **Nguồn và mục tiêu:** FE-01 / UC-001; pha C.
- **Yêu cầu:** Kho mới: khởi tạo schema trong staging rồi đổi tên sau kiểm tra
- **Tiền điều kiện:** Thư mục cục bộ được cấp quyền; schema được hỗ trợ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Kho mới: khởi tạo schema trong staging rồi đổi tên sau kiểm tra.
- **Ràng buộc:** BR-01, BR-02; kiểm soát CMP-01, dữ liệu Workspace, AuditEvent, Job.
- **Kiểm chứng dự kiến:** TC-F-002; trạng thái Chưa thực hiện.

### FR-003 Mở kho và kiểm tra quyền — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-01 / UC-001; pha C.
- **Yêu cầu:** Kho bị phiên khác khóa hoặc SID khác: WorkspaceBusy/AccessDenied; không tự đổi quyền
- **Tiền điều kiện:** Thư mục cục bộ được cấp quyền; schema được hỗ trợ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Kho bị phiên khác khóa hoặc SID khác: WorkspaceBusy/AccessDenied; không tự đổi quyền.
- **Ràng buộc:** BR-01, BR-02; kiểm soát CMP-01, dữ liệu Workspace, AuditEvent, Job.
- **Kiểm chứng dự kiến:** TC-F-003; trạng thái Chưa thực hiện.

### FR-004 Tiếp nhận thư nội bộ đã chọn — luồng chính

- **Nguồn và mục tiêu:** FE-02 / UC-002; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tiếp nhận thư nội bộ đã chọn với đầu vào Thư được chọn, đính kèm, nguồn thư không chứa mật khẩu. Các bước bắt buộc: Xác minh đúng ứng dụng thư; Người dùng chọn thư và đích lưu; Lưu đính kèm vào staging được cấp quyền; Quét/kiểm tra theo quy trình của máy; Tạo hồ sơ tiếp nhận và liên kết nguồn thư; Đối chiếu thư trùng bằng định danh và hash.
- **Tiền điều kiện:** Máy A; tài khoản thư do người dùng đăng nhập; profile đích được khảo sát.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có hồ sơ nguồn và tệp được chọn; chưa tự xác nhận metadata
- **Dữ liệu kiểm tra cụ thể:** Thư mẫu MAIL-01 có 2 đính kèm; chỉ nhập tệp người dùng chọn.
- **Ràng buộc:** BR-03, BR-04; kiểm soát CMP-02, dữ liệu Dossier, Document, ExternalEntryAttempt, AuditEvent, FileObject.
- **Kiểm chứng dự kiến:** TC-F-004; trạng thái Chưa thực hiện.

### FR-005 Tiếp nhận thư nội bộ đã chọn — nhánh thay thế

- **Nguồn và mục tiêu:** FE-02 / UC-002; pha C.
- **Yêu cầu:** Nếu không tương thích UIA: người dùng lưu tệp rồi nhập thủ công
- **Tiền điều kiện:** Máy A; tài khoản thư do người dùng đăng nhập; profile đích được khảo sát.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Nếu không tương thích UIA: người dùng lưu tệp rồi nhập thủ công.
- **Ràng buộc:** BR-03, BR-04; kiểm soát CMP-02, dữ liệu Dossier, Document, ExternalEntryAttempt, AuditEvent, FileObject.
- **Kiểm chứng dự kiến:** TC-F-005; trạng thái Chưa thực hiện.

### FR-006 Tiếp nhận thư nội bộ đã chọn — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-02 / UC-002; pha C.
- **Yêu cầu:** Hết phiên, sai cửa sổ hoặc đính kèm chưa tải đủ: PauseForUser/NeedsReview; không gửi thư
- **Tiền điều kiện:** Máy A; tài khoản thư do người dùng đăng nhập; profile đích được khảo sát.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Hết phiên, sai cửa sổ hoặc đính kèm chưa tải đủ: PauseForUser/NeedsReview; không gửi thư.
- **Ràng buộc:** BR-03, BR-04; kiểm soát CMP-02, dữ liệu Dossier, Document, ExternalEntryAttempt, AuditEvent, FileObject.
- **Kiểm chứng dự kiến:** TC-F-006; trạng thái Chưa thực hiện.

### FR-007 Nhập tệp và giữ bản gốc — luồng chính

- **Nguồn và mục tiêu:** FE-03 / UC-003; pha C.
- **Yêu cầu:** SACD phải hỗ trợ nhập tệp và giữ bản gốc với đầu vào Tệp nguồn, nhãn nguồn, mã hồ sơ nếu đã có. Các bước bắt buộc: Kiểm tra size và chữ ký loại thực; Kiểm tra cấu trúc ZIP/trang/pixel trước giải mã toàn bộ; Chép vào staging và tính SHA256 theo luồng; Xác định trùng tệp và trùng hồ sơ; Commit FileObject, Document và job; Hiển thị kết quả riêng từng tệp.
- **Tiền điều kiện:** Kho ghi được và đủ dung lượng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Bản gốc bất biến được tham chiếu; mỗi tệp có kết quả rõ
- **Dữ liệu kiểm tra cụ thể:** DOC-01 cùng bytes đổi tên vẫn cùng object_hash; không mất lịch sử tiếp nhận.
- **Ràng buộc:** BR-05, BR-06; kiểm soát CMP-02, dữ liệu FileObject, Dossier, Document, Job, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-007; trạng thái Chưa thực hiện.

### FR-008 Nhập tệp và giữ bản gốc — nhánh thay thế

- **Nguồn và mục tiêu:** FE-03 / UC-003; pha C.
- **Yêu cầu:** Cùng bytes ở lần tiếp nhận khác: dùng lại object nhưng giữ nguồn tiếp nhận mới
- **Tiền điều kiện:** Kho ghi được và đủ dung lượng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Cùng bytes ở lần tiếp nhận khác: dùng lại object nhưng giữ nguồn tiếp nhận mới.
- **Ràng buộc:** BR-05, BR-06; kiểm soát CMP-02, dữ liệu FileObject, Dossier, Document, Job, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-008; trạng thái Chưa thực hiện.

### FR-009 Nhập tệp và giữ bản gốc — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-03 / UC-003; pha C.
- **Yêu cầu:** Tệp sai loại/quá giới hạn/đứt chép: reject tệp, dọn staging; không ghi thành Imported
- **Tiền điều kiện:** Kho ghi được và đủ dung lượng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Tệp sai loại/quá giới hạn/đứt chép: reject tệp, dọn staging; không ghi thành Imported.
- **Ràng buộc:** BR-05, BR-06; kiểm soát CMP-02, dữ liệu FileObject, Dossier, Document, Job, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-009; trạng thái Chưa thực hiện.

### FR-010 Đọc văn bản điện tử — luồng chính

- **Nguồn và mục tiêu:** FE-04 / UC-004; pha C.
- **Yêu cầu:** SACD phải hỗ trợ đọc văn bản điện tử với đầu vào DocumentId, bộ đọc và phiên bản công cụ. Các bước bắt buộc: Xác minh hash; Đọc PDF từng trang hoặc DOCX đoạn/ô hoặc TXT dòng; Lưu extractedText nguyên và rawTextNfc; Ghi locator có loại và thứ tự; Tạo revision NeedsReview và chỉ mục hiện hành.
- **Tiền điều kiện:** Bản gốc nhập hợp lệ, có hash.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Text có nguồn và vị trí; chưa Reviewed
- **Dữ liệu kiểm tra cụ thể:** PDF-02 có trang 2 scan: trang 2 được đánh dấu chờ OCR, không bỏ mất.
- **Ràng buộc:** BR-06, BR-07; kiểm soát CMP-03, dữ liệu Document, TextRevision, TextBlock, Job, ToolVersion.
- **Kiểm chứng dự kiến:** TC-F-010; trạng thái Chưa thực hiện.

### FR-011 Đọc văn bản điện tử — nhánh thay thế

- **Nguồn và mục tiêu:** FE-04 / UC-004; pha C.
- **Yêu cầu:** PDF có trang không text: ghi danh sách trang NeedOcr để xử lý tiếp
- **Tiền điều kiện:** Bản gốc nhập hợp lệ, có hash.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** PDF có trang không text: ghi danh sách trang NeedOcr để xử lý tiếp.
- **Ràng buộc:** BR-06, BR-07; kiểm soát CMP-03, dữ liệu Document, TextRevision, TextBlock, Job, ToolVersion.
- **Kiểm chứng dự kiến:** TC-F-011; trạng thái Chưa thực hiện.

### FR-012 Đọc văn bản điện tử — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-04 / UC-004; pha C.
- **Yêu cầu:** Bộ đọc lỗi tại trang: job Partial/Failed; không coi toàn tài liệu hoàn thành
- **Tiền điều kiện:** Bản gốc nhập hợp lệ, có hash.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Bộ đọc lỗi tại trang: job Partial/Failed; không coi toàn tài liệu hoàn thành.
- **Ràng buộc:** BR-06, BR-07; kiểm soát CMP-03, dữ liệu Document, TextRevision, TextBlock, Job, ToolVersion.
- **Kiểm chứng dự kiến:** TC-F-012; trạng thái Chưa thực hiện.

### FR-013 OCR theo trang và hủy tác vụ — luồng chính

- **Nguồn và mục tiêu:** FE-04 / UC-005; pha C.
- **Yêu cầu:** SACD phải hỗ trợ ocr theo trang và hủy tác vụ với đầu vào Trang chọn, DPI và xoay, cấu hình OCR. Các bước bắt buộc: Dừng worker AI nếu đang dùng; Xếp một tác vụ nặng vào hàng đợi; Dựng một trang theo cấu hình; OCR vie+eng và lưu block/bbox/điểm engine; Commit checkpoint trang hoàn tất; Giải phóng ảnh trước trang tiếp; Kết thúc bằng NeedsReview hoặc Partial.
- **Tiền điều kiện:** Tesseract và renderer được khóa hash; nguồn qua kiểm tra giới hạn.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có kết quả và lỗi theo trang; hủy giữ checkpoint đúng
- **Dữ liệu kiểm tra cụ thể:** OCR-01 gồm 10 trang; hủy trang 4 giữ 3 trang hoàn tất và trạng thái chưa đủ.
- **Ràng buộc:** BR-07, BR-08; kiểm soát CMP-03, dữ liệu TextRevision, TextBlock, Job, ToolVersion, Document.
- **Kiểm chứng dự kiến:** TC-F-013; trạng thái Chưa thực hiện.

### FR-014 OCR theo trang và hủy tác vụ — nhánh thay thế

- **Nguồn và mục tiêu:** FE-04 / UC-005; pha C.
- **Yêu cầu:** Người dùng hủy: thu hồi worker rồi ghi Cancelled với lastCompletedPage
- **Tiền điều kiện:** Tesseract và renderer được khóa hash; nguồn qua kiểm tra giới hạn.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Người dùng hủy: thu hồi worker rồi ghi Cancelled với lastCompletedPage.
- **Ràng buộc:** BR-07, BR-08; kiểm soát CMP-03, dữ liệu TextRevision, TextBlock, Job, ToolVersion, Document.
- **Kiểm chứng dự kiến:** TC-F-014; trạng thái Chưa thực hiện.

### FR-015 OCR theo trang và hủy tác vụ — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-04 / UC-005; pha C.
- **Yêu cầu:** Thiếu tessdata, vượt pixel hoặc worker timeout: dừng, không tự giảm DPI/cắt trang
- **Tiền điều kiện:** Tesseract và renderer được khóa hash; nguồn qua kiểm tra giới hạn.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Thiếu tessdata, vượt pixel hoặc worker timeout: dừng, không tự giảm DPI/cắt trang.
- **Ràng buộc:** BR-07, BR-08; kiểm soát CMP-03, dữ liệu TextRevision, TextBlock, Job, ToolVersion, Document.
- **Kiểm chứng dự kiến:** TC-F-015; trạng thái Chưa thực hiện.

### FR-016 Đối chiếu và xác nhận text — luồng chính

- **Nguồn và mục tiêu:** FE-04 / UC-006; pha C.
- **Yêu cầu:** SACD phải hỗ trợ đối chiếu và xác nhận text với đầu vào expectedRevisionId, danh sách block sửa, lý do và trang đã đối chiếu. Các bước bắt buộc: Hiển thị ảnh/bản gốc cạnh text; Người dùng duyệt các trang; Kiểm tra expectedRevisionId; Tạo revision mới nếu nội dung thay đổi; Xác nhận đủ trang và lưu người/thời điểm; Đánh dấu phụ thuộc stale khi revision hiện hành thay đổi.
- **Tiền điều kiện:** Có revision trích xuất; mở được bản gốc.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nguồn cũ giữ nguyên; revision mới được kiểm tra khi đủ điều kiện
- **Dữ liệu kiểm tra cụ thể:** Sửa 13 thành 18 ở revision 2; báo cáo dùng revision 1 chuyển stale.
- **Ràng buộc:** BR-07, BR-09; kiểm soát CMP-03, dữ liệu TextRevision, TextBlock, EvidenceRef, AuditEvent, ReportSource.
- **Kiểm chứng dự kiến:** TC-F-016; trạng thái Chưa thực hiện.

### FR-017 Đối chiếu và xác nhận text — nhánh thay thế

- **Nguồn và mục tiêu:** FE-04 / UC-006; pha C.
- **Yêu cầu:** Nếu chỉ xác nhận không sửa: thêm sự kiện review, giữ nguyên text hash
- **Tiền điều kiện:** Có revision trích xuất; mở được bản gốc.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Nếu chỉ xác nhận không sửa: thêm sự kiện review, giữ nguyên text hash.
- **Ràng buộc:** BR-07, BR-09; kiểm soát CMP-03, dữ liệu TextRevision, TextBlock, EvidenceRef, AuditEvent, ReportSource.
- **Kiểm chứng dự kiến:** TC-F-017; trạng thái Chưa thực hiện.

### FR-018 Đối chiếu và xác nhận text — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-04 / UC-006; pha C.
- **Yêu cầu:** RevisionConflict hoặc còn trang chưa đọc: không Confirm toàn tài liệu
- **Tiền điều kiện:** Có revision trích xuất; mở được bản gốc.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** RevisionConflict hoặc còn trang chưa đọc: không Confirm toàn tài liệu.
- **Ràng buộc:** BR-07, BR-09; kiểm soát CMP-03, dữ liệu TextRevision, TextBlock, EvidenceRef, AuditEvent, ReportSource.
- **Kiểm chứng dự kiến:** TC-F-018; trạng thái Chưa thực hiện.

### FR-019 Xác nhận metadata và nhiệm vụ đề xuất — luồng chính

- **Nguồn và mục tiêu:** FE-05 / UC-007; pha C.
- **Yêu cầu:** SACD phải hỗ trợ xác nhận metadata và nhiệm vụ đề xuất với đầu vào Số/ký hiệu, ngày, đơn vị, người ký, trích yếu, tóm lược, việc liên quan và nguồn. Các bước bắt buộc: Chạy quy tắc đề xuất nếu được bật; Hiển thị từng trường với nguồn; Người dùng sửa/để Chưa xác định; Kiểm tra loại ngày và trường bắt buộc; Ghi phiên bản và lý do sửa; Chuyển nhiệm vụ đề xuất thành TaskItem sau xác nhận.
- **Tiền điều kiện:** Text nguồn đã được xem; danh mục trường hợp lệ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Các giá trị có trạng thái và nguồn; ngày ban hành không tự thành hạn
- **Dữ liệu kiểm tra cụ thể:** META-01 có ngày ban hành 01/10 và hạn 15/10; lưu hai trường tách biệt.
- **Ràng buộc:** BR-09, BR-10; kiểm soát CMP-04, dữ liệu MetadataVersion, EvidenceRef, TaskItem, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-019; trạng thái Chưa thực hiện.

### FR-020 Xác nhận metadata và nhiệm vụ đề xuất — nhánh thay thế

- **Nguồn và mục tiêu:** FE-05 / UC-007; pha C.
- **Yêu cầu:** Thông tin thiếu nguồn: lưu Proposed với manualReason, không đưa vào trường cần Confirmed
- **Tiền điều kiện:** Text nguồn đã được xem; danh mục trường hợp lệ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Thông tin thiếu nguồn: lưu Proposed với manualReason, không đưa vào trường cần Confirmed.
- **Ràng buộc:** BR-09, BR-10; kiểm soát CMP-04, dữ liệu MetadataVersion, EvidenceRef, TaskItem, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-020; trạng thái Chưa thực hiện.

### FR-021 Xác nhận metadata và nhiệm vụ đề xuất — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-05 / UC-007; pha C.
- **Yêu cầu:** Hai nguồn khác ngày/số: NeedsReview; không âm thầm lấy giá trị cuối
- **Tiền điều kiện:** Text nguồn đã được xem; danh mục trường hợp lệ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Hai nguồn khác ngày/số: NeedsReview; không âm thầm lấy giá trị cuối.
- **Ràng buộc:** BR-09, BR-10; kiểm soát CMP-04, dữ liệu MetadataVersion, EvidenceRef, TaskItem, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-021; trạng thái Chưa thực hiện.

### FR-022 Phân loại và ghi thông tin mật giải mật — luồng chính

- **Nguồn và mục tiêu:** FE-06 / UC-008; pha C.
- **Yêu cầu:** SACD phải hỗ trợ phân loại và ghi thông tin mật giải mật với đầu vào Chủ đề, loại, mật/khẩn, mốc giải mật và chứng cứ. Các bước bắt buộc: Chọn chủ đề/loại theo danh mục; Hiển thị gợi ý nhưng không tự áp dụng; Ghi người xác nhận mức mật/khẩn; Nhập mốc và căn cứ liên quan giải mật; Lưu phiên bản phân loại; Lập nhắc kiểm tra nếu có mốc.
- **Tiền điều kiện:** Người dùng có quyền xử lý hồ sơ trên máy; căn cứ được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có bản ghi truy vết; không tự thay đổi thẩm quyền hay giải mật
- **Dữ liệu kiểm tra cụ thể:** SEC-01 đến ngày mốc; vẫn chờ xác nhận có căn cứ, không tự giải mật.
- **Ràng buộc:** BR-10, BR-11; kiểm soát CMP-04, dữ liệu ClassificationRecord, DirectoryEntryVersion, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-022; trạng thái Chưa thực hiện.

### FR-023 Phân loại và ghi thông tin mật giải mật — nhánh thay thế

- **Nguồn và mục tiêu:** FE-06 / UC-008; pha C.
- **Yêu cầu:** Không có thông tin: giữ Unknown, không tự gán Không mật
- **Tiền điều kiện:** Người dùng có quyền xử lý hồ sơ trên máy; căn cứ được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Không có thông tin: giữ Unknown, không tự gán Không mật.
- **Ràng buộc:** BR-10, BR-11; kiểm soát CMP-04, dữ liệu ClassificationRecord, DirectoryEntryVersion, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-023; trạng thái Chưa thực hiện.

### FR-024 Phân loại và ghi thông tin mật giải mật — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-06 / UC-008; pha C.
- **Yêu cầu:** Chỉ có ngày đến mốc nhưng không căn cứ quyết định: cảnh báo kiểm tra, không đổi trạng thái
- **Tiền điều kiện:** Người dùng có quyền xử lý hồ sơ trên máy; căn cứ được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Chỉ có ngày đến mốc nhưng không căn cứ quyết định: cảnh báo kiểm tra, không đổi trạng thái.
- **Ràng buộc:** BR-10, BR-11; kiểm soát CMP-04, dữ liệu ClassificationRecord, DirectoryEntryVersion, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-024; trạng thái Chưa thực hiện.

### FR-025 Xem trước và gửi lệnh in — luồng chính

- **Nguồn và mục tiêu:** FE-07 / UC-009; pha C.
- **Yêu cầu:** SACD phải hỗ trợ xem trước và gửi lệnh in với đầu vào Tệp hoặc revision phiếu, máy in, trang, số bản. Các bước bắt buộc: Tạo preview đúng phiên bản; Kiểm tra máy in, trang và số bản; Người dùng xác nhận; Ghi PrintAttempt Prepared; Gửi lệnh và lưu spoolJobId nếu có; Người dùng đối chiếu giấy và ghi kết quả.
- **Tiền điều kiện:** Máy in/mẫu được chọn; dữ liệu đã đối chiếu đúng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Phân biệt Prepared, Submitted, ConfirmedPrinted và Unknown
- **Dữ liệu kiểm tra cụ thể:** PRINT-01 rớt phản hồi sau gửi; không xuất thêm một bản tự động.
- **Ràng buộc:** BR-12, BR-13; kiểm soát CMP-05, dữ liệu PrintAttempt, FileObject, TemplateVersion.
- **Kiểm chứng dự kiến:** TC-F-025; trạng thái Chưa thực hiện.

### FR-026 Xem trước và gửi lệnh in — nhánh thay thế

- **Nguồn và mục tiêu:** FE-07 / UC-009; pha C.
- **Yêu cầu:** In lại: yêu cầu lý do và xác nhận riêng; không sửa attempt cũ
- **Tiền điều kiện:** Máy in/mẫu được chọn; dữ liệu đã đối chiếu đúng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** In lại: yêu cầu lý do và xác nhận riêng; không sửa attempt cũ.
- **Ràng buộc:** BR-12, BR-13; kiểm soát CMP-05, dữ liệu PrintAttempt, FileObject, TemplateVersion.
- **Kiểm chứng dự kiến:** TC-F-026; trạng thái Chưa thực hiện.

### FR-027 Xem trước và gửi lệnh in — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-07 / UC-009; pha C.
- **Yêu cầu:** Spooler timeout sau gửi: Unknown, không tự gửi lệnh lần hai
- **Tiền điều kiện:** Máy in/mẫu được chọn; dữ liệu đã đối chiếu đúng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Spooler timeout sau gửi: Unknown, không tự gửi lệnh lần hai.
- **Ràng buộc:** BR-12, BR-13; kiểm soát CMP-05, dữ liệu PrintAttempt, FileObject, TemplateVersion.
- **Kiểm chứng dự kiến:** TC-F-027; trạng thái Chưa thực hiện.

### FR-028 Tạo gói chuyển máy — luồng chính

- **Nguồn và mục tiêu:** FE-08 / UC-010; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tạo gói chuyển máy với đầu vào Hồ sơ/revision chọn, scope chuyển, xác nhận phương tiện. Các bước bắt buộc: Chọn dữ liệu và kiểm tra quyền chuyển; Chốt snapshot các revision và khóa tham chiếu; Tạo manifest có provenance, size, hash; Kiểm tra không kèm executable/kịch bản/secret; Ghi gói staging và verify; Người dùng xác nhận chép ra đích; Ghi Prepared/Exported, không ghi Received.
- **Tiền điều kiện:** Máy nguồn đúng profile; phương tiện/đích và dữ liệu được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Gói có định danh và bytes kiểm tra được; chưa đồng nghĩa máy B đã nhận
- **Dữ liệu kiểm tra cụ thể:** PKG-01 chỉ có tài liệu và metadata; không kèm settings/password/script.
- **Ràng buộc:** BR-14, BR-15; kiểm soát CMP-06, dữ liệu TransferPackage, TransferItem, FileObject.
- **Kiểm chứng dự kiến:** TC-F-028; trạng thái Chưa thực hiện.

### FR-029 Tạo gói chuyển máy — nhánh thay thế

- **Nguồn và mục tiêu:** FE-08 / UC-010; pha C.
- **Yêu cầu:** Chưa cắm USB: giữ gói staging, chờ thao tác vật lý
- **Tiền điều kiện:** Máy nguồn đúng profile; phương tiện/đích và dữ liệu được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Chưa cắm USB: giữ gói staging, chờ thao tác vật lý.
- **Ràng buộc:** BR-14, BR-15; kiểm soát CMP-06, dữ liệu TransferPackage, TransferItem, FileObject.
- **Kiểm chứng dự kiến:** TC-F-029; trạng thái Chưa thực hiện.

### FR-030 Tạo gói chuyển máy — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-08 / UC-010; pha C.
- **Yêu cầu:** Chép gián đoạn: không hoàn thành đích; giữ nguồn, không tạo cầu nối mạng
- **Tiền điều kiện:** Máy nguồn đúng profile; phương tiện/đích và dữ liệu được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Chép gián đoạn: không hoàn thành đích; giữ nguồn, không tạo cầu nối mạng.
- **Ràng buộc:** BR-14, BR-15; kiểm soát CMP-06, dữ liệu TransferPackage, TransferItem, FileObject.
- **Kiểm chứng dự kiến:** TC-F-030; trạng thái Chưa thực hiện.

### FR-031 Nhận gói và xử lý xung đột — luồng chính

- **Nguồn và mục tiêu:** FE-08 / UC-011; pha C.
- **Yêu cầu:** SACD phải hỗ trợ nhận gói và xử lý xung đột với đầu vào Gói, xác nhận bước phương tiện, lựa chọn xử lý xung đột. Các bước bắt buộc: Đọc manifest bằng giới hạn luồng; Kiểm tra đường dẫn, symlink, size và hash; So origin+entity+version với ImportMap; Hiển thị bản mới/trùng/xung đột; Người dùng chọn áp dụng hoặc cách ly; Import các tham chiếu trong transaction theo hai lượt; Ghi receipt nhập gói.
- **Tiền điều kiện:** Máy B; kiểm tra phương tiện hoàn tất ngoài SACD; schema gói hỗ trợ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Mỗi entity nhập có provenance; không ghi đè revision hiện hành trái ý người dùng
- **Dữ liệu kiểm tra cụ thể:** PKG-02 cùng ID nhưng đổi bytes: TransferConflict; không dùng timestamp để thắng.
- **Ràng buộc:** BR-14, BR-16; kiểm soát CMP-06, dữ liệu TransferPackage, TransferItem, ImportMap, Dossier, FileObject.
- **Kiểm chứng dự kiến:** TC-F-031; trạng thái Chưa thực hiện.

### FR-032 Nhận gói và xử lý xung đột — nhánh thay thế

- **Nguồn và mục tiêu:** FE-08 / UC-011; pha C.
- **Yêu cầu:** Nhập lại cùng package/hash: trả AlreadyImported; không nhân đôi nghiệp vụ
- **Tiền điều kiện:** Máy B; kiểm tra phương tiện hoàn tất ngoài SACD; schema gói hỗ trợ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Nhập lại cùng package/hash: trả AlreadyImported; không nhân đôi nghiệp vụ.
- **Ràng buộc:** BR-14, BR-16; kiểm soát CMP-06, dữ liệu TransferPackage, TransferItem, ImportMap, Dossier, FileObject.
- **Kiểm chứng dự kiến:** TC-F-032; trạng thái Chưa thực hiện.

### FR-033 Nhận gói và xử lý xung đột — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-08 / UC-011; pha C.
- **Yêu cầu:** Cùng origin/version khác contentHash hoặc thiếu nguồn: quarantine cả gói, không merge nửa chừng
- **Tiền điều kiện:** Máy B; kiểm tra phương tiện hoàn tất ngoài SACD; schema gói hỗ trợ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Cùng origin/version khác contentHash hoặc thiếu nguồn: quarantine cả gói, không merge nửa chừng.
- **Ràng buộc:** BR-14, BR-16; kiểm soát CMP-06, dữ liệu TransferPackage, TransferItem, ImportMap, Dossier, FileObject.
- **Kiểm chứng dự kiến:** TC-F-033; trạng thái Chưa thực hiện.

### FR-034 Tạo và cập nhật nhiệm vụ — luồng chính

- **Nguồn và mục tiêu:** FE-09 / UC-012; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tạo và cập nhật nhiệm vụ với đầu vào Tên việc, người/đội, hạn đề xuất, bằng chứng, expectedRowVersion. Các bước bắt buộc: Chọn hồ sơ và nội dung việc; Chọn người/đội theo danh mục; Ghi hạn hoặc Unknown; Xác nhận nhiệm vụ và lưu event; Cập nhật tiến độ bằng event mới.
- **Tiền điều kiện:** Hồ sơ còn hiệu lực; danh mục người/đội hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Task có người chịu trách nhiệm và lịch sử; không tự hoàn thành khi giao
- **Dữ liệu kiểm tra cụ thể:** TASK-01 có 2 việc với hai hạn khác nhau; nhắc riêng từng việc.
- **Ràng buộc:** BR-17, BR-18; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, Dossier, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-034; trạng thái Chưa thực hiện.

### FR-035 Tạo và cập nhật nhiệm vụ — nhánh thay thế

- **Nguồn và mục tiêu:** FE-09 / UC-012; pha C.
- **Yêu cầu:** Chưa biết nơi xử lý: lưu Draft, không gán ngẫu nhiên
- **Tiền điều kiện:** Hồ sơ còn hiệu lực; danh mục người/đội hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Chưa biết nơi xử lý: lưu Draft, không gán ngẫu nhiên.
- **Ràng buộc:** BR-17, BR-18; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, Dossier, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-035; trạng thái Chưa thực hiện.

### FR-036 Tạo và cập nhật nhiệm vụ — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-09 / UC-012; pha C.
- **Yêu cầu:** Sai rowVersion hoặc ngày bất hợp lý: không commit, yêu cầu tải lại
- **Tiền điều kiện:** Hồ sơ còn hiệu lực; danh mục người/đội hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Sai rowVersion hoặc ngày bất hợp lý: không commit, yêu cầu tải lại.
- **Ràng buộc:** BR-17, BR-18; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, Dossier, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-036; trạng thái Chưa thực hiện.

### FR-037 Tính cảnh báo và ghi gia hạn — luồng chính

- **Nguồn và mục tiêu:** FE-09 / UC-013; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tính cảnh báo và ghi gia hạn với đầu vào AsOf, dueAt, deadlineKind, completedAt, căn cứ gia hạn. Các bước bắt buộc: Chụp thời điểm đối chiếu; Tách hạn ngày và hạn giờ; Tính trạng thái theo bảng quy tắc; Hiển thị việc không rõ hạn; Nếu gia hạn kiểm tra căn cứ và expected version; Ghi hạn mới cùng hạn cũ; Tính lại khi mở ứng dụng.
- **Tiền điều kiện:** Có cấu hình lịch/ngưỡng đã được chốt hoặc đang dùng profile thử.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Cảnh báo theo dữ liệu xác nhận; hoàn thành muộn vẫn được thống kê
- **Dữ liệu kiểm tra cụ thể:** TIME-01 hạn ngày 15/10: 15/10 chưa quá hạn; sang 16/10 mới quá hạn nếu chưa xong.
- **Ràng buộc:** BR-17, BR-19; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, ConfigVersion, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-037; trạng thái Chưa thực hiện.

### FR-038 Tính cảnh báo và ghi gia hạn — nhánh thay thế

- **Nguồn và mục tiêu:** FE-09 / UC-013; pha C.
- **Yêu cầu:** Máy tắt: không giả có nhắc; mở lại tái tính và ghi thời điểm
- **Tiền điều kiện:** Có cấu hình lịch/ngưỡng đã được chốt hoặc đang dùng profile thử.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Máy tắt: không giả có nhắc; mở lại tái tính và ghi thời điểm.
- **Ràng buộc:** BR-17, BR-19; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, ConfigVersion, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-038; trạng thái Chưa thực hiện.

### FR-039 Tính cảnh báo và ghi gia hạn — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-09 / UC-013; pha C.
- **Yêu cầu:** Không có lịch ngày làm việc hoặc đồng hồ đáng ngờ: NeedsReview, không tự suy hạn
- **Tiền điều kiện:** Có cấu hình lịch/ngưỡng đã được chốt hoặc đang dùng profile thử.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Không có lịch ngày làm việc hoặc đồng hồ đáng ngờ: NeedsReview, không tự suy hạn.
- **Ràng buộc:** BR-17, BR-19; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, ConfigVersion, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-039; trạng thái Chưa thực hiện.

### FR-040 Điền dữ liệu vào QLVB và đối soát — luồng chính

- **Nguồn và mục tiêu:** FE-10 / UC-014; pha C.
- **Yêu cầu:** SACD phải hỗ trợ điền dữ liệu vào qlvb và đối soát với đầu vào DossierId, snapshot hash, target profile, mapping version. Các bước bắt buộc: Xem trước dữ liệu vào từng trường; Tìm đúng cửa sổ và bản ứng dụng; Tạo attempt với idempotency key; Chạy các bước điền theo mapping; PauseForUser trước lưu; Ghi ý định gửi và kiểm tra kết quả đích; Lưu mã tham chiếu hoặc NeedsReconciliation.
- **Tiền điều kiện:** Máy B; mapping/profile đã thử; dữ liệu nguồn được Confirmed.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Chỉ Success khi có hậu điều kiện kiểm tra được; giữ attempt riêng
- **Dữ liệu kiểm tra cụ thể:** EXT-01 lưu xong nhưng mất phản hồi; lần chạy sau bắt buộc mở kiểm tra, không thêm mới.
- **Ràng buộc:** BR-20, BR-21; kiểm soát CMP-08, dữ liệu ExternalEntryAttempt, RpaRun, MappingVersion, RpaStepRun, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-040; trạng thái Chưa thực hiện.

### FR-041 Điền dữ liệu vào QLVB và đối soát — nhánh thay thế

- **Nguồn và mục tiêu:** FE-10 / UC-014; pha C.
- **Yêu cầu:** Đã tồn tại mã đích phù hợp: đề nghị liên kết sau người dùng đối chiếu
- **Tiền điều kiện:** Máy B; mapping/profile đã thử; dữ liệu nguồn được Confirmed.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Đã tồn tại mã đích phù hợp: đề nghị liên kết sau người dùng đối chiếu.
- **Ràng buộc:** BR-20, BR-21; kiểm soát CMP-08, dữ liệu ExternalEntryAttempt, RpaRun, MappingVersion, RpaStepRun, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-041; trạng thái Chưa thực hiện.

### FR-042 Điền dữ liệu vào QLVB và đối soát — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-10 / UC-014; pha C.
- **Yêu cầu:** Timeout sau nhấn lưu: Unknown; không retry nút lưu cho đến khi đối soát
- **Tiền điều kiện:** Máy B; mapping/profile đã thử; dữ liệu nguồn được Confirmed.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Timeout sau nhấn lưu: Unknown; không retry nút lưu cho đến khi đối soát.
- **Ràng buộc:** BR-20, BR-21; kiểm soát CMP-08, dữ liệu ExternalEntryAttempt, RpaRun, MappingVersion, RpaStepRun, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-042; trạng thái Chưa thực hiện.

### FR-043 Lập đề xuất xử lý — luồng chính

- **Nguồn và mục tiêu:** FE-11 / UC-015; pha C.
- **Yêu cầu:** SACD phải hỗ trợ lập đề xuất xử lý với đầu vào Lãnh đạo/đội/cán bộ, việc, hạn, nguồn và template phiếu. Các bước bắt buộc: Chọn hồ sơ và người xử lý; Xem gợi ý từ quy tắc nếu có; Cán bộ quyết định nội dung đề xuất; Kiểm tra trường cần có; Lưu proposal revision; Tạo phiếu để xem trước/in.
- **Tiền điều kiện:** Hồ sơ và metadata đủ để đề xuất; chưa khóa bởi quyết định hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có đề xuất Draft kiểm tra được; chưa là phân công đã duyệt
- **Dữ liệu kiểm tra cụ thể:** PROP-01 chọn Đội A; quy tắc gợi ý Đội B không được tự thay lựa chọn.
- **Ràng buộc:** BR-18, BR-22; kiểm soát CMP-09, dữ liệu ProposalVersion, TaskItem, DirectoryEntryVersion, TemplateVersion, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-043; trạng thái Chưa thực hiện.

### FR-044 Lập đề xuất xử lý — nhánh thay thế

- **Nguồn và mục tiêu:** FE-11 / UC-015; pha C.
- **Yêu cầu:** Yêu cầu sửa: tạo version mới, giữ đề xuất trước
- **Tiền điều kiện:** Hồ sơ và metadata đủ để đề xuất; chưa khóa bởi quyết định hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Yêu cầu sửa: tạo version mới, giữ đề xuất trước.
- **Ràng buộc:** BR-18, BR-22; kiểm soát CMP-09, dữ liệu ProposalVersion, TaskItem, DirectoryEntryVersion, TemplateVersion, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-044; trạng thái Chưa thực hiện.

### FR-045 Lập đề xuất xử lý — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-11 / UC-015; pha C.
- **Yêu cầu:** Danh mục đã ngưng hiệu lực hoặc thiếu căn cứ hạn: yêu cầu sửa, không tự thay người
- **Tiền điều kiện:** Hồ sơ và metadata đủ để đề xuất; chưa khóa bởi quyết định hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Danh mục đã ngưng hiệu lực hoặc thiếu căn cứ hạn: yêu cầu sửa, không tự thay người.
- **Ràng buộc:** BR-18, BR-22; kiểm soát CMP-09, dữ liệu ProposalVersion, TaskItem, DirectoryEntryVersion, TemplateVersion, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-045; trạng thái Chưa thực hiện.

### FR-046 Ghi nhận đã trình duyệt — luồng chính

- **Nguồn và mục tiêu:** FE-12 / UC-016; pha C.
- **Yêu cầu:** SACD phải hỗ trợ ghi nhận đã trình duyệt với đầu vào ProposalVersionId, thời điểm trình, nơi trình, căn cứ. Các bước bắt buộc: Mở phiếu và kiểm tra revision; Xác nhận đã trình theo quy trình thực tế; Ghi Submitted và event; Hiển thị chờ quyết định, không tự sinh Approved.
- **Tiền điều kiện:** Phiếu đúng revision đã được chuẩn bị và cán bộ xác nhận đã trình.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có lịch sử trình; chờ lãnh đạo xử lý bên ngoài hoặc hệ thống đích
- **Dữ liệu kiểm tra cụ thể:** APR-01 in phiếu nhưng chưa giao trình; trạng thái chưa trình.
- **Ràng buộc:** BR-12, BR-22; kiểm soát CMP-09, dữ liệu ProposalVersion, WorkflowEvent, PrintAttempt, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-046; trạng thái Chưa thực hiện.

### FR-047 Ghi nhận đã trình duyệt — nhánh thay thế

- **Nguồn và mục tiêu:** FE-12 / UC-016; pha C.
- **Yêu cầu:** Phiếu cần sửa trước trình: giữ Draft, không ghi Submitted
- **Tiền điều kiện:** Phiếu đúng revision đã được chuẩn bị và cán bộ xác nhận đã trình.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Phiếu cần sửa trước trình: giữ Draft, không ghi Submitted.
- **Ràng buộc:** BR-12, BR-22; kiểm soát CMP-09, dữ liệu ProposalVersion, WorkflowEvent, PrintAttempt, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-047; trạng thái Chưa thực hiện.

### FR-048 Ghi nhận đã trình duyệt — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-12 / UC-016; pha C.
- **Yêu cầu:** Chỉ có PrintAttempt Submitted không có xác nhận đã trình: từ chối chuyển chờ duyệt
- **Tiền điều kiện:** Phiếu đúng revision đã được chuẩn bị và cán bộ xác nhận đã trình.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Chỉ có PrintAttempt Submitted không có xác nhận đã trình: từ chối chuyển chờ duyệt.
- **Ràng buộc:** BR-12, BR-22; kiểm soát CMP-09, dữ liệu ProposalVersion, WorkflowEvent, PrintAttempt, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-048; trạng thái Chưa thực hiện.

### FR-049 Cập nhật quyết định thực tế — luồng chính

- **Nguồn và mục tiêu:** FE-12 / UC-017; pha C.
- **Yêu cầu:** SACD phải hỗ trợ cập nhật quyết định thực tế với đầu vào Kết quả duyệt, người quyết định, thời gian, chỉ đạo, chứng từ. Các bước bắt buộc: Mở đúng proposal revision; Ghi Approved/Rejected/ReturnedForEdit; Gắn căn cứ và người quyết định; Kiểm tra dữ liệu không do AI/RPA tự suy; Commit decision+projection; Nếu đổi chỉ đạo đánh dấu dữ liệu liên quan cần rà soát.
- **Tiền điều kiện:** Đã có proposal được trình; người cập nhật có căn cứ quyết định.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có quyết định truy vết, không phải chữ ký số của SACD
- **Dữ liệu kiểm tra cụ thể:** APR-02 quyết định yêu cầu sửa; không thể giao như bản đã duyệt.
- **Ràng buộc:** BR-11, BR-22; kiểm soát CMP-09, dữ liệu DecisionRecord, WorkflowEvent, ProposalVersion, EvidenceRef, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-049; trạng thái Chưa thực hiện.

### FR-050 Cập nhật quyết định thực tế — nhánh thay thế

- **Nguồn và mục tiêu:** FE-12 / UC-017; pha C.
- **Yêu cầu:** ReturnedForEdit: mở bản đề xuất mới, giữ quyết định cũ
- **Tiền điều kiện:** Đã có proposal được trình; người cập nhật có căn cứ quyết định.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** ReturnedForEdit: mở bản đề xuất mới, giữ quyết định cũ.
- **Ràng buộc:** BR-11, BR-22; kiểm soát CMP-09, dữ liệu DecisionRecord, WorkflowEvent, ProposalVersion, EvidenceRef, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-050; trạng thái Chưa thực hiện.

### FR-051 Cập nhật quyết định thực tế — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-12 / UC-017; pha C.
- **Yêu cầu:** Thiếu căn cứ/người quyết định hoặc sửa quyết định cũ tại chỗ: từ chối
- **Tiền điều kiện:** Đã có proposal được trình; người cập nhật có căn cứ quyết định.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Thiếu căn cứ/người quyết định hoặc sửa quyết định cũ tại chỗ: từ chối.
- **Ràng buộc:** BR-11, BR-22; kiểm soát CMP-09, dữ liệu DecisionRecord, WorkflowEvent, ProposalVersion, EvidenceRef, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-051; trạng thái Chưa thực hiện.

### FR-052 Ghi nhận giao văn bản — luồng chính

- **Nguồn và mục tiêu:** FE-13 / UC-018; pha C.
- **Yêu cầu:** SACD phải hỗ trợ ghi nhận giao văn bản với đầu vào Người nhận, danh sách tài liệu/revision, thời gian và chứng từ. Các bước bắt buộc: Chọn quyết định và tài liệu đúng bản; Chọn người nhận; Xác nhận đã giao thực tế; Ghi delivery cùng người cập nhật; Hiển thị AwaitingReceipt.
- **Tiền điều kiện:** Có căn cứ phân công/duyệt phù hợp; đủ quyền giao tài liệu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Đã giao chưa đồng nghĩa đã ký nhận; giữ từng lượt giao
- **Dữ liệu kiểm tra cụ thể:** DEL-01 giao Đội A lúc 09:00; chưa có ký nhận nên hiển thị Chờ nhận.
- **Ràng buộc:** BR-23, BR-24; kiểm soát CMP-10, dữ liệu DeliveryRecord, DecisionRecord, TaskItem, Document, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-052; trạng thái Chưa thực hiện.

### FR-053 Ghi nhận giao văn bản — nhánh thay thế

- **Nguồn và mục tiêu:** FE-13 / UC-018; pha C.
- **Yêu cầu:** Giao bổ sung bản sửa: tạo delivery mới liên kết lượt trước
- **Tiền điều kiện:** Có căn cứ phân công/duyệt phù hợp; đủ quyền giao tài liệu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Giao bổ sung bản sửa: tạo delivery mới liên kết lượt trước.
- **Ràng buộc:** BR-23, BR-24; kiểm soát CMP-10, dữ liệu DeliveryRecord, DecisionRecord, TaskItem, Document, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-053; trạng thái Chưa thực hiện.

### FR-054 Ghi nhận giao văn bản — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-13 / UC-018; pha C.
- **Yêu cầu:** Sai người nhận hoặc quyết định không còn hiệu lực: chặn, không tự đổi trường
- **Tiền điều kiện:** Có căn cứ phân công/duyệt phù hợp; đủ quyền giao tài liệu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Sai người nhận hoặc quyết định không còn hiệu lực: chặn, không tự đổi trường.
- **Ràng buộc:** BR-23, BR-24; kiểm soát CMP-10, dữ liệu DeliveryRecord, DecisionRecord, TaskItem, Document, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-054; trạng thái Chưa thực hiện.

### FR-055 Ghi nhận ký nhận có căn cứ — luồng chính

- **Nguồn và mục tiêu:** FE-13 / UC-019; pha C.
- **Yêu cầu:** SACD phải hỗ trợ ghi nhận ký nhận có căn cứ với đầu vào DeliveryId, người ký nhận, signedAt, chứng từ và expected version. Các bước bắt buộc: Mở lượt giao; Đối chiếu người nhận và chứng từ; Ghi ký nhận thực tế; Liên kết chứng từ/hash; Cập nhật projection và audit.
- **Tiền điều kiện:** Có lượt giao cần cập nhật.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Receipt tách biệt delivery và có căn cứ
- **Dữ liệu kiểm tra cụ thể:** REC-01 thiếu chữ ký: không đánh dấu Signed chỉ vì đã giao.
- **Ràng buộc:** BR-23, BR-24; kiểm soát CMP-10, dữ liệu ReceiptRecord, DeliveryRecord, WorkflowEvent, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-055; trạng thái Chưa thực hiện.

### FR-056 Ghi nhận ký nhận có căn cứ — nhánh thay thế

- **Nguồn và mục tiêu:** FE-13 / UC-019; pha C.
- **Yêu cầu:** Nhận thay có ủy quyền theo quy trình: ghi người thực nhận và căn cứ, không đổi tên người ký
- **Tiền điều kiện:** Có lượt giao cần cập nhật.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Nhận thay có ủy quyền theo quy trình: ghi người thực nhận và căn cứ, không đổi tên người ký.
- **Ràng buộc:** BR-23, BR-24; kiểm soát CMP-10, dữ liệu ReceiptRecord, DeliveryRecord, WorkflowEvent, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-056; trạng thái Chưa thực hiện.

### FR-057 Ghi nhận ký nhận có căn cứ — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-13 / UC-019; pha C.
- **Yêu cầu:** Không có chứng từ hoặc signedAt trước thời điểm giao không được giải thích: NeedsReview
- **Tiền điều kiện:** Có lượt giao cần cập nhật.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Không có chứng từ hoặc signedAt trước thời điểm giao không được giải thích: NeedsReview.
- **Ràng buộc:** BR-23, BR-24; kiểm soát CMP-10, dữ liệu ReceiptRecord, DeliveryRecord, WorkflowEvent, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-057; trạng thái Chưa thực hiện.

### FR-058 Cập nhật kết quả hoàn thành — luồng chính

- **Nguồn và mục tiêu:** FE-13 / UC-020; pha C.
- **Yêu cầu:** SACD phải hỗ trợ cập nhật kết quả hoàn thành với đầu vào Kết quả, completedAt, chứng cứ, expectedRowVersion. Các bước bắt buộc: Mở đúng task; Ghi tiến độ hoặc kết quả; Xác nhận hoàn thành và bằng chứng; Kiểm tra hạn để phân loại đúng/muộn; Commit event và projection; Giữ lịch sử hạn cũ.
- **Tiền điều kiện:** Nhiệm vụ đã giao/đang xử lý; có kết quả thực tế.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có kết quả theo từng nhiệm vụ; không gộp mọi việc thành xong khi một việc xong
- **Dữ liệu kiểm tra cụ thể:** DONE-01 một trong hai việc xong muộn; báo cáo không coi cả hồ sơ hoàn tất đúng hạn.
- **Ràng buộc:** BR-17, BR-24; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, WorkflowEvent, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-058; trạng thái Chưa thực hiện.

### FR-059 Cập nhật kết quả hoàn thành — nhánh thay thế

- **Nguồn và mục tiêu:** FE-13 / UC-020; pha C.
- **Yêu cầu:** Mở lại công việc: event Reopened với lý do, không xóa lần hoàn thành trước
- **Tiền điều kiện:** Nhiệm vụ đã giao/đang xử lý; có kết quả thực tế.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Mở lại công việc: event Reopened với lý do, không xóa lần hoàn thành trước.
- **Ràng buộc:** BR-17, BR-24; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, WorkflowEvent, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-059; trạng thái Chưa thực hiện.

### FR-060 Cập nhật kết quả hoàn thành — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-13 / UC-020; pha C.
- **Yêu cầu:** Thiếu kết quả hoặc thời điểm không hợp lệ: không Completed
- **Tiền điều kiện:** Nhiệm vụ đã giao/đang xử lý; có kết quả thực tế.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Thiếu kết quả hoặc thời điểm không hợp lệ: không Completed.
- **Ràng buộc:** BR-17, BR-24; kiểm soát CMP-07, dữ liệu TaskItem, TaskEvent, WorkflowEvent, EvidenceRef.
- **Kiểm chứng dự kiến:** TC-F-060; trạng thái Chưa thực hiện.

### FR-061 Tìm kiếm và mở nguồn — luồng chính

- **Nguồn và mục tiêu:** FE-14 / UC-021; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tìm kiếm và mở nguồn với đầu vào Từ khóa và bộ lọc số/ngày/đơn vị/chủ đề/hạn/trạng thái. Các bước bắt buộc: Chuẩn hóa truy vấn có/không dấu; Áp dụng bộ lọc có tham số; Tìm FTS và phân trang; Hiển thị loại vị trí và revision; Mở đúng block/bản gốc.
- **Tiền điều kiện:** Kho mở được và chỉ mục tương ứng revision hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Kết quả không mất dấu nguồn; lịch sử không trộn vào tìm hiện hành mặc định
- **Dữ liệu kiểm tra cụ thể:** SEARCH-01 tìm xu ly và xử lý cho cùng tập hiện hành; đ được chuẩn hóa d.
- **Ràng buộc:** BR-06, BR-25; kiểm soát CMP-11, dữ liệu TextBlock, Document, Dossier, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-061; trạng thái Chưa thực hiện.

### FR-062 Tìm kiếm và mở nguồn — nhánh thay thế

- **Nguồn và mục tiêu:** FE-14 / UC-021; pha C.
- **Yêu cầu:** Không có kết quả: trả rỗng cùng bộ lọc, không tự bỏ lọc
- **Tiền điều kiện:** Kho mở được và chỉ mục tương ứng revision hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Không có kết quả: trả rỗng cùng bộ lọc, không tự bỏ lọc.
- **Ràng buộc:** BR-06, BR-25; kiểm soát CMP-11, dữ liệu TextBlock, Document, Dossier, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-062; trạng thái Chưa thực hiện.

### FR-063 Tìm kiếm và mở nguồn — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-14 / UC-021; pha C.
- **Yêu cầu:** Chỉ mục lỗi: báo cần rebuild, không coi nguồn dữ liệu bị mất
- **Tiền điều kiện:** Kho mở được và chỉ mục tương ứng revision hiện hành.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Chỉ mục lỗi: báo cần rebuild, không coi nguồn dữ liệu bị mất.
- **Ràng buộc:** BR-06, BR-25; kiểm soát CMP-11, dữ liệu TextBlock, Document, Dossier, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-063; trạng thái Chưa thực hiện.

### FR-064 Xem lịch sử và lưu trữ logic — luồng chính

- **Nguồn và mục tiêu:** FE-14 / UC-022; pha C.
- **Yêu cầu:** SACD phải hỗ trợ xem lịch sử và lưu trữ logic với đầu vào EntityId, version hoặc lý do archive. Các bước bắt buộc: Xem các phiên bản theo thời gian; So nội dung/metadata và nguồn thay đổi; Xem các báo cáo đang tham chiếu; Nếu archive ghi lý do và event; Giữ nguyên object và các revision đã dùng.
- **Tiền điều kiện:** Người dùng có quyền đọc; đối tượng tồn tại.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Đối tượng được ẩn khỏi mặc định nếu archive, vẫn tra cứu lịch sử được
- **Dữ liệu kiểm tra cụ thể:** ARCH-01 archive nguồn; report lịch sử vẫn mở đúng trích đoạn.
- **Ràng buộc:** BR-05, BR-26; kiểm soát CMP-11, dữ liệu Dossier, TextRevision, WorkflowEvent, ReportSource.
- **Kiểm chứng dự kiến:** TC-F-064; trạng thái Chưa thực hiện.

### FR-065 Xem lịch sử và lưu trữ logic — nhánh thay thế

- **Nguồn và mục tiêu:** FE-14 / UC-022; pha C.
- **Yêu cầu:** Khôi phục archive: event mới, không xóa lịch sử
- **Tiền điều kiện:** Người dùng có quyền đọc; đối tượng tồn tại.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Khôi phục archive: event mới, không xóa lịch sử.
- **Ràng buộc:** BR-05, BR-26; kiểm soát CMP-11, dữ liệu Dossier, TextRevision, WorkflowEvent, ReportSource.
- **Kiểm chứng dự kiến:** TC-F-065; trạng thái Chưa thực hiện.

### FR-066 Xem lịch sử và lưu trữ logic — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-14 / UC-022; pha C.
- **Yêu cầu:** Yêu cầu xóa vật lý nguồn đang được tham chiếu: chặn, không cascade delete
- **Tiền điều kiện:** Người dùng có quyền đọc; đối tượng tồn tại.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Yêu cầu xóa vật lý nguồn đang được tham chiếu: chặn, không cascade delete.
- **Ràng buộc:** BR-05, BR-26; kiểm soát CMP-11, dữ liệu Dossier, TextRevision, WorkflowEvent, ReportSource.
- **Kiểm chứng dự kiến:** TC-F-066; trạng thái Chưa thực hiện.

### FR-067 Xác nhận fact theo chỉ tiêu — luồng chính

- **Nguồn và mục tiêu:** FE-15 / UC-023; pha C.
- **Yêu cầu:** SACD phải hỗ trợ xác nhận fact theo chỉ tiêu với đầu vào MetricVersionId, sourceNumberText, numberFormat, unit, kỳ, grain, evidence. Các bước bắt buộc: Chọn đoạn nguồn; Parse số theo định dạng đã chọn; Kiểm tra kiểu đếm/âm/lẻ/đơn vị; Giữ Missing khác 0; Gắn evidence và event key; Xác nhận tạo fact version.
- **Tiền điều kiện:** Nguồn Reviewed; chỉ tiêu version và định dạng số được xác nhận.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Fact Confirmed hợp lệ, source text giữ nguyên; không sửa bản cũ
- **Dữ liệu kiểm tra cụ thể:** NUM-01 1.234,50 vi-VN thành 1234.50; 1,234 khi chưa biết format bị chặn.
- **Ràng buộc:** BR-27, BR-28; kiểm soát CMP-12, dữ liệu FactVersion, FactEvidence, MetricDefinitionVersion, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-067; trạng thái Chưa thực hiện.

### FR-068 Xác nhận fact theo chỉ tiêu — nhánh thay thế

- **Nguồn và mục tiêu:** FE-15 / UC-023; pha C.
- **Yêu cầu:** Số chưa rõ định dạng: Proposed/NeedsReview, không vào tổng
- **Tiền điều kiện:** Nguồn Reviewed; chỉ tiêu version và định dạng số được xác nhận.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Số chưa rõ định dạng: Proposed/NeedsReview, không vào tổng.
- **Ràng buộc:** BR-27, BR-28; kiểm soát CMP-12, dữ liệu FactVersion, FactEvidence, MetricDefinitionVersion, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-068; trạng thái Chưa thực hiện.

### FR-069 Xác nhận fact theo chỉ tiêu — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-15 / UC-023; pha C.
- **Yêu cầu:** Không có evidence, sai unit hoặc decimal tràn/mất chính xác: không Confirmed
- **Tiền điều kiện:** Nguồn Reviewed; chỉ tiêu version và định dạng số được xác nhận.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Không có evidence, sai unit hoặc decimal tràn/mất chính xác: không Confirmed.
- **Ràng buộc:** BR-27, BR-28; kiểm soát CMP-12, dữ liệu FactVersion, FactEvidence, MetricDefinitionVersion, TextRevision.
- **Kiểm chứng dự kiến:** TC-F-069; trạng thái Chưa thực hiện.

### FR-070 Quyết định trùng và mâu thuẫn — luồng chính

- **Nguồn và mục tiêu:** FE-15 / UC-024; pha C.
- **Yêu cầu:** SACD phải hỗ trợ quyết định trùng và mâu thuẫn với đầu vào MemberFactIds, canonicalFactId, quyết định và lý do. Các bước bắt buộc: Nhóm nghi trùng theo khóa và phạm vi; Mở từng nguồn và giá trị; Người dùng chọn giữ/loại/liên kết hoặc unresolved; Tạo decision version; Đánh dấu aggregation/report phụ thuộc stale.
- **Tiền điều kiện:** Có các fact ứng viên cùng phạm vi cần đối chiếu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Mỗi sự kiện chỉ đóng góp một lần khi đã giải quyết; mọi nguồn được giữ
- **Dữ liệu kiểm tra cụ thể:** DUP-01 A13+B7+B7 trùng ->20; B7/B8 mâu thuẫn ->NeedsReview.
- **Ràng buộc:** BR-28, BR-29; kiểm soát CMP-12, dữ liệu DedupDecisionVersion, DedupMember, FactVersion, MetricDefinitionVersion.
- **Kiểm chứng dự kiến:** TC-F-070; trạng thái Chưa thực hiện.

### FR-071 Quyết định trùng và mâu thuẫn — nhánh thay thế

- **Nguồn và mục tiêu:** FE-15 / UC-024; pha C.
- **Yêu cầu:** Hai số 15 và 18: để unresolved nếu chưa quyết được, không tự lấy số lớn
- **Tiền điều kiện:** Có các fact ứng viên cùng phạm vi cần đối chiếu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Hai số 15 và 18: để unresolved nếu chưa quyết được, không tự lấy số lớn.
- **Ràng buộc:** BR-28, BR-29; kiểm soát CMP-12, dữ liệu DedupDecisionVersion, DedupMember, FactVersion, MetricDefinitionVersion.
- **Kiểm chứng dự kiến:** TC-F-071; trạng thái Chưa thực hiện.

### FR-072 Quyết định trùng và mâu thuẫn — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-15 / UC-024; pha C.
- **Yêu cầu:** Thành viên khác chỉ tiêu/kỳ/unit không tương thích: yêu cầu tách nhóm
- **Tiền điều kiện:** Có các fact ứng viên cùng phạm vi cần đối chiếu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Thành viên khác chỉ tiêu/kỳ/unit không tương thích: yêu cầu tách nhóm.
- **Ràng buộc:** BR-28, BR-29; kiểm soát CMP-12, dữ liệu DedupDecisionVersion, DedupMember, FactVersion, MetricDefinitionVersion.
- **Kiểm chứng dự kiến:** TC-F-072; trạng thái Chưa thực hiện.

### FR-073 Tổng hợp số liệu có kiểm chứng — luồng chính

- **Nguồn và mục tiêu:** FE-15 / UC-025; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tổng hợp số liệu có kiểm chứng với đầu vào Metric versions, kỳ báo cáo, scope và fact version IDs. Các bước bắt buộc: Chốt tập nguồn trong read snapshot; Kiểm tra grain, unit, kỳ và mâu thuẫn; Áp dụng flow/stock/ratio/cumulative theo rule version; Tính decimal checked, không làm tròn trung gian; Lưu contribution và missing scopes; Trả giá trị đầy đủ cùng giá trị trình bày.
- **Tiền điều kiện:** Fact Confirmed, phạm vi/đơn vị/loại kỳ rõ và trùng đã xử lý.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Tổng tái tính được; missing không biến thành 0; lỗi không trả total thành công
- **Dữ liệu kiểm tra cụ thể:** AGG-01 tỷ lệ 1/2 và 9/10 ->10/12; không 140% hoặc 70%.
- **Ràng buộc:** BR-27, BR-29; kiểm soát CMP-12, dữ liệu AggregationRun, AggregationContribution, FactVersion, MetricDefinitionVersion, DedupDecisionVersion, DedupMember.
- **Kiểm chứng dự kiến:** TC-F-073; trạng thái Chưa thực hiện.

### FR-074 Tổng hợp số liệu có kiểm chứng — nhánh thay thế

- **Nguồn và mục tiêu:** FE-15 / UC-025; pha C.
- **Yêu cầu:** Kỳ trước 0: tỷ lệ so sánh NotApplicable, không chia 0
- **Tiền điều kiện:** Fact Confirmed, phạm vi/đơn vị/loại kỳ rõ và trùng đã xử lý.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Kỳ trước 0: tỷ lệ so sánh NotApplicable, không chia 0.
- **Ràng buộc:** BR-27, BR-29; kiểm soát CMP-12, dữ liệu AggregationRun, AggregationContribution, FactVersion, MetricDefinitionVersion, DedupDecisionVersion, DedupMember.
- **Kiểm chứng dự kiến:** TC-F-074; trạng thái Chưa thực hiện.

### FR-075 Tổng hợp số liệu có kiểm chứng — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-15 / UC-025; pha C.
- **Yêu cầu:** Phạm vi chồng, thiếu coverage hoặc tràn: NeedsReview/NumericOverflow
- **Tiền điều kiện:** Fact Confirmed, phạm vi/đơn vị/loại kỳ rõ và trùng đã xử lý.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Phạm vi chồng, thiếu coverage hoặc tràn: NeedsReview/NumericOverflow.
- **Ràng buộc:** BR-27, BR-29; kiểm soát CMP-12, dữ liệu AggregationRun, AggregationContribution, FactVersion, MetricDefinitionVersion, DedupDecisionVersion, DedupMember.
- **Kiểm chứng dự kiến:** TC-F-075; trạng thái Chưa thực hiện.

### FR-076 Đăng ký mẫu Word có mapping — luồng chính

- **Nguồn và mục tiêu:** FE-16 / UC-026; pha C.
- **Yêu cầu:** SACD phải hỗ trợ đăng ký mẫu word có mapping với đầu vào Tệp mẫu, mapping fields/sections/metrics, expected template version. Các bước bắt buộc: Kiểm tra package và tag; Kiểm tra tag scalar một lần và section khớp mapping; Kiểm tra prototype hàng bảng; Chạy thử dữ liệu giả; Lưu file/mapping bất biến theo hash và version.
- **Tiền điều kiện:** DOCX mẫu qua kiểm tra an toàn; người dùng có mẫu được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có template version dùng được; không sửa mẫu đã được report tham chiếu
- **Dữ liệu kiểm tra cụ thể:** TPL-01 thiếu SACD_PERIOD; không coi template là hợp lệ.
- **Ràng buộc:** BR-30, BR-31; kiểm soát CMP-13, dữ liệu TemplateVersion, MappingVersion, FileObject.
- **Kiểm chứng dự kiến:** TC-F-076; trạng thái Chưa thực hiện.

### FR-077 Đăng ký mẫu Word có mapping — nhánh thay thế

- **Nguồn và mục tiêu:** FE-16 / UC-026; pha C.
- **Yêu cầu:** Mẫu thường chưa gắn tag: báo cần chuyển đổi thủ công
- **Tiền điều kiện:** DOCX mẫu qua kiểm tra an toàn; người dùng có mẫu được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Mẫu thường chưa gắn tag: báo cần chuyển đổi thủ công.
- **Ràng buộc:** BR-30, BR-31; kiểm soát CMP-13, dữ liệu TemplateVersion, MappingVersion, FileObject.
- **Kiểm chứng dự kiến:** TC-F-077; trạng thái Chưa thực hiện.

### FR-078 Đăng ký mẫu Word có mapping — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-16 / UC-026; pha C.
- **Yêu cầu:** Tag thiếu/trùng/lạ/nested hoặc trong textbox/header: TemplateInvalid/Unsupported
- **Tiền điều kiện:** DOCX mẫu qua kiểm tra an toàn; người dùng có mẫu được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Tag thiếu/trùng/lạ/nested hoặc trong textbox/header: TemplateInvalid/Unsupported.
- **Ràng buộc:** BR-30, BR-31; kiểm soát CMP-13, dữ liệu TemplateVersion, MappingVersion, FileObject.
- **Kiểm chứng dự kiến:** TC-F-078; trạng thái Chưa thực hiện.

### FR-079 Tạo dự thảo và snapshot — luồng chính

- **Nguồn và mục tiêu:** FE-16 / UC-027; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tạo dự thảo và snapshot với đầu vào Tên/kỳ, templateVersion, source revision IDs, aggregation IDs, nội dung từng mục. Các bước bắt buộc: Chọn nguồn/kỳ/mục; Chốt snapshot phiên bản; Điền dữ liệu xác định và trích đoạn; Người dùng biên tập hoặc yêu cầu AI khi bật; Gắn nguồn cho factual claim; Lưu Draft với blocking issues.
- **Tiền điều kiện:** Có mẫu hợp lệ và nguồn chọn; số liệu đủ điều kiện nếu dùng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Draft bất biến có snapshot; không cần AI để tạo
- **Dữ liệu kiểm tra cụ thể:** REP-01 AI tắt vẫn tạo nháp từ hai đoạn nguồn và bảng tổng đã xác nhận.
- **Ràng buộc:** BR-30, BR-32; kiểm soát CMP-13, dữ liệu Report, ReportRevision, ReportSource, ReportSection, Claim, ReportAggregation, AggregationRun, ClaimEvidence.
- **Kiểm chứng dự kiến:** TC-F-079; trạng thái Chưa thực hiện.

### FR-080 Tạo dự thảo và snapshot — nhánh thay thế

- **Nguồn và mục tiêu:** FE-16 / UC-027; pha C.
- **Yêu cầu:** Thiếu dữ liệu: giữ placeholder cần bổ sung trong Draft, không bịa
- **Tiền điều kiện:** Có mẫu hợp lệ và nguồn chọn; số liệu đủ điều kiện nếu dùng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Thiếu dữ liệu: giữ placeholder cần bổ sung trong Draft, không bịa.
- **Ràng buộc:** BR-30, BR-32; kiểm soát CMP-13, dữ liệu Report, ReportRevision, ReportSource, ReportSection, Claim, ReportAggregation, AggregationRun, ClaimEvidence.
- **Kiểm chứng dự kiến:** TC-F-080; trạng thái Chưa thực hiện.

### FR-081 Tạo dự thảo và snapshot — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-16 / UC-027; pha C.
- **Yêu cầu:** Template/aggregation stale: chặn bản trình và nêu mục cần xử lý
- **Tiền điều kiện:** Có mẫu hợp lệ và nguồn chọn; số liệu đủ điều kiện nếu dùng.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Template/aggregation stale: chặn bản trình và nêu mục cần xử lý.
- **Ràng buộc:** BR-30, BR-32; kiểm soát CMP-13, dữ liệu Report, ReportRevision, ReportSource, ReportSection, Claim, ReportAggregation, AggregationRun, ClaimEvidence.
- **Kiểm chứng dự kiến:** TC-F-081; trạng thái Chưa thực hiện.

### FR-082 Rà soát báo cáo — luồng chính

- **Nguồn và mục tiêu:** FE-16 / UC-028; pha C.
- **Yêu cầu:** SACD phải hỗ trợ rà soát báo cáo với đầu vào expectedReportRevisionId, checklist từng claim/mục, SID và ghi chú. Các bước bắt buộc: Mở từng nhận định và nguồn; Kiểm tra tổng và contribution; Xác nhận các mục bắt buộc; Kiểm tra revision nguồn và cấu hình phụ thuộc; Ghi Reviewed, reviewer, thời điểm và checklist.
- **Tiền điều kiện:** Bản Draft hiện hành, nguồn có thể mở để đối chiếu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Reviewed nội bộ, không đồng nghĩa lãnh đạo duyệt; content không sửa tại chỗ
- **Dữ liệu kiểm tra cụ thể:** REV-01 sửa một câu sau review; phải review lại bản mới.
- **Ràng buộc:** BR-09, BR-32; kiểm soát CMP-13, dữ liệu ReportRevision, Claim, ClaimEvidence, ReportSource, ReportSection, ReportAggregation.
- **Kiểm chứng dự kiến:** TC-F-082; trạng thái Chưa thực hiện.

### FR-083 Rà soát báo cáo — nhánh thay thế

- **Nguồn và mục tiêu:** FE-16 / UC-028; pha C.
- **Yêu cầu:** Sửa câu chữ: tạo Draft mới, bỏ xác nhận cũ
- **Tiền điều kiện:** Bản Draft hiện hành, nguồn có thể mở để đối chiếu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Sửa câu chữ: tạo Draft mới, bỏ xác nhận cũ.
- **Ràng buộc:** BR-09, BR-32; kiểm soát CMP-13, dữ liệu ReportRevision, Claim, ClaimEvidence, ReportSource, ReportSection, ReportAggregation.
- **Kiểm chứng dự kiến:** TC-F-083; trạng thái Chưa thực hiện.

### FR-084 Rà soát báo cáo — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-16 / UC-028; pha C.
- **Yêu cầu:** Thiếu evidence, nguồn stale hoặc checklist chưa đủ: NeedsReview, không Reviewed
- **Tiền điều kiện:** Bản Draft hiện hành, nguồn có thể mở để đối chiếu.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Thiếu evidence, nguồn stale hoặc checklist chưa đủ: NeedsReview, không Reviewed.
- **Ràng buộc:** BR-09, BR-32; kiểm soát CMP-13, dữ liệu ReportRevision, Claim, ClaimEvidence, ReportSource, ReportSection, ReportAggregation.
- **Kiểm chứng dự kiến:** TC-F-084; trạng thái Chưa thực hiện.

### FR-085 Xuất Word nháp hoặc bản trình — luồng chính

- **Nguồn và mục tiêu:** FE-16 / UC-029; pha C.
- **Yêu cầu:** SACD phải hỗ trợ xuất word nháp hoặc bản trình với đầu vào ReportRevisionId, Draft/Submission, đường dẫn. Các bước bắt buộc: Kiểm tra mode và điều kiện review/stale; Điền đúng template snapshot; Tạo nguồn và nhãn nháp nếu cần; Ghi tệp tạm cùng volume; Kiểm tra OOXML và nội dung/tag; Đổi tên nguyên tử, không ghi đè mặc định; Ghi ExportRecord hash.
- **Tiền điều kiện:** Revision được lưu; đích ngoài kho và được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có DOCX đúng revision; mỗi lần xuất là record riêng, không đổi state Reviewed
- **Dữ liệu kiểm tra cụ thể:** EXP-01 Submission của report stale bị chặn; Draft có nhãn DỰ THẢO.
- **Ràng buộc:** BR-30, BR-33; kiểm soát CMP-13, dữ liệu ExportRecord, ReportRevision, TemplateVersion, ReportSource, ReportAggregation.
- **Kiểm chứng dự kiến:** TC-F-085; trạng thái Chưa thực hiện.

### FR-086 Xuất Word nháp hoặc bản trình — nhánh thay thế

- **Nguồn và mục tiêu:** FE-16 / UC-029; pha C.
- **Yêu cầu:** Đích tồn tại: đề nghị tên mới; muốn ghi đè phải quy trình riêng được chốt
- **Tiền điều kiện:** Revision được lưu; đích ngoài kho và được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Đích tồn tại: đề nghị tên mới; muốn ghi đè phải quy trình riêng được chốt.
- **Ràng buộc:** BR-30, BR-33; kiểm soát CMP-13, dữ liệu ExportRecord, ReportRevision, TemplateVersion, ReportSource, ReportAggregation.
- **Kiểm chứng dự kiến:** TC-F-086; trạng thái Chưa thực hiện.

### FR-087 Xuất Word nháp hoặc bản trình — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-16 / UC-029; pha C.
- **Yêu cầu:** Lỗi disk/full hoặc Word giữ file: giữ bản cũ, không đánh dấu Exported thành công
- **Tiền điều kiện:** Revision được lưu; đích ngoài kho và được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Lỗi disk/full hoặc Word giữ file: giữ bản cũ, không đánh dấu Exported thành công.
- **Ràng buộc:** BR-30, BR-33; kiểm soát CMP-13, dữ liệu ExportRecord, ReportRevision, TemplateVersion, ReportSource, ReportAggregation.
- **Kiểm chứng dự kiến:** TC-F-087; trạng thái Chưa thực hiện.

### FR-088 Ghi thao tác có chủ đích — luồng chính

- **Nguồn và mục tiêu:** FE-17 / UC-030; pha C.
- **Yêu cầu:** SACD phải hỗ trợ ghi thao tác có chủ đích với đầu vào Target profile, vùng/loại thao tác được phép, tổ hợp dừng. Các bước bắt buộc: Người dùng bật Record; Cài hook và kiểm tra cửa sổ foreground; Loại vùng password và ngoài phạm vi; Ghi mouse/key/scroll cùng selector và tọa độ; Gom sự kiện nhập thành bước biến nếu an toàn; Dừng và tháo hook; Mở bản ghi để biên tập.
- **Tiền điều kiện:** Chọn đúng ứng dụng đích; phiên ghi hiển thị rõ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Có draft script; không ghi ngầm toàn máy hoặc secret
- **Dữ liệu kiểm tra cụ thể:** RPA-REC-01 gõ ở password và cửa sổ khác: không có chuỗi đó trong script/log.
- **Ràng buộc:** BR-03, BR-34; kiểm soát CMP-14, dữ liệu Script, ScriptVersion, ScriptStep, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-088; trạng thái Chưa thực hiện.

### FR-089 Ghi thao tác có chủ đích — nhánh thay thế

- **Nguồn và mục tiêu:** FE-17 / UC-030; pha C.
- **Yêu cầu:** Focus ra ngoài: pause/drop sự kiện, yêu cầu xác nhận khi quay lại
- **Tiền điều kiện:** Chọn đúng ứng dụng đích; phiên ghi hiển thị rõ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Focus ra ngoài: pause/drop sự kiện, yêu cầu xác nhận khi quay lại.
- **Ràng buộc:** BR-03, BR-34; kiểm soát CMP-14, dữ liệu Script, ScriptVersion, ScriptStep, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-089; trạng thái Chưa thực hiện.

### FR-090 Ghi thao tác có chủ đích — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-17 / UC-030; pha C.
- **Yêu cầu:** Không xác định an toàn trường nhập: không thu nội dung, tạo PauseForUser
- **Tiền điều kiện:** Chọn đúng ứng dụng đích; phiên ghi hiển thị rõ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Không xác định an toàn trường nhập: không thu nội dung, tạo PauseForUser.
- **Ràng buộc:** BR-03, BR-34; kiểm soát CMP-14, dữ liệu Script, ScriptVersion, ScriptStep, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-090; trạng thái Chưa thực hiện.

### FR-091 Biên tập và khóa phiên bản kịch bản — luồng chính

- **Nguồn và mục tiêu:** FE-17 / UC-031; pha C.
- **Yêu cầu:** SACD phải hỗ trợ biên tập và khóa phiên bản kịch bản với đầu vào Bước, biến, selectors, tọa độ dự phòng, conditions. Các bước bắt buộc: Xem từng bước và ảnh minh họa được phép nếu có; Xóa/sắp xếp bước; Thay dữ liệu nhạy cảm bằng biến nguồn; Gắn pre/post và timeout; Đặt điểm PauseForUser trước tác dụng phụ; Validate whitelist hành động; Publish script version và hash.
- **Tiền điều kiện:** Có draft script và profile đích.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Script version bất biến có schema/hash; chưa mặc nhiên tương thích production
- **Dữ liệu kiểm tra cụ thể:** SCRIPT-01 bước nhập SoVanBan lấy từ metadata Confirmed, không hardcode hồ sơ thật.
- **Ràng buộc:** BR-34, BR-35; kiểm soát CMP-14, dữ liệu ScriptVersion, ScriptStep, MappingVersion.
- **Kiểm chứng dự kiến:** TC-F-091; trạng thái Chưa thực hiện.

### FR-092 Biên tập và khóa phiên bản kịch bản — nhánh thay thế

- **Nguồn và mục tiêu:** FE-17 / UC-031; pha C.
- **Yêu cầu:** Thay selector hoặc mapping: tạo version mới cần chạy thử lại
- **Tiền điều kiện:** Có draft script và profile đích.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Thay selector hoặc mapping: tạo version mới cần chạy thử lại.
- **Ràng buộc:** BR-34, BR-35; kiểm soát CMP-14, dữ liệu ScriptVersion, ScriptStep, MappingVersion.
- **Kiểm chứng dự kiến:** TC-F-092; trạng thái Chưa thực hiện.

### FR-093 Biên tập và khóa phiên bản kịch bản — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-17 / UC-031; pha C.
- **Yêu cầu:** Lệnh shell/arbitrary code hoặc thiếu điểm xác nhận hành động rủi ro: ScriptInvalid
- **Tiền điều kiện:** Có draft script và profile đích.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Lệnh shell/arbitrary code hoặc thiếu điểm xác nhận hành động rủi ro: ScriptInvalid.
- **Ràng buộc:** BR-34, BR-35; kiểm soát CMP-14, dữ liệu ScriptVersion, ScriptStep, MappingVersion.
- **Kiểm chứng dự kiến:** TC-F-093; trạng thái Chưa thực hiện.

### FR-094 Chạy kịch bản có điều kiện — luồng chính

- **Nguồn và mục tiêu:** FE-18 / UC-032; pha C.
- **Yêu cầu:** SACD phải hỗ trợ chạy kịch bản có điều kiện với đầu vào ScriptVersionId, snapshot dữ liệu, run mode, idempotency key. Các bước bắt buộc: Kiểm tra executable/window fingerprint và DPI; Khóa runner và resolve biến; Chờ selector đúng/duy nhất; Kiểm tra focus trước SendInput; Ghi ý định trước hành động có tác dụng phụ; Thực hiện và kiểm tra postcondition; Ghi bước thành công hoặc pause/fail.
- **Tiền điều kiện:** Script/profile được thử, đúng Windows session, quyền hợp lệ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Mỗi bước có kết quả; Success chỉ khi postconditions được kiểm tra
- **Dữ liệu kiểm tra cụ thể:** RPA-RUN-01 cửa sổ đổi chỗ: UIA vẫn đúng; coordinate profile sai DPI phải dừng.
- **Ràng buộc:** BR-20, BR-35; kiểm soát CMP-15, dữ liệu RpaRun, RpaStepRun, Job, ScriptVersion, ScriptStep, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-094; trạng thái Chưa thực hiện.

### FR-095 Chạy kịch bản có điều kiện — nhánh thay thế

- **Nguồn và mục tiêu:** FE-18 / UC-032; pha C.
- **Yêu cầu:** Chạy từng bước: sau mỗi bước chờ người dùng; fallback tọa độ chỉ khi profile đã cho phép
- **Tiền điều kiện:** Script/profile được thử, đúng Windows session, quyền hợp lệ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Chạy từng bước: sau mỗi bước chờ người dùng; fallback tọa độ chỉ khi profile đã cho phép.
- **Ràng buộc:** BR-20, BR-35; kiểm soát CMP-15, dữ liệu RpaRun, RpaStepRun, Job, ScriptVersion, ScriptStep, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-095; trạng thái Chưa thực hiện.

### FR-096 Chạy kịch bản có điều kiện — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-18 / UC-032; pha C.
- **Yêu cầu:** Sai cửa sổ, nhiều match, DPI đổi hoặc UIPI: dừng; không nhấp mù hay tự nâng quyền
- **Tiền điều kiện:** Script/profile được thử, đúng Windows session, quyền hợp lệ.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Sai cửa sổ, nhiều match, DPI đổi hoặc UIPI: dừng; không nhấp mù hay tự nâng quyền.
- **Ràng buộc:** BR-20, BR-35; kiểm soát CMP-15, dữ liệu RpaRun, RpaStepRun, Job, ScriptVersion, ScriptStep, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-096; trạng thái Chưa thực hiện.

### FR-097 Tạm dừng hủy và đối soát kết quả chưa rõ — luồng chính

- **Nguồn và mục tiêu:** FE-18 / UC-033; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tạm dừng hủy và đối soát kết quả chưa rõ với đầu vào RunId, Pause/Resume/Cancel hoặc kết quả đối soát. Các bước bắt buộc: Nhận lệnh dừng ưu tiên; Ngừng phát thêm input và nhả phím đang giữ; Ghi checkpoint và bước đang chờ; Nếu side effect chưa xác định: NeedsReconciliation; Người dùng kiểm tra hệ thống đích; Chỉ Resume khi precondition còn đúng.
- **Tiền điều kiện:** Run đang chạy hoặc chờ người dùng; có quyền session.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Không tự retry tác dụng phụ Unknown; Cancel không giả rollback hệ thống ngoài
- **Dữ liệu kiểm tra cụ thể:** RPA-STOP-01 hủy ngay sau lưu; giữ trạng thái chưa rõ, không xóa bản ghi đích.
- **Ràng buộc:** BR-13, BR-21; kiểm soát CMP-15, dữ liệu RpaRun, RpaStepRun, ExternalEntryAttempt.
- **Kiểm chứng dự kiến:** TC-F-097; trạng thái Chưa thực hiện.

### FR-098 Tạm dừng hủy và đối soát kết quả chưa rõ — nhánh thay thế

- **Nguồn và mục tiêu:** FE-18 / UC-033; pha C.
- **Yêu cầu:** Resume sau mất focus: tìm lại đích và revalidate; không dùng handle cũ mặc định
- **Tiền điều kiện:** Run đang chạy hoặc chờ người dùng; có quyền session.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Resume sau mất focus: tìm lại đích và revalidate; không dùng handle cũ mặc định.
- **Ràng buộc:** BR-13, BR-21; kiểm soát CMP-15, dữ liệu RpaRun, RpaStepRun, ExternalEntryAttempt.
- **Kiểm chứng dự kiến:** TC-F-098; trạng thái Chưa thực hiện.

### FR-099 Tạm dừng hủy và đối soát kết quả chưa rõ — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-18 / UC-033; pha C.
- **Yêu cầu:** Mất điện giữa save: khi mở lại đưa run sang Interrupted/NeedsReconciliation, không Succeeded
- **Tiền điều kiện:** Run đang chạy hoặc chờ người dùng; có quyền session.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Mất điện giữa save: khi mở lại đưa run sang Interrupted/NeedsReconciliation, không Succeeded.
- **Ràng buộc:** BR-13, BR-21; kiểm soát CMP-15, dữ liệu RpaRun, RpaStepRun, ExternalEntryAttempt.
- **Kiểm chứng dự kiến:** TC-F-099; trạng thái Chưa thực hiện.

### FR-100 Khóa cấu hình và công cụ — luồng chính

- **Nguồn và mục tiêu:** FE-19 / UC-034; pha C.
- **Yêu cầu:** SACD phải hỗ trợ khóa cấu hình và công cụ với đầu vào Profile A/B, đường dẫn/hash công cụ, ngưỡng, policy version. Các bước bắt buộc: Chọn đường dẫn đúng loại công cụ; Tính hash theo luồng và kiểm tra bản phụ thuộc; Chạy self-test bằng dữ liệu giả; Lưu config version và manifest; Dừng tác vụ đang dùng cấu hình cũ trước áp dụng; Đánh dấu đối tượng phụ thuộc cần xem lại.
- **Tiền điều kiện:** Chủ kho hoặc người quản lý được ACL cấp quyền.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Config có lịch sử; không tự cập nhật qua Internet
- **Dữ liệu kiểm tra cụ thể:** CFG-01 thay tesseract.exe sau khóa hash: ToolHashMismatch.
- **Ràng buộc:** BR-02, BR-36; kiểm soát CMP-16, dữ liệu ConfigVersion, ToolVersion, MappingVersion, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-100; trạng thái Chưa thực hiện.

### FR-101 Khóa cấu hình và công cụ — nhánh thay thế

- **Nguồn và mục tiêu:** FE-19 / UC-034; pha C.
- **Yêu cầu:** Không đủ công cụ: Disabled/NotConfigured, có manual fallback
- **Tiền điều kiện:** Chủ kho hoặc người quản lý được ACL cấp quyền.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Không đủ công cụ: Disabled/NotConfigured, có manual fallback.
- **Ràng buộc:** BR-02, BR-36; kiểm soát CMP-16, dữ liệu ConfigVersion, ToolVersion, MappingVersion, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-101; trạng thái Chưa thực hiện.

### FR-102 Khóa cấu hình và công cụ — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-19 / UC-034; pha C.
- **Yêu cầu:** Hash thay đổi hoặc binary không tương thích: chặn worker, không tự chấp nhận hash mới
- **Tiền điều kiện:** Chủ kho hoặc người quản lý được ACL cấp quyền.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Hash thay đổi hoặc binary không tương thích: chặn worker, không tự chấp nhận hash mới.
- **Ràng buộc:** BR-02, BR-36; kiểm soát CMP-16, dữ liệu ConfigVersion, ToolVersion, MappingVersion, DirectoryEntryVersion.
- **Kiểm chứng dự kiến:** TC-F-102; trạng thái Chưa thực hiện.

### FR-103 Tra cứu nhật ký metadata — luồng chính

- **Nguồn và mục tiêu:** FE-19 / UC-035; pha C.
- **Yêu cầu:** SACD phải hỗ trợ tra cứu nhật ký metadata với đầu vào Bộ lọc thời gian, SID, action, objectId, correlationId. Các bước bắt buộc: Áp dụng bộ lọc có tham số; Hiển thị sự kiện và kết quả; Mở liên kết object/revision nếu được quyền; Xuất metadata log theo phạm vi cho phép.
- **Tiền điều kiện:** Có quyền đọc kho.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Không có prompt, password hoặc toàn văn trong log
- **Dữ liệu kiểm tra cụ thể:** LOG-01 tra runId chỉ ra bước lỗi, không xuất nội dung ô mật khẩu.
- **Ràng buộc:** BR-03, BR-37; kiểm soát CMP-16, dữ liệu AuditEvent, Workspace.
- **Kiểm chứng dự kiến:** TC-F-103; trạng thái Chưa thực hiện.

### FR-104 Tra cứu nhật ký metadata — nhánh thay thế

- **Nguồn và mục tiêu:** FE-19 / UC-035; pha C.
- **Yêu cầu:** Object archive: vẫn xem được event liên quan
- **Tiền điều kiện:** Có quyền đọc kho.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Object archive: vẫn xem được event liên quan.
- **Ràng buộc:** BR-03, BR-37; kiểm soát CMP-16, dữ liệu AuditEvent, Workspace.
- **Kiểm chứng dự kiến:** TC-F-104; trạng thái Chưa thực hiện.

### FR-105 Tra cứu nhật ký metadata — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-19 / UC-035; pha C.
- **Yêu cầu:** Yêu cầu sửa/xóa sự kiện từ UI: không hỗ trợ; thiếu quyền thì AccessDenied
- **Tiền điều kiện:** Có quyền đọc kho.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Yêu cầu sửa/xóa sự kiện từ UI: không hỗ trợ; thiếu quyền thì AccessDenied.
- **Ràng buộc:** BR-03, BR-37; kiểm soát CMP-16, dữ liệu AuditEvent, Workspace.
- **Kiểm chứng dự kiến:** TC-F-105; trạng thái Chưa thực hiện.

### FR-106 Sao lưu snapshot đầy đủ — luồng chính

- **Nguồn và mục tiêu:** FE-19 / UC-036; pha C.
- **Yêu cầu:** SACD phải hỗ trợ sao lưu snapshot đầy đủ với đầu vào Kho, đích, backup policy version. Các bước bắt buộc: Đóng nhận job ghi và chờ/huỷ worker; Chụp SQLite bằng backup API; Liệt kê object được snapshot tham chiếu; Chép và verify hash theo luồng; Tạo manifest, verify FK/integrity bản sao; Đổi tên backup staging; Ghi BackupRecord và mở lại queue.
- **Tiền điều kiện:** Đích ngoài kho, dung lượng đủ và được phép; không còn writer job.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Backup gồm DB và object, chưa được coi là phục hồi thành công nếu chưa drill
- **Dữ liệu kiểm tra cụ thể:** BKP-01 DB WAL đang hoạt động vẫn chụp bằng API, không copy nóng riêng tệp DB.
- **Ràng buộc:** BR-05, BR-38; kiểm soát CMP-16, dữ liệu BackupRecord, FileObject, Job.
- **Kiểm chứng dự kiến:** TC-F-106; trạng thái Chưa thực hiện.

### FR-107 Sao lưu snapshot đầy đủ — nhánh thay thế

- **Nguồn và mục tiêu:** FE-19 / UC-036; pha C.
- **Yêu cầu:** Người dùng hủy trước hoàn tất: đánh dấu Failed/Cancelled, không dùng gói dở
- **Tiền điều kiện:** Đích ngoài kho, dung lượng đủ và được phép; không còn writer job.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Người dùng hủy trước hoàn tất: đánh dấu Failed/Cancelled, không dùng gói dở.
- **Ràng buộc:** BR-05, BR-38; kiểm soát CMP-16, dữ liệu BackupRecord, FileObject, Job.
- **Kiểm chứng dự kiến:** TC-F-107; trạng thái Chưa thực hiện.

### FR-108 Sao lưu snapshot đầy đủ — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-19 / UC-036; pha C.
- **Yêu cầu:** Thiếu object/disk full: không tạo trạng thái Verified; nguồn đang chạy được giữ
- **Tiền điều kiện:** Đích ngoài kho, dung lượng đủ và được phép; không còn writer job.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Thiếu object/disk full: không tạo trạng thái Verified; nguồn đang chạy được giữ.
- **Ràng buộc:** BR-05, BR-38; kiểm soát CMP-16, dữ liệu BackupRecord, FileObject, Job.
- **Kiểm chứng dự kiến:** TC-F-108; trạng thái Chưa thực hiện.

### FR-109 Phục hồi vào kho mới và diễn tập — luồng chính

- **Nguồn và mục tiêu:** FE-19 / UC-037; pha C.
- **Yêu cầu:** SACD phải hỗ trợ phục hồi vào kho mới và diễn tập với đầu vào BackupId, new path, expected owner SID, policy di chuyển nếu có. Các bước bắt buộc: Kiểm tra path/size/hash/symlink; Chép vào staging kho mới; Kiểm tra integrity/FK/schema/object refs; Kiểm tra owner SID, không tự đổi SID; Mở thử nguồn và báo cáo; Ghi kết quả drill; Chỉ chọn kho mới sau xác nhận.
- **Tiền điều kiện:** Manifest backup hỗ trợ; đích mới; người vận hành được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Kho cũ giữ nguyên; kết quả drill có bằng chứng
- **Dữ liệu kiểm tra cụ thể:** REST-01 sửa 1 byte object trong backup: restore thất bại, kho cũ không đổi.
- **Ràng buộc:** BR-38, BR-39; kiểm soát CMP-16, dữ liệu BackupRecord, Workspace, FileObject.
- **Kiểm chứng dự kiến:** TC-F-109; trạng thái Chưa thực hiện.

### FR-110 Phục hồi vào kho mới và diễn tập — nhánh thay thế

- **Nguồn và mục tiêu:** FE-19 / UC-037; pha C.
- **Yêu cầu:** Khác tài khoản/máy: dừng chờ quy trình chuyển chủ được duyệt
- **Tiền điều kiện:** Manifest backup hỗ trợ; đích mới; người vận hành được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Khác tài khoản/máy: dừng chờ quy trình chuyển chủ được duyệt.
- **Ràng buộc:** BR-38, BR-39; kiểm soát CMP-16, dữ liệu BackupRecord, Workspace, FileObject.
- **Kiểm chứng dự kiến:** TC-F-110; trạng thái Chưa thực hiện.

### FR-111 Phục hồi vào kho mới và diễn tập — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-19 / UC-037; pha C.
- **Yêu cầu:** Manifest sai hoặc thiếu object: từ chối, xóa staging do tác vụ tạo; không tác động kho hiện hành
- **Tiền điều kiện:** Manifest backup hỗ trợ; đích mới; người vận hành được phép.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Manifest sai hoặc thiếu object: từ chối, xóa staging do tác vụ tạo; không tác động kho hiện hành.
- **Ràng buộc:** BR-38, BR-39; kiểm soát CMP-16, dữ liệu BackupRecord, Workspace, FileObject.
- **Kiểm chứng dự kiến:** TC-F-111; trạng thái Chưa thực hiện.

### FR-112 Tích hợp AI tùy chọn và kiểm tra đầu ra — luồng chính

- **Nguồn và mục tiêu:** FE-20 / UC-038; pha E.
- **Yêu cầu:** SACD phải hỗ trợ tích hợp ai tùy chọn và kiểm tra đầu ra với đầu vào Task kind, selected evidence, config/model hash và giới hạn token. Các bước bắt buộc: Nếu Disabled trả NotConfigured ngay; Nếu bật xác minh model/worker chỉ loopback; Tokenize với tokenizer cùng model và giới hạn ngân sách; Gọi model không tool/shell; Kiểm tra JSON, IDs và trạng thái hoàn tất; Lưu đề xuất cần con người rà soát, không auto-confirm.
- **Tiền điều kiện:** Lõi chạy được khi AI Disabled; chỉ bật khi công cụ/model được chấp thuận.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Không có AI vẫn dùng được toàn luồng lõi; đầu ra AI chỉ Proposed
- **Dữ liệu kiểm tra cụ thể:** AI-OFF-01 xóa model: tạo báo cáo và chạy RPA vẫn được; không có request mạng AI.
- **Ràng buộc:** BR-03, BR-40; kiểm soát CMP-17, dữ liệu AiInvocation, EvidenceRef, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-112; trạng thái Chưa thực hiện.

### FR-113 Tích hợp AI tùy chọn và kiểm tra đầu ra — nhánh thay thế

- **Nguồn và mục tiêu:** FE-20 / UC-038; pha E.
- **Yêu cầu:** Model chưa cài: hướng dẫn manual, không tải tự động hoặc hiện lỗi chặn chương trình
- **Tiền điều kiện:** Lõi chạy được khi AI Disabled; chỉ bật khi công cụ/model được chấp thuận.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Model chưa cài: hướng dẫn manual, không tải tự động hoặc hiện lỗi chặn chương trình.
- **Ràng buộc:** BR-03, BR-40; kiểm soát CMP-17, dữ liệu AiInvocation, EvidenceRef, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-113; trạng thái Chưa thực hiện.

### FR-114 Tích hợp AI tùy chọn và kiểm tra đầu ra — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-20 / UC-038; pha E.
- **Yêu cầu:** Timeout/quá budget/ID ngoài tập: không lưu như câu trả lời hợp lệ
- **Tiền điều kiện:** Lõi chạy được khi AI Disabled; chỉ bật khi công cụ/model được chấp thuận.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Timeout/quá budget/ID ngoài tập: không lưu như câu trả lời hợp lệ.
- **Ràng buộc:** BR-03, BR-40; kiểm soát CMP-17, dữ liệu AiInvocation, EvidenceRef, ConfigVersion.
- **Kiểm chứng dự kiến:** TC-F-114; trạng thái Chưa thực hiện.

### FR-115 Quản lý danh mục và ánh xạ — luồng chính

- **Nguồn và mục tiêu:** FE-21 / UC-039; pha C.
- **Yêu cầu:** SACD phải hỗ trợ quản lý danh mục và ánh xạ với đầu vào Đơn vị/người/chủ đề/loại, mapping, effective date và expected version. Các bước bắt buộc: Tạo/sửa bản danh mục; Kiểm tra trùng mã và quan hệ cấp trên; Chặn vòng cây tổ chức; Publish version; Ngừng hiệu lực thay vì xóa bản đã dùng; Đánh dấu mapping/cấu hình liên quan cần kiểm tra.
- **Tiền điều kiện:** Người quản lý có quyền; danh mục đang tham chiếu được bảo toàn.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Giữ tên/mã đúng snapshot; không thay lịch sử báo cáo khi đổi danh mục
- **Dữ liệu kiểm tra cụ thể:** DIR-01 đổi tên Đội A; báo cáo cũ vẫn giữ phiên bản tên tại thời điểm lập.
- **Ràng buộc:** BR-18, BR-36; kiểm soát CMP-18, dữ liệu DirectoryEntryVersion, MappingVersion, ConfigVersion, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-115; trạng thái Chưa thực hiện.

### FR-116 Quản lý danh mục và ánh xạ — nhánh thay thế

- **Nguồn và mục tiêu:** FE-21 / UC-039; pha C.
- **Yêu cầu:** Đổi tên đơn vị: version mới với logicalId giữ nguyên
- **Tiền điều kiện:** Người quản lý có quyền; danh mục đang tham chiếu được bảo toàn.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Nhánh thay thế hiển thị rõ và giữ đúng trạng thái/nguồn; không tự bỏ điều kiện bảo vệ.
- **Dữ liệu kiểm tra cụ thể:** Đổi tên đơn vị: version mới với logicalId giữ nguyên.
- **Ràng buộc:** BR-18, BR-36; kiểm soát CMP-18, dữ liệu DirectoryEntryVersion, MappingVersion, ConfigVersion, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-116; trạng thái Chưa thực hiện.

### FR-117 Quản lý danh mục và ánh xạ — lỗi và biên an toàn

- **Nguồn và mục tiêu:** FE-21 / UC-039; pha C.
- **Yêu cầu:** Mapping trỏ trường lạ hoặc dùng version không tồn tại: từ chối
- **Tiền điều kiện:** Người quản lý có quyền; danh mục đang tham chiếu được bảo toàn.
- **Chấp nhận:** Given đúng tiền điều kiện; When thực hiện nhánh này; Then Lỗi trả code và correlationId, không báo thành công hoặc thay đổi nghiệp vụ ngoài các commit đã được nêu.
- **Dữ liệu kiểm tra cụ thể:** Mapping trỏ trường lạ hoặc dùng version không tồn tại: từ chối.
- **Ràng buộc:** BR-18, BR-36; kiểm soát CMP-18, dữ liệu DirectoryEntryVersion, MappingVersion, ConfigVersion, AuditEvent.
- **Kiểm chứng dự kiến:** TC-F-117; trạng thái Chưa thực hiện.

## 5 Yêu cầu phi chức năng

“Đề xuất DEC” nghĩa mục tiêu kỹ thuật cần chốt sau thử máy, không là số đo hoặc cam kết đã duyệt. Các yêu cầu bắt buộc về tính đúng, biên mạng, con người kiểm soát và không phụ thuộc AI không được hạ bằng cách giảm target số liệu.

### NFR-01 Offline và cách ly

**Yêu cầu:** Chạy toàn bộ luồng lõi với kết nối Internet tắt; không có DNS/HTTP ngoài loopback do SACD phát sinh; không nối máy A/B. Email client hiện hữu được đo riêng.

**Cách đo:** Packet capture/process attribution trong phiên thử; 0 kết nối ngoại vi của SACD trong 10 luồng mẫu.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-01 — Chưa thực hiện.

### NFR-02 Không phụ thuộc AI

**Yêu cầu:** Xóa/không cài model và chọn Disabled vẫn nhập, tìm, lập phiếu, RPA, báo cáo, backup; không có worker AI được khởi chạy.

**Cách đo:** Luồng E2E-01 và AI-OFF trên máy sạch; không process/token/request AI.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-02 — Chưa thực hiện.

### NFR-03 Kiểm soát quyền

**Yêu cầu:** Thao tác ghi/xuất/đổi config kiểm tra ACL/SID tại service; tài khoản khác không có quyền không đọc được kho theo ACL đã thử.

**Cách đo:** Thử hai tài khoản Windows; đối chiếu OS denial và không đổi DB.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-03 — Chưa thực hiện.

### NFR-04 Toàn vẹn và nguyên tử

**Yêu cầu:** Mất điện/kill trước và sau commit không tạo revision/nguồn/report nửa chừng hoặc Success giả.

**Cách đo:** Fault injection ở staging, rename, commit, audit và recovery; invariants đúng sau mở lại.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-04 — Chưa thực hiện.

### NFR-05 Bộ nhớ

**Yêu cầu:** Một heavy worker; mục tiêu tổng working set app+worker không quá 4 GiB trong OCR 10 trang rồi AI tùy chọn trên máy đích.

**Cách đo:** Lấy PID working set 500 ms, cộng tại từng mẫu; báo private bytes và RAM khả dụng; chưa cam kết trước đo.

**Tình trạng:** Đề xuất DEC-03. **Kiểm chứng:** TC-N-05 — Chưa thực hiện.

### NFR-06 Tìm kiếm

**Yêu cầu:** Mục tiêu p95 không quá 3 giây với 10.000 block, 30 truy vấn chuẩn, cache ấm và không heavy job.

**Cách đo:** 5 warmup; 30 thời gian sắp tăng, nearest-rank thứ 29; cold ghi riêng, không trộn.

**Tình trạng:** Đề xuất DEC-03. **Kiểm chứng:** TC-N-06 — Chưa thực hiện.

### NFR-07 Phản hồi UI

**Yêu cầu:** Tác vụ I/O/OCR/RPA không chặn dispatcher; mục tiêu phản hồi hủy/tạm dừng hiển thị trong 1 giây.

**Cách đo:** Đo request đến trạng thái UI trên máy đích; không đồng nhất với worker đã dừng.

**Tình trạng:** Đề xuất DEC-03. **Kiểm chứng:** TC-N-07 — Chưa thực hiện.

### NFR-08 Hủy worker

**Yêu cầu:** Mục tiêu không quá 10 giây từ lệnh hủy đến worker dừng và job Cancelled/NeedsReconciliation; không phát thêm input.

**Cách đo:** Theo PID, event timestamp và log bước; tác dụng phụ ngoài app giữ Unknown nếu chưa biết.

**Tình trạng:** Đề xuất DEC-03. **Kiểm chứng:** TC-N-08 — Chưa thực hiện.

### NFR-09 Chất lượng OCR

**Yêu cầu:** Mục tiêu CER không quá 5% trên 30 trang in tiếng Việt sạch 300 DPI; nhóm mờ/nghiêng/bảng đo riêng.

**Cách đo:** Levenshtein trên Unicode scalar NFC, giữ dấu/khoảng trắng/punct, CRLF->LF; tổng edit/tổng GT chars.

**Tình trạng:** Đề xuất DEC-04. **Kiểm chứng:** TC-N-09 — Chưa thực hiện.

### NFR-10 Độ đúng số liệu

**Yêu cầu:** 100% fixtures F01–F09 và các biên tràn/precision phải đúng oracle; không dùng LLM để tính tổng.

**Cách đo:** Kiểm tra decimal, contribution, canonical groups và các case Missing/0/unit/grain/coverage.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-10 — Chưa thực hiện.

### NFR-11 An toàn RPA

**Yêu cầu:** Không input khi sai target, password, nhiều selector matches, UAC hoặc thiếu người xác nhận tác dụng phụ.

**Cách đo:** UI sandbox giả lập với đổi focus/DPI, dialog bất ngờ, timeout sau save; 0 side effect không được phép.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-11 — Chưa thực hiện.

### NFR-12 Nhận gói an toàn

**Yêu cầu:** Chặn traversal/symlink/ZIP bomb/missing hash/duplicate entries/unknown schema; không thực thi nội dung gói.

**Cách đo:** Corpus PKG-NEG; toàn gói quarantine và DB hiện hành không đổi nếu kiểm tra fail.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-12 — Chưa thực hiện.

### NFR-13 Truy vết nguồn

**Yêu cầu:** Mỗi fact Confirmed và factual claim khi Submission có evidence hợp lệ cùng revision, quote hash và locator.

**Cách đo:** Thử source edit, offset surrogate, hash sai và ID giả; chặn xuất nếu sai.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-13 — Chưa thực hiện.

### NFR-14 Nhật ký và riêng tư

**Yêu cầu:** Không có password/token/toàn văn/prompt trong audit hoặc crash log; metadata đủ actor/action/object/result/time.

**Cách đo:** Canary secret trong dữ liệu thử; quét log và crash dump được phép; cleanup bản thử.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-14 — Chưa thực hiện.

### NFR-15 Phục hồi

**Yêu cầu:** Mục tiêu RPO 1 ngày nếu backup cuối ngày và RTO 30 phút với kho 1 GiB; phải có drill thực tế.

**Cách đo:** Thời gian restore đến mở được nguồn/report; cảnh báo backup quá hạn; target chờ chốt policy.

**Tình trạng:** Đề xuất DEC-10. **Kiểm chứng:** TC-N-15 — Chưa thực hiện.

### NFR-16 Tương thích môi trường

**Yêu cầu:** Chỉ công bố các build Windows x64, CPU, tool, email/QLVB và DPI đã được khảo sát/kiểm thử.

**Cách đo:** Compatibility matrix ghi đúng build; không suy từ RAM hoặc compile thành công.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-16 — Chưa thực hiện.

### NFR-17 Sử dụng tiếng Việt

**Yêu cầu:** Hiển thị Unicode đầy đủ, định dạng ngày/số theo cấu hình; thao tác bàn phím và thông báo lỗi gắn trường.

**Cách đo:** Người dùng thử nhập/sửa/tìm/xuất các ký tự ă â đ ê ô ơ ư và chuỗi Unicode tổ hợp.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-17 — Chưa thực hiện.

### NFR-18 Khả năng bảo trì

**Yêu cầu:** Domain không phụ thuộc WPF/SQLite/HTTP; interface cho adapter OCR/RPA/AI; migration và contract version rõ.

**Cách đo:** Kiểm tra dependency graph và review code; contract tests chạy cùng build.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-18 — Chưa thực hiện.

### NFR-19 Đóng gói offline

**Yêu cầu:** Gói có runtime/dependency khóa version, hash, license/SBOM và rollback; máy đích không tự restore NuGet.

**Cách đo:** Thử máy Windows sạch không SDK/Internet; đối chiếu manifest trước chạy.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-19 — Chưa thực hiện.

### NFR-20 Giới hạn tài nguyên đầu vào

**Yêu cầu:** Profile khởi điểm: 50 MiB/file, 100 trang PDF/frame TIFF, 20MP/trang, DOCX 10.000 entries/250 MiB unzip.

**Cách đo:** Kiểm tra đúng giới hạn và giới hạn+1 trước decode toàn bộ; con số cần chốt sau thử máy.

**Tình trạng:** Đề xuất DEC-03. **Kiểm chứng:** TC-N-20 — Chưa thực hiện.

### NFR-21 Giới hạn gói chuyển/backup

**Yêu cầu:** Profile khởi điểm 1 GiB tổng unzip và 10.000 entries; kiểm tra bytes thực khi stream, không chỉ tin manifest.

**Cách đo:** Corpus khai size giả/đường dẫn trùng/case collision; reject không dùng gói dở.

**Tình trạng:** Đề xuất DEC-05. **Kiểm chứng:** TC-N-21 — Chưa thực hiện.

### NFR-22 Chất lượng AI khi bật

**Yêu cầu:** Nếu nghiệm thu nhánh E: 100% evidence IDs hợp lệ, mục tiêu >=90% factual claims có căn cứ, trả >=45/50 câu có nguồn và từ chối đúng 10/10 câu thiếu nguồn.

**Cách đo:** Người chấm đánh giá từng claim; câu rỗng/từ chối trong bộ có nguồn không được tính trả lời đúng.

**Tình trạng:** Mở rộng DEC-09. **Kiểm chứng:** TC-N-22 — Chưa thực hiện.

### NFR-23 Thời gian và hạn

**Yêu cầu:** UTC cho sự kiện; DateOnly cho ngày nghiệp vụ; Asia/Ho_Chi_Minh cho hạn ngày; phát hiện clock change cần xác minh.

**Cách đo:** Biên 23:59:59/00:00, completedAt và gia hạn; test clock injection, không sửa đồng hồ máy thật.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-23 — Chưa thực hiện.

### NFR-24 Phân loại bảo vệ hồ sơ

**Yêu cầu:** Mốc mật/giải mật chỉ nhắc kiểm tra; UI tránh hiển thị nội dung nhạy cảm trong thông báo Windows ngoài app.

**Cách đo:** Đến mốc vẫn giữ trạng thái cũ; thông báo chỉ mã hồ sơ/việc theo policy được chốt.

**Tình trạng:** Bắt buộc. **Kiểm chứng:** TC-N-24 — Chưa thực hiện.

## 6 Quy tắc nghiệp vụ

| Mã | Quy tắc | Chi tiết bắt buộc |
|---|---|---|
| BR-01 | Một kho có một phiên ghi; không dùng tệp SQLite chung qua mạng | Kho bị khóa phải dừng hoặc chỉ đọc theo thiết kế đã kiểm thử; không xóa lock để vượt qua |
| BR-02 | Quyền được lấy từ Windows SID và ACL thực tế | ActorContext không được mang cờ isAdmin do UI tự khai; đổi chủ kho có quy trình riêng |
| BR-03 | Giới hạn dữ liệu nhạy cảm và mạng | Không log password/token/prompt/toàn văn; không tự gửi dữ liệu Internet hoặc tự gửi email |
| BR-04 | Phiên email phải do người dùng đăng nhập đúng môi trường | Không ghi thông tin đăng nhập trong recorder; email tích hợp chỉ trên máy A và profile được phép |
| BR-05 | Bản gốc và nội dung revision đã tham chiếu không sửa đè | Lưu trữ logic giữ nguồn; file hash sai phải cách ly, không tự đổi hash để khớp |
| BR-06 | Vị trí nguồn đúng loại và đúng revision | PDF dùng trang 1-based, bbox point gốc trái trên sau transform; DOCX đoạn/ô; TXT dòng 1-based |
| BR-07 | Nguồn chỉ Reviewed sau con người kiểm tra đủ phạm vi | Không tự confirm theo điểm OCR; các trang thiếu/lỗi ngăn xác nhận toàn tài liệu |
| BR-08 | Một tác vụ nặng tại một thời điểm | OCR/AI tuần tự trên profile cấu hình thấp; ảnh từng trang được giải phóng; hủy phải thu hồi worker |
| BR-09 | Tham chiếu nguồn là khoảng UTF16 half-open trên NFC | Offset 0-based [start,end), không tách surrogate, hash UTF8 của quote; đổi nguồn đánh dấu stale |
| BR-10 | Không có dữ liệu khác với giá trị phủ định | Unknown ngày/hạn/mật không biến thành Không có hạn hoặc Không mật; đề xuất cần xác nhận |
| BR-11 | Mật giải mật và quyết định thẩm quyền không tự suy | Ngày đến mốc chỉ nhắc kiểm tra; thay trạng thái phải có căn cứ và người cập nhật |
| BR-12 | In và trình là hai sự kiện khác nhau | Submitted vào spooler không là đã in giấy; đã in không là đã trình hoặc đã duyệt |
| BR-13 | Không retry mù tác dụng phụ chưa xác định | Print/save/transfer sau timeout Unknown phải đối soát; Cancel không rollback hệ thống ngoài |
| BR-14 | Gói trao đổi không tạo kết nối mạng giữa máy | Người dùng chuyển USB vật lý theo quy trình; gói nghiệp vụ không mang binary, script hoặc secret |
| BR-15 | SHA256 không là chứng nhận an toàn hoặc chữ ký | Hash chỉ đối chiếu bytes; bước phương tiện/nguồn chuyển vẫn do quy trình được phép kiểm soát |
| BR-16 | Import cùng origin/version phải có cùng contentHash | Lặp đúng -> AlreadyImported; cùng ID khác bytes -> Conflict; không last-write-wins theo đồng hồ |
| BR-17 | Hạn là thuộc tính nhiệm vụ được xác nhận | Date: qua ngày kế tiếp theo Asia/Ho_Chi_Minh mới quá hạn; Instant: now>dueUtc; Unknown luôn NeedsReview |
| BR-18 | Chủ thể tổ chức và danh mục có version | Người đề xuất/người quyết định/người nhận là dữ liệu nghiệp vụ; không mặc nhiên cấp tài khoản mạng |
| BR-19 | Gia hạn không xóa lịch sử hoặc lỗi hoàn thành muộn | Giữ due cũ, due mới, căn cứ; completedAt so với hạn có hiệu lực theo quyết định được ghi |
| BR-20 | RPA chỉ trên ứng dụng đích và quyền được phép | Kiểm tra executable/profile/session/focus; không bypass UAC/UIPI hoặc điều khiển phiên khác |
| BR-21 | Thành công ở phần mềm ngoài cần hậu điều kiện | Có mã đích/giá trị đối soát hoặc xác nhận con người với bằng chứng; thiếu thì NeedsReconciliation |
| BR-22 | Đề xuất trình và quyết định là các bản ghi riêng | Trưởng phòng/lãnh đạo quyết định thực tế; người dùng chỉ cập nhật căn cứ, không ký thay |
| BR-23 | Đã giao khác với đã ký nhận | Receipt phải trỏ một delivery, đúng chủ thể và thời điểm hoặc ngoại lệ được ghi lý do |
| BR-24 | Hoàn thành xét theo từng nhiệm vụ | Hồ sơ có việc chưa xong không bị coi hoàn tất toàn bộ từ một receipt hoặc một task Completed |
| BR-25 | Chỉ mục FTS có thể tái tạo và không là nguồn sự thật | Mặc định tìm current revisions, giữ bản dấu để hiển thị; lọc có tham số và phân trang |
| BR-26 | Archive không xóa bằng chứng tham chiếu | Phục hồi archive thêm event; tiêu hủy ngoài phạm vi và chưa có delete cascade nguồn |
| BR-27 | Số parse theo format người dùng xác nhận và decimal exact | Không O->0, không tự nhận dấu phẩy/chấm; không làm tròn khi nhập nếu decimal mất chính xác |
| BR-28 | Fact Confirmed cần nguồn Reviewed và reviewer | Thiếu số là Missing, nguồn ghi 0 là 0; đơn vị, kỳ, grain và metric version phải khớp |
| BR-29 | Quy tắc tổng hợp theo loại chỉ tiêu và coverage | Flow cộng rời; stock lấy cuối kỳ; ratio tổng tử/tổng mẫu; cumulative không cộng chồng; tràn không trả 0 |
| BR-30 | Mẫu DOCX có hợp đồng tag và version | Scalar TITLE/PERIOD/SOURCES một lần; mỗi SECTION khớp mapping; METRICS có header+prototype; không nested/textbox/header điền |
| BR-31 | Mẫu đã dùng không sửa đè và không hiểu mọi Word | Control không thuộc SACD được giữ nếu an toàn; thiếu/không hỗ trợ phải chỉ vị trí lỗi |
| BR-32 | Nội dung report bất biến theo revision và checklist review | Sửa tạo Draft mới; source/metric/dedup/template/config đổi ảnh hưởng thì stale; Reviewed không là phê duyệt ngoài |
| BR-33 | Xuất là sự kiện riêng, ghi atomic và kiểm tra trước ghi nhận | Không thay Reviewed thành Exported; mỗi lần có ExportRecord, hash và đích; không ghi đè mặc định |
| BR-34 | Recorder chỉ ghi phiên được bật và vùng an toàn | Thoát focus drop/pause; unknown/password không thu nội dung; dừng tháo hook và nhả phím |
| BR-35 | Script chỉ whitelist hành động và có version hash | Không eval/shell/arbitrary code; selector không duy nhất hoặc sai DPI phải dừng, fallback không âm thầm |
| BR-36 | Config công cụ mapping danh mục phải có phiên bản | Hash exe/model không chứng minh provenance; khóa cả manifest dependency/tessdata; thay phải self-test lại |
| BR-37 | Audit append từ ứng dụng, không cam kết chống local admin | Metadata có correlationId/actor/result; không tạo bản log chứa dữ liệu nguồn để tiện debug |
| BR-38 | Backup chụp DB bằng API và đủ object của snapshot | Dừng writer queue, verify hash/integrity/FK; gói dở không được dùng phục hồi |
| BR-39 | Restore vào kho mới và kiểm tra chủ/schema | Không ghi đè kho sống hoặc tự đổi SID; rollback giữ kho cũ; khác máy/SID chờ policy được duyệt |
| BR-40 | AI mặc định Disabled, chỉ trả đề xuất | Không model vẫn chạy lõi; AI không tool/shell, không tự duyệt/giải mật/ký nhận/tính tổng; context phải đo token |

## 7 Dữ liệu và giao diện ngoài

Dữ liệu số dùng decimal invariant dạng TEXT, không dùng SQLite REAL để lưu fact tài chính/thống kê chính xác. Dữ liệu nguồn giữ extractedText và rawTextNfc; metadata/claim/fact tham chiếu revision cụ thể. JSON có schemaVersion, kích thước và trường allowlist; nội dung tài liệu không được diễn giải như lệnh/script.

Tích hợp qua các hợp đồng typed request/result trong SDS; chỉ định dạng gói USB, script RPA và template được mô tả ở tài liệu hợp đồng. Không thiết kế REST backend cho SACD. Nếu email/QLVB chưa có quyền API, chỉ thao tác giao diện đã khảo sát hoặc làm thủ công. Không điều khiển scanner trực tiếp nếu chưa có adapter được chốt; nhận file từ phần mềm scan là luồng nền.

## 8 Điều kiện loại trừ và chuyển tiếp

Ngoài phạm vi: cloud, mobile riêng, nối mạng hai máy, ký số/chứng nhận/giải mật tự động, bypass Windows, ghi ngầm, tự gửi email/SMS và tự tiêu hủy. Xem BRD để đối chiếu từng bước nghiệp vụ. Một chức năng C có điều kiện tích hợp chỉ được nghiệm thu phạm vi tự động khi có profile ứng dụng đích và ca thử thật; manual fallback giữ hoạt động nhưng không được dùng để tuyên bố RPA đã đạt.

## 9 Kiểm soát phiên bản và truy vết

Mỗi FR/BR/NFR liên kết tới [Đặc tả Use Case](04_SACD_Use_Case_v1.1.md), [SDS đặc tả thiết kế phần mềm](03_SACD_SDS_Dac_ta_thiet_ke_v1.1.md), [Mô hình dữ liệu ERD và từ điển dữ liệu](05_SACD_Du_lieu_ERD_v1.1.md) và [Chiến lược và ca kiểm thử](10_SACD_Kiem_thu_Test_Cases_v1.1.md). [Ma trận truy vết và độ bao phủ thiết kế](07_SACD_Ma_tran_truy_vet_v1.1.md) là bảng đối chiếu chính; DEC mở ở [ADR quyết định kiến trúc và vấn đề mở](17_SACD_ADR_Quyet_dinh_Cau_hoi_v1.1.md). Baseline chỉ được chốt theo checklist và người có trách nhiệm trong [Kế hoạch UAT và tiêu chí nghiệm thu](11_SACD_UAT_Nghiem_thu_v1.1.md).
