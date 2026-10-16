# SACD Đặc tả Use Case

> Cập nhật 16/10/2026: bảng thao tác theo 12 bước thực tế tại [quy trình hợp nhất v1.2](08_SACD_Quy_trinh_hop_nhat_v1.2.md). Thứ tự: Received → Reviewed → Printed tại Máy 1; Imported → Registered → Submitted → Approved → Delivered → Acknowledged → Completed tại Máy 2. Mỗi xác nhận cần căn cứ; đề xuất người nhận khác quyết định được duyệt. Email, chữ ký lãnh đạo/người nhận, kiểm tra mã độc và kết quả in là tác vụ thực tế được cán bộ xác nhận, chưa được tự động kiểm chứng bởi SACD.

**Mã tài liệu:** SACD-DOC-04  
**Phiên bản:** 1.1  
**Ngày:** 15/10/2026  
**Người biên soạn:** Đàm Quang Trung  
**Trạng thái:** Dự thảo chi tiết để rà soát; chưa phê duyệt baseline triển khai.

**Căn cứ:** Phiếu đăng ký và Report 1 SACD v1.0 trong `references/`. Các đặc tả bổ sung ở bản 1.1 là thiết kế đề xuất để hiện thực phạm vi đã nêu, không phải bằng chứng phần mềm đã có. AI mặc định tắt. Các mục DEC còn mở không được coi là quyết định đã được đơn vị chấp thuận.

## 1 Quy ước Use Case

UC mô tả hành vi quan sát được của mục tiêu, không phải danh sách cửa sổ hoặc mỗi cú click. Các bước “xác nhận” phải gắn đúng người thao tác, dữ liệu và phiên bản. ACT-01 là người thao tác thông thường; cấu hình/quyền/backup do ACT-02 được phân quyền theo Windows. Lãnh đạo/người nhận là chủ thể nghiệp vụ, không mặc nhiên là người dùng đăng nhập SACD.

**Include bắt buộc:** kiểm tra quyền, kiểm tra schema/version, validation và audit tại mọi mutation. **Extend có điều kiện:** OCR khi thiếu text; AI khi bật; manual fallback khi tích hợp chưa sẵn sàng. Các quan hệ này không cho phép bỏ bước kiểm soát hoặc biến AI thành điều kiện tiên quyết.

## 2 Danh mục

| UC | Tên | FE | Thiết kế | Pha |
|---|---|---|---|---|
| UC-001 | Mở kho và kiểm tra quyền | FE-01 | CMP-01 | C |
| UC-002 | Tiếp nhận thư nội bộ đã chọn | FE-02 | CMP-02 | C |
| UC-003 | Nhập tệp và giữ bản gốc | FE-03 | CMP-02 | C |
| UC-004 | Đọc văn bản điện tử | FE-04 | CMP-03 | C |
| UC-005 | OCR theo trang và hủy tác vụ | FE-04 | CMP-03 | C |
| UC-006 | Đối chiếu và xác nhận text | FE-04 | CMP-03 | C |
| UC-007 | Xác nhận metadata và nhiệm vụ đề xuất | FE-05 | CMP-04 | C |
| UC-008 | Phân loại và ghi thông tin mật giải mật | FE-06 | CMP-04 | C |
| UC-009 | Xem trước và gửi lệnh in | FE-07 | CMP-05 | C |
| UC-010 | Tạo gói chuyển máy | FE-08 | CMP-06 | C |
| UC-011 | Nhận gói và xử lý xung đột | FE-08 | CMP-06 | C |
| UC-012 | Tạo và cập nhật nhiệm vụ | FE-09 | CMP-07 | C |
| UC-013 | Tính cảnh báo và ghi gia hạn | FE-09 | CMP-07 | C |
| UC-014 | Điền dữ liệu vào QLVB và đối soát | FE-10 | CMP-08 | C |
| UC-015 | Lập đề xuất xử lý | FE-11 | CMP-09 | C |
| UC-016 | Ghi nhận đã trình duyệt | FE-12 | CMP-09 | C |
| UC-017 | Cập nhật quyết định thực tế | FE-12 | CMP-09 | C |
| UC-018 | Ghi nhận giao văn bản | FE-13 | CMP-10 | C |
| UC-019 | Ghi nhận ký nhận có căn cứ | FE-13 | CMP-10 | C |
| UC-020 | Cập nhật kết quả hoàn thành | FE-13 | CMP-07 | C |
| UC-021 | Tìm kiếm và mở nguồn | FE-14 | CMP-11 | C |
| UC-022 | Xem lịch sử và lưu trữ logic | FE-14 | CMP-11 | C |
| UC-023 | Xác nhận fact theo chỉ tiêu | FE-15 | CMP-12 | C |
| UC-024 | Quyết định trùng và mâu thuẫn | FE-15 | CMP-12 | C |
| UC-025 | Tổng hợp số liệu có kiểm chứng | FE-15 | CMP-12 | C |
| UC-026 | Đăng ký mẫu Word có mapping | FE-16 | CMP-13 | C |
| UC-027 | Tạo dự thảo và snapshot | FE-16 | CMP-13 | C |
| UC-028 | Rà soát báo cáo | FE-16 | CMP-13 | C |
| UC-029 | Xuất Word nháp hoặc bản trình | FE-16 | CMP-13 | C |
| UC-030 | Ghi thao tác có chủ đích | FE-17 | CMP-14 | C |
| UC-031 | Biên tập và khóa phiên bản kịch bản | FE-17 | CMP-14 | C |
| UC-032 | Chạy kịch bản có điều kiện | FE-18 | CMP-15 | C |
| UC-033 | Tạm dừng hủy và đối soát kết quả chưa rõ | FE-18 | CMP-15 | C |
| UC-034 | Khóa cấu hình và công cụ | FE-19 | CMP-16 | C |
| UC-035 | Tra cứu nhật ký metadata | FE-19 | CMP-16 | C |
| UC-036 | Sao lưu snapshot đầy đủ | FE-19 | CMP-16 | C |
| UC-037 | Phục hồi vào kho mới và diễn tập | FE-19 | CMP-16 | C |
| UC-038 | Tích hợp AI tùy chọn và kiểm tra đầu ra | FE-20 | CMP-17 | E |
| UC-039 | Quản lý danh mục và ánh xạ | FE-21 | CMP-18 | C |

## 3 Đặc tả chi tiết

### UC-001 Mở kho và kiểm tra quyền

**Mục tiêu:** Kho đúng SID, một phiên ghi; không tự tạo kết nối mạng.

**Tác nhân chính:** ACT-02 hoặc ACT-01 được cấp quyền tương ứng. **Kích hoạt:** người dùng chọn chức năng mở kho và kiểm tra quyền, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Thư mục cục bộ được cấp quyền; schema được hỗ trợ.

**Dữ liệu đầu vào:** Đường dẫn kho, SID hệ điều hành.

**Luồng thành công:**

1. Chuẩn hóa đường dẫn và từ chối UNC/reparse không được phép.
2. Kiểm tra ACL thực tế và chủ kho.
3. Lấy khóa một phiên ghi.
4. Kiểm tra schema, integrity và trạng thái job cũ.
5. Hiển thị profile máy và kho đang mở.

**Luồng thay thế A1:** Kho mới: khởi tạo schema trong staging rồi đổi tên sau kiểm tra.

**Ngoại lệ E1:** Kho bị phiên khác khóa hoặc SID khác: WorkspaceBusy/AccessDenied; không tự đổi quyền.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Kho đúng SID, một phiên ghi; không tự tạo kết nối mạng. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-01, BR-02. **Dữ liệu:** Workspace, AuditEvent, Job. **Yêu cầu:** FR-001, FR-002, FR-003.

**Ví dụ và oracle:** Mở kho A ở hai tiến trình; tiến trình thứ hai bị chặn. **Kiểm thử liên quan:** TC-F-001, TC-F-002, TC-F-003, đều Chưa thực hiện.

### UC-002 Tiếp nhận thư nội bộ đã chọn

**Mục tiêu:** Có hồ sơ nguồn và tệp được chọn; chưa tự xác nhận metadata.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tiếp nhận thư nội bộ đã chọn, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Máy A; tài khoản thư do người dùng đăng nhập; profile đích được khảo sát.

**Dữ liệu đầu vào:** Thư được chọn, đính kèm, nguồn thư không chứa mật khẩu.

**Luồng thành công:**

1. Xác minh đúng ứng dụng thư.
2. Người dùng chọn thư và đích lưu.
3. Lưu đính kèm vào staging được cấp quyền.
4. Quét/kiểm tra theo quy trình của máy.
5. Tạo hồ sơ tiếp nhận và liên kết nguồn thư.
6. Đối chiếu thư trùng bằng định danh và hash.

