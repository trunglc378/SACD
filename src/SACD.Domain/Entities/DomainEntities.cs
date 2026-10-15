using System;
using System.Collections.Generic;

namespace SACD.Domain.Entities
{
    public class WorkspaceEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string OwnerSid { get; set; } = string.Empty;
        public string MachineProfile { get; set; } = "Intake"; // Intake, Management, Test
        public int SchemaVersion { get; set; } = 1;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class FileObjectEntity
    {
        public string Hash { get; set; } = string.Empty; // 64 chars hex
        public string RelativePath { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string MediaType { get; set; } = "application/octet-stream";
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class DossierEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string BusinessCode { get; set; } = string.Empty;
        public string State { get; set; } = "Received";
        public int RowVersion { get; set; } = 1;
        public string ReceivedUtc { get; set; } = DateTime.UtcNow.ToString("o");
        public string? ArchivedUtc { get; set; }
    }

    public class DocumentEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DossierId { get; set; } = string.Empty;
        public string ObjectHash { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public string SourceKind { get; set; } = "File"; // Mail, File, Scan, Transfer, Manual
        public string? SourceRef { get; set; }
        public string? CurrentRevisionId { get; set; }
        public string Status { get; set; } = "Imported";
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class TextRevisionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DocumentId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string Method { get; set; } = "Extract"; // Extract, OCR, Manual
        public string ReviewState { get; set; } = "NeedsReview"; // NeedsReview, Partial, Reviewed
        public string? ReviewerSid { get; set; }
        public string? ReviewedUtc { get; set; }
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class TextBlockEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string RevisionId { get; set; } = string.Empty;
        public int Ordinal { get; set; } = 1;
        public string LocatorJson { get; set; } = "{}";
        public string ExtractedText { get; set; } = string.Empty;
        public string RawTextNfc { get; set; } = string.Empty;
        public string SearchText { get; set; } = string.Empty;
        public int Reviewed { get; set; } = 0; // 0 or 1
    }

    public class EvidenceRefEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string RevisionId { get; set; } = string.Empty;
        public string BlockId { get; set; } = string.Empty;
        public int StartOffset { get; set; }
        public int EndOffset { get; set; }
        public string QuoteHash { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class MetadataVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DossierId { get; set; } = string.Empty;
        public string FieldKey { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string? ValueJson { get; set; }
        public string Status { get; set; } = "Unknown"; // Unknown, Proposed, Confirmed, Rejected
        public string? EvidenceId { get; set; }
        public string? ManualReason { get; set; }
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class DirectoryEntryVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string LogicalId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string Kind { get; set; } = "Unit"; // Unit, Person, Leader, Topic, Classification
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? ParentVersionId { get; set; }
        public int Active { get; set; } = 1;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ClassificationRecordEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DossierId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string ClassificationJson { get; set; } = "{}";
        public string? MilestoneDate { get; set; }
        public string? EvidenceId { get; set; }
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class TaskItemEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DossierId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? AssigneeVersionId { get; set; }
        public string State { get; set; } = "Draft"; // Draft, Assigned, InProgress, Completed, Cancelled, Reopened
        public string DueKind { get; set; } = "Unknown"; // Unknown, Date, Instant
        public string? DueDate { get; set; }
        public string? DueUtc { get; set; }
        public string? CompletedUtc { get; set; }
        public int RowVersion { get; set; } = 1;
    }

    public class TaskEventEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string TaskId { get; set; } = string.Empty;
        public int SequenceNo { get; set; } = 1;
        public string Kind { get; set; } = "Created"; // Created, Assigned, Extended, Completed, Reopened
        public string PayloadJson { get; set; } = "{}";
        public string? EvidenceId { get; set; }
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class WorkflowEventEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DossierId { get; set; } = string.Empty;
        public int SequenceNo { get; set; } = 1;
        public string FromState { get; set; } = string.Empty;
        public string ToState { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class TemplateVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string LogicalId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string ObjectHash { get; set; } = string.Empty;
        public string MappingJson { get; set; } = "{}";
        public string MappingHash { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ProposalVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DossierId { get; set; } = string.Empty;
        public string LogicalId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string RecipientVersionId { get; set; } = string.Empty;
        public string ContentJson { get; set; } = "{}";
        public string? TemplateVersionId { get; set; }
        public string State { get; set; } = "Draft"; // Draft, Submitted, Superseded
        public string? SubmittedUtc { get; set; }
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class DecisionRecordEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ProposalVersionId { get; set; } = string.Empty;
        public string Outcome { get; set; } = "Approved"; // Approved, Rejected, ReturnedForEdit, Superseded
        public string DecisionMakerVersionId { get; set; } = string.Empty;
        public string DecidedUtc { get; set; } = DateTime.UtcNow.ToString("o");
        public string EvidenceId { get; set; } = string.Empty;
        public string? SupersedesId { get; set; }
        public string RecordedBySid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class DeliveryRecordEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DossierId { get; set; } = string.Empty;
        public string DecisionId { get; set; } = string.Empty;
        public string RecipientVersionId { get; set; } = string.Empty;
        public string SnapshotJson { get; set; } = "{}";
        public string DeliveredUtc { get; set; } = DateTime.UtcNow.ToString("o");
        public string RecordedBySid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ReceiptRecordEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DeliveryId { get; set; } = string.Empty;
        public string ReceiverVersionId { get; set; } = string.Empty;
        public string SignedUtc { get; set; } = DateTime.UtcNow.ToString("o");
        public string EvidenceId { get; set; } = string.Empty;
        public string? ExceptionReason { get; set; }
        public string RecordedBySid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class MappingVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string LogicalId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string Kind { get; set; } = "QLVB";
        public string PayloadJson { get; set; } = "{}";
        public string ContentHash { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ExternalEntryAttemptEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string DossierId { get; set; } = string.Empty;
        public string MappingVersionId { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString("N");
        public string SnapshotHash { get; set; } = string.Empty;
        public string State { get; set; } = "Prepared"; // Prepared, Executing, AwaitingUser, Verified, Failed, NeedsReconciliation, Cancelled
        public string? ExternalRef { get; set; }
        public string? ReconciledBySid { get; set; }
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class PrintAttemptEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string? DossierId { get; set; }
        public string ObjectHash { get; set; } = string.Empty;
        public string PrinterProfileJson { get; set; } = "{}";
        public string State { get; set; } = "Prepared"; // Prepared, Submitted, ConfirmedPrinted, Failed, Unknown, Cancelled
        public string? SpoolJobId { get; set; }
        public string? ConfirmedBySid { get; set; }
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class TransferPackageEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string OriginWorkspaceId { get; set; } = string.Empty;
        public int SchemaVersion { get; set; } = 1;
        public string ManifestHash { get; set; } = string.Empty;
        public string State { get; set; } = "Prepared";
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
        public string? ImportedUtc { get; set; }
    }

    public class TransferItemEntity
    {
        public string PackageId { get; set; } = string.Empty;
        public string RelativePath { get; set; } = string.Empty;
        public long SizeBytes { get; set; }
        public string ContentHash { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string OriginEntityId { get; set; } = string.Empty;
        public string OriginVersion { get; set; } = string.Empty;
    }

    public class ImportMapEntity
    {
        public string OriginWorkspaceId { get; set; } = string.Empty;
        public string EntityType { get; set; } = string.Empty;
        public string OriginEntityId { get; set; } = string.Empty;
        public string OriginVersion { get; set; } = string.Empty;
        public string ContentHash { get; set; } = string.Empty;
        public string LocalEntityId { get; set; } = string.Empty;
        public string PackageId { get; set; } = string.Empty;
    }

    public class MetricDefinitionVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string LogicalId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string Name { get; set; } = string.Empty;
        public string UnitCode { get; set; } = string.Empty;
        public string Kind { get; set; } = "Flow"; // Flow, Stock, Ratio, Cumulative
        public string RuleJson { get; set; } = "{}";
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class FactVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string LogicalId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string MetricVersionId { get; set; } = string.Empty;
        public string SourceRevisionId { get; set; } = string.Empty;
        public string? SourceNumberText { get; set; }
        public string? NumberFormat { get; set; }
        public string ValueStatus { get; set; } = "Known"; // Known, Missing
        public string? ValueDecimalText { get; set; }
        public string PeriodStart { get; set; } = string.Empty;
        public string PeriodEnd { get; set; } = string.Empty;
        public string PeriodKind { get; set; } = "Month";
        public string ScopeJson { get; set; } = "{}";
        public string Status { get; set; } = "Proposed"; // Proposed, Confirmed, Rejected
        public string? ReviewerSid { get; set; }
        public string? ReviewedUtc { get; set; }
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class FactEvidenceEntity
    {
        public string FactVersionId { get; set; } = string.Empty;
        public string EvidenceId { get; set; } = string.Empty;
    }

    public class DedupDecisionVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string LogicalId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string? CanonicalFactVersionId { get; set; }
        public string State { get; set; } = "Resolved"; // Resolved, Unresolved
        public string Reason { get; set; } = string.Empty;
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class DedupMemberEntity
    {
        public string DecisionVersionId { get; set; } = string.Empty;
        public string FactVersionId { get; set; } = string.Empty;
        public string Role { get; set; } = "Canonical"; // Canonical, Equivalent, Excluded, Unresolved
    }

    public class AggregationRunEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string MetricVersionId { get; set; } = string.Empty;
        public string QueryJson { get; set; } = "{}";
        public string ResultJson { get; set; } = "{}";
        public string SnapshotHash { get; set; } = string.Empty;
        public string State { get; set; } = "Valid"; // Valid, NeedsReview, Failed
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class AggregationContributionEntity
    {
        public string AggregationId { get; set; } = string.Empty;
        public string FactVersionId { get; set; } = string.Empty;
        public string GroupKey { get; set; } = string.Empty;
        public string Operation { get; set; } = "Sum";
        public string? ContributionDecimalText { get; set; }
        public string? DedupDecisionVersionId { get; set; }
    }

    public class ReportEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ReportRevisionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ReportId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string TemplateVersionId { get; set; } = string.Empty;
        public string SnapshotJson { get; set; } = "{}";
        public string State { get; set; } = "Draft"; // Draft, Reviewed
        public int IsStale { get; set; } = 0; // 0 or 1
        public string? ReviewerSid { get; set; }
        public string? ReviewedUtc { get; set; }
        public string? ChecklistJson { get; set; }
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ReportSectionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ReportRevisionId { get; set; } = string.Empty;
        public string SectionKey { get; set; } = string.Empty;
        public int Ordinal { get; set; } = 1;
        public string Text { get; set; } = string.Empty;
    }

    public class ClaimEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string SectionId { get; set; } = string.Empty;
        public string Kind { get; set; } = "Factual"; // Factual, Commentary
        public string Text { get; set; } = string.Empty;
        public int Checked { get; set; } = 0; // 0 or 1
        public string? CommentaryReason { get; set; }
    }

    public class ClaimEvidenceEntity
    {
        public string ClaimId { get; set; } = string.Empty;
        public string EvidenceId { get; set; } = string.Empty;
    }

    public class ReportSourceEntity
    {
        public string ReportRevisionId { get; set; } = string.Empty;
        public string SourceRevisionId { get; set; } = string.Empty;
        public string SourceHash { get; set; } = string.Empty;
    }

    public class ReportAggregationEntity
    {
        public string ReportRevisionId { get; set; } = string.Empty;
        public string AggregationId { get; set; } = string.Empty;
    }

    public class ExportRecordEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ReportRevisionId { get; set; } = string.Empty;
        public string Mode { get; set; } = "Draft"; // Draft, Submission
        public string OutputHash { get; set; } = string.Empty;
        public string OutputPath { get; set; } = string.Empty;
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ScriptEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Name { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ScriptVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ScriptId { get; set; } = string.Empty;
        public int VersionNo { get; set; } = 1;
        public string State { get; set; } = "Draft"; // Draft, Published, Retired
        public string ProfileJson { get; set; } = "{}";
        public string ContentHash { get; set; } = string.Empty;
        public string? MappingVersionId { get; set; }
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ScriptStepEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ScriptVersionId { get; set; } = string.Empty;
        public int Ordinal { get; set; } = 1;
        public string Action { get; set; } = "WaitWindow"; // WaitWindow, WaitElement, Invoke, SetValue, Click, DoubleClick, Scroll, Move, KeyChord, TypeVariable, Assert, PauseForUser
        public string? SelectorJson { get; set; }
        public string ParamsJson { get; set; } = "{}";
        public int TimeoutMs { get; set; } = 10000;
        public string RetryPolicy { get; set; } = "None"; // None, ReadOnlyBounded
    }

    public class RpaRunEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ScriptVersionId { get; set; } = string.Empty;
        public string? ExternalAttemptId { get; set; }
        public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString("N");
        public string InputSnapshotHash { get; set; } = string.Empty;
        public string State { get; set; } = "Queued"; // Queued, Running, Paused, AwaitingUser, Succeeded, Failed, Cancelled, Interrupted, NeedsReconciliation
        public int CurrentStep { get; set; } = 0;
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class RpaStepRunEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string RunId { get; set; } = string.Empty;
        public string StepId { get; set; } = string.Empty;
        public int AttemptNo { get; set; } = 1;
        public string State { get; set; } = "Prepared"; // Prepared, Dispatched, Verified, Failed, Cancelled, Unknown, AwaitingUser
        public string? ObservedJson { get; set; }
        public string StartedUtc { get; set; } = DateTime.UtcNow.ToString("o");
        public string? EndedUtc { get; set; }
    }

    public class ToolVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Kind { get; set; } = "OCR"; // OCR, Poppler, OpenXml, FlaUI, LocalAi
        public string VersionLabel { get; set; } = string.Empty;
        public string ManifestJson { get; set; } = "{}";
        public string ManifestHash { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class ConfigVersionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Kind { get; set; } = "System"; // System, Limits, Security, RPA
        public int VersionNo { get; set; } = 1;
        public string PayloadJson { get; set; } = "{}";
        public string ContentHash { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class JobEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string Kind { get; set; } = "Import"; // Import, Ocr, RPA, Export, Backup
        public string EntityId { get; set; } = string.Empty;
        public string IdempotencyKey { get; set; } = Guid.NewGuid().ToString("N");
        public string State { get; set; } = "Queued"; // Queued, Running, Partial, Succeeded, Failed, Cancelled, Interrupted
        public string? CheckpointJson { get; set; }
        public string? ErrorCode { get; set; }
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
        public string UpdatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class AuditEventEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public long SequenceNo { get; set; }
        public string ActorSid { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string ObjectType { get; set; } = string.Empty;
        public string ObjectId { get; set; } = string.Empty;
        public string? RevisionId { get; set; }
        public string ResultCode { get; set; } = "Success";
        public string CorrelationId { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class BackupRecordEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ManifestHash { get; set; } = string.Empty;
        public string RelativeOrSelectedPath { get; set; } = string.Empty;
        public string State { get; set; } = "Creating"; // Creating, Verified, Failed, Cancelled
        public string DrillState { get; set; } = "NotRun"; // NotRun, Passed, Failed
        public string ActorSid { get; set; } = string.Empty;
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }

    public class AiInvocationEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string ConfigVersionId { get; set; } = string.Empty;
        public string ModelHash { get; set; } = string.Empty;
        public string SourceIdsJson { get; set; } = "[]";
        public int InputTokens { get; set; }
        public int OutputTokens { get; set; }
        public string State { get; set; } = "Completed";
        public string CreatedUtc { get; set; } = DateTime.UtcNow.ToString("o");
    }
}
