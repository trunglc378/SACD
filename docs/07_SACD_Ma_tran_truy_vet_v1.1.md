> Cập nhật 20/10/2026: [bản sửa, chạy thử và giới hạn](09_SACD_Ban_sua_va_chay_thu_2026-10-20.md). Các yêu cầu bên dưới là baseline thiết kế, không tự đồng nghĩa đã hiện thực.

# SACD Ma trận truy vết và độ bao phủ thiết kế

> Bổ sung bằng chứng triển khai 16/10/2026: `CorrespondenceWorkflowTests` kiểm tra luồng hai máy, trạng thái, USB, rowVersion, vai trò, revision/block, DOCX/QR, script version, báo cáo nháp/snapshot và hạn hoàn thành. `SACD.Desktop.Smoke` kiểm tra thao tác WPF và adapter UIA trên biểu mẫu giả lập. Tổng xUnit: 23 ca đạt trên máy phát triển. Kết quả này chưa ánh xạ đủ từng FR/NFR, chưa thay UAT hay thử QLVB/email/máy in của đơn vị. Phạm vi và hướng dẫn: [v1.2](08_SACD_Quy_trinh_hop_nhat_v1.2.md).

**Mã tài liệu:** SACD-DOC-07  
**Phiên bản:** 1.1  
**Ngày:** 15/10/2026  
**Người biên soạn:** Đàm Quang Trung  
**Trạng thái:** Dự thảo chi tiết để rà soát; chưa phê duyệt baseline triển khai.

**Căn cứ:** Phiếu đăng ký và Report 1 SACD v1.0 trong `references/`. Các đặc tả bổ sung ở bản 1.1 là thiết kế đề xuất để hiện thực phạm vi đã nêu, không phải bằng chứng phần mềm đã có. AI mặc định tắt. Các mục DEC còn mở không được coi là quyết định đã được đơn vị chấp thuận.

## 1 Quy ước đọc và trạng thái

Chuỗi truy vết: **FE → FR → UC → BR → CMP → bảng dữ liệu → TC**. Mỗi FR có đúng một TC chính để quản lý, còn TC có thể chứa nhánh biên; test tự động hóa sâu hơn có thể tách UT/IT mà vẫn giữ mapping. Mã C: lõi mục tiêu, E: AI mở rộng. “Đã ánh xạ” chỉ là độ bao phủ thiết kế, không phải kiểm thử đạt.

Bảng này sinh cùng registry với SRS/UC để tránh gán nhầm số; vẫn cần review nghiệp vụ để xác nhận ý nghĩa. Mã của SRS cũ không được dùng làm bằng chứng đạt ở baseline mới.

## 2 Truy vết chức năng