**Luồng thay thế A1:** Nếu không tương thích UIA: người dùng lưu tệp rồi nhập thủ công.

**Ngoại lệ E1:** Hết phiên, sai cửa sổ hoặc đính kèm chưa tải đủ: PauseForUser/NeedsReview; không gửi thư.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có hồ sơ nguồn và tệp được chọn; chưa tự xác nhận metadata. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-03, BR-04. **Dữ liệu:** Dossier, Document, ExternalEntryAttempt, AuditEvent, FileObject. **Yêu cầu:** FR-004, FR-005, FR-006.

**Ví dụ và oracle:** Thư mẫu MAIL-01 có 2 đính kèm; chỉ nhập tệp người dùng chọn. **Kiểm thử liên quan:** TC-F-004, TC-F-005, TC-F-006, đều Chưa thực hiện.

### UC-003 Nhập tệp và giữ bản gốc

**Mục tiêu:** Bản gốc bất biến được tham chiếu; mỗi tệp có kết quả rõ.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng nhập tệp và giữ bản gốc, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Kho ghi được và đủ dung lượng.

**Dữ liệu đầu vào:** Tệp nguồn, nhãn nguồn, mã hồ sơ nếu đã có.

**Luồng thành công:**

1. Kiểm tra size và chữ ký loại thực.
2. Kiểm tra cấu trúc ZIP/trang/pixel trước giải mã toàn bộ.
3. Chép vào staging và tính SHA256 theo luồng.
4. Xác định trùng tệp và trùng hồ sơ.
5. Commit FileObject, Document và job.
6. Hiển thị kết quả riêng từng tệp.

**Luồng thay thế A1:** Cùng bytes ở lần tiếp nhận khác: dùng lại object nhưng giữ nguồn tiếp nhận mới.

**Ngoại lệ E1:** Tệp sai loại/quá giới hạn/đứt chép: reject tệp, dọn staging; không ghi thành Imported.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Bản gốc bất biến được tham chiếu; mỗi tệp có kết quả rõ. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-05, BR-06. **Dữ liệu:** FileObject, Dossier, Document, Job, AuditEvent. **Yêu cầu:** FR-007, FR-008, FR-009.

**Ví dụ và oracle:** DOC-01 cùng bytes đổi tên vẫn cùng object_hash; không mất lịch sử tiếp nhận. **Kiểm thử liên quan:** TC-F-007, TC-F-008, TC-F-009, đều Chưa thực hiện.

### UC-004 Đọc văn bản điện tử

**Mục tiêu:** Text có nguồn và vị trí; chưa Reviewed.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng đọc văn bản điện tử, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Bản gốc nhập hợp lệ, có hash.

**Dữ liệu đầu vào:** DocumentId, bộ đọc và phiên bản công cụ.

**Luồng thành công:**

1. Xác minh hash.
2. Đọc PDF từng trang hoặc DOCX đoạn/ô hoặc TXT dòng.
3. Lưu extractedText nguyên và rawTextNfc.
4. Ghi locator có loại và thứ tự.
5. Tạo revision NeedsReview và chỉ mục hiện hành.

**Luồng thay thế A1:** PDF có trang không text: ghi danh sách trang NeedOcr để xử lý tiếp.

**Ngoại lệ E1:** Bộ đọc lỗi tại trang: job Partial/Failed; không coi toàn tài liệu hoàn thành.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Text có nguồn và vị trí; chưa Reviewed. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-06, BR-07. **Dữ liệu:** Document, TextRevision, TextBlock, Job, ToolVersion. **Yêu cầu:** FR-010, FR-011, FR-012.

**Ví dụ và oracle:** PDF-02 có trang 2 scan: trang 2 được đánh dấu chờ OCR, không bỏ mất. **Kiểm thử liên quan:** TC-F-010, TC-F-011, TC-F-012, đều Chưa thực hiện.

### UC-005 OCR theo trang và hủy tác vụ

**Mục tiêu:** Có kết quả và lỗi theo trang; hủy giữ checkpoint đúng.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng ocr theo trang và hủy tác vụ, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Tesseract và renderer được khóa hash; nguồn qua kiểm tra giới hạn.

**Dữ liệu đầu vào:** Trang chọn, DPI và xoay, cấu hình OCR.

**Luồng thành công:**

1. Dừng worker AI nếu đang dùng.
2. Xếp một tác vụ nặng vào hàng đợi.
3. Dựng một trang theo cấu hình.
4. OCR vie+eng và lưu block/bbox/điểm engine.
5. Commit checkpoint trang hoàn tất.
6. Giải phóng ảnh trước trang tiếp.
7. Kết thúc bằng NeedsReview hoặc Partial.

**Luồng thay thế A1:** Người dùng hủy: thu hồi worker rồi ghi Cancelled với lastCompletedPage.

**Ngoại lệ E1:** Thiếu tessdata, vượt pixel hoặc worker timeout: dừng, không tự giảm DPI/cắt trang.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có kết quả và lỗi theo trang; hủy giữ checkpoint đúng. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-07, BR-08. **Dữ liệu:** TextRevision, TextBlock, Job, ToolVersion, Document. **Yêu cầu:** FR-013, FR-014, FR-015.

**Ví dụ và oracle:** OCR-01 gồm 10 trang; hủy trang 4 giữ 3 trang hoàn tất và trạng thái chưa đủ. **Kiểm thử liên quan:** TC-F-013, TC-F-014, TC-F-015, đều Chưa thực hiện.

### UC-006 Đối chiếu và xác nhận text

**Mục tiêu:** Nguồn cũ giữ nguyên; revision mới được kiểm tra khi đủ điều kiện.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng đối chiếu và xác nhận text, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Có revision trích xuất; mở được bản gốc.

**Dữ liệu đầu vào:** expectedRevisionId, danh sách block sửa, lý do và trang đã đối chiếu.

**Luồng thành công:**

1. Hiển thị ảnh/bản gốc cạnh text.
2. Người dùng duyệt các trang.
3. Kiểm tra expectedRevisionId.
4. Tạo revision mới nếu nội dung thay đổi.
5. Xác nhận đủ trang và lưu người/thời điểm.
6. Đánh dấu phụ thuộc stale khi revision hiện hành thay đổi.

**Luồng thay thế A1:** Nếu chỉ xác nhận không sửa: thêm sự kiện review, giữ nguyên text hash.

**Ngoại lệ E1:** RevisionConflict hoặc còn trang chưa đọc: không Confirm toàn tài liệu.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Nguồn cũ giữ nguyên; revision mới được kiểm tra khi đủ điều kiện. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-07, BR-09. **Dữ liệu:** TextRevision, TextBlock, EvidenceRef, AuditEvent, ReportSource. **Yêu cầu:** FR-016, FR-017, FR-018.

**Ví dụ và oracle:** Sửa 13 thành 18 ở revision 2; báo cáo dùng revision 1 chuyển stale. **Kiểm thử liên quan:** TC-F-016, TC-F-017, TC-F-018, đều Chưa thực hiện.

### UC-007 Xác nhận metadata và nhiệm vụ đề xuất

**Mục tiêu:** Các giá trị có trạng thái và nguồn; ngày ban hành không tự thành hạn.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng xác nhận metadata và nhiệm vụ đề xuất, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Text nguồn đã được xem; danh mục trường hợp lệ.

**Dữ liệu đầu vào:** Số/ký hiệu, ngày, đơn vị, người ký, trích yếu, tóm lược, việc liên quan và nguồn.

**Luồng thành công:**

1. Chạy quy tắc đề xuất nếu được bật.
2. Hiển thị từng trường với nguồn.
3. Người dùng sửa/để Chưa xác định.
4. Kiểm tra loại ngày và trường bắt buộc.
5. Ghi phiên bản và lý do sửa.
6. Chuyển nhiệm vụ đề xuất thành TaskItem sau xác nhận.

**Luồng thay thế A1:** Thông tin thiếu nguồn: lưu Proposed với manualReason, không đưa vào trường cần Confirmed.

**Ngoại lệ E1:** Hai nguồn khác ngày/số: NeedsReview; không âm thầm lấy giá trị cuối.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Các giá trị có trạng thái và nguồn; ngày ban hành không tự thành hạn. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-09, BR-10. **Dữ liệu:** MetadataVersion, EvidenceRef, TaskItem, TextRevision. **Yêu cầu:** FR-019, FR-020, FR-021.

