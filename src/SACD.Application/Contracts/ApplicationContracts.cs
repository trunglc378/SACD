using System;
using System.Collections.Generic;

namespace SACD.Application.Contracts
{
    // CMP-01 Workspace
    public record WorkspaceOpenRequest(string Path, string Mode, int ExpectedSchemaVersion);
    public record WorkspaceInfo(string WorkspaceId, string Path, string OwnerSid, string MachineProfile, int SchemaVersion, string CreatedUtc);

    // CMP-02 Intake
    public record ImportRequest(IReadOnlyList<string> FilePaths, string? DossierId, string SourceKind, string? SourceRef, int LimitsConfigVersion);
    public record MailIntakeRequest(string Subject, string Sender, string ReceivedUtc, IReadOnlyList<string> SelectedAttachmentPaths, string? MailSourceRef);
    public record ImportItemResult(string OriginalPath, string DocumentId, string ObjectHash, long SizeBytes, bool Deduplicated, string Status);
    public record ImportBatchResult(string DossierId, string BusinessCode, IReadOnlyList<ImportItemResult> Items);

    // CMP-03 Text
    public record ExtractRequest(string DocumentId, string ToolVersionId);
    public record OcrRequest(string DocumentId, IReadOnlyList<int>? SpecificPages, string Language);
    public record TextBlockDto(string Id, int Ordinal, string LocatorJson, string ExtractedText, string RawTextNfc, int Reviewed);
    public record TextReviewRequest(string DocumentId, string ExpectedRevisionId, IReadOnlyList<TextBlockDto> EditedBlocks, IReadOnlyList<int> ReviewedPages, string Reason);
    public record TextRevisionResult(string RevisionId, string DocumentId, int VersionNo, string ReviewState, IReadOnlyList<TextBlockDto> Blocks);

    // CMP-04 Metadata
    public record MetadataRequest(string DossierId, string FieldKey, string TypedValueJson, string Status, string? EvidenceId, int ExpectedVersion);
    public record ClassificationRequest(string DossierId, string ClassificationJson, string? MilestoneDate, string? EvidenceId);
    public record MetadataResult(string DossierId, string FieldKey, int VersionNo, string Status, string ValueJson);

    // CMP-05 Print
    public record PrintRequest(string? DossierId, string ObjectHash, string PrinterName, int Copies, string PageRange);
    public record PrintConfirmation(string AttemptId, string SpoolJobId, bool Success);
    public record PrintAttemptInfo(string AttemptId, string? DossierId, string ObjectHash, string State, string? SpoolJobId, string CreatedUtc);

    // CMP-06 Transfer
    public record TransferPrepareRequest(string DestinationDirectory, IReadOnlyList<string> DossierIds, IReadOnlyList<string> DocumentIds);
    public record TransferImportRequest(string PackageZipPath, bool OverwriteExisting);
    public record TransferResult(string PackageId, string ManifestHash, int TotalFiles, long TotalBytes, string State, IReadOnlyList<string> ImportedEntityIds);

    // CMP-07 Workflow & Deadline
    public record TaskRequest(string DossierId, string? TaskId, int ExpectedRowVersion, string? AssigneeVersionId, string Name, string DueKind, string? DueDate, string? DueUtc, string? EvidenceId);
    public record DeadlineQuery(string? AssigneeId, string? StateFilter, int WarningHours);
    public record CompleteTaskRequest(string TaskId, int ExpectedRowVersion, string? EvidenceId, string? CompletionSummary);
    public record TaskItemDto(string Id, string DossierId, string DossierCode, string Name, string? AssigneeName, string State, string DueKind, string? DueDate, string? DueUtc, string? CompletedUtc, string Urgency, string DeadlineText, int RowVersion);
    public record TaskResult(string TaskId, string State, int RowVersion);
    public record DeadlinePage(IReadOnlyList<TaskItemDto> Tasks, int TotalOverdue, int TotalUpcoming, int TotalOnTime);

    // CMP-08 External Entry
    public record ExternalFillRequest(string DossierId, string MappingVersionId, string IdempotencyKey);
    public record ReconciliationRequest(string AttemptId, string ExternalRef, string OutcomeProof);
    public record ExternalEntryResult(string AttemptId, string State, string? ExternalRef);

    // CMP-09 Proposal & Decision
    public record ProposalRequest(string DossierId, string RecipientVersionId, string ContentJson, string? TemplateVersionId);
    public record SubmitProposalRequest(string ProposalVersionId);
    public record DecisionRequest(string ProposalVersionId, string Outcome, string DecisionMakerVersionId, string DecidedUtc, string EvidenceId, string? SupersedesId);
    public record ProposalResult(string ProposalId, string LogicalId, int VersionNo, string State);

    // CMP-10 Delivery & Receipt
    public record DeliveryRequest(string DossierId, string DecisionId, string RecipientVersionId, string SnapshotJson);
    public record ReceiptRequest(string DeliveryId, string ReceiverVersionId, string SignedUtc, string EvidenceId, string? ExceptionReason);
    public record DeliveryResult(string DeliveryId, string RecipientName, string DeliveredUtc, string? ReceiptId, string? SignedUtc);

