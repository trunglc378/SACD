using System;
using System.Collections.Generic;
using SACD.Domain.Entities;
using SACD.Domain.Enums;
using SACD.Domain.ValueObjects;

namespace SACD.Domain.Rules
{
    public class DomainValidationException : Exception
    {
        public string ErrorCode { get; }

        public DomainValidationException(string errorCode, string message) : base(message)
        {
            ErrorCode = errorCode;
        }
    }

    public static class BusinessRuleValidator
    {
        /// <summary>
        /// BR-01 & BR-02: Validate Workspace invariants.
        /// </summary>
        public static void ValidateWorkspace(WorkspaceEntity ws, int supportedSchemaVersion)
        {
            if (string.IsNullOrWhiteSpace(ws.OwnerSid))
                throw new DomainValidationException("AccessDenied", "Owner SID must not be empty (BR-01).");

            if (ws.SchemaVersion != supportedSchemaVersion)
                throw new DomainValidationException("SchemaMismatch", 
                    $"Workspace schema version {ws.SchemaVersion} does not match supported version {supportedSchemaVersion} (BR-02).");
        }

        /// <summary>
        /// BR-05: Validate FileObject immutability and SHA256 integrity.
        /// </summary>
        public static void ValidateFileObject(FileObjectEntity file)
        {
            _ = new Sha256Hash(file.Hash); // Throws if not 64 hex characters

            if (file.SizeBytes < 0)
                throw new DomainValidationException("InputLimitExceeded", "File size cannot be negative (BR-05).");

            if (string.IsNullOrWhiteSpace(file.RelativePath))
                throw new DomainValidationException("InvalidPath", "File relative path must not be empty (BR-05).");
        }

        /// <summary>
        /// BR-08: Validate TextRevision review transition.
        /// </summary>
        public static void ValidateTextRevisionReview(TextRevisionEntity rev)
        {
            if (rev.ReviewState == "Reviewed")
            {
                if (string.IsNullOrWhiteSpace(rev.ReviewerSid) || string.IsNullOrWhiteSpace(rev.ReviewedUtc))
                {
                    throw new DomainValidationException("InvalidEvidence", 
                        "TextRevision cannot be marked Reviewed without reviewerSid and reviewedUtc (BR-08).");
                }
            }
        }

        /// <summary>
        /// BR-09: Validate Metadata item confirmation.
        /// </summary>
        public static void ValidateMetadataConfirmation(MetadataVersionEntity meta)
        {
            if (meta.Status == "Confirmed")
            {
                if (string.IsNullOrWhiteSpace(meta.ValueJson) || string.IsNullOrWhiteSpace(meta.EvidenceId))
                {
                    throw new DomainValidationException("InvalidEvidence", 
                        "Metadata cannot be Confirmed without ValueJson and an EvidenceRef (BR-09).");
                }
            }
        }

        /// <summary>
        /// BR-09 & BR-24: Validate Fact confirmation and exact numeric integrity.
        /// </summary>
        public static void ValidateFactVersion(FactVersionEntity fact)
        {
            if (string.CompareOrdinal(fact.PeriodStart, fact.PeriodEnd) > 0)
            {
                throw new DomainValidationException("InvalidPeriod", "PeriodStart cannot be after PeriodEnd (BR-24).");
            }

            if (fact.ValueStatus == "Known" && string.IsNullOrWhiteSpace(fact.ValueDecimalText))
            {
                throw new DomainValidationException("NumericPrecisionLoss", "Fact with status Known must contain ValueDecimalText (BR-24).");
            }

            if (fact.ValueStatus == "Missing" && !string.IsNullOrWhiteSpace(fact.ValueDecimalText))
            {
                throw new DomainValidationException("NumericPrecisionLoss", "Fact with status Missing must have null ValueDecimalText (BR-24).");
            }

            if (fact.Status == "Confirmed")
            {
                if (string.IsNullOrWhiteSpace(fact.ReviewerSid) || string.IsNullOrWhiteSpace(fact.ReviewedUtc))
                {
                    throw new DomainValidationException("NeedsReview", "Confirmed fact must have reviewerSid and reviewedUtc (BR-09).");
                }
            }
        }