**Ví dụ và oracle:** META-01 có ngày ban hành 01/10 và hạn 15/10; lưu hai trường tách biệt. **Kiểm thử liên quan:** TC-F-019, TC-F-020, TC-F-021, đều Chưa thực hiện.

### UC-008 Phân loại và ghi thông tin mật giải mật

**Mục tiêu:** Có bản ghi truy vết; không tự thay đổi thẩm quyền hay giải mật.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng phân loại và ghi thông tin mật giải mật, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Người dùng có quyền xử lý hồ sơ trên máy; căn cứ được phép.

**Dữ liệu đầu vào:** Chủ đề, loại, mật/khẩn, mốc giải mật và chứng cứ.

**Luồng thành công:**

1. Chọn chủ đề/loại theo danh mục.
2. Hiển thị gợi ý nhưng không tự áp dụng.
3. Ghi người xác nhận mức mật/khẩn.
4. Nhập mốc và căn cứ liên quan giải mật.
5. Lưu phiên bản phân loại.
6. Lập nhắc kiểm tra nếu có mốc.

**Luồng thay thế A1:** Không có thông tin: giữ Unknown, không tự gán Không mật.

**Ngoại lệ E1:** Chỉ có ngày đến mốc nhưng không căn cứ quyết định: cảnh báo kiểm tra, không đổi trạng thái.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có bản ghi truy vết; không tự thay đổi thẩm quyền hay giải mật. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-10, BR-11. **Dữ liệu:** ClassificationRecord, DirectoryEntryVersion, EvidenceRef. **Yêu cầu:** FR-022, FR-023, FR-024.

**Ví dụ và oracle:** SEC-01 đến ngày mốc; vẫn chờ xác nhận có căn cứ, không tự giải mật. **Kiểm thử liên quan:** TC-F-022, TC-F-023, TC-F-024, đều Chưa thực hiện.

### UC-009 Xem trước và gửi lệnh in

**Mục tiêu:** Phân biệt Prepared, Submitted, ConfirmedPrinted và Unknown.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng xem trước và gửi lệnh in, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Máy in/mẫu được chọn; dữ liệu đã đối chiếu đúng.

**Dữ liệu đầu vào:** Tệp hoặc revision phiếu, máy in, trang, số bản.

**Luồng thành công:**

1. Tạo preview đúng phiên bản.
2. Kiểm tra máy in, trang và số bản.
3. Người dùng xác nhận.
4. Ghi PrintAttempt Prepared.
5. Gửi lệnh và lưu spoolJobId nếu có.
6. Người dùng đối chiếu giấy và ghi kết quả.

**Luồng thay thế A1:** In lại: yêu cầu lý do và xác nhận riêng; không sửa attempt cũ.

**Ngoại lệ E1:** Spooler timeout sau gửi: Unknown, không tự gửi lệnh lần hai.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Phân biệt Prepared, Submitted, ConfirmedPrinted và Unknown. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-12, BR-13. **Dữ liệu:** PrintAttempt, FileObject, TemplateVersion. **Yêu cầu:** FR-025, FR-026, FR-027.

**Ví dụ và oracle:** PRINT-01 rớt phản hồi sau gửi; không xuất thêm một bản tự động. **Kiểm thử liên quan:** TC-F-025, TC-F-026, TC-F-027, đều Chưa thực hiện.

### UC-010 Tạo gói chuyển máy

**Mục tiêu:** Gói có định danh và bytes kiểm tra được; chưa đồng nghĩa máy B đã nhận.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tạo gói chuyển máy, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Máy nguồn đúng profile; phương tiện/đích và dữ liệu được phép.

**Dữ liệu đầu vào:** Hồ sơ/revision chọn, scope chuyển, xác nhận phương tiện.

**Luồng thành công:**

1. Chọn dữ liệu và kiểm tra quyền chuyển.
2. Chốt snapshot các revision và khóa tham chiếu.
3. Tạo manifest có provenance, size, hash.
4. Kiểm tra không kèm executable/kịch bản/secret.
5. Ghi gói staging và verify.
6. Người dùng xác nhận chép ra đích.
7. Ghi Prepared/Exported, không ghi Received.

**Luồng thay thế A1:** Chưa cắm USB: giữ gói staging, chờ thao tác vật lý.

**Ngoại lệ E1:** Chép gián đoạn: không hoàn thành đích; giữ nguồn, không tạo cầu nối mạng.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Gói có định danh và bytes kiểm tra được; chưa đồng nghĩa máy B đã nhận. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-14, BR-15. **Dữ liệu:** TransferPackage, TransferItem, FileObject. **Yêu cầu:** FR-028, FR-029, FR-030.

**Ví dụ và oracle:** PKG-01 chỉ có tài liệu và metadata; không kèm settings/password/script. **Kiểm thử liên quan:** TC-F-028, TC-F-029, TC-F-030, đều Chưa thực hiện.

### UC-011 Nhận gói và xử lý xung đột

**Mục tiêu:** Mỗi entity nhập có provenance; không ghi đè revision hiện hành trái ý người dùng.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng nhận gói và xử lý xung đột, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Máy B; kiểm tra phương tiện hoàn tất ngoài SACD; schema gói hỗ trợ.

**Dữ liệu đầu vào:** Gói, xác nhận bước phương tiện, lựa chọn xử lý xung đột.

**Luồng thành công:**

1. Đọc manifest bằng giới hạn luồng.
2. Kiểm tra đường dẫn, symlink, size và hash.
3. So origin+entity+version với ImportMap.
4. Hiển thị bản mới/trùng/xung đột.
5. Người dùng chọn áp dụng hoặc cách ly.
6. Import các tham chiếu trong transaction theo hai lượt.
7. Ghi receipt nhập gói.

**Luồng thay thế A1:** Nhập lại cùng package/hash: trả AlreadyImported; không nhân đôi nghiệp vụ.

**Ngoại lệ E1:** Cùng origin/version khác contentHash hoặc thiếu nguồn: quarantine cả gói, không merge nửa chừng.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Mỗi entity nhập có provenance; không ghi đè revision hiện hành trái ý người dùng. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-14, BR-16. **Dữ liệu:** TransferPackage, TransferItem, ImportMap, Dossier, FileObject. **Yêu cầu:** FR-031, FR-032, FR-033.

**Ví dụ và oracle:** PKG-02 cùng ID nhưng đổi bytes: TransferConflict; không dùng timestamp để thắng. **Kiểm thử liên quan:** TC-F-031, TC-F-032, TC-F-033, đều Chưa thực hiện.

### UC-012 Tạo và cập nhật nhiệm vụ

**Mục tiêu:** Task có người chịu trách nhiệm và lịch sử; không tự hoàn thành khi giao.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tạo và cập nhật nhiệm vụ, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Hồ sơ còn hiệu lực; danh mục người/đội hiện hành.

**Dữ liệu đầu vào:** Tên việc, người/đội, hạn đề xuất, bằng chứng, expectedRowVersion.

**Luồng thành công:**

1. Chọn hồ sơ và nội dung việc.
2. Chọn người/đội theo danh mục.
3. Ghi hạn hoặc Unknown.
4. Xác nhận nhiệm vụ và lưu event.
5. Cập nhật tiến độ bằng event mới.

**Luồng thay thế A1:** Chưa biết nơi xử lý: lưu Draft, không gán ngẫu nhiên.

**Ngoại lệ E1:** Sai rowVersion hoặc ngày bất hợp lý: không commit, yêu cầu tải lại.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Task có người chịu trách nhiệm và lịch sử; không tự hoàn thành khi giao. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-17, BR-18. **Dữ liệu:** TaskItem, TaskEvent, Dossier, DirectoryEntryVersion. **Yêu cầu:** FR-034, FR-035, FR-036.

**Ví dụ và oracle:** TASK-01 có 2 việc với hai hạn khác nhau; nhắc riêng từng việc. **Kiểm thử liên quan:** TC-F-034, TC-F-035, TC-F-036, đều Chưa thực hiện.

### UC-013 Tính cảnh báo và ghi gia hạn

**Mục tiêu:** Cảnh báo theo dữ liệu xác nhận; hoàn thành muộn vẫn được thống kê.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tính cảnh báo và ghi gia hạn, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Có cấu hình lịch/ngưỡng đã được chốt hoặc đang dùng profile thử.

**Dữ liệu đầu vào:** AsOf, dueAt, deadlineKind, completedAt, căn cứ gia hạn.

**Luồng thành công:**

1. Chụp thời điểm đối chiếu.
2. Tách hạn ngày và hạn giờ.
3. Tính trạng thái theo bảng quy tắc.
4. Hiển thị việc không rõ hạn.
5. Nếu gia hạn kiểm tra căn cứ và expected version.
6. Ghi hạn mới cùng hạn cũ.
7. Tính lại khi mở ứng dụng.