| FR | FE | UC | BR | CMP | Bảng liên quan | Test chính | Pha | Thực thi |
|---|---|---|---|---|---|---|---|---|
| FR-001 | FE-01 | UC-001 | BR-01, BR-02 | CMP-01 | Workspace, AuditEvent, Job | TC-F-001 | C | Chưa thực hiện |
| FR-002 | FE-01 | UC-001 | BR-01, BR-02 | CMP-01 | Workspace, AuditEvent, Job | TC-F-002 | C | Chưa thực hiện |
| FR-003 | FE-01 | UC-001 | BR-01, BR-02 | CMP-01 | Workspace, AuditEvent, Job | TC-F-003 | C | Chưa thực hiện |
| FR-004 | FE-02 | UC-002 | BR-03, BR-04 | CMP-02 | Dossier, Document, ExternalEntryAttempt, AuditEvent, FileObject | TC-F-004 | C | Chưa thực hiện |
| FR-005 | FE-02 | UC-002 | BR-03, BR-04 | CMP-02 | Dossier, Document, ExternalEntryAttempt, AuditEvent, FileObject | TC-F-005 | C | Chưa thực hiện |
| FR-006 | FE-02 | UC-002 | BR-03, BR-04 | CMP-02 | Dossier, Document, ExternalEntryAttempt, AuditEvent, FileObject | TC-F-006 | C | Chưa thực hiện |
| FR-007 | FE-03 | UC-003 | BR-05, BR-06 | CMP-02 | FileObject, Dossier, Document, Job, AuditEvent | TC-F-007 | C | Chưa thực hiện |
| FR-008 | FE-03 | UC-003 | BR-05, BR-06 | CMP-02 | FileObject, Dossier, Document, Job, AuditEvent | TC-F-008 | C | Chưa thực hiện |
| FR-009 | FE-03 | UC-003 | BR-05, BR-06 | CMP-02 | FileObject, Dossier, Document, Job, AuditEvent | TC-F-009 | C | Chưa thực hiện |
| FR-010 | FE-04 | UC-004 | BR-06, BR-07 | CMP-03 | Document, TextRevision, TextBlock, Job, ToolVersion | TC-F-010 | C | Chưa thực hiện |
| FR-011 | FE-04 | UC-004 | BR-06, BR-07 | CMP-03 | Document, TextRevision, TextBlock, Job, ToolVersion | TC-F-011 | C | Chưa thực hiện |
| FR-012 | FE-04 | UC-004 | BR-06, BR-07 | CMP-03 | Document, TextRevision, TextBlock, Job, ToolVersion | TC-F-012 | C | Chưa thực hiện |
| FR-013 | FE-04 | UC-005 | BR-07, BR-08 | CMP-03 | TextRevision, TextBlock, Job, ToolVersion, Document | TC-F-013 | C | Chưa thực hiện |
| FR-014 | FE-04 | UC-005 | BR-07, BR-08 | CMP-03 | TextRevision, TextBlock, Job, ToolVersion, Document | TC-F-014 | C | Chưa thực hiện |
| FR-015 | FE-04 | UC-005 | BR-07, BR-08 | CMP-03 | TextRevision, TextBlock, Job, ToolVersion, Document | TC-F-015 | C | Chưa thực hiện |
| FR-016 | FE-04 | UC-006 | BR-07, BR-09 | CMP-03 | TextRevision, TextBlock, EvidenceRef, AuditEvent, ReportSource | TC-F-016 | C | Chưa thực hiện |
| FR-017 | FE-04 | UC-006 | BR-07, BR-09 | CMP-03 | TextRevision, TextBlock, EvidenceRef, AuditEvent, ReportSource | TC-F-017 | C | Chưa thực hiện |
| FR-018 | FE-04 | UC-006 | BR-07, BR-09 | CMP-03 | TextRevision, TextBlock, EvidenceRef, AuditEvent, ReportSource | TC-F-018 | C | Chưa thực hiện |
| FR-019 | FE-05 | UC-007 | BR-09, BR-10 | CMP-04 | MetadataVersion, EvidenceRef, TaskItem, TextRevision | TC-F-019 | C | Chưa thực hiện |
| FR-020 | FE-05 | UC-007 | BR-09, BR-10 | CMP-04 | MetadataVersion, EvidenceRef, TaskItem, TextRevision | TC-F-020 | C | Chưa thực hiện |
| FR-021 | FE-05 | UC-007 | BR-09, BR-10 | CMP-04 | MetadataVersion, EvidenceRef, TaskItem, TextRevision | TC-F-021 | C | Chưa thực hiện |
| FR-022 | FE-06 | UC-008 | BR-10, BR-11 | CMP-04 | ClassificationRecord, DirectoryEntryVersion, EvidenceRef | TC-F-022 | C | Chưa thực hiện |
| FR-023 | FE-06 | UC-008 | BR-10, BR-11 | CMP-04 | ClassificationRecord, DirectoryEntryVersion, EvidenceRef | TC-F-023 | C | Chưa thực hiện |
| FR-024 | FE-06 | UC-008 | BR-10, BR-11 | CMP-04 | ClassificationRecord, DirectoryEntryVersion, EvidenceRef | TC-F-024 | C | Chưa thực hiện |
| FR-025 | FE-07 | UC-009 | BR-12, BR-13 | CMP-05 | PrintAttempt, FileObject, TemplateVersion | TC-F-025 | C | Chưa thực hiện |
| FR-026 | FE-07 | UC-009 | BR-12, BR-13 | CMP-05 | PrintAttempt, FileObject, TemplateVersion | TC-F-026 | C | Chưa thực hiện |
| FR-027 | FE-07 | UC-009 | BR-12, BR-13 | CMP-05 | PrintAttempt, FileObject, TemplateVersion | TC-F-027 | C | Chưa thực hiện |
| FR-028 | FE-08 | UC-010 | BR-14, BR-15 | CMP-06 | TransferPackage, TransferItem, FileObject | TC-F-028 | C | Chưa thực hiện |
| FR-029 | FE-08 | UC-010 | BR-14, BR-15 | CMP-06 | TransferPackage, TransferItem, FileObject | TC-F-029 | C | Chưa thực hiện |
| FR-030 | FE-08 | UC-010 | BR-14, BR-15 | CMP-06 | TransferPackage, TransferItem, FileObject | TC-F-030 | C | Chưa thực hiện |
| FR-031 | FE-08 | UC-011 | BR-14, BR-16 | CMP-06 | TransferPackage, TransferItem, ImportMap, Dossier, FileObject | TC-F-031 | C | Chưa thực hiện |
| FR-032 | FE-08 | UC-011 | BR-14, BR-16 | CMP-06 | TransferPackage, TransferItem, ImportMap, Dossier, FileObject | TC-F-032 | C | Chưa thực hiện |
| FR-033 | FE-08 | UC-011 | BR-14, BR-16 | CMP-06 | TransferPackage, TransferItem, ImportMap, Dossier, FileObject | TC-F-033 | C | Chưa thực hiện |
| FR-034 | FE-09 | UC-012 | BR-17, BR-18 | CMP-07 | TaskItem, TaskEvent, Dossier, DirectoryEntryVersion | TC-F-034 | C | Chưa thực hiện |
| FR-035 | FE-09 | UC-012 | BR-17, BR-18 | CMP-07 | TaskItem, TaskEvent, Dossier, DirectoryEntryVersion | TC-F-035 | C | Chưa thực hiện |
| FR-036 | FE-09 | UC-012 | BR-17, BR-18 | CMP-07 | TaskItem, TaskEvent, Dossier, DirectoryEntryVersion | TC-F-036 | C | Chưa thực hiện |
| FR-037 | FE-09 | UC-013 | BR-17, BR-19 | CMP-07 | TaskItem, TaskEvent, ConfigVersion, AuditEvent | TC-F-037 | C | Chưa thực hiện |
| FR-038 | FE-09 | UC-013 | BR-17, BR-19 | CMP-07 | TaskItem, TaskEvent, ConfigVersion, AuditEvent | TC-F-038 | C | Chưa thực hiện |
| FR-039 | FE-09 | UC-013 | BR-17, BR-19 | CMP-07 | TaskItem, TaskEvent, ConfigVersion, AuditEvent | TC-F-039 | C | Chưa thực hiện |
| FR-040 | FE-10 | UC-014 | BR-20, BR-21 | CMP-08 | ExternalEntryAttempt, RpaRun, MappingVersion, RpaStepRun, ConfigVersion | TC-F-040 | C | Chưa thực hiện |
| FR-041 | FE-10 | UC-014 | BR-20, BR-21 | CMP-08 | ExternalEntryAttempt, RpaRun, MappingVersion, RpaStepRun, ConfigVersion | TC-F-041 | C | Chưa thực hiện |
| FR-042 | FE-10 | UC-014 | BR-20, BR-21 | CMP-08 | ExternalEntryAttempt, RpaRun, MappingVersion, RpaStepRun, ConfigVersion | TC-F-042 | C | Chưa thực hiện |
| FR-043 | FE-11 | UC-015 | BR-18, BR-22 | CMP-09 | ProposalVersion, TaskItem, DirectoryEntryVersion, TemplateVersion, EvidenceRef | TC-F-043 | C | Chưa thực hiện |
| FR-044 | FE-11 | UC-015 | BR-18, BR-22 | CMP-09 | ProposalVersion, TaskItem, DirectoryEntryVersion, TemplateVersion, EvidenceRef | TC-F-044 | C | Chưa thực hiện |
| FR-045 | FE-11 | UC-015 | BR-18, BR-22 | CMP-09 | ProposalVersion, TaskItem, DirectoryEntryVersion, TemplateVersion, EvidenceRef | TC-F-045 | C | Chưa thực hiện |
| FR-046 | FE-12 | UC-016 | BR-12, BR-22 | CMP-09 | ProposalVersion, WorkflowEvent, PrintAttempt, AuditEvent | TC-F-046 | C | Chưa thực hiện |
| FR-047 | FE-12 | UC-016 | BR-12, BR-22 | CMP-09 | ProposalVersion, WorkflowEvent, PrintAttempt, AuditEvent | TC-F-047 | C | Chưa thực hiện |
| FR-048 | FE-12 | UC-016 | BR-12, BR-22 | CMP-09 | ProposalVersion, WorkflowEvent, PrintAttempt, AuditEvent | TC-F-048 | C | Chưa thực hiện |
| FR-049 | FE-12 | UC-017 | BR-11, BR-22 | CMP-09 | DecisionRecord, WorkflowEvent, ProposalVersion, EvidenceRef, DirectoryEntryVersion | TC-F-049 | C | Chưa thực hiện |
| FR-050 | FE-12 | UC-017 | BR-11, BR-22 | CMP-09 | DecisionRecord, WorkflowEvent, ProposalVersion, EvidenceRef, DirectoryEntryVersion | TC-F-050 | C | Chưa thực hiện |
| FR-051 | FE-12 | UC-017 | BR-11, BR-22 | CMP-09 | DecisionRecord, WorkflowEvent, ProposalVersion, EvidenceRef, DirectoryEntryVersion | TC-F-051 | C | Chưa thực hiện |
| FR-052 | FE-13 | UC-018 | BR-23, BR-24 | CMP-10 | DeliveryRecord, DecisionRecord, TaskItem, Document, DirectoryEntryVersion | TC-F-052 | C | Chưa thực hiện |
| FR-053 | FE-13 | UC-018 | BR-23, BR-24 | CMP-10 | DeliveryRecord, DecisionRecord, TaskItem, Document, DirectoryEntryVersion | TC-F-053 | C | Chưa thực hiện |
| FR-054 | FE-13 | UC-018 | BR-23, BR-24 | CMP-10 | DeliveryRecord, DecisionRecord, TaskItem, Document, DirectoryEntryVersion | TC-F-054 | C | Chưa thực hiện |
| FR-055 | FE-13 | UC-019 | BR-23, BR-24 | CMP-10 | ReceiptRecord, DeliveryRecord, WorkflowEvent, EvidenceRef | TC-F-055 | C | Chưa thực hiện |
| FR-056 | FE-13 | UC-019 | BR-23, BR-24 | CMP-10 | ReceiptRecord, DeliveryRecord, WorkflowEvent, EvidenceRef | TC-F-056 | C | Chưa thực hiện |
| FR-057 | FE-13 | UC-019 | BR-23, BR-24 | CMP-10 | ReceiptRecord, DeliveryRecord, WorkflowEvent, EvidenceRef | TC-F-057 | C | Chưa thực hiện |
| FR-058 | FE-13 | UC-020 | BR-17, BR-24 | CMP-07 | TaskItem, TaskEvent, WorkflowEvent, EvidenceRef | TC-F-058 | C | Chưa thực hiện |
| FR-059 | FE-13 | UC-020 | BR-17, BR-24 | CMP-07 | TaskItem, TaskEvent, WorkflowEvent, EvidenceRef | TC-F-059 | C | Chưa thực hiện |
| FR-060 | FE-13 | UC-020 | BR-17, BR-24 | CMP-07 | TaskItem, TaskEvent, WorkflowEvent, EvidenceRef | TC-F-060 | C | Chưa thực hiện |
| FR-061 | FE-14 | UC-021 | BR-06, BR-25 | CMP-11 | TextBlock, Document, Dossier, TextRevision | TC-F-061 | C | Chưa thực hiện |
| FR-062 | FE-14 | UC-021 | BR-06, BR-25 | CMP-11 | TextBlock, Document, Dossier, TextRevision | TC-F-062 | C | Chưa thực hiện |
| FR-063 | FE-14 | UC-021 | BR-06, BR-25 | CMP-11 | TextBlock, Document, Dossier, TextRevision | TC-F-063 | C | Chưa thực hiện |
| FR-064 | FE-14 | UC-022 | BR-05, BR-26 | CMP-11 | Dossier, TextRevision, WorkflowEvent, ReportSource | TC-F-064 | C | Chưa thực hiện |
| FR-065 | FE-14 | UC-022 | BR-05, BR-26 | CMP-11 | Dossier, TextRevision, WorkflowEvent, ReportSource | TC-F-065 | C | Chưa thực hiện |
| FR-066 | FE-14 | UC-022 | BR-05, BR-26 | CMP-11 | Dossier, TextRevision, WorkflowEvent, ReportSource | TC-F-066 | C | Chưa thực hiện |
| FR-067 | FE-15 | UC-023 | BR-27, BR-28 | CMP-12 | FactVersion, FactEvidence, MetricDefinitionVersion, TextRevision | TC-F-067 | C | Chưa thực hiện |
| FR-068 | FE-15 | UC-023 | BR-27, BR-28 | CMP-12 | FactVersion, FactEvidence, MetricDefinitionVersion, TextRevision | TC-F-068 | C | Chưa thực hiện |
| FR-069 | FE-15 | UC-023 | BR-27, BR-28 | CMP-12 | FactVersion, FactEvidence, MetricDefinitionVersion, TextRevision | TC-F-069 | C | Chưa thực hiện |
| FR-070 | FE-15 | UC-024 | BR-28, BR-29 | CMP-12 | DedupDecisionVersion, DedupMember, FactVersion, MetricDefinitionVersion | TC-F-070 | C | Chưa thực hiện |
| FR-071 | FE-15 | UC-024 | BR-28, BR-29 | CMP-12 | DedupDecisionVersion, DedupMember, FactVersion, MetricDefinitionVersion | TC-F-071 | C | Chưa thực hiện |
| FR-072 | FE-15 | UC-024 | BR-28, BR-29 | CMP-12 | DedupDecisionVersion, DedupMember, FactVersion, MetricDefinitionVersion | TC-F-072 | C | Chưa thực hiện |
| FR-073 | FE-15 | UC-025 | BR-27, BR-29 | CMP-12 | AggregationRun, AggregationContribution, FactVersion, MetricDefinitionVersion, DedupDecisionVersion, DedupMember | TC-F-073 | C | Chưa thực hiện |
| FR-074 | FE-15 | UC-025 | BR-27, BR-29 | CMP-12 | AggregationRun, AggregationContribution, FactVersion, MetricDefinitionVersion, DedupDecisionVersion, DedupMember | TC-F-074 | C | Chưa thực hiện |
| FR-075 | FE-15 | UC-025 | BR-27, BR-29 | CMP-12 | AggregationRun, AggregationContribution, FactVersion, MetricDefinitionVersion, DedupDecisionVersion, DedupMember | TC-F-075 | C | Chưa thực hiện |
| FR-076 | FE-16 | UC-026 | BR-30, BR-31 | CMP-13 | TemplateVersion, MappingVersion, FileObject | TC-F-076 | C | Chưa thực hiện |
| FR-077 | FE-16 | UC-026 | BR-30, BR-31 | CMP-13 | TemplateVersion, MappingVersion, FileObject | TC-F-077 | C | Chưa thực hiện |
| FR-078 | FE-16 | UC-026 | BR-30, BR-31 | CMP-13 | TemplateVersion, MappingVersion, FileObject | TC-F-078 | C | Chưa thực hiện |
| FR-079 | FE-16 | UC-027 | BR-30, BR-32 | CMP-13 | Report, ReportRevision, ReportSource, ReportSection, Claim, ReportAggregation, AggregationRun, ClaimEvidence | TC-F-079 | C | Chưa thực hiện |
| FR-080 | FE-16 | UC-027 | BR-30, BR-32 | CMP-13 | Report, ReportRevision, ReportSource, ReportSection, Claim, ReportAggregation, AggregationRun, ClaimEvidence | TC-F-080 | C | Chưa thực hiện |
| FR-081 | FE-16 | UC-027 | BR-30, BR-32 | CMP-13 | Report, ReportRevision, ReportSource, ReportSection, Claim, ReportAggregation, AggregationRun, ClaimEvidence | TC-F-081 | C | Chưa thực hiện |
| FR-082 | FE-16 | UC-028 | BR-09, BR-32 | CMP-13 | ReportRevision, Claim, ClaimEvidence, ReportSource, ReportSection, ReportAggregation | TC-F-082 | C | Chưa thực hiện |
| FR-083 | FE-16 | UC-028 | BR-09, BR-32 | CMP-13 | ReportRevision, Claim, ClaimEvidence, ReportSource, ReportSection, ReportAggregation | TC-F-083 | C | Chưa thực hiện |
| FR-084 | FE-16 | UC-028 | BR-09, BR-32 | CMP-13 | ReportRevision, Claim, ClaimEvidence, ReportSource, ReportSection, ReportAggregation | TC-F-084 | C | Chưa thực hiện |
| FR-085 | FE-16 | UC-029 | BR-30, BR-33 | CMP-13 | ExportRecord, ReportRevision, TemplateVersion, ReportSource, ReportAggregation | TC-F-085 | C | Chưa thực hiện |
| FR-086 | FE-16 | UC-029 | BR-30, BR-33 | CMP-13 | ExportRecord, ReportRevision, TemplateVersion, ReportSource, ReportAggregation | TC-F-086 | C | Chưa thực hiện |
| FR-087 | FE-16 | UC-029 | BR-30, BR-33 | CMP-13 | ExportRecord, ReportRevision, TemplateVersion, ReportSource, ReportAggregation | TC-F-087 | C | Chưa thực hiện |
| FR-088 | FE-17 | UC-030 | BR-03, BR-34 | CMP-14 | Script, ScriptVersion, ScriptStep, ConfigVersion | TC-F-088 | C | Chưa thực hiện |
| FR-089 | FE-17 | UC-030 | BR-03, BR-34 | CMP-14 | Script, ScriptVersion, ScriptStep, ConfigVersion | TC-F-089 | C | Chưa thực hiện |
| FR-090 | FE-17 | UC-030 | BR-03, BR-34 | CMP-14 | Script, ScriptVersion, ScriptStep, ConfigVersion | TC-F-090 | C | Chưa thực hiện |
| FR-091 | FE-17 | UC-031 | BR-34, BR-35 | CMP-14 | ScriptVersion, ScriptStep, MappingVersion | TC-F-091 | C | Chưa thực hiện |
| FR-092 | FE-17 | UC-031 | BR-34, BR-35 | CMP-14 | ScriptVersion, ScriptStep, MappingVersion | TC-F-092 | C | Chưa thực hiện |
| FR-093 | FE-17 | UC-031 | BR-34, BR-35 | CMP-14 | ScriptVersion, ScriptStep, MappingVersion | TC-F-093 | C | Chưa thực hiện |
| FR-094 | FE-18 | UC-032 | BR-20, BR-35 | CMP-15 | RpaRun, RpaStepRun, Job, ScriptVersion, ScriptStep, ConfigVersion | TC-F-094 | C | Chưa thực hiện |
| FR-095 | FE-18 | UC-032 | BR-20, BR-35 | CMP-15 | RpaRun, RpaStepRun, Job, ScriptVersion, ScriptStep, ConfigVersion | TC-F-095 | C | Chưa thực hiện |
| FR-096 | FE-18 | UC-032 | BR-20, BR-35 | CMP-15 | RpaRun, RpaStepRun, Job, ScriptVersion, ScriptStep, ConfigVersion | TC-F-096 | C | Chưa thực hiện |
| FR-097 | FE-18 | UC-033 | BR-13, BR-21 | CMP-15 | RpaRun, RpaStepRun, ExternalEntryAttempt | TC-F-097 | C | Chưa thực hiện |
| FR-098 | FE-18 | UC-033 | BR-13, BR-21 | CMP-15 | RpaRun, RpaStepRun, ExternalEntryAttempt | TC-F-098 | C | Chưa thực hiện |
| FR-099 | FE-18 | UC-033 | BR-13, BR-21 | CMP-15 | RpaRun, RpaStepRun, ExternalEntryAttempt | TC-F-099 | C | Chưa thực hiện |
| FR-100 | FE-19 | UC-034 | BR-02, BR-36 | CMP-16 | ConfigVersion, ToolVersion, MappingVersion, DirectoryEntryVersion | TC-F-100 | C | Chưa thực hiện |
| FR-101 | FE-19 | UC-034 | BR-02, BR-36 | CMP-16 | ConfigVersion, ToolVersion, MappingVersion, DirectoryEntryVersion | TC-F-101 | C | Chưa thực hiện |
| FR-102 | FE-19 | UC-034 | BR-02, BR-36 | CMP-16 | ConfigVersion, ToolVersion, MappingVersion, DirectoryEntryVersion | TC-F-102 | C | Chưa thực hiện |
| FR-103 | FE-19 | UC-035 | BR-03, BR-37 | CMP-16 | AuditEvent, Workspace | TC-F-103 | C | Chưa thực hiện |
| FR-104 | FE-19 | UC-035 | BR-03, BR-37 | CMP-16 | AuditEvent, Workspace | TC-F-104 | C | Chưa thực hiện |
| FR-105 | FE-19 | UC-035 | BR-03, BR-37 | CMP-16 | AuditEvent, Workspace | TC-F-105 | C | Chưa thực hiện |
| FR-106 | FE-19 | UC-036 | BR-05, BR-38 | CMP-16 | BackupRecord, FileObject, Job | TC-F-106 | C | Chưa thực hiện |
| FR-107 | FE-19 | UC-036 | BR-05, BR-38 | CMP-16 | BackupRecord, FileObject, Job | TC-F-107 | C | Chưa thực hiện |
| FR-108 | FE-19 | UC-036 | BR-05, BR-38 | CMP-16 | BackupRecord, FileObject, Job | TC-F-108 | C | Chưa thực hiện |
| FR-109 | FE-19 | UC-037 | BR-38, BR-39 | CMP-16 | BackupRecord, Workspace, FileObject | TC-F-109 | C | Chưa thực hiện |
| FR-110 | FE-19 | UC-037 | BR-38, BR-39 | CMP-16 | BackupRecord, Workspace, FileObject | TC-F-110 | C | Chưa thực hiện |
| FR-111 | FE-19 | UC-037 | BR-38, BR-39 | CMP-16 | BackupRecord, Workspace, FileObject | TC-F-111 | C | Chưa thực hiện |
| FR-112 | FE-20 | UC-038 | BR-03, BR-40 | CMP-17 | AiInvocation, EvidenceRef, ConfigVersion | TC-F-112 | E | Chưa thực hiện |
| FR-113 | FE-20 | UC-038 | BR-03, BR-40 | CMP-17 | AiInvocation, EvidenceRef, ConfigVersion | TC-F-113 | E | Chưa thực hiện |
| FR-114 | FE-20 | UC-038 | BR-03, BR-40 | CMP-17 | AiInvocation, EvidenceRef, ConfigVersion | TC-F-114 | E | Chưa thực hiện |
| FR-115 | FE-21 | UC-039 | BR-18, BR-36 | CMP-18 | DirectoryEntryVersion, MappingVersion, ConfigVersion, AuditEvent | TC-F-115 | C | Chưa thực hiện |
| FR-116 | FE-21 | UC-039 | BR-18, BR-36 | CMP-18 | DirectoryEntryVersion, MappingVersion, ConfigVersion, AuditEvent | TC-F-116 | C | Chưa thực hiện |
| FR-117 | FE-21 | UC-039 | BR-18, BR-36 | CMP-18 | DirectoryEntryVersion, MappingVersion, ConfigVersion, AuditEvent | TC-F-117 | C | Chưa thực hiện |

