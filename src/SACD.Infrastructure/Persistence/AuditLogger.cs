using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SACD.Domain.ValueObjects;

namespace SACD.Infrastructure.Persistence
{
    public interface IAuditLogger
    {
        Task LogAsync(
            SqliteConnection connection,
            SqliteTransaction? transaction,
            ActorContext actor,
            string action,
            string objectType,
            string objectId,
            string? revisionId = null,
            string resultCode = "Success",
            CancellationToken ct = default);
    }

    public class AuditLogger : IAuditLogger
    {
        public async Task LogAsync(
            SqliteConnection connection,
            SqliteTransaction? transaction,
            ActorContext actor,
            string action,
            string objectType,
            string objectId,
            string? revisionId = null,
            string resultCode = "Success",
            CancellationToken ct = default)
        {
            // BR-03 & BR-37: sanitize action and objectId - never log passwords, tokens, full texts
            string safeAction = Sanitize(action);
            string safeObjectType = Sanitize(objectType);
            string safeObjectId = Sanitize(objectId);
            string safeActor = string.IsNullOrWhiteSpace(actor.WindowsSid) ? "UNKNOWN_USER" : actor.WindowsSid;
            string safeCorrelation = string.IsNullOrWhiteSpace(actor.CorrelationId) ? Guid.NewGuid().ToString("N") : actor.CorrelationId;
            string now = DateTime.UtcNow.ToString("o");

            using var cmd = connection.CreateCommand();
            if (transaction != null)
            {
                cmd.Transaction = transaction;
            }

            cmd.CommandText = @"
INSERT INTO AuditEvent (id, sequenceNo, actorSid, action, objectType, objectId, revisionId, resultCode, correlationId, createdUtc)
VALUES (
    @id,
    (SELECT COALESCE(MAX(sequenceNo), 0) + 1 FROM AuditEvent),
    @actorSid,
    @action,
    @objectType,
    @objectId,
    @revisionId,
    @resultCode,
    @correlationId,
    @createdUtc
);";

            cmd.Parameters.AddWithValue("@id", Guid.NewGuid().ToString("N"));
            cmd.Parameters.AddWithValue("@actorSid", safeActor);
            cmd.Parameters.AddWithValue("@action", safeAction);
            cmd.Parameters.AddWithValue("@objectType", safeObjectType);
            cmd.Parameters.AddWithValue("@objectId", safeObjectId);
            cmd.Parameters.AddWithValue("@revisionId", (object?)revisionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@resultCode", resultCode);
            cmd.Parameters.AddWithValue("@correlationId", safeCorrelation);
            cmd.Parameters.AddWithValue("@createdUtc", now);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        private static string Sanitize(string input)
        {
            if (string.IsNullOrEmpty(input)) return string.Empty;
            // Trim and truncate to reasonable length to prevent payload leakage
            string trimmed = input.Trim();
            return trimmed.Length > 200 ? trimmed[..200] : trimmed;
        }
    }
}