**Luồng thay thế A1:** Máy tắt: không giả có nhắc; mở lại tái tính và ghi thời điểm.

**Ngoại lệ E1:** Không có lịch ngày làm việc hoặc đồng hồ đáng ngờ: NeedsReview, không tự suy hạn.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Cảnh báo theo dữ liệu xác nhận; hoàn thành muộn vẫn được thống kê. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-17, BR-19. **Dữ liệu:** TaskItem, TaskEvent, ConfigVersion, AuditEvent. **Yêu cầu:** FR-037, FR-038, FR-039.

**Ví dụ và oracle:** TIME-01 hạn ngày 15/10: 15/10 chưa quá hạn; sang 16/10 mới quá hạn nếu chưa xong. **Kiểm thử liên quan:** TC-F-037, TC-F-038, TC-F-039, đều Chưa thực hiện.

### UC-014 Điền dữ liệu vào QLVB và đối soát

**Mục tiêu:** Chỉ Success khi có hậu điều kiện kiểm tra được; giữ attempt riêng.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng điền dữ liệu vào qlvb và đối soát, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Máy B; mapping/profile đã thử; dữ liệu nguồn được Confirmed.

**Dữ liệu đầu vào:** DossierId, snapshot hash, target profile, mapping version.

**Luồng thành công:**

1. Xem trước dữ liệu vào từng trường.
2. Tìm đúng cửa sổ và bản ứng dụng.
3. Tạo attempt với idempotency key.
4. Chạy các bước điền theo mapping.
5. PauseForUser trước lưu.
6. Ghi ý định gửi và kiểm tra kết quả đích.
7. Lưu mã tham chiếu hoặc NeedsReconciliation.

**Luồng thay thế A1:** Đã tồn tại mã đích phù hợp: đề nghị liên kết sau người dùng đối chiếu.

**Ngoại lệ E1:** Timeout sau nhấn lưu: Unknown; không retry nút lưu cho đến khi đối soát.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Chỉ Success khi có hậu điều kiện kiểm tra được; giữ attempt riêng. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-20, BR-21. **Dữ liệu:** ExternalEntryAttempt, RpaRun, MappingVersion, RpaStepRun, ConfigVersion. **Yêu cầu:** FR-040, FR-041, FR-042.

**Ví dụ và oracle:** EXT-01 lưu xong nhưng mất phản hồi; lần chạy sau bắt buộc mở kiểm tra, không thêm mới. **Kiểm thử liên quan:** TC-F-040, TC-F-041, TC-F-042, đều Chưa thực hiện.

### UC-015 Lập đề xuất xử lý

**Mục tiêu:** Có đề xuất Draft kiểm tra được; chưa là phân công đã duyệt.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng lập đề xuất xử lý, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Hồ sơ và metadata đủ để đề xuất; chưa khóa bởi quyết định hiện hành.

**Dữ liệu đầu vào:** Lãnh đạo/đội/cán bộ, việc, hạn, nguồn và template phiếu.

**Luồng thành công:**

1. Chọn hồ sơ và người xử lý.
2. Xem gợi ý từ quy tắc nếu có.
3. Cán bộ quyết định nội dung đề xuất.
4. Kiểm tra trường cần có.
5. Lưu proposal revision.
6. Tạo phiếu để xem trước/in.

**Luồng thay thế A1:** Yêu cầu sửa: tạo version mới, giữ đề xuất trước.

**Ngoại lệ E1:** Danh mục đã ngưng hiệu lực hoặc thiếu căn cứ hạn: yêu cầu sửa, không tự thay người.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có đề xuất Draft kiểm tra được; chưa là phân công đã duyệt. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-18, BR-22. **Dữ liệu:** ProposalVersion, TaskItem, DirectoryEntryVersion, TemplateVersion, EvidenceRef. **Yêu cầu:** FR-043, FR-044, FR-045.

**Ví dụ và oracle:** PROP-01 chọn Đội A; quy tắc gợi ý Đội B không được tự thay lựa chọn. **Kiểm thử liên quan:** TC-F-043, TC-F-044, TC-F-045, đều Chưa thực hiện.

### UC-016 Ghi nhận đã trình duyệt

**Mục tiêu:** Có lịch sử trình; chờ lãnh đạo xử lý bên ngoài hoặc hệ thống đích.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng ghi nhận đã trình duyệt, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Phiếu đúng revision đã được chuẩn bị và cán bộ xác nhận đã trình.

**Dữ liệu đầu vào:** ProposalVersionId, thời điểm trình, nơi trình, căn cứ.

**Luồng thành công:**

1. Mở phiếu và kiểm tra revision.
2. Xác nhận đã trình theo quy trình thực tế.
3. Ghi Submitted và event.
4. Hiển thị chờ quyết định, không tự sinh Approved.

**Luồng thay thế A1:** Phiếu cần sửa trước trình: giữ Draft, không ghi Submitted.

**Ngoại lệ E1:** Chỉ có PrintAttempt Submitted không có xác nhận đã trình: từ chối chuyển chờ duyệt.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có lịch sử trình; chờ lãnh đạo xử lý bên ngoài hoặc hệ thống đích. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-12, BR-22. **Dữ liệu:** ProposalVersion, WorkflowEvent, PrintAttempt, AuditEvent. **Yêu cầu:** FR-046, FR-047, FR-048.

**Ví dụ và oracle:** APR-01 in phiếu nhưng chưa giao trình; trạng thái chưa trình. **Kiểm thử liên quan:** TC-F-046, TC-F-047, TC-F-048, đều Chưa thực hiện.

### UC-017 Cập nhật quyết định thực tế

**Mục tiêu:** Có quyết định truy vết, không phải chữ ký số của SACD.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng cập nhật quyết định thực tế, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Đã có proposal được trình; người cập nhật có căn cứ quyết định.

**Dữ liệu đầu vào:** Kết quả duyệt, người quyết định, thời gian, chỉ đạo, chứng từ.

**Luồng thành công:**

1. Mở đúng proposal revision.
2. Ghi Approved/Rejected/ReturnedForEdit.
3. Gắn căn cứ và người quyết định.
4. Kiểm tra dữ liệu không do AI/RPA tự suy.
5. Commit decision+projection.
6. Nếu đổi chỉ đạo đánh dấu dữ liệu liên quan cần rà soát.

**Luồng thay thế A1:** ReturnedForEdit: mở bản đề xuất mới, giữ quyết định cũ.

**Ngoại lệ E1:** Thiếu căn cứ/người quyết định hoặc sửa quyết định cũ tại chỗ: từ chối.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có quyết định truy vết, không phải chữ ký số của SACD. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-11, BR-22. **Dữ liệu:** DecisionRecord, WorkflowEvent, ProposalVersion, EvidenceRef, DirectoryEntryVersion. **Yêu cầu:** FR-049, FR-050, FR-051.

**Ví dụ và oracle:** APR-02 quyết định yêu cầu sửa; không thể giao như bản đã duyệt. **Kiểm thử liên quan:** TC-F-049, TC-F-050, TC-F-051, đều Chưa thực hiện.

### UC-018 Ghi nhận giao văn bản

**Mục tiêu:** Đã giao chưa đồng nghĩa đã ký nhận; giữ từng lượt giao.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng ghi nhận giao văn bản, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Có căn cứ phân công/duyệt phù hợp; đủ quyền giao tài liệu.

**Dữ liệu đầu vào:** Người nhận, danh sách tài liệu/revision, thời gian và chứng từ.

**Luồng thành công:**

1. Chọn quyết định và tài liệu đúng bản.
2. Chọn người nhận.
3. Xác nhận đã giao thực tế.
4. Ghi delivery cùng người cập nhật.
5. Hiển thị AwaitingReceipt.

**Luồng thay thế A1:** Giao bổ sung bản sửa: tạo delivery mới liên kết lượt trước.

**Ngoại lệ E1:** Sai người nhận hoặc quyết định không còn hiệu lực: chặn, không tự đổi trường.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Đã giao chưa đồng nghĩa đã ký nhận; giữ từng lượt giao. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-23, BR-24. **Dữ liệu:** DeliveryRecord, DecisionRecord, TaskItem, Document, DirectoryEntryVersion. **Yêu cầu:** FR-052, FR-053, FR-054.

**Ví dụ và oracle:** DEL-01 giao Đội A lúc 09:00; chưa có ký nhận nên hiển thị Chờ nhận. **Kiểm thử liên quan:** TC-F-052, TC-F-053, TC-F-054, đều Chưa thực hiện.

