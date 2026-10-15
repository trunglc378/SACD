# SACD Chỉ mục và kiểm soát bộ tài liệu

**Mã tài liệu:** SACD-DOC-00  
**Phiên bản:** 1.1  
**Ngày:** 15/10/2026  
**Người biên soạn:** Đàm Quang Trung  
**Trạng thái:** Dự thảo chi tiết để rà soát; chưa phê duyệt baseline triển khai.

**Căn cứ:** Phiếu đăng ký và Report 1 SACD v1.0 trong `references/`. Các đặc tả bổ sung ở bản 1.1 là thiết kế đề xuất để hiện thực phạm vi đã nêu, không phải bằng chứng phần mềm đã có. AI mặc định tắt. Các mục DEC còn mở không được coi là quyết định đã được đơn vị chấp thuận.

## 1 Mục đích và tình trạng

Bộ tài liệu phục vụ khảo sát, thiết kế, xây dựng, kiểm thử, thí điểm và vận hành **SACD — Software for Automatically Compiling Documents** trên desktop Windows. Phạm vi gồm tiếp nhận văn bản, OCR/đối chiếu, xử lý hồ sơ qua hai máy cách ly, hỗ trợ QLVB, phiếu/quyết định/giao nhận, hạn xử lý, RPA, số liệu có kiểm chứng và báo cáo Word. AI cục bộ để ngỏ, mặc định tắt.

Đây là **baseline dự thảo thiết kế v1.1**, chưa là hồ sơ nghiệm thu hoặc tuyên bố phần mềm đã hiện thực đủ. Nội dung bổ sung làm rõ thiết kế cho 21 feature nguồn; tham số và chính sách còn mở có mã DEC. Cần người chủ nghiệp vụ và quản lý máy rà soát trước dùng dữ liệu thật.

## 2 Danh mục 19 tài liệu mới

Bộ mới gồm chỉ mục này và 18 tài liệu chuyên đề dưới đây.

| Mã | Tài liệu | Mục đích |
|---|---|---|
| 01 | [BRD và quy trình nghiệp vụ](01_SACD_BRD_Quy_trinh_nghiep_vu_v1.1.md) | BRD và quy trình nghiệp vụ |
| 02 | [SRS đặc tả yêu cầu phần mềm](02_SACD_SRS_Dac_ta_yeu_cau_v1.1.md) | SRS đặc tả yêu cầu phần mềm |
| 03 | [SDS đặc tả thiết kế phần mềm](03_SACD_SDS_Dac_ta_thiet_ke_v1.1.md) | SDS đặc tả thiết kế phần mềm |
| 04 | [Đặc tả Use Case](04_SACD_Use_Case_v1.1.md) | Đặc tả Use Case |
| 05 | [Mô hình dữ liệu ERD và từ điển dữ liệu](05_SACD_Du_lieu_ERD_v1.1.md) | Mô hình dữ liệu ERD và từ điển dữ liệu |
| 06 | [Hợp đồng tích hợp RPA USB Word và AI](06_SACD_Hop_dong_tich_hop_RPA_USB_Word_v1.1.md) | Hợp đồng tích hợp RPA USB Word và AI |
| 07 | [Ma trận truy vết và độ bao phủ thiết kế](07_SACD_Ma_tran_truy_vet_v1.1.md) | Ma trận truy vết và độ bao phủ thiết kế |
| 08 | [Đặc tả giao diện và tương tác](08_SACD_UX_UI_v1.1.md) | Đặc tả giao diện và tương tác |
| 09 | [Bảo mật mô hình đe dọa và quyền riêng tư](09_SACD_Bao_mat_Threat_Model_v1.1.md) | Bảo mật mô hình đe dọa và quyền riêng tư |
| 10 | [Chiến lược và ca kiểm thử](10_SACD_Kiem_thu_Test_Cases_v1.1.md) | Chiến lược và ca kiểm thử |
| 11 | [Kế hoạch UAT và tiêu chí nghiệm thu](11_SACD_UAT_Nghiem_thu_v1.1.md) | Kế hoạch UAT và tiêu chí nghiệm thu |
| 12 | [Kế hoạch backlog nguồn lực và rủi ro](12_SACD_Ke_hoach_Backlog_Rui_ro_RACI_v1.1.md) | Kế hoạch backlog nguồn lực và rủi ro |
| 13 | [Quản lý cấu hình thay đổi và phát hành](13_SACD_Cau_hinh_Thay_doi_Phat_hanh_v1.1.md) | Quản lý cấu hình thay đổi và phát hành |
| 14 | [Cài đặt triển khai và chuyển đổi dữ liệu](14_SACD_Cai_dat_Trien_khai_v1.1.md) | Cài đặt triển khai và chuyển đổi dữ liệu |
| 15 | [Hướng dẫn người dùng](15_SACD_Huong_dan_nguoi_dung_v1.1.md) | Hướng dẫn người dùng |
| 16 | [Vận hành sao lưu phục hồi và sự cố](16_SACD_Van_hanh_Sao_luu_Su_co_v1.1.md) | Vận hành sao lưu phục hồi và sự cố |
| 17 | [ADR quyết định kiến trúc và vấn đề mở](17_SACD_ADR_Quyet_dinh_Cau_hoi_v1.1.md) | ADR quyết định kiến trúc và vấn đề mở |
| 18 | [Biểu mẫu bằng chứng và đánh giá sẵn sàng](18_SACD_Bang_chung_kiem_thu_Readiness_v1.1.md) | Biểu mẫu bằng chứng và đánh giá sẵn sàng |

