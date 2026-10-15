using System;

namespace SACD.Domain.Enums
{
    public enum MachineProfile
    {
        Intake,
        Management,
        Test
    }

    public enum DossierState
    {
        Received,
        Checking,
        Prepared,
        AwaitingDecision,
        Approved,
        Returned,
        Rejected,
        Delivered,
        Acknowledged,
        InProgress,
        Completed,
        Archived
    }

    public enum DocumentSourceKind
    {
        Mail,
        File,
        Scan,
        Transfer,
        Manual
    }

    public enum ReviewState
    {
        NeedsReview,
        Partial,
        Reviewed
    }

    public enum MetadataStatus
    {
        Unknown,
        Proposed,
        Confirmed,
        Rejected
    }

    public enum TaskState
    {
        Draft,
        Assigned,
        InProgress,
        Completed,
        Cancelled,
        Reopened
    }

    public enum DueKind
    {
        Unknown,
        Date,
        Instant
    }

    public enum ProposalState
    {
        Draft,
        Submitted,
        Superseded
    }

    public enum DecisionOutcome
    {
        Approved,
        Rejected,
        ReturnedForEdit,
        Superseded
    }

    public enum ExternalEntryState
    {
        Prepared,
        Executing,
        AwaitingUser,
        Verified,
        Failed,
        NeedsReconciliation,
        Cancelled
    }

    public enum PrintAttemptState
    {
        Prepared,
        Submitted,
        ConfirmedPrinted,
        Failed,
        Unknown,
        Cancelled
    }

    public enum TransferPackageState
    {
        Draft,
        Prepared,
        Exported,
        Imported,
        Quarantined,
        Failed
    }

    public enum MetricKind
    {
        Flow,
        Stock,
        Ratio,
        Cumulative
    }

    public enum FactStatus
    {
        Proposed,
        Confirmed,
        Rejected
    }

    public enum FactValueStatus
    {
        Known,
        Missing
    }

    public enum DedupDecisionState
    {
        Resolved,
        Unresolved
    }

    public enum DedupMemberRole
    {
        Canonical,
        Equivalent,
        Excluded,
        Unresolved
    }

    public enum AggregationState
    {
        Valid,
        NeedsReview,
        Failed
    }

    public enum ReportState
    {
        Draft,
        Reviewed
    }

    public enum ClaimKind
    {
        Factual,
        Commentary
    }

    public enum ExportMode
    {
        Draft,
        Submission
    }

    public enum ScriptVersionState
    {
        Draft,
        Published,
        Retired
    }

    public enum RpaAction
    {
        WaitWindow,
        WaitElement,
        Invoke,
        SetValue,
        Click,
        DoubleClick,
        Scroll,
        Move,
        KeyChord,
        TypeVariable,
        Assert,
        PauseForUser
    }

    public enum RpaRetryPolicy
    {
        None,
        ReadOnlyBounded
    }

    public enum RpaRunState
    {
        Queued,
        Running,
        Paused,
        AwaitingUser,
        Succeeded,
        Failed,
        Cancelled,
        Interrupted,
        NeedsReconciliation
    }

    public enum RpaStepState
    {
        Prepared,
        Dispatched,
        Verified,
        Failed,
        Cancelled,
        Unknown,
        AwaitingUser
    }

    public enum JobState
    {
        Queued,
        Running,
        Partial,
        Succeeded,
        Failed,
        Cancelled,
        Interrupted
    }

    public enum BackupState
    {
        Creating,
        Verified,
        Failed,
        Cancelled
    }

    public enum DrillState
    {
        NotRun,
        Passed,
        Failed
    }
}