### UC-019 Ghi nhận ký nhận có căn cứ

**Mục tiêu:** Receipt tách biệt delivery và có căn cứ.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng ghi nhận ký nhận có căn cứ, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Có lượt giao cần cập nhật.

**Dữ liệu đầu vào:** DeliveryId, người ký nhận, signedAt, chứng từ và expected version.

**Luồng thành công:**

1. Mở lượt giao.
2. Đối chiếu người nhận và chứng từ.
3. Ghi ký nhận thực tế.
4. Liên kết chứng từ/hash.
5. Cập nhật projection và audit.

**Luồng thay thế A1:** Nhận thay có ủy quyền theo quy trình: ghi người thực nhận và căn cứ, không đổi tên người ký.

**Ngoại lệ E1:** Không có chứng từ hoặc signedAt trước thời điểm giao không được giải thích: NeedsReview.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Receipt tách biệt delivery và có căn cứ. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-23, BR-24. **Dữ liệu:** ReceiptRecord, DeliveryRecord, WorkflowEvent, EvidenceRef. **Yêu cầu:** FR-055, FR-056, FR-057.

**Ví dụ và oracle:** REC-01 thiếu chữ ký: không đánh dấu Signed chỉ vì đã giao. **Kiểm thử liên quan:** TC-F-055, TC-F-056, TC-F-057, đều Chưa thực hiện.

### UC-020 Cập nhật kết quả hoàn thành

**Mục tiêu:** Có kết quả theo từng nhiệm vụ; không gộp mọi việc thành xong khi một việc xong.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng cập nhật kết quả hoàn thành, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Nhiệm vụ đã giao/đang xử lý; có kết quả thực tế.

**Dữ liệu đầu vào:** Kết quả, completedAt, chứng cứ, expectedRowVersion.

**Luồng thành công:**

1. Mở đúng task.
2. Ghi tiến độ hoặc kết quả.
3. Xác nhận hoàn thành và bằng chứng.
4. Kiểm tra hạn để phân loại đúng/muộn.
5. Commit event và projection.
6. Giữ lịch sử hạn cũ.

**Luồng thay thế A1:** Mở lại công việc: event Reopened với lý do, không xóa lần hoàn thành trước.

**Ngoại lệ E1:** Thiếu kết quả hoặc thời điểm không hợp lệ: không Completed.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có kết quả theo từng nhiệm vụ; không gộp mọi việc thành xong khi một việc xong. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-17, BR-24. **Dữ liệu:** TaskItem, TaskEvent, WorkflowEvent, EvidenceRef. **Yêu cầu:** FR-058, FR-059, FR-060.

**Ví dụ và oracle:** DONE-01 một trong hai việc xong muộn; báo cáo không coi cả hồ sơ hoàn tất đúng hạn. **Kiểm thử liên quan:** TC-F-058, TC-F-059, TC-F-060, đều Chưa thực hiện.

### UC-021 Tìm kiếm và mở nguồn

**Mục tiêu:** Kết quả không mất dấu nguồn; lịch sử không trộn vào tìm hiện hành mặc định.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tìm kiếm và mở nguồn, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Kho mở được và chỉ mục tương ứng revision hiện hành.

**Dữ liệu đầu vào:** Từ khóa và bộ lọc số/ngày/đơn vị/chủ đề/hạn/trạng thái.

**Luồng thành công:**

1. Chuẩn hóa truy vấn có/không dấu.
2. Áp dụng bộ lọc có tham số.
3. Tìm FTS và phân trang.
4. Hiển thị loại vị trí và revision.
5. Mở đúng block/bản gốc.

**Luồng thay thế A1:** Không có kết quả: trả rỗng cùng bộ lọc, không tự bỏ lọc.

**Ngoại lệ E1:** Chỉ mục lỗi: báo cần rebuild, không coi nguồn dữ liệu bị mất.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Kết quả không mất dấu nguồn; lịch sử không trộn vào tìm hiện hành mặc định. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-06, BR-25. **Dữ liệu:** TextBlock, Document, Dossier, TextRevision. **Yêu cầu:** FR-061, FR-062, FR-063.

**Ví dụ và oracle:** SEARCH-01 tìm xu ly và xử lý cho cùng tập hiện hành; đ được chuẩn hóa d. **Kiểm thử liên quan:** TC-F-061, TC-F-062, TC-F-063, đều Chưa thực hiện.

### UC-022 Xem lịch sử và lưu trữ logic

**Mục tiêu:** Đối tượng được ẩn khỏi mặc định nếu archive, vẫn tra cứu lịch sử được.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng xem lịch sử và lưu trữ logic, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Người dùng có quyền đọc; đối tượng tồn tại.

**Dữ liệu đầu vào:** EntityId, version hoặc lý do archive.

**Luồng thành công:**

1. Xem các phiên bản theo thời gian.
2. So nội dung/metadata và nguồn thay đổi.
3. Xem các báo cáo đang tham chiếu.
4. Nếu archive ghi lý do và event.
5. Giữ nguyên object và các revision đã dùng.

**Luồng thay thế A1:** Khôi phục archive: event mới, không xóa lịch sử.

**Ngoại lệ E1:** Yêu cầu xóa vật lý nguồn đang được tham chiếu: chặn, không cascade delete.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Đối tượng được ẩn khỏi mặc định nếu archive, vẫn tra cứu lịch sử được. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-05, BR-26. **Dữ liệu:** Dossier, TextRevision, WorkflowEvent, ReportSource. **Yêu cầu:** FR-064, FR-065, FR-066.

**Ví dụ và oracle:** ARCH-01 archive nguồn; report lịch sử vẫn mở đúng trích đoạn. **Kiểm thử liên quan:** TC-F-064, TC-F-065, TC-F-066, đều Chưa thực hiện.

### UC-023 Xác nhận fact theo chỉ tiêu

**Mục tiêu:** Fact Confirmed hợp lệ, source text giữ nguyên; không sửa bản cũ.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng xác nhận fact theo chỉ tiêu, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Nguồn Reviewed; chỉ tiêu version và định dạng số được xác nhận.

**Dữ liệu đầu vào:** MetricVersionId, sourceNumberText, numberFormat, unit, kỳ, grain, evidence.

**Luồng thành công:**

1. Chọn đoạn nguồn.
2. Parse số theo định dạng đã chọn.
3. Kiểm tra kiểu đếm/âm/lẻ/đơn vị.
4. Giữ Missing khác 0.
5. Gắn evidence và event key.
6. Xác nhận tạo fact version.

**Luồng thay thế A1:** Số chưa rõ định dạng: Proposed/NeedsReview, không vào tổng.

**Ngoại lệ E1:** Không có evidence, sai unit hoặc decimal tràn/mất chính xác: không Confirmed.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Fact Confirmed hợp lệ, source text giữ nguyên; không sửa bản cũ. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-27, BR-28. **Dữ liệu:** FactVersion, FactEvidence, MetricDefinitionVersion, TextRevision. **Yêu cầu:** FR-067, FR-068, FR-069.

**Ví dụ và oracle:** NUM-01 1.234,50 vi-VN thành 1234.50; 1,234 khi chưa biết format bị chặn. **Kiểm thử liên quan:** TC-F-067, TC-F-068, TC-F-069, đều Chưa thực hiện.

### UC-024 Quyết định trùng và mâu thuẫn

**Mục tiêu:** Mỗi sự kiện chỉ đóng góp một lần khi đã giải quyết; mọi nguồn được giữ.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng quyết định trùng và mâu thuẫn, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Có các fact ứng viên cùng phạm vi cần đối chiếu.

**Dữ liệu đầu vào:** MemberFactIds, canonicalFactId, quyết định và lý do.

**Luồng thành công:**

1. Nhóm nghi trùng theo khóa và phạm vi.
2. Mở từng nguồn và giá trị.
3. Người dùng chọn giữ/loại/liên kết hoặc unresolved.
4. Tạo decision version.
5. Đánh dấu aggregation/report phụ thuộc stale.

**Luồng thay thế A1:** Hai số 15 và 18: để unresolved nếu chưa quyết được, không tự lấy số lớn.

**Ngoại lệ E1:** Thành viên khác chỉ tiêu/kỳ/unit không tương thích: yêu cầu tách nhóm.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Mỗi sự kiện chỉ đóng góp một lần khi đã giải quyết; mọi nguồn được giữ. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-28, BR-29. **Dữ liệu:** DedupDecisionVersion, DedupMember, FactVersion, MetricDefinitionVersion. **Yêu cầu:** FR-070, FR-071, FR-072.