Hai Markdown nguồn v1.0 giữ nguyên trong `references/`, phục vụ đối chiếu khung và phạm vi, không tự thay trạng thái dự thảo v1.1.

## 3 Cách đọc theo công việc

- Nghiệp vụ/chủ đề tài: BRD -> SRS -> Use Case -> UX -> UAT -> DEC.
- Phát triển: SRS/UC -> SDS -> dữ liệu/ERD -> contracts -> RTM -> test và release.
- Quản lý dự án: backlog/RACI/risk -> config/CR -> readiness -> UAT/bàn giao.
- Người sử dụng/vận hành: hướng dẫn người dùng -> cài đặt -> runbook -> bảo mật/sự cố.

## 4 Quy ước mã và số lượng thiết kế

| Loại | Số lượng | Ý nghĩa |
|---|---|---|
| Feature | 21 | FE-01..21; FE-20 là mở rộng tùy chọn |
| Use Case | 39 | UC-001..039; gồm chính/thay thế/lỗi |
| Functional requirement | 117 | FR-001..117, mỗi yêu cầu có TC |
| Nonfunctional requirement | 24 | NFR-01..24; ngưỡng đề xuất phải chốt DEC |
| Business rule | 40 | BR-01..40 |
| Component | 18 | CMP-01..18 |
| Bảng nghiệp vụ | 50 | Không tính FTS virtual/shadow tables |
| Test case thiết kế | 141 | 117 TC-F + 24 TC-N; chưa chạy ứng dụng |
| UAT scenario | 17 | UAT-01..17; AI scenario riêng khi mở nhánh E |
| ADR / quyết định mở | 12 / 12 | Không coi ADR đề xuất là quyết định cơ quan |

C = core; “core có điều kiện tương thích/công cụ” vẫn trong phạm vi mục tiêu, cần đúng tool/target để nghiệm thu. E = AI extension, không chặn lõi. Mã feature và rule dùng xuyên suốt; RTM có chiều thuận/ngược. Số lượng tài liệu/yêu cầu không chứng minh chất lượng phần mềm.

## 5 Thuật ngữ

| Thuật ngữ | Nghĩa trong SACD |
|---|---|
| Hồ sơ / Dossier | Nhóm tài liệu và công việc cùng luồng xử lý, không đồng nghĩa một file |
| Revision | Phiên bản nội dung khóa lại; sửa nội dung tạo bản mới |
| Evidence | Tham chiếu đoạn nguồn có revision, vị trí và hash; không tự chứng minh diễn giải đúng |
| Fact | Số liệu/sự kiện có chỉ tiêu, kỳ, đơn vị, phạm vi và nguồn |
| Grain / coverage | Mức chi tiết của một fact / phần phạm vi mà số liệu bao phủ |
| Flow / stock / cumulative / ratio | Phát sinh trong kỳ / tồn tại thời điểm / lũy kế / tỷ lệ tử-mẫu |
| Claim | Nhận định trong báo cáo cần phân loại factual hoặc commentary |
| Stale | Đầu ra dùng snapshot vẫn còn nhưng nguồn/quy tắc liên quan đã đổi, cần rà soát |
| RPA / UIA | Tự động thao tác giao diện / nhận diện phần tử qua Windows UI Automation |
| Idempotency | Xử lý cùng yêu cầu logic mà không phát sinh lặp tại SACD; không bảo đảm exactly-once bên ngoài |
| Unknown / đối soát | Chưa biết side effect đã xảy ra; phải kiểm thực tế trước retry |
| Baseline / CR / ADR | Mốc phạm vi được kiểm soát / yêu cầu thay đổi / ghi nhận quyết định kiến trúc |