## 3 Truy vết phi chức năng

| NFR | Nội dung | Test | Tình trạng mục tiêu | Thực thi |
|---|---|---|---|---|
| NFR-01 | Offline và cách ly | TC-N-01 | Bắt buộc | Chưa thực hiện |
| NFR-02 | Không phụ thuộc AI | TC-N-02 | Bắt buộc | Chưa thực hiện |
| NFR-03 | Kiểm soát quyền | TC-N-03 | Bắt buộc | Chưa thực hiện |
| NFR-04 | Toàn vẹn và nguyên tử | TC-N-04 | Bắt buộc | Chưa thực hiện |
| NFR-05 | Bộ nhớ | TC-N-05 | Đề xuất DEC-03 | Chưa thực hiện |
| NFR-06 | Tìm kiếm | TC-N-06 | Đề xuất DEC-03 | Chưa thực hiện |
| NFR-07 | Phản hồi UI | TC-N-07 | Đề xuất DEC-03 | Chưa thực hiện |
| NFR-08 | Hủy worker | TC-N-08 | Đề xuất DEC-03 | Chưa thực hiện |
| NFR-09 | Chất lượng OCR | TC-N-09 | Đề xuất DEC-04 | Chưa thực hiện |
| NFR-10 | Độ đúng số liệu | TC-N-10 | Bắt buộc | Chưa thực hiện |
| NFR-11 | An toàn RPA | TC-N-11 | Bắt buộc | Chưa thực hiện |
| NFR-12 | Nhận gói an toàn | TC-N-12 | Bắt buộc | Chưa thực hiện |
| NFR-13 | Truy vết nguồn | TC-N-13 | Bắt buộc | Chưa thực hiện |
| NFR-14 | Nhật ký và riêng tư | TC-N-14 | Bắt buộc | Chưa thực hiện |
| NFR-15 | Phục hồi | TC-N-15 | Đề xuất DEC-10 | Chưa thực hiện |
| NFR-16 | Tương thích môi trường | TC-N-16 | Bắt buộc | Chưa thực hiện |
| NFR-17 | Sử dụng tiếng Việt | TC-N-17 | Bắt buộc | Chưa thực hiện |
| NFR-18 | Khả năng bảo trì | TC-N-18 | Bắt buộc | Chưa thực hiện |
| NFR-19 | Đóng gói offline | TC-N-19 | Bắt buộc | Chưa thực hiện |
| NFR-20 | Giới hạn tài nguyên đầu vào | TC-N-20 | Đề xuất DEC-03 | Chưa thực hiện |
| NFR-21 | Giới hạn gói chuyển/backup | TC-N-21 | Đề xuất DEC-05 | Chưa thực hiện |
| NFR-22 | Chất lượng AI khi bật | TC-N-22 | Mở rộng DEC-09 | Chưa thực hiện |
| NFR-23 | Thời gian và hạn | TC-N-23 | Bắt buộc | Chưa thực hiện |
| NFR-24 | Phân loại bảo vệ hồ sơ | TC-N-24 | Bắt buộc | Chưa thực hiện |