        /// <summary>
        /// BR-17, BR-18, BR-19: Validate TaskItem state and due dates.
        /// </summary>
        public static void ValidateTaskItem(TaskItemEntity task)
        {
            if (task.DueKind == "Unknown")
            {
                if (!string.IsNullOrEmpty(task.DueDate) || !string.IsNullOrEmpty(task.DueUtc))
                    throw new DomainValidationException("InvalidTaskDueDate", "DueKind Unknown cannot have DueDate or DueUtc set (BR-17).");
            }
            else if (task.DueKind == "Date")
            {
                if (string.IsNullOrEmpty(task.DueDate) || !string.IsNullOrEmpty(task.DueUtc))
                    throw new DomainValidationException("InvalidTaskDueDate", "DueKind Date must have DueDate set and DueUtc null (BR-17).");
            }
            else if (task.DueKind == "Instant")
            {
                if (string.IsNullOrEmpty(task.DueUtc) || !string.IsNullOrEmpty(task.DueDate))
                    throw new DomainValidationException("InvalidTaskDueDate", "DueKind Instant must have DueUtc set and DueDate null (BR-17).");
            }

            if (task.State == "Completed" && string.IsNullOrEmpty(task.CompletedUtc))
            {
                throw new DomainValidationException("TaskIncomplete", "Completed task must record completedUtc (BR-19).");
            }
        }

        /// <summary>
        /// BR-11, BR-12, BR-13: Validate Decision record and Proposal relationship.
        /// </summary>
        public static void ValidateDecisionRecord(DecisionRecordEntity decision, ProposalVersionEntity proposal)
        {
            if (decision.ProposalVersionId != proposal.Id)
            {
                throw new DomainValidationException("RelationMismatch", "Decision must reference the specific ProposalVersionId (BR-13).");
            }

            if (string.IsNullOrWhiteSpace(decision.DecisionMakerVersionId))
            {
                throw new DomainValidationException("NeedsReview", "Decision must have a designated decisionMakerVersionId (BR-13).");
            }

            if (string.IsNullOrWhiteSpace(decision.EvidenceId))
            {
                throw new DomainValidationException("InvalidEvidence", "Decision must have an EvidenceRef for the directive/document (BR-13).");
            }
        }

        /// <summary>
        /// BR-14 & BR-15: Validate Delivery and Receipt records.
        /// </summary>
        public static void ValidateReceiptRecord(ReceiptRecordEntity receipt, DeliveryRecordEntity delivery)
        {
            if (receipt.DeliveryId != delivery.Id)
            {
                throw new DomainValidationException("RelationMismatch", "Receipt must reference the specific DeliveryId (BR-14).");
            }

            if (string.IsNullOrWhiteSpace(receipt.ReceiverVersionId))
            {
                throw new DomainValidationException("NeedsReview", "Receipt must designate receiverVersionId (BR-15).");
            }

            if (string.IsNullOrWhiteSpace(receipt.EvidenceId))
            {
                throw new DomainValidationException("InvalidEvidence", "Receipt must link to a valid signed document evidenceId (BR-15).");
            }
        }

        /// <summary>
        /// BR-27, BR-28, BR-29, BR-30: Validate ReportRevision and claims.
        /// </summary>
        public static void ValidateReportReview(ReportRevisionEntity report, IReadOnlyList<ClaimEntity> claims)
        {
            if (report.State == "Reviewed")
            {
                if (string.IsNullOrWhiteSpace(report.ReviewerSid) || string.IsNullOrWhiteSpace(report.ReviewedUtc) || string.IsNullOrWhiteSpace(report.ChecklistJson))
                {
                    throw new DomainValidationException("NeedsReview", "Reviewed report must have reviewerSid, reviewedUtc, and checklistJson (BR-29).");
                }

                foreach (var claim in claims)
                {
                    if (claim.Checked == 0)
                    {
                        throw new DomainValidationException("NeedsReview", $"Claim '{claim.Text}' has not been checked yet (BR-28).");
                    }

                    if (claim.Kind == "Commentary" && string.IsNullOrWhiteSpace(claim.CommentaryReason))
                    {
                        throw new DomainValidationException("InvalidEvidence", $"Commentary claim '{claim.Text}' must state a commentaryReason (BR-28).");
                    }
                }
            }
        }