**Ví dụ và oracle:** DUP-01 A13+B7+B7 trùng ->20; B7/B8 mâu thuẫn ->NeedsReview. **Kiểm thử liên quan:** TC-F-070, TC-F-071, TC-F-072, đều Chưa thực hiện.

### UC-025 Tổng hợp số liệu có kiểm chứng

**Mục tiêu:** Tổng tái tính được; missing không biến thành 0; lỗi không trả total thành công.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tổng hợp số liệu có kiểm chứng, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Fact Confirmed, phạm vi/đơn vị/loại kỳ rõ và trùng đã xử lý.

**Dữ liệu đầu vào:** Metric versions, kỳ báo cáo, scope và fact version IDs.

**Luồng thành công:**

1. Chốt tập nguồn trong read snapshot.
2. Kiểm tra grain, unit, kỳ và mâu thuẫn.
3. Áp dụng flow/stock/ratio/cumulative theo rule version.
4. Tính decimal checked, không làm tròn trung gian.
5. Lưu contribution và missing scopes.
6. Trả giá trị đầy đủ cùng giá trị trình bày.

**Luồng thay thế A1:** Kỳ trước 0: tỷ lệ so sánh NotApplicable, không chia 0.

**Ngoại lệ E1:** Phạm vi chồng, thiếu coverage hoặc tràn: NeedsReview/NumericOverflow.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Tổng tái tính được; missing không biến thành 0; lỗi không trả total thành công. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-27, BR-29. **Dữ liệu:** AggregationRun, AggregationContribution, FactVersion, MetricDefinitionVersion, DedupDecisionVersion, DedupMember. **Yêu cầu:** FR-073, FR-074, FR-075.

**Ví dụ và oracle:** AGG-01 tỷ lệ 1/2 và 9/10 ->10/12; không 140% hoặc 70%. **Kiểm thử liên quan:** TC-F-073, TC-F-074, TC-F-075, đều Chưa thực hiện.

### UC-026 Đăng ký mẫu Word có mapping

**Mục tiêu:** Có template version dùng được; không sửa mẫu đã được report tham chiếu.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng đăng ký mẫu word có mapping, không tự chạy nền ngoài policy.

**Tiền điều kiện:** DOCX mẫu qua kiểm tra an toàn; người dùng có mẫu được phép.

**Dữ liệu đầu vào:** Tệp mẫu, mapping fields/sections/metrics, expected template version.

**Luồng thành công:**

1. Kiểm tra package và tag.
2. Kiểm tra tag scalar một lần và section khớp mapping.
3. Kiểm tra prototype hàng bảng.
4. Chạy thử dữ liệu giả.
5. Lưu file/mapping bất biến theo hash và version.

**Luồng thay thế A1:** Mẫu thường chưa gắn tag: báo cần chuyển đổi thủ công.

**Ngoại lệ E1:** Tag thiếu/trùng/lạ/nested hoặc trong textbox/header: TemplateInvalid/Unsupported.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có template version dùng được; không sửa mẫu đã được report tham chiếu. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-30, BR-31. **Dữ liệu:** TemplateVersion, MappingVersion, FileObject. **Yêu cầu:** FR-076, FR-077, FR-078.

**Ví dụ và oracle:** TPL-01 thiếu SACD_PERIOD; không coi template là hợp lệ. **Kiểm thử liên quan:** TC-F-076, TC-F-077, TC-F-078, đều Chưa thực hiện.

### UC-027 Tạo dự thảo và snapshot

**Mục tiêu:** Draft bất biến có snapshot; không cần AI để tạo.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tạo dự thảo và snapshot, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Có mẫu hợp lệ và nguồn chọn; số liệu đủ điều kiện nếu dùng.

**Dữ liệu đầu vào:** Tên/kỳ, templateVersion, source revision IDs, aggregation IDs, nội dung từng mục.

**Luồng thành công:**

1. Chọn nguồn/kỳ/mục.
2. Chốt snapshot phiên bản.
3. Điền dữ liệu xác định và trích đoạn.
4. Người dùng biên tập hoặc yêu cầu AI khi bật.
5. Gắn nguồn cho factual claim.
6. Lưu Draft với blocking issues.

**Luồng thay thế A1:** Thiếu dữ liệu: giữ placeholder cần bổ sung trong Draft, không bịa.

**Ngoại lệ E1:** Template/aggregation stale: chặn bản trình và nêu mục cần xử lý.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Draft bất biến có snapshot; không cần AI để tạo. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-30, BR-32. **Dữ liệu:** Report, ReportRevision, ReportSource, ReportSection, Claim, ReportAggregation, AggregationRun, ClaimEvidence. **Yêu cầu:** FR-079, FR-080, FR-081.

**Ví dụ và oracle:** REP-01 AI tắt vẫn tạo nháp từ hai đoạn nguồn và bảng tổng đã xác nhận. **Kiểm thử liên quan:** TC-F-079, TC-F-080, TC-F-081, đều Chưa thực hiện.

### UC-028 Rà soát báo cáo

**Mục tiêu:** Reviewed nội bộ, không đồng nghĩa lãnh đạo duyệt; content không sửa tại chỗ.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng rà soát báo cáo, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Bản Draft hiện hành, nguồn có thể mở để đối chiếu.

**Dữ liệu đầu vào:** expectedReportRevisionId, checklist từng claim/mục, SID và ghi chú.

**Luồng thành công:**

1. Mở từng nhận định và nguồn.
2. Kiểm tra tổng và contribution.
3. Xác nhận các mục bắt buộc.
4. Kiểm tra revision nguồn và cấu hình phụ thuộc.
5. Ghi Reviewed, reviewer, thời điểm và checklist.

**Luồng thay thế A1:** Sửa câu chữ: tạo Draft mới, bỏ xác nhận cũ.

**Ngoại lệ E1:** Thiếu evidence, nguồn stale hoặc checklist chưa đủ: NeedsReview, không Reviewed.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Reviewed nội bộ, không đồng nghĩa lãnh đạo duyệt; content không sửa tại chỗ. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-09, BR-32. **Dữ liệu:** ReportRevision, Claim, ClaimEvidence, ReportSource, ReportSection, ReportAggregation. **Yêu cầu:** FR-082, FR-083, FR-084.

**Ví dụ và oracle:** REV-01 sửa một câu sau review; phải review lại bản mới. **Kiểm thử liên quan:** TC-F-082, TC-F-083, TC-F-084, đều Chưa thực hiện.

### UC-029 Xuất Word nháp hoặc bản trình

**Mục tiêu:** Có DOCX đúng revision; mỗi lần xuất là record riêng, không đổi state Reviewed.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng xuất word nháp hoặc bản trình, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Revision được lưu; đích ngoài kho và được phép.

**Dữ liệu đầu vào:** ReportRevisionId, Draft/Submission, đường dẫn.

**Luồng thành công:**

1. Kiểm tra mode và điều kiện review/stale.
2. Điền đúng template snapshot.
3. Tạo nguồn và nhãn nháp nếu cần.
4. Ghi tệp tạm cùng volume.
5. Kiểm tra OOXML và nội dung/tag.
6. Đổi tên nguyên tử, không ghi đè mặc định.
7. Ghi ExportRecord hash.

**Luồng thay thế A1:** Đích tồn tại: đề nghị tên mới; muốn ghi đè phải quy trình riêng được chốt.

**Ngoại lệ E1:** Lỗi disk/full hoặc Word giữ file: giữ bản cũ, không đánh dấu Exported thành công.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có DOCX đúng revision; mỗi lần xuất là record riêng, không đổi state Reviewed. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-30, BR-33. **Dữ liệu:** ExportRecord, ReportRevision, TemplateVersion, ReportSource, ReportAggregation. **Yêu cầu:** FR-085, FR-086, FR-087.

**Ví dụ và oracle:** EXP-01 Submission của report stale bị chặn; Draft có nhãn DỰ THẢO. **Kiểm thử liên quan:** TC-F-085, TC-F-086, TC-F-087, đều Chưa thực hiện.

### UC-030 Ghi thao tác có chủ đích

**Mục tiêu:** Có draft script; không ghi ngầm toàn máy hoặc secret.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng ghi thao tác có chủ đích, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Chọn đúng ứng dụng đích; phiên ghi hiển thị rõ.

**Dữ liệu đầu vào:** Target profile, vùng/loại thao tác được phép, tổ hợp dừng.

**Luồng thành công:**

1. Người dùng bật Record.
2. Cài hook và kiểm tra cửa sổ foreground.
3. Loại vùng password và ngoài phạm vi.
4. Ghi mouse/key/scroll cùng selector và tọa độ.
5. Gom sự kiện nhập thành bước biến nếu an toàn.
6. Dừng và tháo hook.
7. Mở bản ghi để biên tập.