    // CMP-11 Search & Archive
    public record SearchRequest(string Keyword, string? DossierCode, string? DateFrom, string? DateTo, int Page, int PageSize);
    public record SearchResultDto(string DocumentId, string DossierId, string DossierCode, string DocumentName, string Snippet, int VersionNo);
    public record SearchPage(IReadOnlyList<SearchResultDto> Results, int TotalCount, int Page);
    public record HistoryRequest(string DossierId);
    public record HistoryItemDto(string EntityType, string Id, string Action, string Details, string ActorSid, string CreatedUtc);
    public record HistoryPage(string DossierId, IReadOnlyList<HistoryItemDto> Events);
    public record ArchiveRequest(string DossierId, string Reason);

    // CMP-12 Fact & Aggregation
    public record FactRequest(string MetricVersionId, string SourceRevisionId, string ValueStatus, string? ValueDecimalText, string PeriodStart, string PeriodEnd, string PeriodKind, string ScopeJson, string? EvidenceId);
    public record DedupRequest(string MetricVersionId, IReadOnlyList<string> FactVersionIds, string CanonicalFactVersionId, string Reason);
    public record AggregateRequest(string MetricVersionId, string PeriodStart, string PeriodEnd, string GroupBy, IReadOnlyList<string> ConfirmedFactVersionIds);
    public record FactResult(string FactVersionId, string MetricName, string Period, string ValueText, string Status);
    public record DedupResult(string DecisionVersionId, string State, string CanonicalFactId);
    public record AggregateResult(string AggregationId, string MetricName, decimal TotalValue, string FormattedTotal, int ContributingFactsCount, string State);

    // CMP-13 Report
    public record TemplateRequest(string Name, string DocxFilePath, string MappingJson);
    public record ReportRequest(string? ReportId, string Name, string TemplateVersionId, string PeriodStart, string PeriodEnd, IReadOnlyList<string> SourceRevisionIds, IReadOnlyList<string> AggregationIds, IReadOnlyDictionary<string, string> SectionTexts);
    public record ReviewRequest(string ReportRevisionId, int ExpectedVersion, IReadOnlyList<string> CheckedClaimIds, string ChecklistJson, string ReviewNotes);
    public record ExportRequest(string ReportRevisionId, string ExportMode, string DestinationDirectory, bool Overwrite);
    public record TemplateResult(string TemplateVersionId, string LogicalId, int VersionNo, string ObjectHash);
    public record ReportRevisionInfo(string ReportId, string RevisionId, int VersionNo, string State, bool IsStale, string CreatedUtc);
    public record ExportInfo(string ExportRecordId, string OutputPath, string OutputHash, string Mode, string CreatedUtc);

    // CMP-14 Recorder
    public record RecordRequest(string TargetApplicationTitle, string? ProcessName);
    public record PublishScriptRequest(string ScriptId, string Name, IReadOnlyList<ScriptStepDto> Steps, string ProfileJson);
    public record ScriptStepDto(int Ordinal, string Action, string? SelectorJson, string ParamsJson, int TimeoutMs, string RetryPolicy);
    public record ScriptDraftInfo(string ScriptId, string SessionToken, string TargetApp);
    public record ScriptVersionInfo(string ScriptVersionId, string Name, int VersionNo, int StepCount, string ContentHash);

    // CMP-15 Runner
    public record RunRequest(string ScriptVersionId, string IdempotencyKey, IReadOnlyDictionary<string, string> VariableBindings, bool StepByStepMode);
    public record RunControlRequest(string RunId, string Action, string? ReconciliationProof); // Pause, Resume, Cancel, Reconcile
    public record RunInfo(string RunId, string ScriptVersionId, string State, int CurrentStep, string? ExternalAttemptId);

    // CMP-16 Operations
    public record ConfigRequest(string Kind, string PayloadJson);
    public record AuditQuery(string? ActorSid, string? Action, string? ObjectType, string? DateFrom, string? DateTo, int Limit);
    public record BackupRequest(string DestinationDirectory, string Reason);
    public record RestoreRequest(string BackupZipPath, string NewWorkspaceDirectory);
    public record ConfigInfo(string ConfigVersionId, string Kind, int VersionNo, string ContentHash);
    public record AuditItemDto(long SequenceNo, string ActorSid, string Action, string ObjectType, string ObjectId, string ResultCode, string CorrelationId, string CreatedUtc);
    public record AuditPage(IReadOnlyList<AuditItemDto> Events, int TotalCount);
    public record BackupInfo(string BackupId, string BackupPath, string ManifestHash, long SizeBytes, string State, string CreatedUtc);
    public record RestoreInfo(string RestoredWorkspaceId, string Path, bool IntegrityOk, string Message);

    // CMP-17 AI Assistant (Optional extension, default disabled)
    public record AiRequest(string TaskKind, IReadOnlyList<string> EvidenceTexts, string Prompt, int MaxTokens);
    public record AiProposal(string ProposalText, IReadOnlyList<string> ExtractedReferences, double ConfidenceScore, bool Available);

    // CMP-18 Directory
    public record DirectoryRequest(string Kind, string Code, string Name, string? ParentVersionId, bool Active);
    public record MappingRequest(string Kind, string LogicalId, string PayloadJson);
    public record DirectoryVersionInfo(string Id, string LogicalId, int VersionNo, string Kind, string Code, string Name, bool Active);
}