## 4 Ma trận ngược theo FE

| FE | UC | Số FR | Thành phần | Phạm vi |
|---|---|---|---|---|
| FE-01 | UC-001 | 3 | CMP-01 | Cốt lõi |
| FE-02 | UC-002 | 3 | CMP-02 | Cốt lõi có điều kiện tích hợp |
| FE-03 | UC-003 | 3 | CMP-02 | Cốt lõi |
| FE-04 | UC-004, UC-005, UC-006 | 9 | CMP-03 | Cốt lõi có điều kiện công cụ |
| FE-05 | UC-007 | 3 | CMP-04 | Cốt lõi |
| FE-06 | UC-008 | 3 | CMP-04 | Cốt lõi |
| FE-07 | UC-009 | 3 | CMP-05 | Cốt lõi |
| FE-08 | UC-010, UC-011 | 6 | CMP-06 | Cốt lõi |
| FE-09 | UC-012, UC-013 | 6 | CMP-07 | Cốt lõi |
| FE-10 | UC-014 | 3 | CMP-08 | Cốt lõi có điều kiện tương thích |
| FE-11 | UC-015 | 3 | CMP-09 | Cốt lõi |
| FE-12 | UC-016, UC-017 | 6 | CMP-09 | Cốt lõi |
| FE-13 | UC-018, UC-019, UC-020 | 9 | CMP-07, CMP-10 | Cốt lõi |
| FE-14 | UC-021, UC-022 | 6 | CMP-11 | Cốt lõi |
| FE-15 | UC-023, UC-024, UC-025 | 9 | CMP-12 | Cốt lõi |
| FE-16 | UC-026, UC-027, UC-028, UC-029 | 12 | CMP-13 | Cốt lõi |
| FE-17 | UC-030, UC-031 | 6 | CMP-14 | Cốt lõi |
| FE-18 | UC-032, UC-033 | 6 | CMP-15 | Cốt lõi có điều kiện tương thích |
| FE-19 | UC-034, UC-035, UC-036, UC-037 | 12 | CMP-16 | Cốt lõi |
| FE-20 | UC-038 | 3 | CMP-17 | Mở rộng chưa bắt buộc |
| FE-21 | UC-039 | 3 | CMP-18 | Cốt lõi |