**Luồng thay thế A1:** Focus ra ngoài: pause/drop sự kiện, yêu cầu xác nhận khi quay lại.

**Ngoại lệ E1:** Không xác định an toàn trường nhập: không thu nội dung, tạo PauseForUser.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Có draft script; không ghi ngầm toàn máy hoặc secret. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-03, BR-34. **Dữ liệu:** Script, ScriptVersion, ScriptStep, ConfigVersion. **Yêu cầu:** FR-088, FR-089, FR-090.

**Ví dụ và oracle:** RPA-REC-01 gõ ở password và cửa sổ khác: không có chuỗi đó trong script/log. **Kiểm thử liên quan:** TC-F-088, TC-F-089, TC-F-090, đều Chưa thực hiện.

### UC-031 Biên tập và khóa phiên bản kịch bản

**Mục tiêu:** Script version bất biến có schema/hash; chưa mặc nhiên tương thích production.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng biên tập và khóa phiên bản kịch bản, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Có draft script và profile đích.

**Dữ liệu đầu vào:** Bước, biến, selectors, tọa độ dự phòng, conditions.

**Luồng thành công:**

1. Xem từng bước và ảnh minh họa được phép nếu có.
2. Xóa/sắp xếp bước.
3. Thay dữ liệu nhạy cảm bằng biến nguồn.
4. Gắn pre/post và timeout.
5. Đặt điểm PauseForUser trước tác dụng phụ.
6. Validate whitelist hành động.
7. Publish script version và hash.

**Luồng thay thế A1:** Thay selector hoặc mapping: tạo version mới cần chạy thử lại.

**Ngoại lệ E1:** Lệnh shell/arbitrary code hoặc thiếu điểm xác nhận hành động rủi ro: ScriptInvalid.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Script version bất biến có schema/hash; chưa mặc nhiên tương thích production. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-34, BR-35. **Dữ liệu:** ScriptVersion, ScriptStep, MappingVersion. **Yêu cầu:** FR-091, FR-092, FR-093.

**Ví dụ và oracle:** SCRIPT-01 bước nhập SoVanBan lấy từ metadata Confirmed, không hardcode hồ sơ thật. **Kiểm thử liên quan:** TC-F-091, TC-F-092, TC-F-093, đều Chưa thực hiện.

### UC-032 Chạy kịch bản có điều kiện

**Mục tiêu:** Mỗi bước có kết quả; Success chỉ khi postconditions được kiểm tra.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng chạy kịch bản có điều kiện, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Script/profile được thử, đúng Windows session, quyền hợp lệ.

**Dữ liệu đầu vào:** ScriptVersionId, snapshot dữ liệu, run mode, idempotency key.

**Luồng thành công:**

1. Kiểm tra executable/window fingerprint và DPI.
2. Khóa runner và resolve biến.
3. Chờ selector đúng/duy nhất.
4. Kiểm tra focus trước SendInput.
5. Ghi ý định trước hành động có tác dụng phụ.
6. Thực hiện và kiểm tra postcondition.
7. Ghi bước thành công hoặc pause/fail.

**Luồng thay thế A1:** Chạy từng bước: sau mỗi bước chờ người dùng; fallback tọa độ chỉ khi profile đã cho phép.

**Ngoại lệ E1:** Sai cửa sổ, nhiều match, DPI đổi hoặc UIPI: dừng; không nhấp mù hay tự nâng quyền.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Mỗi bước có kết quả; Success chỉ khi postconditions được kiểm tra. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-20, BR-35. **Dữ liệu:** RpaRun, RpaStepRun, Job, ScriptVersion, ScriptStep, ConfigVersion. **Yêu cầu:** FR-094, FR-095, FR-096.

**Ví dụ và oracle:** RPA-RUN-01 cửa sổ đổi chỗ: UIA vẫn đúng; coordinate profile sai DPI phải dừng. **Kiểm thử liên quan:** TC-F-094, TC-F-095, TC-F-096, đều Chưa thực hiện.

### UC-033 Tạm dừng hủy và đối soát kết quả chưa rõ

**Mục tiêu:** Không tự retry tác dụng phụ Unknown; Cancel không giả rollback hệ thống ngoài.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tạm dừng hủy và đối soát kết quả chưa rõ, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Run đang chạy hoặc chờ người dùng; có quyền session.

**Dữ liệu đầu vào:** RunId, Pause/Resume/Cancel hoặc kết quả đối soát.

**Luồng thành công:**

1. Nhận lệnh dừng ưu tiên.
2. Ngừng phát thêm input và nhả phím đang giữ.
3. Ghi checkpoint và bước đang chờ.
4. Nếu side effect chưa xác định: NeedsReconciliation.
5. Người dùng kiểm tra hệ thống đích.
6. Chỉ Resume khi precondition còn đúng.

**Luồng thay thế A1:** Resume sau mất focus: tìm lại đích và revalidate; không dùng handle cũ mặc định.

**Ngoại lệ E1:** Mất điện giữa save: khi mở lại đưa run sang Interrupted/NeedsReconciliation, không Succeeded.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Không tự retry tác dụng phụ Unknown; Cancel không giả rollback hệ thống ngoài. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-13, BR-21. **Dữ liệu:** RpaRun, RpaStepRun, ExternalEntryAttempt. **Yêu cầu:** FR-097, FR-098, FR-099.

**Ví dụ và oracle:** RPA-STOP-01 hủy ngay sau lưu; giữ trạng thái chưa rõ, không xóa bản ghi đích. **Kiểm thử liên quan:** TC-F-097, TC-F-098, TC-F-099, đều Chưa thực hiện.

### UC-034 Khóa cấu hình và công cụ

**Mục tiêu:** Config có lịch sử; không tự cập nhật qua Internet.

**Tác nhân chính:** ACT-02 hoặc ACT-01 được cấp quyền tương ứng. **Kích hoạt:** người dùng chọn chức năng khóa cấu hình và công cụ, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Chủ kho hoặc người quản lý được ACL cấp quyền.

**Dữ liệu đầu vào:** Profile A/B, đường dẫn/hash công cụ, ngưỡng, policy version.

**Luồng thành công:**

1. Chọn đường dẫn đúng loại công cụ.
2. Tính hash theo luồng và kiểm tra bản phụ thuộc.
3. Chạy self-test bằng dữ liệu giả.
4. Lưu config version và manifest.
5. Dừng tác vụ đang dùng cấu hình cũ trước áp dụng.
6. Đánh dấu đối tượng phụ thuộc cần xem lại.

**Luồng thay thế A1:** Không đủ công cụ: Disabled/NotConfigured, có manual fallback.

**Ngoại lệ E1:** Hash thay đổi hoặc binary không tương thích: chặn worker, không tự chấp nhận hash mới.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Config có lịch sử; không tự cập nhật qua Internet. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-02, BR-36. **Dữ liệu:** ConfigVersion, ToolVersion, MappingVersion, DirectoryEntryVersion. **Yêu cầu:** FR-100, FR-101, FR-102.

**Ví dụ và oracle:** CFG-01 thay tesseract.exe sau khóa hash: ToolHashMismatch. **Kiểm thử liên quan:** TC-F-100, TC-F-101, TC-F-102, đều Chưa thực hiện.

### UC-035 Tra cứu nhật ký metadata

**Mục tiêu:** Không có prompt, password hoặc toàn văn trong log.

**Tác nhân chính:** ACT-02 hoặc ACT-01 được cấp quyền tương ứng. **Kích hoạt:** người dùng chọn chức năng tra cứu nhật ký metadata, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Có quyền đọc kho.

**Dữ liệu đầu vào:** Bộ lọc thời gian, SID, action, objectId, correlationId.

**Luồng thành công:**

1. Áp dụng bộ lọc có tham số.
2. Hiển thị sự kiện và kết quả.
3. Mở liên kết object/revision nếu được quyền.
4. Xuất metadata log theo phạm vi cho phép.

**Luồng thay thế A1:** Object archive: vẫn xem được event liên quan.

**Ngoại lệ E1:** Yêu cầu sửa/xóa sự kiện từ UI: không hỗ trợ; thiếu quyền thì AccessDenied.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Không có prompt, password hoặc toàn văn trong log. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-03, BR-37. **Dữ liệu:** AuditEvent, Workspace. **Yêu cầu:** FR-103, FR-104, FR-105.

**Ví dụ và oracle:** LOG-01 tra runId chỉ ra bước lỗi, không xuất nội dung ô mật khẩu. **Kiểm thử liên quan:** TC-F-103, TC-F-104, TC-F-105, đều Chưa thực hiện.

