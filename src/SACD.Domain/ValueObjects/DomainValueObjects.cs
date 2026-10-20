using System;
using System.Text.RegularExpressions;

namespace SACD.Domain.ValueObjects
{
    public readonly record struct Sha256Hash
    {
        private static readonly Regex HexRegex = new("^[a-fA-F0-9]{64}$", RegexOptions.Compiled);

        public string Value { get; }

        public Sha256Hash(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !HexRegex.IsMatch(value))
            {
                throw new ArgumentException("Mã kiểm tra toàn vẹn phải gồm đúng 64 ký tự hệ thập lục phân.", nameof(value));
            }
            Value = value.ToLowerInvariant();
        }

        public override string ToString() => Value;

        public static implicit operator string(Sha256Hash hash) => hash.Value;
        public static explicit operator Sha256Hash(string value) => new(value);
    }

    public record ActorContext(string WindowsSid, string WorkspaceId, string CorrelationId)
    {
        public static ActorContext Create(string sid, string workspaceId, string? correlationId = null)
        {
            return new ActorContext(
                string.IsNullOrWhiteSpace(sid) ? "S-1-5-LOCAL-USER" : sid,
                string.IsNullOrWhiteSpace(workspaceId) ? "default-ws" : workspaceId,
                string.IsNullOrWhiteSpace(correlationId) ? Guid.NewGuid().ToString("N") : correlationId
            );
        }
    }

    public enum DeadlineUrgency
    {
        Unknown,
        OnTime,
        UpcomingSoon, // within warning threshold e.g. 24h
        Overdue,
        Completed
    }

    public record DeadlineStatus(
        DeadlineUrgency Urgency,
        string DisplayText,
        TimeSpan? RemainingTime,
        bool IsOverdue
    );

    public static class DeadlineCalculator
    {
        public static DeadlineStatus Evaluate(
            string dueKind,
            string? dueDateStr,
            string? dueUtcStr,
            string? completedUtcStr,
            DateTime nowUtc,
            TimeSpan warningWindow)
        {
            if (!string.IsNullOrEmpty(completedUtcStr))
            {
                return new DeadlineStatus(DeadlineUrgency.Completed, "Đã hoàn thành", null, false);
            }

            if (dueKind == "Unknown" || (string.IsNullOrEmpty(dueDateStr) && string.IsNullOrEmpty(dueUtcStr)))
            {
                return new DeadlineStatus(DeadlineUrgency.Unknown, "Chưa xác định hạn", null, false);
            }

            DateTime targetUtc;
            if (dueKind == "Date" && !string.IsNullOrEmpty(dueDateStr))
            {
                if (DateTime.TryParse(dueDateStr, out var parsedDate))
                {
                    // NFR-23 & BR-17: Asia/Ho_Chi_Minh (UTC+7) business date ends at 23:59:59 VN time
                    // 23:59:59 VN time = 16:59:59 UTC of the same day
                    var vnDateTime = new DateTimeOffset(parsedDate.Year, parsedDate.Month, parsedDate.Day, 23, 59, 59, TimeSpan.FromHours(7));
                    targetUtc = vnDateTime.UtcDateTime;
                }
                else
                {
                    return new DeadlineStatus(DeadlineUrgency.Unknown, "Định dạng ngày hạn không hợp lệ", null, false);
                }
            }
            else if (dueKind == "Instant" && !string.IsNullOrEmpty(dueUtcStr))
            {
                if (DateTime.TryParse(dueUtcStr, null, System.Globalization.DateTimeStyles.RoundtripKind, out var parsedUtc))
                {
                    targetUtc = parsedUtc.ToUniversalTime();
                }
                else
                {
                    return new DeadlineStatus(DeadlineUrgency.Unknown, "Định dạng thời điểm hạn không hợp lệ", null, false);
                }
            }
            else
            {
                return new DeadlineStatus(DeadlineUrgency.Unknown, "Chưa xác định", null, false);
            }

            var diff = targetUtc - nowUtc;
            if (diff < TimeSpan.Zero)
            {
                var overdueSpan = nowUtc - targetUtc;
                string text = overdueSpan.TotalDays >= 1
                    ? $"Quá hạn {(int)overdueSpan.TotalDays} ngày"
                    : $"Quá hạn {(int)overdueSpan.TotalHours} giờ";
                return new DeadlineStatus(DeadlineUrgency.Overdue, text, diff, true);
            }

            if (diff <= warningWindow)
            {
                string text = diff.TotalHours >= 1
                    ? $"Sắp hết hạn (còn {(int)diff.TotalHours} giờ)"
                    : $"Sắp hết hạn (còn {(int)diff.TotalMinutes} phút)";
                return new DeadlineStatus(DeadlineUrgency.UpcomingSoon, text, diff, false);
            }

            string onTimeText = diff.TotalDays >= 1
                ? $"Còn {(int)diff.TotalDays} ngày"
                : $"Còn {(int)diff.TotalHours} giờ";
            return new DeadlineStatus(DeadlineUrgency.OnTime, onTimeText, diff, false);
        }
    }

    /// <summary>
    /// Exact rational representation to prevent intermediate precision loss in ratio calculations per BR-29 & DEC-08.
    /// </summary>
    public readonly record struct ExactRatio
    {
        public decimal Numerator { get; }
        public decimal Denominator { get; }

        public ExactRatio(decimal numerator, decimal denominator)
        {
            if (denominator == 0)
            {
                throw new DivideByZeroException("Mẫu số không được bằng không; tỷ lệ này không áp dụng.");
            }
            Numerator = numerator;
            Denominator = denominator;
        }

        public decimal ToDecimal(int roundDecimals = 4)
        {
            return Math.Round(Numerator / Denominator, roundDecimals, MidpointRounding.AwayFromZero);
        }

        public string ToPercentageString(int decimals = 2)
        {
            decimal pct = Math.Round((Numerator / Denominator) * 100m, decimals, MidpointRounding.AwayFromZero);
            return $"{pct.ToString($"F{decimals}", System.Globalization.CultureInfo.InvariantCulture)}%";
        }

        public override string ToString() => $"{Numerator}/{Denominator}";
    }
}
