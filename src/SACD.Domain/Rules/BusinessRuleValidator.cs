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
                throw new DomainValidationException("AccessDenied", "Phải có định danh tài khoản sở hữu kho.");

            if (ws.SchemaVersion != supportedSchemaVersion)
                throw new DomainValidationException("SchemaMismatch", 
                    $"Phiên bản cấu trúc kho {ws.SchemaVersion} không khớp phiên bản được hỗ trợ {supportedSchemaVersion}.");
        }

        /// <summary>
        /// BR-05: Validate FileObject immutability and SHA256 integrity.
        /// </summary>
        public static void ValidateFileObject(FileObjectEntity file)
        {
            _ = new Sha256Hash(file.Hash); // Throws if not 64 hex characters

            if (file.SizeBytes < 0)
                throw new DomainValidationException("InputLimitExceeded", "Dung lượng tệp không được âm.");

            if (string.IsNullOrWhiteSpace(file.RelativePath))
                throw new DomainValidationException("InvalidPath", "Đường dẫn tương đối của tệp không được để trống.");
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
                        "Cần ghi nhận người đối chiếu và thời điểm đối chiếu trước khi xác nhận nội dung.");
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
                        "Cần có giá trị thông tin và căn cứ trước khi xác nhận.");
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
                throw new DomainValidationException("InvalidPeriod", "Ngày bắt đầu kỳ không được sau ngày kết thúc kỳ.");
            }

            if (fact.ValueStatus == "Known" && string.IsNullOrWhiteSpace(fact.ValueDecimalText))
            {
                throw new DomainValidationException("NumericPrecisionLoss", "Số liệu đã biết phải có giá trị thập phân.");
            }

            if (fact.ValueStatus == "Missing" && !string.IsNullOrWhiteSpace(fact.ValueDecimalText))
            {
                throw new DomainValidationException("NumericPrecisionLoss", "Số liệu còn thiếu phải để trống giá trị thập phân.");
            }

            if (fact.Status == "Confirmed")
            {
                if (string.IsNullOrWhiteSpace(fact.ReviewerSid) || string.IsNullOrWhiteSpace(fact.ReviewedUtc))
                {
                    throw new DomainValidationException("NeedsReview", "Số liệu đã xác nhận phải có người và thời điểm kiểm tra.");
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
                    throw new DomainValidationException("InvalidTaskDueDate", "Nhiệm vụ chưa xác định hạn phải để trống ngày và thời điểm đến hạn.");
            }
            else if (task.DueKind == "Date")
            {
                if (string.IsNullOrEmpty(task.DueDate) || !string.IsNullOrEmpty(task.DueUtc))
                    throw new DomainValidationException("InvalidTaskDueDate", "Nhiệm vụ có hạn theo ngày phải ghi ngày đến hạn và để trống thời điểm đến hạn.");
            }
            else if (task.DueKind == "Instant")
            {
                if (string.IsNullOrEmpty(task.DueUtc) || !string.IsNullOrEmpty(task.DueDate))
                    throw new DomainValidationException("InvalidTaskDueDate", "Nhiệm vụ có hạn theo thời điểm phải ghi thời điểm đến hạn và để trống hạn theo ngày.");
            }

            if (task.State == "Completed" && string.IsNullOrEmpty(task.CompletedUtc))
            {
                throw new DomainValidationException("TaskIncomplete", "Nhiệm vụ đã hoàn thành phải ghi nhận thời điểm hoàn thành.");
            }
        }

        /// <summary>
        /// BR-11, BR-12, BR-13: Validate Decision record and Proposal relationship.
        /// </summary>
        public static void ValidateDecisionRecord(DecisionRecordEntity decision, ProposalVersionEntity proposal)
        {
            if (decision.ProposalVersionId != proposal.Id)
            {
                throw new DomainValidationException("RelationMismatch", "Quyết định phải liên kết đúng phiên bản đề xuất.");
            }

            if (string.IsNullOrWhiteSpace(decision.DecisionMakerVersionId))
            {
                throw new DomainValidationException("NeedsReview", "Quyết định phải ghi rõ người phê duyệt.");
            }

            if (string.IsNullOrWhiteSpace(decision.EvidenceId))
            {
                throw new DomainValidationException("InvalidEvidence", "Quyết định phải có căn cứ chỉ đạo hoặc văn bản phê duyệt.");
            }
        }

        /// <summary>
        /// BR-14 & BR-15: Validate Delivery and Receipt records.
        /// </summary>
        public static void ValidateReceiptRecord(ReceiptRecordEntity receipt, DeliveryRecordEntity delivery)
        {
            if (receipt.DeliveryId != delivery.Id)
            {
                throw new DomainValidationException("RelationMismatch", "Phiếu nhận phải liên kết đúng lần giao tài liệu.");
            }

            if (string.IsNullOrWhiteSpace(receipt.ReceiverVersionId))
            {
                throw new DomainValidationException("NeedsReview", "Phiếu nhận phải ghi rõ người nhận.");
            }

            if (string.IsNullOrWhiteSpace(receipt.EvidenceId))
            {
                throw new DomainValidationException("InvalidEvidence", "Phiếu nhận phải có căn cứ tài liệu đã ký hợp lệ.");
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
                    throw new DomainValidationException("NeedsReview", "Báo cáo đã rà soát phải có người kiểm tra, thời điểm kiểm tra và danh mục kiểm tra.");
                }

                foreach (var claim in claims)
                {
                    if (claim.Checked == 0)
                    {
                        throw new DomainValidationException("NeedsReview", $"Nhận định '{claim.Text}' chưa được kiểm tra.");
                    }

                    if (claim.Kind == "Commentary" && string.IsNullOrWhiteSpace(claim.CommentaryReason))
                    {
                        throw new DomainValidationException("InvalidEvidence", $"Nhận xét '{claim.Text}' phải nêu rõ lý do.");
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
                throw new DomainValidationException("NeedsReview", "Chưa thể xuất bản trình lãnh đạo khi báo cáo vẫn là bản nháp.");
            }

            if (report.IsStale != 0)
            {
                throw new DomainValidationException("RevisionConflict", "Chưa thể xuất bản trình lãnh đạo khi nguồn báo cáo đã thay đổi và chưa được rà soát lại.");
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
                throw new DomainValidationException("InvalidTransition", "Không thể chuyển bước tự động hóa theo trình tự đã yêu cầu.");
            }

            if (fromState == "Dispatched" && toState == "Prepared")
            {
                throw new DomainValidationException("InvalidTransition", "Không thể quay về bước chuẩn bị sau khi đã thực hiện; cần đối soát kết quả trước.");
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
                    $"Vị trí căn cứ [{ev.StartOffset}, {ev.EndOffset}) nằm ngoài giới hạn đoạn văn bản [0, {rawBlockText.Length}].");
            }

            // Surrogate pair protection
            if (char.IsLowSurrogate(rawBlockText[ev.StartOffset]))
            {
                throw new DomainValidationException("InvalidEvidence", "Điểm bắt đầu căn cứ không được cắt đôi một ký tự.");
            }

            if (char.IsHighSurrogate(rawBlockText[ev.EndOffset - 1]))
            {
                throw new DomainValidationException("InvalidEvidence", "Điểm kết thúc căn cứ không được cắt đôi một ký tự.");
            }

            string quote = rawBlockText.Substring(ev.StartOffset, ev.EndOffset - ev.StartOffset);
            using var sha = System.Security.Cryptography.SHA256.Create();
            byte[] hashBytes = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(quote));
            string computedHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();

            if (!string.Equals(computedHash, ev.QuoteHash, StringComparison.OrdinalIgnoreCase))
            {
                throw new DomainValidationException("InvalidEvidence", 
                    "Mã kiểm tra trích dẫn không khớp nội dung nguồn; hãy kiểm tra lại căn cứ.");
            }
        }

        /// <summary>
        /// BR-05 & NFR-05: Check magic bytes for PDF, DOCX/ZIP, and plain text.
        /// </summary>
        public static void ValidateFileSignature(byte[] headerBytes, string extension)
        {
            if (headerBytes == null || headerBytes.Length < 4)
                throw new DomainValidationException("InputLimitExceeded", "Phần đầu tệp quá ngắn để xác định định dạng.");

            string ext = extension.ToLowerInvariant();
            if (ext == ".pdf")
            {
                // %PDF = 0x25, 0x50, 0x44, 0x46
                if (headerBytes[0] != 0x25 || headerBytes[1] != 0x50 || headerBytes[2] != 0x44 || headerBytes[3] != 0x46)
                {
                    throw new DomainValidationException("InputLimitExceeded", "Tệp có đuôi PDF nhưng phần nhận dạng không đúng định dạng PDF.");
                }
            }
            else if (ext == ".docx" || ext == ".zip")
            {
                // PK\x03\x04 = 0x50, 0x4B, 0x03, 0x04
                if (headerBytes[0] != 0x50 || headerBytes[1] != 0x4B)
                {
                    throw new DomainValidationException("InputLimitExceeded", "Tệp có đuôi DOCX hoặc ZIP nhưng phần nhận dạng không đúng định dạng.");
                }
            }
        }
    }
}