### UC-036 Sao lưu snapshot đầy đủ

**Mục tiêu:** Backup gồm DB và object, chưa được coi là phục hồi thành công nếu chưa drill.

**Tác nhân chính:** ACT-02 hoặc ACT-01 được cấp quyền tương ứng. **Kích hoạt:** người dùng chọn chức năng sao lưu snapshot đầy đủ, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Đích ngoài kho, dung lượng đủ và được phép; không còn writer job.

**Dữ liệu đầu vào:** Kho, đích, backup policy version.

**Luồng thành công:**

1. Đóng nhận job ghi và chờ/huỷ worker.
2. Chụp SQLite bằng backup API.
3. Liệt kê object được snapshot tham chiếu.
4. Chép và verify hash theo luồng.
5. Tạo manifest, verify FK/integrity bản sao.
6. Đổi tên backup staging.
7. Ghi BackupRecord và mở lại queue.

**Luồng thay thế A1:** Người dùng hủy trước hoàn tất: đánh dấu Failed/Cancelled, không dùng gói dở.

**Ngoại lệ E1:** Thiếu object/disk full: không tạo trạng thái Verified; nguồn đang chạy được giữ.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Backup gồm DB và object, chưa được coi là phục hồi thành công nếu chưa drill. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-05, BR-38. **Dữ liệu:** BackupRecord, FileObject, Job. **Yêu cầu:** FR-106, FR-107, FR-108.

**Ví dụ và oracle:** BKP-01 DB WAL đang hoạt động vẫn chụp bằng API, không copy nóng riêng tệp DB. **Kiểm thử liên quan:** TC-F-106, TC-F-107, TC-F-108, đều Chưa thực hiện.

### UC-037 Phục hồi vào kho mới và diễn tập

**Mục tiêu:** Kho cũ giữ nguyên; kết quả drill có bằng chứng.

**Tác nhân chính:** ACT-02 hoặc ACT-01 được cấp quyền tương ứng. **Kích hoạt:** người dùng chọn chức năng phục hồi vào kho mới và diễn tập, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Manifest backup hỗ trợ; đích mới; người vận hành được phép.

**Dữ liệu đầu vào:** BackupId, new path, expected owner SID, policy di chuyển nếu có.

**Luồng thành công:**

1. Kiểm tra path/size/hash/symlink.
2. Chép vào staging kho mới.
3. Kiểm tra integrity/FK/schema/object refs.
4. Kiểm tra owner SID, không tự đổi SID.
5. Mở thử nguồn và báo cáo.
6. Ghi kết quả drill.
7. Chỉ chọn kho mới sau xác nhận.

**Luồng thay thế A1:** Khác tài khoản/máy: dừng chờ quy trình chuyển chủ được duyệt.

**Ngoại lệ E1:** Manifest sai hoặc thiếu object: từ chối, xóa staging do tác vụ tạo; không tác động kho hiện hành.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Kho cũ giữ nguyên; kết quả drill có bằng chứng. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-38, BR-39. **Dữ liệu:** BackupRecord, Workspace, FileObject. **Yêu cầu:** FR-109, FR-110, FR-111.

**Ví dụ và oracle:** REST-01 sửa 1 byte object trong backup: restore thất bại, kho cũ không đổi. **Kiểm thử liên quan:** TC-F-109, TC-F-110, TC-F-111, đều Chưa thực hiện.

### UC-038 Tích hợp AI tùy chọn và kiểm tra đầu ra

**Mục tiêu:** Không có AI vẫn dùng được toàn luồng lõi; đầu ra AI chỉ Proposed.

**Tác nhân chính:** ACT-01. **Kích hoạt:** người dùng chọn chức năng tích hợp ai tùy chọn và kiểm tra đầu ra, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Lõi chạy được khi AI Disabled; chỉ bật khi công cụ/model được chấp thuận.

**Dữ liệu đầu vào:** Task kind, selected evidence, config/model hash và giới hạn token.

**Luồng thành công:**

1. Nếu Disabled trả NotConfigured ngay.
2. Nếu bật xác minh model/worker chỉ loopback.
3. Tokenize với tokenizer cùng model và giới hạn ngân sách.
4. Gọi model không tool/shell.
5. Kiểm tra JSON, IDs và trạng thái hoàn tất.
6. Lưu đề xuất cần con người rà soát, không auto-confirm.

**Luồng thay thế A1:** Model chưa cài: hướng dẫn manual, không tải tự động hoặc hiện lỗi chặn chương trình.

**Ngoại lệ E1:** Timeout/quá budget/ID ngoài tập: không lưu như câu trả lời hợp lệ.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Không có AI vẫn dùng được toàn luồng lõi; đầu ra AI chỉ Proposed. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-03, BR-40. **Dữ liệu:** AiInvocation, EvidenceRef, ConfigVersion. **Yêu cầu:** FR-112, FR-113, FR-114.

**Ví dụ và oracle:** AI-OFF-01 xóa model: tạo báo cáo và chạy RPA vẫn được; không có request mạng AI. **Kiểm thử liên quan:** TC-F-112, TC-F-113, TC-F-114, đều Chưa thực hiện.

### UC-039 Quản lý danh mục và ánh xạ

**Mục tiêu:** Giữ tên/mã đúng snapshot; không thay lịch sử báo cáo khi đổi danh mục.

**Tác nhân chính:** ACT-02 hoặc ACT-01 được cấp quyền tương ứng. **Kích hoạt:** người dùng chọn chức năng quản lý danh mục và ánh xạ, không tự chạy nền ngoài policy.

**Tiền điều kiện:** Người quản lý có quyền; danh mục đang tham chiếu được bảo toàn.

**Dữ liệu đầu vào:** Đơn vị/người/chủ đề/loại, mapping, effective date và expected version.

**Luồng thành công:**

1. Tạo/sửa bản danh mục.
2. Kiểm tra trùng mã và quan hệ cấp trên.
3. Chặn vòng cây tổ chức.
4. Publish version.
5. Ngừng hiệu lực thay vì xóa bản đã dùng.
6. Đánh dấu mapping/cấu hình liên quan cần kiểm tra.

**Luồng thay thế A1:** Đổi tên đơn vị: version mới với logicalId giữ nguyên.

**Ngoại lệ E1:** Mapping trỏ trường lạ hoặc dùng version không tồn tại: từ chối.

**Đảm bảo tối thiểu:** giữ bản nguồn/revision đã commit; không phát sinh trạng thái thành công giả. Nếu đã gửi tác dụng phụ ra hệ thống ngoài, không hứa rollback; giữ kết quả Unknown/NeedsReconciliation và yêu cầu đối soát.

**Hậu điều kiện thành công:** Giữ tên/mã đúng snapshot; không thay lịch sử báo cáo khi đổi danh mục. Audit lưu actor thực, root/version, code và correlationId; không sao chép toàn văn vào log.

**Quy tắc:** BR-18, BR-36. **Dữ liệu:** DirectoryEntryVersion, MappingVersion, ConfigVersion, AuditEvent. **Yêu cầu:** FR-115, FR-116, FR-117.

**Ví dụ và oracle:** DIR-01 đổi tên Đội A; báo cáo cũ vẫn giữ phiên bản tên tại thời điểm lập. **Kiểm thử liên quan:** TC-F-115, TC-F-116, TC-F-117, đều Chưa thực hiện.

## 4 Các chuỗi nghiệp vụ liên UC

- **Máy A:** UC-001 → UC-002/UC-003 → UC-004/UC-005 → UC-006 → UC-007 → UC-008 → UC-009 → UC-010. Thư/scan chỉ là nguồn; không tự coi trích xuất là đã kiểm tra.
- **Máy B:** UC-011 → UC-012/UC-013 → UC-014 → UC-015 → UC-009 → UC-016 → quyết định ngoài hệ thống → UC-017 → UC-018 → ký nhận thực tế → UC-019 → UC-020.
- **Báo cáo:** UC-021 → UC-023 → UC-024 → UC-025 → UC-026 → UC-027 → UC-028 → UC-029. Nếu không có số liệu, vẫn tạo nháp văn bản với chỗ thiếu rõ; không tạo tổng giả.
- **RPA:** UC-030 → UC-031 → kiểm thử sandbox → UC-032; UC-033 xử lý pause/cancel/unknown trước mọi resume/retry.
- **Vận hành:** UC-034/UC-039 cấu hình; UC-035 kiểm tra; UC-036 → UC-037 drill. UC-038 mở rộng không là dependency của các chuỗi trên.