        /// <summary>
        /// BR-30: Validate Report export for submission mode.
        /// </summary>
        public static void ValidateExportSubmission(ReportRevisionEntity report)
        {
            if (report.State != "Reviewed")
            {
                throw new DomainValidationException("NeedsReview", "Cannot export in Submission mode when report is still Draft (BR-30).");
            }

            if (report.IsStale != 0)
            {
                throw new DomainValidationException("RevisionConflict", "Cannot export in Submission mode when report has been marked Stale (BR-30).");
            }
        }

        /// <summary>
        /// BR-20 & BR-21: Validate RPA run state machine transitions.
        /// </summary>
        public static void ValidateRpaStepTransition(string fromState, string toState)
        {
            // Valid transitions: Prepared -> Dispatched -> Verified / Unknown / Failed / AwaitingUser
            if (fromState == "Prepared" && toState != "Dispatched" && toState != "Cancelled")
            {
                throw new DomainValidationException("InvalidTransition", $"Invalid RPA step transition from {fromState} to {toState}.");
            }

            if (fromState == "Dispatched" && toState == "Prepared")
            {
                throw new DomainValidationException("InvalidTransition", "Cannot revert from Dispatched back to Prepared (side effects may have occurred) (BR-20).");
            }
        }

        /// <summary>
        /// BR-09: Validate Evidence UTF-16 half-open interval [start, end) on NFC string without splitting surrogate pairs,
        /// and ensure quoteHash matches SHA256 of the extracted UTF-8 substring.
        /// </summary>
        public static void ValidateEvidenceInterval(EvidenceRefEntity ev, string rawBlockText)
        {
            if (ev.StartOffset < 0 || ev.EndOffset <= ev.StartOffset || ev.EndOffset > rawBlockText.Length)
            {
                throw new DomainValidationException("InvalidEvidence", 
                    $"Evidence offset [{ev.StartOffset}, {ev.EndOffset}) is outside valid block bounds [0, {rawBlockText.Length}] (BR-09).");
            }

            // Surrogate pair protection
            if (char.IsLowSurrogate(rawBlockText[ev.StartOffset]))
            {
                throw new DomainValidationException("InvalidEvidence", "Evidence startOffset cannot split a UTF-16 surrogate pair (BR-09).");
            }

            if (char.IsHighSurrogate(rawBlockText[ev.EndOffset - 1]))
            {
                throw new DomainValidationException("InvalidEvidence", "Evidence endOffset cannot split a UTF-16 surrogate pair (BR-09).");
            }

            string quote = rawBlockText.Substring(ev.StartOffset, ev.EndOffset - ev.StartOffset);
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(quote));
            string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            if (!string.Equals(computedHash, ev.QuoteHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainValidationException("InvalidEvidence", 
                    $"Evidence quote hash mismatch. Expected {computedHash}, but record has {ev.QuoteHash} (BR-09).");
            }
        }

        /// <summary>
        /// BR-05 & NFR-05: Check magic bytes for PDF, DOCX/ZIP, and plain text.
        /// </summary>
        public static void ValidateFileSignature(byte[] headerBytes, string extension)
        {
            if (headerBytes == null || headerBytes.Length < 4)
                throw new DomainValidationException("InputLimitExceeded", "File header too small to determine signature.");

            string ext = extension.ToLowerInvariant();
            if (ext == ".pdf")
            {
                // %PDF = 0x25, 0x50, 0x44, 0x46
                if (headerBytes[0] != 0x25 || headerBytes[1] != 0x50 || headerBytes[2] != 0x44 || headerBytes[3] != 0x46)
                {
                    throw new DomainValidationException("InputLimitExceeded", "File claims to be PDF but magic bytes %PDF do not match (BR-05).");
                }
            }
            else if (ext == ".docx" || ext == ".zip")
            {
                // PK\x03\x04 = 0x50, 0x4B, 0x03, 0x04
                if (headerBytes[0] != 0x50 || headerBytes[1] != 0x4B)
                {
                    throw new DomainValidationException("InputLimitExceeded", "File claims to be DOCX/ZIP but PK header does not match (BR-05).");
                }
            }
        }
    }
}
