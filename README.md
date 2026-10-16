# SACD — Software for Automatically Compiling Documents
**Phần mềm hỗ trợ tự động hóa quy trình xử lý văn bản và tổng hợp tài liệu**

[![.NET 8.0](https://img.shields.io/badge/.NET-8.0-blue.svg)](https://dotnet.microsoft.com/)
[![WPF](https://img.shields.io/badge/UI-WPF%20MVVM-green.svg)]()
[![SQLite](https://img.shields.io/badge/Database-SQLite%20FTS5-lightgrey.svg)]()
[![OpenXML](https://img.shields.io/badge/Word-OpenXML%20SDK-purple.svg)]()
[![Tests](https://img.shields.io/badge/Tests-23%20Passed-success.svg)]()

**Bản cập nhật luồng nghiệp vụ 1.2 (16/10/2026):** kết hợp Trợ lý văn thư với RPA hỗ trợ QLVB. Sau khi chọn kho Máy 1 (`Intake`) hoặc Máy 2 (`Management`), dùng tab **Quy trình văn thư** để tiếp nhận tệp thật, kiểm tra thông tin, ghi nhận in, chuyển USB, đối soát QLVB, tạo phiếu Word có QR, trình duyệt, giao và ký nhận. Xem [quy trình và hướng dẫn từng bước](docs/08_SACD_Quy_trinh_hop_nhat_v1.2.md).

**Phạm vi hiện tại:** TXT/DOCX, biểu mẫu thủ công, USB có metadata/checksum, theo dõi hạn và UIA với mapping. Email tự động, OCR scan/PDF thực, AI và mapping cho QLVB của đơn vị chưa được tích hợp. Tab RPA mô phỏng chỉ thử trạng thái. Các đặc tả v1.1 là mục tiêu thiết kế, không phải bằng chứng mọi chức năng đã hoàn thành.

---

## 1. Giới thiệu dự án

**SACD** là ứng dụng Windows Desktop độc lập, cách ly mạng (Air-gap), phục vụ cán bộ tiếp nhận, văn thư và cán bộ tổng hợp trong cơ quan nhà nước / đơn vị nghiệp vụ:
- Hoạt động độc lập giữa **Máy A (Tiếp nhận/Email)** và **Máy B (Quản lý văn bản/Tổng hợp)** thông qua gói ZIP và bảng kê SHA256 chuyển vật lý qua USB an toàn. Gói hiện chưa mã hóa hoặc ký số.
- Hỗ trợ tự động hóa thao tác nhập liệu vào phần mềm Quản lý văn bản (QLVB) hiện có qua kịch bản RPA có kiểm soát và điểm dừng can thiệp `PauseForUser`.
- Quản lý hạn xử lý văn bản và nhiệm vụ (Còn hạn, Sắp hết hạn, Quá hạn) theo thời gian thực.
- Tổng hợp số liệu nghiệp vụ chính xác (`Flow`, `Stock`, `Ratio`, `Cumulative`) không làm tròn trung gian.
- Xuất báo cáo tổng hợp Microsoft Word (`.docx`) bằng **Open XML SDK** với định dạng Bản nháp (Draft) hoặc Bản trình lãnh đạo chính thức (Submission).
- Tích hợp tìm kiếm toàn văn tiếng Việt có dấu và không dấu bằng **SQLite FTS5**.
- AI cục bộ mặc định tắt, giữ an toàn thông tin tuyệt đối.

---

## 2. Kiến trúc hệ thống (Clean Architecture & Modular Monolith)

Dự án tổ chức thành các tầng theo định hướng **SACD-DOC-03 (SDS v1.1)**; một số adapter còn ở mức prototype:

```
SACD/
├── src/
│   ├── SACD.Domain/            # Thực thể nghiệp vụ, State Machines, Invariants, BR-01..BR-40
│   ├── SACD.Application/       # 18 Component Services (CMP-01..18), DTOs, OperationResult<T>
│   ├── SACD.Infrastructure/    # SQLite 50 bảng + FTS5, FileStore, OpenXML, RPA, USB Transfer
│   └── SACD.Desktop/           # Giao diện WPF MVVM hiện đại (Views, ViewModels, RelayCommand)
├── tests/
│   └── SACD.Tests/             # Bộ kiểm thử xUnit: DDL, Invariants, Word, FTS5, RPA, Concurrency
├── docs/                       # Toàn bộ tài liệu đặc tả SRS, SDS, ERD, Use Case, Traceability
├── SACD.sln                    # Solution file
└── README.md
```

### Danh mục 18 Thành phần cốt lõi (CMP-01 đến CMP-18)

| Mã CMP | Tên thành phần | Trách nhiệm chính |
|---|---|---|
| **CMP-01** | `WorkspaceService` | Mở/khởi tạo kho, kiểm tra SID hệ điều hành, khóa độc quyền 1 phiên ghi |
| **CMP-02** | `IntakeService` | Tiếp nhận thư nội bộ, nhập tệp PDF/DOCX/TXT và lưu trữ bản gốc bất biến |
| **CMP-03** | `TextService` | Trích xuất văn bản, xử lý OCR theo trang, đối chiếu và gắn nhãn Reviewed |
| **CMP-04** | `MetadataService` | Xác nhận số hiệu, ngày, trích yếu, phân loại thông tin mật/giải mật |
| **CMP-05** | `PrintService` | Xem trước, gửi lệnh in và xác nhận bản in có căn cứ |
| **CMP-06** | `TransferService` | Đóng gói USB với `manifest.json`, kiểm tra mã băm SHA256 khi nhận gói |
| **CMP-07** | `WorkflowService` | Tạo nhiệm vụ, tính toán cảnh báo thời hạn và ghi nhận hoàn thành |
| **CMP-08** | `ExternalEntryService` | Điền dữ liệu vào QLVB, đối soát và xử lý ngoại lệ `NeedsReconciliation` |
| **CMP-09** | `ProposalService` | Lập phiếu đề xuất xử lý, trình duyệt và cập nhật quyết định chỉ đạo |
| **CMP-10** | `DeliveryService` | Giao văn bản cho đơn vị/cán bộ và ghi nhận ký nhận có căn cứ chứng từ |
| **CMP-11** | `SearchService` | Tra cứu toàn văn FTS5 (có/không dấu), xem lịch sử và lưu trữ logic |
| **CMP-12** | `FactService` | Xác nhận số liệu, giải quyết trùng lặp/mâu thuẫn, tổng hợp có kiểm chứng |
| **CMP-13** | `ReportService` | Quản lý mẫu Word, kiểm tra claim/nguồn, xuất DOCX nháp hoặc bản trình |
| **CMP-14** | `RecorderService` | Ghi thao tác chuột/bàn phím có chủ đích và biên tập kịch bản |
| **CMP-15** | `RunnerService` | Thực thi kịch bản RPA với điểm dừng `PauseForUser` và hủy an toàn |
| **CMP-16** | `OperationsService` | Khóa phiên bản cấu hình/công cụ, sao lưu snapshot và diễn tập phục hồi |
| **CMP-17** | `AiAssistant` | Điểm mở rộng AI cục bộ (Mặc định tắt theo chính sách bảo mật) |
| **CMP-18** | `DirectoryService` | Quản lý danh mục đơn vị, lãnh đạo, cán bộ và quy tắc ánh xạ |

---

## 3. Cơ sở dữ liệu SQLite (50 bảng nghiệp vụ chuẩn v1.1)

Mô hình dữ liệu tham chiếu **SACD-DOC-05 (ERD v1.1)** với `PRAGMA foreign_keys=ON;` và chỉ mục FTS5. Bản 1.2 bổ sung `CorrespondenceCase`, `CorrespondenceCaseEvent`, `CorrespondenceCaseImport` cho luồng hợp nhất; tổng cộng 53 bảng nghiệp vụ, chưa tính FTS/shadow:
- **Nhóm nguồn & chứng cứ:** `Workspace`, `FileObject`, `Dossier`, `Document`, `TextRevision`, `TextBlock`, `EvidenceRef`, `MetadataVersion`, `ClassificationRecord`.
- **Nhóm quy trình & giao nhận:** `TaskItem`, `TaskEvent`, `WorkflowEvent`, `TemplateVersion`, `ProposalVersion`, `DecisionRecord`, `DeliveryRecord`, `ReceiptRecord`.
- **Nhóm số liệu & báo cáo:** `MetricDefinitionVersion`, `FactVersion`, `FactEvidence`, `DedupDecisionVersion`, `DedupMember`, `AggregationRun`, `AggregationContribution`, `Report`, `ReportRevision`, `ReportSection`, `Claim`, `ClaimEvidence`, `ReportSource`, `ReportAggregation`, `ExportRecord`.
- **Nhóm kịch bản & RPA:** `Script`, `ScriptVersion`, `ScriptStep`, `RpaRun`, `RpaStepRun`, `MappingVersion`, `ExternalEntryAttempt`, `PrintAttempt`.
- **Nhóm vận hành & chuyển máy:** `TransferPackage`, `TransferItem`, `ImportMap`, `ToolVersion`, `ConfigVersion`, `Job`, `AuditEvent`, `BackupRecord`, `AiInvocation`.
- **Chỉ mục FTS5:** Bảng ảo `TextSearch` hỗ trợ tìm kiếm tiếng Việt toàn văn không phân biệt dấu.

---

## 4. Hướng dẫn cài đặt và chạy ứng dụng

### Yêu cầu môi trường
- Hệ điều hành: Windows 10 / Windows 11 (64-bit).
- .NET 8.0 SDK trở lên.

### Lệnh biên dịch và chạy
```powershell
# Di chuyển vào thư mục dự án
cd D:\Project\SACD

# Restore và build toàn bộ Solution
dotnet build SACD.sln

# Chạy toàn bộ 23 ca kiểm thử tự động xUnit
dotnet test SACD.sln

# Chạy ứng dụng giao diện WPF
dotnet run --project src/SACD.Desktop/SACD.Desktop.csproj

# Smoke test giao diện và RPA trên biểu mẫu giả lập (Windows)
dotnet run --project tests/SACD.Desktop.Smoke -- D:\Project\SACD\TestResults\workflow-ui.png
```

---

## 5. Bản quyền và Người thực hiện
- **Tác giả:** Đàm Quang Trung
- **Đồ án:** SACD Capstone Project (Lào Cai, 10/2026)