## 6 Kiểm soát thay đổi và review

Nguồn yêu cầu mới -> CR -> đánh giá BR/FR/UC/thiết kế/data/test/docs -> người có quyền chấp thuận -> cập nhật baseline và RTM. Không tái dùng ID đã loại. Phiên bản tài liệu, app, schema, package, template và script độc lập. Tên Đàm Quang Trung theo nguồn v1.0; người hướng dẫn, đơn vị, lịch và chữ ký chưa được cung cấp phải chốt DEC-11/12.

Review bắt buộc: chủ nghiệp vụ xác nhận flow/hạn/metric/authority; kỹ thuật xác nhận contracts/schema/recovery; quản lý máy xác nhận quyền/USB/offline/OS; QA xác nhận oracle và bằng chứng; người có quyền nghiệm thu xác nhận gates. Nếu kiêm nhiệm phải ghi người thực sự, không tự gán tên có trong mẫu.

## 7 Nguồn đóng băng và giới hạn bao phủ

| Tệp nguồn | Bytes | SHA256 |
|---|---|---|
| SACD_Phieu_dang_ky_de_tai_v1.0.md | 15676 | ce0fab4dd3b10882b34f60ae0757ac9af16f52a0021d3f52dfe20f5ad3843bc8 |
| SACD_Report1_Gioi_thieu_du_an_v1.0.md | 61966 | d7173f4e67b1f3197f0a546695576802fa66a725446b0fee6426de72010f4902 |

- [SACD_Phieu_dang_ky_de_tai_v1.0.md](references/SACD_Phieu_dang_ky_de_tai_v1.0.md)
- [SACD_Report1_Gioi_thieu_du_an_v1.0.md](references/SACD_Report1_Gioi_thieu_du_an_v1.0.md)

Chưa đối chiếu toàn văn liên kết ChatGPT riêng tư mà người dùng dẫn. Vì vậy không khẳng định đã bao phủ tuyệt đối mọi yêu cầu trong cuộc trò chuyện đó; DEC-01 theo dõi phần này. Bản hiện tại bám nguồn v1.0 và đoạn chat có thể truy xuất, không nhập các dữ kiện chưa xác minh như quy định cơ quan hoặc khả năng thực tế của máy.

## 8 Kiểm tra chất lượng tài liệu

DOC-QA-01 ngày 15/10/2026: 469/469 phép kiểm tra nhất quán tài liệu/DDL đạt; 27 liên kết tệp tương đối hợp lệ; 21 feature, 39 UC, 117 FR, 24 NFR và 40 BR không có tham chiếu mồ côi. Cả 50 bảng có footprint trong UC/RTM. Markdown parser đọc thành công 19 tài liệu mới với 107 bảng và 591 đề mục; JSON ví dụ hợp lệ. DDL tạo thành công trên SQLite có FTS5, integrity/FK sạch; các fixture sai khóa/phiên bản/cặp nguồn/hạn/PK bị chặn đúng. Hai tệp nguồn giữ nguyên bytes. Sơ đồ Mermaid được kiểm tra cấu trúc nguồn; chưa xác nhận hiển thị trong mọi trình đọc Markdown. Kết quả này không thay functional/integration/security/performance/RPA/Word/UAT test. Các TC và UAT vẫn NotRun cho toàn phạm vi 1.1 cho đến khi có build và bằng chứng thực tế.

## 9 Đề nghị chốt trước lập trình/thí điểm

Đối chiếu transcript/phạm vi, khảo sát hai máy và target QLVB/email, cung cấp mẫu chính thức và corpus được phép, chốt hạn/danh mục/chỉ tiêu/coverage, xác nhận phương tiện và quyền dữ liệu, người nghiệm thu và kế hoạch. Có thể xây nền tảng bằng dữ liệu giả trong lúc chờ các đầu vào; phần phụ thuộc chưa đủ phải thể hiện Blocked, không đoán quy tắc để chạy thật.