## 5 Bao phủ quy tắc

| BR | Tên | UC kiểm tra |
|---|---|---|
| BR-01 | Một kho có một phiên ghi; không dùng tệp SQLite chung qua mạng | UC-001 |
| BR-02 | Quyền được lấy từ Windows SID và ACL thực tế | UC-001, UC-034 |
| BR-03 | Giới hạn dữ liệu nhạy cảm và mạng | UC-002, UC-030, UC-035, UC-038 |
| BR-04 | Phiên email phải do người dùng đăng nhập đúng môi trường | UC-002 |
| BR-05 | Bản gốc và nội dung revision đã tham chiếu không sửa đè | UC-003, UC-022, UC-036 |
| BR-06 | Vị trí nguồn đúng loại và đúng revision | UC-003, UC-004, UC-021 |
| BR-07 | Nguồn chỉ Reviewed sau con người kiểm tra đủ phạm vi | UC-004, UC-005, UC-006 |
| BR-08 | Một tác vụ nặng tại một thời điểm | UC-005 |
| BR-09 | Tham chiếu nguồn là khoảng UTF16 half-open trên NFC | UC-006, UC-007, UC-028 |
| BR-10 | Không có dữ liệu khác với giá trị phủ định | UC-007, UC-008 |
| BR-11 | Mật giải mật và quyết định thẩm quyền không tự suy | UC-008, UC-017 |
| BR-12 | In và trình là hai sự kiện khác nhau | UC-009, UC-016 |
| BR-13 | Không retry mù tác dụng phụ chưa xác định | UC-009, UC-033 |
| BR-14 | Gói trao đổi không tạo kết nối mạng giữa máy | UC-010, UC-011 |
| BR-15 | SHA256 không là chứng nhận an toàn hoặc chữ ký | UC-010 |
| BR-16 | Import cùng origin/version phải có cùng contentHash | UC-011 |
| BR-17 | Hạn là thuộc tính nhiệm vụ được xác nhận | UC-012, UC-013, UC-020 |
| BR-18 | Chủ thể tổ chức và danh mục có version | UC-012, UC-015, UC-039 |
| BR-19 | Gia hạn không xóa lịch sử hoặc lỗi hoàn thành muộn | UC-013 |
| BR-20 | RPA chỉ trên ứng dụng đích và quyền được phép | UC-014, UC-032 |
| BR-21 | Thành công ở phần mềm ngoài cần hậu điều kiện | UC-014, UC-033 |
| BR-22 | Đề xuất trình và quyết định là các bản ghi riêng | UC-015, UC-016, UC-017 |
| BR-23 | Đã giao khác với đã ký nhận | UC-018, UC-019 |
| BR-24 | Hoàn thành xét theo từng nhiệm vụ | UC-018, UC-019, UC-020 |
| BR-25 | Chỉ mục FTS có thể tái tạo và không là nguồn sự thật | UC-021 |
| BR-26 | Archive không xóa bằng chứng tham chiếu | UC-022 |
| BR-27 | Số parse theo format người dùng xác nhận và decimal exact | UC-023, UC-025 |
| BR-28 | Fact Confirmed cần nguồn Reviewed và reviewer | UC-023, UC-024 |
| BR-29 | Quy tắc tổng hợp theo loại chỉ tiêu và coverage | UC-024, UC-025 |
| BR-30 | Mẫu DOCX có hợp đồng tag và version | UC-026, UC-027, UC-029 |
| BR-31 | Mẫu đã dùng không sửa đè và không hiểu mọi Word | UC-026 |
| BR-32 | Nội dung report bất biến theo revision và checklist review | UC-027, UC-028 |
| BR-33 | Xuất là sự kiện riêng, ghi atomic và kiểm tra trước ghi nhận | UC-029 |
| BR-34 | Recorder chỉ ghi phiên được bật và vùng an toàn | UC-030, UC-031 |
| BR-35 | Script chỉ whitelist hành động và có version hash | UC-031, UC-032 |
| BR-36 | Config công cụ mapping danh mục phải có phiên bản | UC-034, UC-039 |
| BR-37 | Audit append từ ứng dụng, không cam kết chống local admin | UC-035 |
| BR-38 | Backup chụp DB bằng API và đủ object của snapshot | UC-036, UC-037 |
| BR-39 | Restore vào kho mới và kiểm tra chủ/schema | UC-037 |
| BR-40 | AI mặc định Disabled, chỉ trả đề xuất | UC-038 |

## 6 Cách cập nhật sau thay đổi

CR phải chỉ rõ FE/FR/UC/BR/CMP/bảng/contract/test bị ảnh hưởng. Không đánh dấu TC Đạt nếu chỉ sửa tài liệu hoặc compile. Chuyển FR sang Retired phải giữ lịch sử và lý do, không đánh số lại toàn bộ. Mỗi build kiểm tra tham chiếu mồ côi, thiếu TC, mã trùng, thuộc tính pha và compatibility matrix; ý nghĩa nghiệp vụ cần người rà soát ký checklist.

Phạm vi ngữ nghĩa còn chờ DEC-01, profile đích DEC-02 và DEC về nghiệp vụ/hạn/số liệu. Số lượng ánh xạ đầy đủ không thể chứng minh yêu cầu chưa truy xuất được đã được bao phủ.
