using System;
using System.Collections.Generic;

namespace SACD.Application.Common
{
    public static class ErrorCodes
    {
        public const string Success = "Success";
        public const string AccessDenied = "AccessDenied";
        public const string WorkspaceBusy = "WorkspaceBusy";
        public const string RevisionConflict = "RevisionConflict";
        public const string InputLimitExceeded = "InputLimitExceeded";
        public const string InvalidEvidence = "InvalidEvidence";
        public const string NeedsReview = "NeedsReview";
        public const string TemplateInvalid = "TemplateInvalid";
        public const string TemplateUnsupported = "TemplateUnsupported";
        public const string TargetMismatch = "TargetMismatch";
        public const string MultipleMatches = "MultipleMatches";
        public const string NeedsReconciliation = "NeedsReconciliation";
        public const string TransferConflict = "TransferConflict";
        public const string PackageInvalid = "PackageInvalid";
        public const string ToolHashMismatch = "ToolHashMismatch";
        public const string NumericOverflow = "NumericOverflow";
        public const string NumericPrecisionLoss = "NumericPrecisionLoss";
        public const string Cancelled = "Cancelled";
        public const string TimedOut = "TimedOut";
        public const string NotConfigured = "NotConfigured";
        public const string StorageFailure = "StorageFailure";
        public const string NotFound = "NotFound";
        public const string SchemaMismatch = "SchemaMismatch";
    }

    public record OperationResult<T>(
        bool Success,
        T? Value,
        string Code,
        string Message,
        string CorrelationId,
        IReadOnlyList<string> Warnings
    )
    {
        public static OperationResult<T> Ok(T value, string correlationId, IReadOnlyList<string>? warnings = null)
        {
            return new OperationResult<T>(
                Success: true,
                Value: value,
                Code: ErrorCodes.Success,
                Message: "Operation completed successfully.",
                CorrelationId: correlationId,
                Warnings: warnings ?? Array.Empty<string>()
            );
        }

        public static OperationResult<T> Fail(string code, string message, string correlationId, IReadOnlyList<string>? warnings = null)
        {
            return new OperationResult<T>(
                Success: false,
                Value: default,
                Code: code,
                Message: message,
                CorrelationId: correlationId,
                Warnings: warnings ?? Array.Empty<string>()
            );
        }
    }

    public record OperationResult(
        bool Success,
        string Code,
        string Message,
        string CorrelationId,
        IReadOnlyList<string> Warnings
    )
    {
        public static OperationResult Ok(string correlationId, IReadOnlyList<string>? warnings = null)
        {
            return new OperationResult(
                Success: true,
                Code: ErrorCodes.Success,
                Message: "Operation completed successfully.",
                CorrelationId: correlationId,
                Warnings: warnings ?? Array.Empty<string>()
            );
        }

        public static OperationResult Fail(string code, string message, string correlationId, IReadOnlyList<string>? warnings = null)
        {
            return new OperationResult(
                Success: false,
                Code: code,
                Message: message,
                CorrelationId: correlationId,
                Warnings: warnings ?? Array.Empty<string>()
            );
        }
    }
}
