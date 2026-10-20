using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SACD.Application.Common;
using SACD.Application.Contracts;
using SACD.Application.Services;
using SACD.Domain.Entities;
using SACD.Domain.Enums;
using SACD.Domain.Rules;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Storage;
using SACD.Infrastructure.Text;
using SACD.Infrastructure.Word;
using SACD.Infrastructure.Transfer;
using SACD.Infrastructure.Rpa;

namespace SACD.Infrastructure.Services
{
    // CMP-05: PrintService
    public class PrintService : IPrintService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public PrintService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<PrintAttemptInfo>> PrepareAsync(PrintRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string attemptId = Guid.NewGuid().ToString("N");
            string profileJson = JsonSerializer.Serialize(new { request.PrinterName, request.Copies, request.PageRange });
            string now = DateTime.UtcNow.ToString("o");

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO PrintAttempt (id, dossierId, objectHash, printerProfileJson, state, createdUtc)
VALUES (@id, @dossierId, @hash, @profile, 'Prepared', @created);";
            cmd.Parameters.AddWithValue("@id", attemptId);
            cmd.Parameters.AddWithValue("@dossierId", (object?)request.DossierId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@hash", request.ObjectHash);
            cmd.Parameters.AddWithValue("@profile", profileJson);
            cmd.Parameters.AddWithValue("@created", now);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<PrintAttemptInfo>.Ok(new PrintAttemptInfo(
                attemptId, request.DossierId, request.ObjectHash, "Prepared", null, now
            ), actor.CorrelationId);
        }

        public Task<OperationResult<PrintAttemptInfo>> SubmitAsync(string attemptId, ActorContext actor, CancellationToken ct = default)
            => Task.FromResult(OperationResult<PrintAttemptInfo>.Fail(ErrorCodes.NotConfigured,
                "Chưa cấu hình bộ gửi máy in. In trong ứng dụng đọc rồi xác nhận bản giấy; không có mã spool giả.", actor.CorrelationId));

        public async Task<OperationResult<PrintAttemptInfo>> ConfirmAsync(PrintConfirmation confirmation, ActorContext actor, CancellationToken ct = default)
        {
            string finalState = confirmation.Success ? "ConfirmedPrinted" : "Failed";
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
UPDATE PrintAttempt 
SET state=@state, confirmedBySid=@sid 
WHERE id=@id;";
            cmd.Parameters.AddWithValue("@state", finalState);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@id", confirmation.AttemptId);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<PrintAttemptInfo>.Ok(new PrintAttemptInfo(
                confirmation.AttemptId, null, "", finalState, confirmation.SpoolJobId, DateTime.UtcNow.ToString("o")
            ), actor.CorrelationId);
        }
    }

    // CMP-06: TransferService
    public class TransferService : ITransferService
    {
        private readonly SqliteDatabaseContext _dbContext;
        private readonly IFileStore _fileStore;
        private readonly IUsbTransferManager _transferManager;

        public TransferService(SqliteDatabaseContext dbContext, IFileStore fileStore, IUsbTransferManager transferManager)
        {
            _dbContext = dbContext;
            _fileStore = fileStore;
            _transferManager = transferManager;
        }

        public async Task<OperationResult<TransferResult>> PrepareAsync(TransferPrepareRequest request, ActorContext actor, CancellationToken ct = default)
        {
            var docs = new List<DocumentEntity>();
            using var conn = _dbContext.CreateConnection();

            foreach (var docId in request.DocumentIds)
            {
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT id, dossierId, objectHash, originalName, currentRevisionId FROM Document WHERE id=@id;";
                cmd.Parameters.AddWithValue("@id", docId);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (await reader.ReadAsync(ct))
                {
                    docs.Add(new DocumentEntity
                    {
                        Id = reader.GetString(0),
                        DossierId = reader.GetString(1),
                        ObjectHash = reader.GetString(2),
                        OriginalName = reader.GetString(3),
                        CurrentRevisionId = reader.IsDBNull(4) ? null : reader.GetString(4)
                    });
                }
            }

            string zipFileName = $"SACD_Transfer_{DateTime.UtcNow:yyyyMMdd_HHmmss}.zip";
            string destZip = Path.Combine(request.DestinationDirectory, zipFileName);

            var (zipPath, manifestHash, pkg, items) = await _transferManager.CreatePackageAsync(destZip, actor.WorkspaceId, docs, _fileStore, ct);

            // Record TransferPackage and Items in DB
            using var tx = conn.BeginTransaction();
            using var cmdPkg = conn.CreateCommand();
            cmdPkg.Transaction = tx;
            cmdPkg.CommandText = @"
INSERT INTO TransferPackage (id, originWorkspaceId, schemaVersion, manifestHash, state, createdUtc)
VALUES (@id, @ws, @schema, @hash, 'Prepared', @created);";
            cmdPkg.Parameters.AddWithValue("@id", pkg.Id);
            cmdPkg.Parameters.AddWithValue("@ws", pkg.OriginWorkspaceId);
            cmdPkg.Parameters.AddWithValue("@schema", pkg.SchemaVersion);
            cmdPkg.Parameters.AddWithValue("@hash", pkg.ManifestHash);
            cmdPkg.Parameters.AddWithValue("@created", pkg.CreatedUtc);
            await cmdPkg.ExecuteNonQueryAsync(ct);

            foreach (var itm in items)
            {
                using var cmdItm = conn.CreateCommand();
                cmdItm.Transaction = tx;
                cmdItm.CommandText = @"
INSERT INTO TransferItem (packageId, relativePath, sizeBytes, contentHash, entityType, originEntityId, originVersion)
VALUES (@pkgId, @rel, @size, @hash, @type, @entId, @ver);";
                cmdItm.Parameters.AddWithValue("@pkgId", itm.PackageId);
                cmdItm.Parameters.AddWithValue("@rel", itm.RelativePath);
                cmdItm.Parameters.AddWithValue("@size", itm.SizeBytes);
                cmdItm.Parameters.AddWithValue("@hash", itm.ContentHash);
                cmdItm.Parameters.AddWithValue("@type", itm.EntityType);
                cmdItm.Parameters.AddWithValue("@entId", itm.OriginEntityId);
                cmdItm.Parameters.AddWithValue("@ver", itm.OriginVersion);
                await cmdItm.ExecuteNonQueryAsync(ct);
            }
            await tx.CommitAsync(ct);

            return OperationResult<TransferResult>.Ok(new TransferResult(
                pkg.Id, manifestHash, items.Count, new FileInfo(zipPath).Length, "Prepared", request.DocumentIds
            ), actor.CorrelationId);
        }

        public async Task<OperationResult<TransferResult>> ImportAsync(TransferImportRequest request, ActorContext actor, CancellationToken ct = default)
        {
            var (manifest, manifestHash, valid) = await _transferManager.InspectPackageAsync(request.PackageZipPath, ct);
            if (!valid)
            {
                return OperationResult<TransferResult>.Fail(ErrorCodes.PackageInvalid, "Gói dữ liệu hỏng hoặc sai SHA256 mã kiểm tra (Quarantined).", actor.CorrelationId);
            }

            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            // BR-16: Conflict detection against ImportMap
            foreach (var item in manifest.Items)
            {
                using var cmdCheck = conn.CreateCommand();
                cmdCheck.Transaction = tx;
                cmdCheck.CommandText = @"
SELECT contentHash FROM ImportMap 
WHERE originWorkspaceId=@ws AND entityType=@type AND originEntityId=@origId AND originVersion=@ver;";
                cmdCheck.Parameters.AddWithValue("@ws", manifest.OriginWorkspaceId);
                cmdCheck.Parameters.AddWithValue("@type", item.EntityType);
                cmdCheck.Parameters.AddWithValue("@origId", item.OriginEntityId);
                cmdCheck.Parameters.AddWithValue("@ver", item.OriginVersion);
                var existingHash = await cmdCheck.ExecuteScalarAsync(ct);
                if (existingHash != null && !string.Equals(existingHash.ToString(), item.ContentHash, StringComparison.OrdinalIgnoreCase))
                {
                    tx.Rollback();
                    return OperationResult<TransferResult>.Fail(
                        ErrorCodes.TransferConflict,
                        $"Xung đột dữ liệu tiếp nhận (BR-16): Thực thể {item.EntityType} [{item.OriginEntityId}] v{item.OriginVersion} từ kho {manifest.OriginWorkspaceId} đã tồn tại với mã băm khác ({existingHash} != {item.ContentHash}).",
                        actor.CorrelationId
                    );
                }
            }

            // Record TransferPackage
            using var cmdPkg = conn.CreateCommand();
            cmdPkg.Transaction = tx;
            cmdPkg.CommandText = @"
INSERT OR REPLACE INTO TransferPackage (id, originWorkspaceId, schemaVersion, manifestHash, state, createdUtc, importedUtc)
VALUES (@id, @ws, @schema, @hash, 'Imported', @created, @imported);";
            cmdPkg.Parameters.AddWithValue("@id", manifest.PackageId);
            cmdPkg.Parameters.AddWithValue("@ws", manifest.OriginWorkspaceId);
            cmdPkg.Parameters.AddWithValue("@schema", manifest.SchemaVersion);
            cmdPkg.Parameters.AddWithValue("@hash", manifestHash);
            cmdPkg.Parameters.AddWithValue("@created", manifest.CreatedUtc);
            cmdPkg.Parameters.AddWithValue("@imported", DateTime.UtcNow.ToString("o"));
            await cmdPkg.ExecuteNonQueryAsync(ct);

            // Extract objects to FileStore
            using var archive = ZipFile.OpenRead(request.PackageZipPath);
            var importedIds = new List<string>();

            foreach (var item in manifest.Items)
            {
                var entry = archive.GetEntry(item.RelativePath);
                if (entry != null)
                {
                    string targetObj = _fileStore.GetObjectAbsolutePath(item.ContentHash);
                    if (!File.Exists(targetObj))
                    {
                        entry.ExtractToFile(targetObj, true);
                    }

                    // Record FileObject in DB
                    using var cmdFO = conn.CreateCommand();
                    cmdFO.Transaction = tx;
                    cmdFO.CommandText = @"
INSERT OR IGNORE INTO FileObject (hash, relativePath, sizeBytes, mediaType, createdUtc)
VALUES (@hash, @rel, @size, 'application/octet-stream', @created);";
                    cmdFO.Parameters.AddWithValue("@hash", item.ContentHash);
                    cmdFO.Parameters.AddWithValue("@rel", $"objects/{item.ContentHash}");
                    cmdFO.Parameters.AddWithValue("@size", item.SizeBytes);
                    cmdFO.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("o"));
                    await cmdFO.ExecuteNonQueryAsync(ct);

                    // Record ImportMap
                    using var cmdMap = conn.CreateCommand();
                    cmdMap.Transaction = tx;
                    cmdMap.CommandText = @"
INSERT OR REPLACE INTO ImportMap (originWorkspaceId, entityType, originEntityId, originVersion, contentHash, localEntityId, packageId)
VALUES (@ws, @type, @origId, @ver, @hash, @localId, @pkgId);";
                    cmdMap.Parameters.AddWithValue("@ws", manifest.OriginWorkspaceId);
                    cmdMap.Parameters.AddWithValue("@type", item.EntityType);
                    cmdMap.Parameters.AddWithValue("@origId", item.OriginEntityId);
                    cmdMap.Parameters.AddWithValue("@ver", item.OriginVersion);
                    cmdMap.Parameters.AddWithValue("@hash", item.ContentHash);
                    cmdMap.Parameters.AddWithValue("@localId", item.OriginEntityId);
                    cmdMap.Parameters.AddWithValue("@pkgId", manifest.PackageId);
                    await cmdMap.ExecuteNonQueryAsync(ct);

                    importedIds.Add(item.OriginEntityId);
                }
            }

            await tx.CommitAsync(ct);

            return OperationResult<TransferResult>.Ok(new TransferResult(
                manifest.PackageId, manifestHash, manifest.Items.Count, new FileInfo(request.PackageZipPath).Length, "Imported", importedIds
            ), actor.CorrelationId);
        }
    }

    // CMP-07: WorkflowService
    public class WorkflowService : IWorkflowService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public WorkflowService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<TaskResult>> SaveTaskAsync(TaskRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string taskId = request.TaskId ?? Guid.NewGuid().ToString("N");
            var task = new TaskItemEntity
            {
                Id = taskId,
                DossierId = request.DossierId,
                Name = request.Name,
                AssigneeVersionId = request.AssigneeVersionId,
                State = "Assigned",
                DueKind = request.DueKind,
                DueDate = request.DueDate,
                DueUtc = request.DueUtc,
                RowVersion = request.ExpectedRowVersion + 1
            };

            BusinessRuleValidator.ValidateTaskItem(task);

            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            if (request.TaskId == null)
            {
                cmd.CommandText = @"
INSERT INTO TaskItem (id, dossierId, name, assigneeVersionId, state, dueKind, dueDate, dueUtc, rowVersion)
VALUES (@id, @dossierId, @name, @assignee, @state, @dueKind, @dueDate, @dueUtc, 1);";
            }
            else
            {
                cmd.CommandText = @"
UPDATE TaskItem 
SET name=@name, assigneeVersionId=@assignee, state=@state, dueKind=@dueKind, dueDate=@dueDate, dueUtc=@dueUtc, rowVersion=rowVersion+1
WHERE id=@id AND rowVersion=@expVer;";
                cmd.Parameters.AddWithValue("@expVer", request.ExpectedRowVersion);
            }

            cmd.Parameters.AddWithValue("@id", task.Id);
            cmd.Parameters.AddWithValue("@dossierId", task.DossierId);
            cmd.Parameters.AddWithValue("@name", task.Name);
            cmd.Parameters.AddWithValue("@assignee", (object?)task.AssigneeVersionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@state", task.State);
            cmd.Parameters.AddWithValue("@dueKind", task.DueKind);
            cmd.Parameters.AddWithValue("@dueDate", (object?)task.DueDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@dueUtc", (object?)task.DueUtc ?? DBNull.Value);

            int affected = await cmd.ExecuteNonQueryAsync(ct);
            if (request.TaskId != null && affected == 0)
            {
                return OperationResult<TaskResult>.Fail(ErrorCodes.RevisionConflict, "Xung đột phiên bản Task (RowVersion đã thay đổi).", actor.CorrelationId);
            }

            // Record TaskEvent
            using var cmdEvt = conn.CreateCommand();
            cmdEvt.Transaction = tx;
            cmdEvt.CommandText = @"
INSERT INTO TaskEvent (id, taskId, sequenceNo, kind, payloadJson, evidenceId, actorSid, createdUtc)
VALUES (@id, @taskId, (SELECT COALESCE(MAX(sequenceNo),0)+1 FROM TaskEvent WHERE taskId=@taskId), 'SaveTask', '{}', @evId, @sid, @created);";
            cmdEvt.Parameters.AddWithValue("@id", Guid.NewGuid().ToString("N"));
            cmdEvt.Parameters.AddWithValue("@taskId", taskId);
            cmdEvt.Parameters.AddWithValue("@evId", (object?)request.EvidenceId ?? DBNull.Value);
            cmdEvt.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmdEvt.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("o"));
            await cmdEvt.ExecuteNonQueryAsync(ct);

            await tx.CommitAsync(ct);

            return OperationResult<TaskResult>.Ok(new TaskResult(taskId, task.State, task.RowVersion), actor.CorrelationId);
        }

        public async Task<OperationResult<DeadlinePage>> EvaluateDeadlinesAsync(DeadlineQuery query, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT t.id, t.dossierId, d.businessCode, t.name, COALESCE(dir.name, 'Chưa giao'), t.state, t.dueKind, t.dueDate, t.dueUtc, t.completedUtc, t.rowVersion
FROM TaskItem t
JOIN Dossier d ON t.dossierId = d.id
LEFT JOIN DirectoryEntryVersion dir ON t.assigneeVersionId = dir.id;";

            var list = new List<TaskItemDto>();
            int overdue = 0, upcoming = 0, onTime = 0;
            var now = DateTime.UtcNow;
            var warningWindow = TimeSpan.FromHours(query.WarningHours > 0 ? query.WarningHours : 24);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                string id = reader.GetString(0);
                string dossierId = reader.GetString(1);
                string dossierCode = reader.GetString(2);
                string name = reader.GetString(3);
                string assigneeName = reader.GetString(4);
                string state = reader.GetString(5);
                string dueKind = reader.GetString(6);
                string? dueDate = reader.IsDBNull(7) ? null : reader.GetString(7);
                string? dueUtc = reader.IsDBNull(8) ? null : reader.GetString(8);
                string? completedUtc = reader.IsDBNull(9) ? null : reader.GetString(9);
                int rowVersion = reader.GetInt32(10);

                var status = DeadlineCalculator.Evaluate(dueKind, dueDate, dueUtc, completedUtc, now, warningWindow);

                if (status.Urgency == DeadlineUrgency.Overdue) overdue++;
                else if (status.Urgency == DeadlineUrgency.UpcomingSoon) upcoming++;
                else if (status.Urgency == DeadlineUrgency.OnTime) onTime++;

                list.Add(new TaskItemDto(
                    id, dossierId, dossierCode, name, assigneeName, state, dueKind, dueDate, dueUtc, completedUtc,
                    status.Urgency.ToString(), status.DisplayText, rowVersion
                ));
            }

            return OperationResult<DeadlinePage>.Ok(new DeadlinePage(list, overdue, upcoming, onTime), actor.CorrelationId);
        }

        public async Task<OperationResult<TaskResult>> CompleteAsync(CompleteTaskRequest request, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            string nowUtc = DateTime.UtcNow.ToString("o");
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
UPDATE TaskItem 
SET state='Completed', completedUtc=@now, rowVersion=rowVersion+1 
WHERE id=@id AND rowVersion=@ver;";
            cmd.Parameters.AddWithValue("@now", nowUtc);
            cmd.Parameters.AddWithValue("@id", request.TaskId);
            cmd.Parameters.AddWithValue("@ver", request.ExpectedRowVersion);
            int rows = await cmd.ExecuteNonQueryAsync(ct);

            if (rows == 0)
            {
                return OperationResult<TaskResult>.Fail(ErrorCodes.RevisionConflict, "Xung đột phiên bản Task khi hoàn thành.", actor.CorrelationId);
            }

            // Task event
            using var cmdEvt = conn.CreateCommand();
            cmdEvt.Transaction = tx;
            cmdEvt.CommandText = @"
INSERT INTO TaskEvent (id, taskId, sequenceNo, kind, payloadJson, evidenceId, actorSid, createdUtc)
VALUES (@id, @taskId, (SELECT COALESCE(MAX(sequenceNo),0)+1 FROM TaskEvent WHERE taskId=@taskId), 'Completed', @payload, @evId, @sid, @created);";
            cmdEvt.Parameters.AddWithValue("@id", Guid.NewGuid().ToString("N"));
            cmdEvt.Parameters.AddWithValue("@taskId", request.TaskId);
            cmdEvt.Parameters.AddWithValue("@payload", JsonSerializer.Serialize(new { request.CompletionSummary }));
            cmdEvt.Parameters.AddWithValue("@evId", (object?)request.EvidenceId ?? DBNull.Value);
            cmdEvt.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmdEvt.Parameters.AddWithValue("@created", nowUtc);
            await cmdEvt.ExecuteNonQueryAsync(ct);

            await tx.CommitAsync(ct);

            return OperationResult<TaskResult>.Ok(new TaskResult(request.TaskId, "Completed", request.ExpectedRowVersion + 1), actor.CorrelationId);
        }
    }

    // CMP-08: ExternalEntryService
    public class ExternalEntryService : IExternalEntryService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public ExternalEntryService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<OperationResult<ExternalEntryResult>> FillAsync(ExternalFillRequest request, ActorContext actor, CancellationToken ct = default)
            => Task.FromResult(OperationResult<ExternalEntryResult>.Fail(ErrorCodes.NotConfigured,
                "Chưa cấu hình connector QLVB. Dùng adapter đã khảo sát trong Quy trình văn thư; chưa tạo mã hồ sơ đích.", actor.CorrelationId));

        public async Task<OperationResult<ExternalEntryResult>> ReconcileAsync(ReconciliationRequest request, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
UPDATE ExternalEntryAttempt 
SET state='Verified', externalRef=@ref, reconciledBySid=@sid 
WHERE id=@id;";
            cmd.Parameters.AddWithValue("@ref", request.ExternalRef);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@id", request.AttemptId);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<ExternalEntryResult>.Ok(new ExternalEntryResult(
                request.AttemptId, "Verified", request.ExternalRef
            ), actor.CorrelationId);
        }
    }

    // CMP-09: ProposalService
    public class ProposalService : IProposalService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public ProposalService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<ProposalResult>> CreateAsync(ProposalRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string id = Guid.NewGuid().ToString("N");
            string logicalId = $"PROP-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO ProposalVersion (id, dossierId, logicalId, versionNo, recipientVersionId, contentJson, templateVersionId, state, actorSid, createdUtc)
VALUES (@id, @dossierId, @logId, 1, @recip, @content, @tpl, 'Draft', @sid, @created);";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@dossierId", request.DossierId);
            cmd.Parameters.AddWithValue("@logId", logicalId);
            cmd.Parameters.AddWithValue("@recip", request.RecipientVersionId);
            cmd.Parameters.AddWithValue("@content", request.ContentJson);
            cmd.Parameters.AddWithValue("@tpl", (object?)request.TemplateVersionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("o"));
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<ProposalResult>.Ok(new ProposalResult(id, logicalId, 1, "Draft"), actor.CorrelationId);
        }

        public async Task<OperationResult<ProposalResult>> SubmitAsync(SubmitProposalRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string now = DateTime.UtcNow.ToString("o");
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
UPDATE ProposalVersion 
SET state='Submitted', submittedUtc=@now 
WHERE id=@id;";
            cmd.Parameters.AddWithValue("@now", now);
            cmd.Parameters.AddWithValue("@id", request.ProposalVersionId);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<ProposalResult>.Ok(new ProposalResult(request.ProposalVersionId, "", 1, "Submitted"), actor.CorrelationId);
        }

        public async Task<OperationResult<string>> RecordDecisionAsync(DecisionRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string id = Guid.NewGuid().ToString("N");
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO DecisionRecord (id, proposalVersionId, outcome, decisionMakerVersionId, decidedUtc, evidenceId, supersedesId, recordedBySid, createdUtc)
VALUES (@id, @propId, @outcome, @dmId, @decided, @evId, @supId, @sid, @created);";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@propId", request.ProposalVersionId);
            cmd.Parameters.AddWithValue("@outcome", request.Outcome);
            cmd.Parameters.AddWithValue("@dmId", request.DecisionMakerVersionId);
            cmd.Parameters.AddWithValue("@decided", request.DecidedUtc);
            cmd.Parameters.AddWithValue("@evId", request.EvidenceId);
            cmd.Parameters.AddWithValue("@supId", (object?)request.SupersedesId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("o"));
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<string>.Ok(id, actor.CorrelationId);
        }
    }

    // CMP-10: DeliveryService
    public class DeliveryService : IDeliveryService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public DeliveryService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<DeliveryResult>> DeliverAsync(DeliveryRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string id = Guid.NewGuid().ToString("N");
            string now = DateTime.UtcNow.ToString("o");

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO DeliveryRecord (id, dossierId, decisionId, recipientVersionId, snapshotJson, deliveredUtc, recordedBySid, createdUtc)
VALUES (@id, @dossierId, @decId, @recip, @snap, @now, @sid, @created);";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@dossierId", request.DossierId);
            cmd.Parameters.AddWithValue("@decId", request.DecisionId);
            cmd.Parameters.AddWithValue("@recip", request.RecipientVersionId);
            cmd.Parameters.AddWithValue("@snap", request.SnapshotJson);
            cmd.Parameters.AddWithValue("@now", now);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@created", now);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<DeliveryResult>.Ok(new DeliveryResult(id, request.RecipientVersionId, now, null, null), actor.CorrelationId);
        }

        public async Task<OperationResult<DeliveryResult>> RecordReceiptAsync(ReceiptRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string id = Guid.NewGuid().ToString("N");
            string now = DateTime.UtcNow.ToString("o");

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO ReceiptRecord (id, deliveryId, receiverVersionId, signedUtc, evidenceId, exceptionReason, recordedBySid, createdUtc)
VALUES (@id, @delId, @recId, @signed, @evId, @exc, @sid, @created);";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@delId", request.DeliveryId);
            cmd.Parameters.AddWithValue("@recId", request.ReceiverVersionId);
            cmd.Parameters.AddWithValue("@signed", request.SignedUtc);
            cmd.Parameters.AddWithValue("@evId", request.EvidenceId);
            cmd.Parameters.AddWithValue("@exc", (object?)request.ExceptionReason ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@created", now);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<DeliveryResult>.Ok(new DeliveryResult(request.DeliveryId, request.ReceiverVersionId, now, id, request.SignedUtc), actor.CorrelationId);
        }
    }

    // CMP-11: SearchService
    public class SearchService : ISearchService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public SearchService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<SearchPage>> SearchAsync(SearchRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string searchParam = VietnameseTextNormalizer.RemoveDiacritics(request.Keyword).Trim();
            var results = new List<SearchResultDto>();

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT s.documentId, d.dossierId, dos.businessCode, d.originalName, snippet(TextSearch, 4, '<b>', '</b>', '...', 16), tr.versionNo
FROM TextSearch s
JOIN Document d ON s.documentId = d.id
JOIN Dossier dos ON d.dossierId = dos.id
JOIN TextRevision tr ON s.revisionId = tr.id
WHERE TextSearch MATCH @keyword AND s.revisionId=d.currentRevisionId
LIMIT @limit OFFSET @offset;";
            cmd.Parameters.AddWithValue("@keyword", $"\"{searchParam.Replace("\"", "\"\"")}\"*");
            cmd.Parameters.AddWithValue("@limit", request.PageSize > 0 ? request.PageSize : 20);
            cmd.Parameters.AddWithValue("@offset", (Math.Max(1, request.Page) - 1) * (request.PageSize > 0 ? request.PageSize : 20));

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                results.Add(new SearchResultDto(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetInt32(5)
                ));
            }

            return OperationResult<SearchPage>.Ok(new SearchPage(results, results.Count, request.Page), actor.CorrelationId);
        }

        public async Task<OperationResult<HistoryPage>> HistoryAsync(HistoryRequest request, ActorContext actor, CancellationToken ct = default)
        {
            var list = new List<HistoryItemDto>();
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
SELECT 'Workflow', id, fromState || ' -> ' || toState, reason, actorSid, createdUtc
FROM WorkflowEvent
WHERE dossierId=@id
UNION ALL
SELECT 'Task', t.id, t.name, t.state, 'SYSTEM', datetime('now')
FROM TaskItem t
WHERE t.dossierId=@id
ORDER BY 6 DESC;";
            cmd.Parameters.AddWithValue("@id", request.DossierId);
            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                list.Add(new HistoryItemDto(reader.GetString(0), reader.GetString(1), reader.GetString(2), reader.GetString(3), reader.GetString(4), reader.GetString(5)));
            }

            return OperationResult<HistoryPage>.Ok(new HistoryPage(request.DossierId, list), actor.CorrelationId);
        }

        public async Task<OperationResult<bool>> ArchiveAsync(ArchiveRequest request, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Dossier SET state='Archived', archivedUtc=@now WHERE id=@id;";
            cmd.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
            cmd.Parameters.AddWithValue("@id", request.DossierId);
            int rows = await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<bool>.Ok(rows > 0, actor.CorrelationId);
        }
    }

    // CMP-12: FactService
    public class FactService : IFactService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public FactService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<FactResult>> ConfirmAsync(FactRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string id = Guid.NewGuid().ToString("N");
            string logicalId = $"FACT-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            string now = DateTime.UtcNow.ToString("o");

            var fact = new FactVersionEntity
            {
                Id = id,
                LogicalId = logicalId,
                VersionNo = 1,
                MetricVersionId = request.MetricVersionId,
                SourceRevisionId = request.SourceRevisionId,
                ValueStatus = request.ValueStatus,
                ValueDecimalText = request.ValueDecimalText,
                PeriodStart = request.PeriodStart,
                PeriodEnd = request.PeriodEnd,
                PeriodKind = request.PeriodKind,
                ScopeJson = request.ScopeJson,
                Status = "Confirmed",
                ReviewerSid = actor.WindowsSid,
                ReviewedUtc = now,
                CreatedUtc = now
            };

            BusinessRuleValidator.ValidateFactVersion(fact);
            if(fact.ValueStatus=="Known" && !InvariantDecimal.TryParseExact(fact.ValueDecimalText!,out _))
                return OperationResult<FactResult>.Fail(ErrorCodes.NumericPrecisionLoss,"Giá trị không phải decimal invariant chính xác hoặc vượt giới hạn độ chính xác.",actor.CorrelationId);

            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            var evidenceError = await ReportReadiness.ValidateEvidenceAsync(conn, tx, request.EvidenceId, request.SourceRevisionId, ct);
            if (evidenceError != null)
                return OperationResult<FactResult>.Fail(ErrorCodes.InvalidEvidence, evidenceError, actor.CorrelationId);
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
INSERT INTO FactVersion (id, logicalId, versionNo, metricVersionId, sourceRevisionId, valueStatus, valueDecimalText, periodStart, periodEnd, periodKind, scopeJson, status, reviewerSid, reviewedUtc, createdUtc)
VALUES (@id, @logId, 1, @metId, @srcId, @valStat, @valDec, @pStart, @pEnd, @pKind, @scope, 'Confirmed', @sid, @now, @created);";
            cmd.Parameters.AddWithValue("@id", fact.Id);
            cmd.Parameters.AddWithValue("@logId", fact.LogicalId);
            cmd.Parameters.AddWithValue("@metId", fact.MetricVersionId);
            cmd.Parameters.AddWithValue("@srcId", fact.SourceRevisionId);
            cmd.Parameters.AddWithValue("@valStat", fact.ValueStatus);
            cmd.Parameters.AddWithValue("@valDec", (object?)fact.ValueDecimalText ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@pStart", fact.PeriodStart);
            cmd.Parameters.AddWithValue("@pEnd", fact.PeriodEnd);
            cmd.Parameters.AddWithValue("@pKind", fact.PeriodKind);
            cmd.Parameters.AddWithValue("@scope", fact.ScopeJson);
            cmd.Parameters.AddWithValue("@sid", fact.ReviewerSid);
            cmd.Parameters.AddWithValue("@now", fact.ReviewedUtc);
            cmd.Parameters.AddWithValue("@created", fact.CreatedUtc);
            await cmd.ExecuteNonQueryAsync(ct);

            if (!string.IsNullOrEmpty(request.EvidenceId))
            {
                using var cmdEv = conn.CreateCommand();
                cmdEv.Transaction = tx;
                cmdEv.CommandText = "INSERT INTO FactEvidence (factVersionId, evidenceId) VALUES (@factId, @evId);";
                cmdEv.Parameters.AddWithValue("@factId", fact.Id);
                cmdEv.Parameters.AddWithValue("@evId", request.EvidenceId);
                await cmdEv.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            return OperationResult<FactResult>.Ok(new FactResult(
                fact.Id, fact.LogicalId, $"{fact.PeriodStart} - {fact.PeriodEnd}", fact.ValueDecimalText ?? "N/A", "Confirmed"
            ), actor.CorrelationId);
        }

        public async Task<OperationResult<DedupResult>> ResolveDuplicateAsync(DedupRequest request, ActorContext actor, CancellationToken ct = default)
        {
            if(request.FactVersionIds.Count<2 || request.FactVersionIds.Distinct().Count()!=request.FactVersionIds.Count || !request.FactVersionIds.Contains(request.CanonicalFactVersionId))
                return OperationResult<DedupResult>.Fail(ErrorCodes.NeedsReview,"Nhóm trùng cần các ID riêng biệt và có số liệu đại diện trong nhóm.",actor.CorrelationId);
            string id = Guid.NewGuid().ToString("N");
            string logicalId = $"DEDUP-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
            string now = DateTime.UtcNow.ToString("o");

            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
INSERT INTO DedupDecisionVersion (id, logicalId, versionNo, canonicalFactVersionId, state, reason, actorSid, createdUtc)
VALUES (@id, @logId, 1, @canId, 'Resolved', @reason, @sid, @now);";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@logId", logicalId);
            cmd.Parameters.AddWithValue("@canId", request.CanonicalFactVersionId);
            cmd.Parameters.AddWithValue("@reason", request.Reason);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@now", now);
            await cmd.ExecuteNonQueryAsync(ct);

            foreach (var factId in request.FactVersionIds)
            {
                string role = factId == request.CanonicalFactVersionId ? "Canonical" : "Equivalent";
                using var cmdMem = conn.CreateCommand();
                cmdMem.Transaction = tx;
                cmdMem.CommandText = "INSERT INTO DedupMember (decisionVersionId, factVersionId, role) VALUES (@decId, @fId, @role);";
                cmdMem.Parameters.AddWithValue("@decId", id);
                cmdMem.Parameters.AddWithValue("@fId", factId);
                cmdMem.Parameters.AddWithValue("@role", role);
                await cmdMem.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            return OperationResult<DedupResult>.Ok(new DedupResult(id, "Resolved", request.CanonicalFactVersionId), actor.CorrelationId);
        }

        public Task<OperationResult<AggregateResult>> AggregateAsync(AggregateRequest request, ActorContext actor, CancellationToken ct = default)
            => new VerifiedAggregation(_dbContext).AggregateAsync(request, actor, ct);
    }

    // CMP-13: ReportService
    public class ReportService : IReportService
    {
        private readonly SqliteDatabaseContext _dbContext;
        private readonly IFileStore _fileStore;
        private readonly IWordReportGenerator _wordGenerator;

        public ReportService(SqliteDatabaseContext dbContext, IFileStore fileStore, IWordReportGenerator wordGenerator)
        {
            _dbContext = dbContext;
            _fileStore = fileStore;
            _wordGenerator = wordGenerator;
        }

        public async Task<ReportStatistics> GetStatisticsAsync(string periodStart,string periodEnd,CancellationToken ct=default)
        {
            var start=DateOnly.ParseExact(periodStart,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
            var end=DateOnly.ParseExact(periodEnd,"yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture);
            if(start>end)throw new ArgumentException("Kỳ báo cáo không hợp lệ.");
            var from=new DateTimeOffset(start.ToDateTime(TimeOnly.MinValue),TimeSpan.FromHours(7)).UtcDateTime;
            var until=new DateTimeOffset(end.AddDays(1).ToDateTime(TimeOnly.MinValue),TimeSpan.FromHours(7)).UtcDateTime;
            using var c=_dbContext.CreateConnection();using var q=c.CreateCommand();
            q.CommandText=@"SELECT COUNT(*),
                (SELECT COUNT(*) FROM TaskItem t JOIN Dossier x ON x.id=t.dossierId WHERE julianday(x.receivedUtc)>=julianday(@start) AND julianday(x.receivedUtc)<julianday(@end) AND t.state NOT IN ('Completed','Cancelled')),
                (SELECT COUNT(*) FROM TaskItem t JOIN Dossier x ON x.id=t.dossierId WHERE julianday(x.receivedUtc)>=julianday(@start) AND julianday(x.receivedUtc)<julianday(@end) AND t.state='Completed')
                FROM Dossier d WHERE julianday(d.receivedUtc)>=julianday(@start) AND julianday(d.receivedUtc)<julianday(@end);";
            q.Parameters.AddWithValue("@start",from.ToString("o"));q.Parameters.AddWithValue("@end",until.ToString("o"));
            using var r=await q.ExecuteReaderAsync(ct);await r.ReadAsync(ct);return new(r.GetInt32(0),r.GetInt32(1),r.GetInt32(2));
        }

        public async Task<OperationResult<TemplateResult>> ValidateTemplateAsync(TemplateRequest request, ActorContext actor, CancellationToken ct = default)
        {
            using (var document = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(request.DocxFilePath, false))
            {
                if (document.MainDocumentPart?.Document?.Body == null)
                    return OperationResult<TemplateResult>.Fail(ErrorCodes.TemplateInvalid, "Mẫu Word không có nội dung.", actor.CorrelationId);
            }
            try { using var mapping = JsonDocument.Parse(request.MappingJson); }
            catch (JsonException) { return OperationResult<TemplateResult>.Fail(ErrorCodes.TemplateInvalid, "Mapping phải là JSON hợp lệ.", actor.CorrelationId); }
            var fileObj = await _fileStore.StoreFileAsync(request.DocxFilePath, ct);
            string id = Guid.NewGuid().ToString("N");
            string logicalId = $"TPL-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT OR IGNORE INTO FileObject (hash, relativePath, sizeBytes, mediaType, createdUtc)
VALUES (@hash, @relative, @size, @media, @created);
INSERT INTO TemplateVersion (id, logicalId, versionNo, objectHash, mappingJson, mappingHash, createdUtc)
VALUES (@id, @logId, 1, @hash, @map, @mapHash, @created);";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@logId", logicalId);
            cmd.Parameters.AddWithValue("@hash", fileObj.Hash);
            cmd.Parameters.AddWithValue("@map", request.MappingJson);
            cmd.Parameters.AddWithValue("@relative", fileObj.RelativePath);
            cmd.Parameters.AddWithValue("@size", fileObj.SizeBytes);
            cmd.Parameters.AddWithValue("@media", fileObj.MediaType);
            cmd.Parameters.AddWithValue("@mapHash", Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.MappingJson))).ToLowerInvariant());
            cmd.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("o"));
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<TemplateResult>.Ok(new TemplateResult(id, logicalId, 1, fileObj.Hash), actor.CorrelationId);
        }

        public async Task<OperationResult<ReportRevisionInfo>> CreateDraftAsync(ReportRequest request, ActorContext actor, CancellationToken ct = default)
        {
            if (request.TemplateVersionId == "TPL-DEFAULT")
            {
                using var lookup = _dbContext.CreateConnection();
                using var query = lookup.CreateCommand();
                query.CommandText = "SELECT id FROM TemplateVersion WHERE logicalId='SACD-BUILTIN-DRAFT' ORDER BY versionNo DESC LIMIT 1;";
                string? templateId = await query.ExecuteScalarAsync(ct) as string;
                if (templateId == null)
                {
                    string templatePath = Path.Combine(_dbContext.WorkspaceDirectory,"staging",Guid.NewGuid().ToString("N")+".docx");
                    try
                    {
                        await _wordGenerator.GenerateReportDocxAsync(templatePath,new ReportRevisionEntity {VersionNo=1,State="Draft"},
                            Array.Empty<ReportSectionEntity>(),Array.Empty<ClaimEntity>(),Array.Empty<FactVersionEntity>(),"Draft",ct);
                        var stored = await _fileStore.StoreFileAsync(templatePath,ct);
                        templateId = Guid.NewGuid().ToString("N");
                        using var insert = lookup.CreateCommand();
                        insert.CommandText = @"INSERT OR IGNORE INTO FileObject VALUES(@hash,@path,@size,@type,@now);
INSERT INTO TemplateVersion(id,logicalId,versionNo,objectHash,mappingJson,mappingHash,createdUtc)
VALUES(@id,'SACD-BUILTIN-DRAFT',1,@hash,'{}',@hash,@now);";
                        insert.Parameters.AddWithValue("@hash",stored.Hash);insert.Parameters.AddWithValue("@path",stored.RelativePath);
                        insert.Parameters.AddWithValue("@size",stored.SizeBytes);insert.Parameters.AddWithValue("@type",stored.MediaType);
                        insert.Parameters.AddWithValue("@now",DateTime.UtcNow.ToString("o"));insert.Parameters.AddWithValue("@id",templateId);
                        await insert.ExecuteNonQueryAsync(ct);
                    }
                    finally { if(File.Exists(templatePath)) File.Delete(templatePath); }
                }
                request = request with { TemplateVersionId=templateId };
            }
            string reportId = request.ReportId ?? Guid.NewGuid().ToString("N");
            string revisionId = Guid.NewGuid().ToString("N");
            string now = DateTime.UtcNow.ToString("o");

            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            if (request.ReportId == null)
            {
                using var cmdRep = conn.CreateCommand();
                cmdRep.Transaction = tx;
                cmdRep.CommandText = "INSERT INTO Report (id, name, createdUtc) VALUES (@id, @name, @now);";
                cmdRep.Parameters.AddWithValue("@id", reportId);
                cmdRep.Parameters.AddWithValue("@name", request.Name);
                cmdRep.Parameters.AddWithValue("@now", now);
                await cmdRep.ExecuteNonQueryAsync(ct);
            }

            int verNo = 1;
            using var cmdVer = conn.CreateCommand();
            cmdVer.Transaction = tx;
            cmdVer.CommandText = "SELECT COALESCE(MAX(versionNo),0)+1 FROM ReportRevision WHERE reportId=@rId;";
            cmdVer.Parameters.AddWithValue("@rId", reportId);
            verNo = Convert.ToInt32(await cmdVer.ExecuteScalarAsync(ct));

            using var cmdRev = conn.CreateCommand();
            cmdRev.Transaction = tx;
            cmdRev.CommandText = @"
INSERT INTO ReportRevision (id, reportId, versionNo, templateVersionId, snapshotJson, state, isStale, createdUtc)
VALUES (@id, @rId, @ver, @tpl, @snap, 'Draft', 0, @now);";
            cmdRev.Parameters.AddWithValue("@id", revisionId);
            cmdRev.Parameters.AddWithValue("@rId", reportId);
            cmdRev.Parameters.AddWithValue("@ver", verNo);
            cmdRev.Parameters.AddWithValue("@tpl", request.TemplateVersionId);
            cmdRev.Parameters.AddWithValue("@snap", JsonSerializer.Serialize(new { request.PeriodStart, request.PeriodEnd }));
            cmdRev.Parameters.AddWithValue("@now", now);
            await cmdRev.ExecuteNonQueryAsync(ct);

            int secOrd = 1;
            foreach (var kvp in request.SectionTexts)
            {
                string secId = Guid.NewGuid().ToString("N");
                using var cmdSec = conn.CreateCommand();
                cmdSec.Transaction = tx;
                cmdSec.CommandText = @"
INSERT INTO ReportSection (id, reportRevisionId, sectionKey, ordinal, text)
VALUES (@id, @revId, @key, @ord, @text);";
                cmdSec.Parameters.AddWithValue("@id", secId);
                cmdSec.Parameters.AddWithValue("@revId", revisionId);
                cmdSec.Parameters.AddWithValue("@key", kvp.Key);
                cmdSec.Parameters.AddWithValue("@ord", secOrd++);
                cmdSec.Parameters.AddWithValue("@text", kvp.Value);
                await cmdSec.ExecuteNonQueryAsync(ct);

                // Default claim
                using var cmdClaim = conn.CreateCommand();
                cmdClaim.Transaction = tx;
                cmdClaim.CommandText = @"
INSERT INTO Claim (id, sectionId, kind, text, checked, commentaryReason)
VALUES (@id, @secId, 'Factual', @text, 0, NULL);";
                cmdClaim.Parameters.AddWithValue("@id", Guid.NewGuid().ToString("N"));
                cmdClaim.Parameters.AddWithValue("@secId", secId);
                cmdClaim.Parameters.AddWithValue("@text", $"Thông tin trong phần {kvp.Key}");
                await cmdClaim.ExecuteNonQueryAsync(ct);
            }

            foreach (var aggregationId in request.AggregationIds.Distinct())
            {
                using var aggregation = conn.CreateCommand(); aggregation.Transaction = tx;
                aggregation.CommandText = "INSERT INTO ReportAggregation(reportRevisionId,aggregationId) SELECT @rev,id FROM AggregationRun WHERE id=@id AND state='Valid';";
                aggregation.Parameters.AddWithValue("@rev", revisionId); aggregation.Parameters.AddWithValue("@id", aggregationId);
                if (await aggregation.ExecuteNonQueryAsync(ct) != 1)
                    return OperationResult<ReportRevisionInfo>.Fail(ErrorCodes.NeedsReview, "Tổng hợp nguồn không tồn tại hoặc chưa hợp lệ.", actor.CorrelationId);
            }
            // ReportSource
            foreach (var sId in request.SourceRevisionIds)
            {
                using var cmdSrc = conn.CreateCommand();
                cmdSrc.Transaction = tx;
                cmdSrc.CommandText = @"
INSERT OR IGNORE INTO ReportSource (reportRevisionId, sourceRevisionId, sourceHash)
SELECT @revId, @srcId, d.objectHash FROM TextRevision t JOIN Document d ON d.id=t.documentId WHERE t.id=@srcId;";
                cmdSrc.Parameters.AddWithValue("@revId", revisionId);
                cmdSrc.Parameters.AddWithValue("@srcId", sId);
                await cmdSrc.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            return OperationResult<ReportRevisionInfo>.Ok(new ReportRevisionInfo(
                reportId, revisionId, verNo, "Draft", false, now
            ), actor.CorrelationId);
        }

        public async Task<OperationResult<ReportRevisionInfo>> ReviewAsync(ReviewRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string now = DateTime.UtcNow.ToString("o");
            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            var reviewError = await ReportReadiness.ValidateAsync(conn, tx, request.ReportRevisionId, request.ExpectedVersion, request.CheckedClaimIds, ct);
            if (reviewError != null)
                return OperationResult<ReportRevisionInfo>.Fail(ErrorCodes.NeedsReview, reviewError, actor.CorrelationId);
            // Mark claims checked
            foreach (var claimId in request.CheckedClaimIds)
            {
                using var cmdClaim = conn.CreateCommand();
                cmdClaim.Transaction = tx;
                cmdClaim.CommandText = "UPDATE Claim SET checked=1 WHERE id=@id AND sectionId IN (SELECT id FROM ReportSection WHERE reportRevisionId=@rev);";
                cmdClaim.Parameters.AddWithValue("@rev", request.ReportRevisionId);
                cmdClaim.Parameters.AddWithValue("@id", claimId);
                await cmdClaim.ExecuteNonQueryAsync(ct);
            }

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
UPDATE ReportRevision 
SET state='Reviewed', reviewerSid=@sid, reviewedUtc=@now, checklistJson=@chk 
WHERE id=@id;";
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@now", now);
            cmd.Parameters.AddWithValue("@chk", request.ChecklistJson);
            cmd.Parameters.AddWithValue("@id", request.ReportRevisionId);
            await cmd.ExecuteNonQueryAsync(ct);

            await tx.CommitAsync(ct);

            return OperationResult<ReportRevisionInfo>.Ok(new ReportRevisionInfo(
                "", request.ReportRevisionId, request.ExpectedVersion, "Reviewed", false, now
            ), actor.CorrelationId);
        }

        public async Task<OperationResult<ExportInfo>> ExportAsync(ExportRequest request, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            ReportRevisionEntity rev;
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, reportId, versionNo, state, isStale FROM ReportRevision WHERE id=@id;";
                cmd.Parameters.AddWithValue("@id", request.ReportRevisionId);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                if (!await reader.ReadAsync(ct))
                {
                    return OperationResult<ExportInfo>.Fail(ErrorCodes.NotFound, "Không tìm thấy phiên bản báo cáo.", actor.CorrelationId);
                }
                rev = new ReportRevisionEntity
                {
                    Id = reader.GetString(0),
                    ReportId = reader.GetString(1),
                    VersionNo = reader.GetInt32(2),
                    State = reader.GetString(3),
                    IsStale = reader.GetInt32(4)
                };
            }

            if (request.ExportMode == "Submission")
            {
                try
                {
                    BusinessRuleValidator.ValidateExportSubmission(rev);
                    var readinessError = await ReportReadiness.ValidateAsync(conn, null, rev.Id, rev.VersionNo, Array.Empty<string>(), ct);
                    if (readinessError != null)
                        return OperationResult<ExportInfo>.Fail(ErrorCodes.NeedsReview, readinessError, actor.CorrelationId);
                }
                catch (DomainValidationException ex)
                {
                    return OperationResult<ExportInfo>.Fail(ex.ErrorCode, ex.Message, actor.CorrelationId);
                }
            }

            using (var template = conn.CreateCommand())
            {
                template.CommandText = "SELECT t.logicalId FROM TemplateVersion t JOIN ReportRevision r ON r.templateVersionId=t.id WHERE r.id=@id;";
                template.Parameters.AddWithValue("@id", rev.Id);
                if ((string?)await template.ExecuteScalarAsync(ct) != "SACD-BUILTIN-DRAFT")
                    return OperationResult<ExportInfo>.Fail(ErrorCodes.TemplateUnsupported,
                        "Bộ xuất hiện chỉ hỗ trợ mẫu nháp dựng sẵn; chưa điền mẫu Word tùy chỉnh. Không thay mẫu đơn vị bằng mẫu giả.", actor.CorrelationId);
            }
            // Fetch sections
            var sections = new List<ReportSectionEntity>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, reportRevisionId, sectionKey, ordinal, text FROM ReportSection WHERE reportRevisionId=@id ORDER BY ordinal;";
                cmd.Parameters.AddWithValue("@id", request.ReportRevisionId);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    sections.Add(new ReportSectionEntity
                    {
                        Id = reader.GetString(0),
                        ReportRevisionId = reader.GetString(1),
                        SectionKey = reader.GetString(2),
                        Ordinal = reader.GetInt32(3),
                        Text = reader.GetString(4)
                    });
                }
            }

            // Fetch claims
            var claims = new List<ClaimEntity>();
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
SELECT c.id, c.sectionId, c.kind, c.text, c.checked, c.commentaryReason
FROM Claim c
JOIN ReportSection s ON c.sectionId = s.id
WHERE s.reportRevisionId=@id;";
                cmd.Parameters.AddWithValue("@id", request.ReportRevisionId);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    claims.Add(new ClaimEntity
                    {
                        Id = reader.GetString(0),
                        SectionId = reader.GetString(1),
                        Kind = reader.GetString(2),
                        Text = reader.GetString(3),
                        Checked = reader.GetInt32(4),
                        CommentaryReason = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }

            string outFileName = $"BaoCao_{rev.VersionNo}_{request.ExportMode}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.docx";
            string targetPath = Path.Combine(request.DestinationDirectory, outFileName);

            var (genPath, hash) = await _wordGenerator.GenerateReportDocxAsync(
                targetPath, rev, sections, claims, Array.Empty<FactVersionEntity>(), request.ExportMode, ct
            );

            // Record ExportRecord
            string exportId = Guid.NewGuid().ToString("N");
            string now = DateTime.UtcNow.ToString("o");
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO ExportRecord (id, reportRevisionId, mode, outputHash, outputPath, actorSid, createdUtc)
VALUES (@id, @revId, @mode, @hash, @path, @sid, @created);";
                cmd.Parameters.AddWithValue("@id", exportId);
                cmd.Parameters.AddWithValue("@revId", rev.Id);
                cmd.Parameters.AddWithValue("@mode", request.ExportMode);
                cmd.Parameters.AddWithValue("@hash", hash);
                cmd.Parameters.AddWithValue("@path", genPath);
                cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
                cmd.Parameters.AddWithValue("@created", now);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            return OperationResult<ExportInfo>.Ok(new ExportInfo(
                exportId, genPath, hash, request.ExportMode, now
            ), actor.CorrelationId);
        }
    }

    // CMP-14: RecorderService
    public class RecorderService : IRecorderService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public RecorderService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<OperationResult<ScriptDraftInfo>> StartAsync(RecordRequest request, ActorContext actor, CancellationToken ct = default)
        {
            return Task.FromResult(OperationResult<ScriptDraftInfo>.Fail(ErrorCodes.NotConfigured,
                "Chưa có bộ ghi thao tác thật. Kịch bản cấu hình/mô phỏng là chức năng riêng.",actor.CorrelationId));
        }

        public Task<OperationResult<ScriptDraftInfo>> StopAsync(string scriptId, ActorContext actor, CancellationToken ct = default)
        {
            return Task.FromResult(OperationResult<ScriptDraftInfo>.Fail(ErrorCodes.NotConfigured,"Chưa có phiên ghi thao tác thật.",actor.CorrelationId));
        }

        public async Task<OperationResult<ScriptVersionInfo>> PublishScriptAsync(PublishScriptRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string versionId = Guid.NewGuid().ToString("N");
            string now = DateTime.UtcNow.ToString("o");
            string contentHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(new { request.ProfileJson, request.Steps })))).ToLowerInvariant();

            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            using var cmdScript = conn.CreateCommand();
            cmdScript.Transaction = tx;
            cmdScript.CommandText = "INSERT OR IGNORE INTO Script (id, name, createdUtc) VALUES (@id, @name, @now);";
            cmdScript.Parameters.AddWithValue("@id", request.ScriptId);
            cmdScript.Parameters.AddWithValue("@name", request.Name);
            cmdScript.Parameters.AddWithValue("@now", now);
            await cmdScript.ExecuteNonQueryAsync(ct);

            using var cmdNext = conn.CreateCommand();
            cmdNext.Transaction = tx;
            cmdNext.CommandText = "SELECT COALESCE(MAX(versionNo),0)+1 FROM ScriptVersion WHERE scriptId=@id;";
            cmdNext.Parameters.AddWithValue("@id", request.ScriptId);
            int versionNo = Convert.ToInt32(await cmdNext.ExecuteScalarAsync(ct));

            using var cmdVer = conn.CreateCommand();
            cmdVer.Transaction = tx;
            cmdVer.CommandText = @"
INSERT INTO ScriptVersion (id, scriptId, versionNo, state, profileJson, contentHash, createdUtc)
VALUES (@id, @scriptId, @version, 'Published', @profile, @hash, @now);";
            cmdVer.Parameters.AddWithValue("@version", versionNo);
            cmdVer.Parameters.AddWithValue("@id", versionId);
            cmdVer.Parameters.AddWithValue("@scriptId", request.ScriptId);
            cmdVer.Parameters.AddWithValue("@profile", request.ProfileJson);
            cmdVer.Parameters.AddWithValue("@hash", contentHash);
            cmdVer.Parameters.AddWithValue("@now", now);
            await cmdVer.ExecuteNonQueryAsync(ct);

            foreach (var step in request.Steps)
            {
                using var cmdStep = conn.CreateCommand();
                cmdStep.Transaction = tx;
                cmdStep.CommandText = @"
INSERT INTO ScriptStep (id, scriptVersionId, ordinal, action, selectorJson, paramsJson, timeoutMs, retryPolicy)
VALUES (@id, @verId, @ord, @act, @sel, @params, @timeout, @retry);";
                cmdStep.Parameters.AddWithValue("@id", Guid.NewGuid().ToString("N"));
                cmdStep.Parameters.AddWithValue("@verId", versionId);
                cmdStep.Parameters.AddWithValue("@ord", step.Ordinal);
                cmdStep.Parameters.AddWithValue("@act", step.Action);
                cmdStep.Parameters.AddWithValue("@sel", (object?)step.SelectorJson ?? DBNull.Value);
                cmdStep.Parameters.AddWithValue("@params", step.ParamsJson);
                cmdStep.Parameters.AddWithValue("@timeout", step.TimeoutMs);
                cmdStep.Parameters.AddWithValue("@retry", step.RetryPolicy);
                await cmdStep.ExecuteNonQueryAsync(ct);
            }

            await tx.CommitAsync(ct);

            return OperationResult<ScriptVersionInfo>.Ok(new ScriptVersionInfo(
                versionId, request.Name, versionNo, request.Steps.Count, contentHash
            ), actor.CorrelationId);
        }
    }

    // CMP-15: RunnerService
    public class RunnerService : IRunnerService
    {
        private readonly SqliteDatabaseContext _dbContext;
        private readonly IRpaEngine _rpaEngine;

        public RunnerService(SqliteDatabaseContext dbContext, IRpaEngine rpaEngine)
        {
            _dbContext = dbContext;
            _rpaEngine = rpaEngine;
        }

        public async Task<OperationResult<RunInfo>> RunAsync(RunRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string runId = Guid.NewGuid().ToString("N");
            string now = DateTime.UtcNow.ToString("o");

            var steps = new List<ScriptStepEntity>();
            using (var conn = _dbContext.CreateConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, scriptVersionId, ordinal, action, selectorJson, paramsJson, timeoutMs, retryPolicy FROM ScriptStep WHERE scriptVersionId=@id ORDER BY ordinal;";
                cmd.Parameters.AddWithValue("@id", request.ScriptVersionId);
                using var reader = await cmd.ExecuteReaderAsync(ct);
                while (await reader.ReadAsync(ct))
                {
                    steps.Add(new ScriptStepEntity
                    {
                        Id = reader.GetString(0),
                        ScriptVersionId = reader.GetString(1),
                        Ordinal = reader.GetInt32(2),
                        Action = reader.GetString(3),
                        SelectorJson = reader.IsDBNull(4) ? null : reader.GetString(4),
                        ParamsJson = reader.GetString(5),
                        TimeoutMs = reader.GetInt32(6),
                        RetryPolicy = reader.GetString(7)
                    });
                }
            }

            var runEntity = new RpaRunEntity
            {
                Id = runId,
                ScriptVersionId = request.ScriptVersionId,
                IdempotencyKey = request.IdempotencyKey,
                InputSnapshotHash = Guid.NewGuid().ToString("N"),
                State = "Running",
                CurrentStep = 0,
                ActorSid = actor.WindowsSid,
                CreatedUtc = now
            };

            using (var conn = _dbContext.CreateConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = @"
INSERT INTO RpaRun (id, scriptVersionId, idempotencyKey, inputSnapshotHash, state, currentStep, actorSid, createdUtc)
VALUES (@id, @verId, @idem, @hash, @state, @cur, @sid, @now);";
                cmd.Parameters.AddWithValue("@id", runEntity.Id);
                cmd.Parameters.AddWithValue("@verId", runEntity.ScriptVersionId);
                cmd.Parameters.AddWithValue("@idem", runEntity.IdempotencyKey);
                cmd.Parameters.AddWithValue("@hash", runEntity.InputSnapshotHash);
                cmd.Parameters.AddWithValue("@state", runEntity.State);
                cmd.Parameters.AddWithValue("@cur", runEntity.CurrentStep);
                cmd.Parameters.AddWithValue("@sid", runEntity.ActorSid);
                cmd.Parameters.AddWithValue("@now", runEntity.CreatedUtc);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            var finishedRun = await _rpaEngine.ExecuteScriptAsync(runEntity, steps, request.VariableBindings, request.StepByStepMode, null, ct);

            using (var conn = _dbContext.CreateConnection())
            using (var cmd = conn.CreateCommand())
            {
                cmd.CommandText = "UPDATE RpaRun SET state=@state, currentStep=@cur WHERE id=@id;";
                cmd.Parameters.AddWithValue("@state", finishedRun.State);
                cmd.Parameters.AddWithValue("@cur", finishedRun.CurrentStep);
                cmd.Parameters.AddWithValue("@id", finishedRun.Id);
                await cmd.ExecuteNonQueryAsync(ct);
            }

            return OperationResult<RunInfo>.Ok(new RunInfo(
                finishedRun.Id, finishedRun.ScriptVersionId, finishedRun.State, finishedRun.CurrentStep, finishedRun.ExternalAttemptId
            ), actor.CorrelationId);
        }

        public async Task<OperationResult<RunInfo>> ControlAsync(RunControlRequest request, ActorContext actor, CancellationToken ct = default)
        {
            await Task.CompletedTask;
            return OperationResult<RunInfo>.Fail(ErrorCodes.NotConfigured,"Bộ chạy mô phỏng chưa hỗ trợ điều khiển worker đang chạy; không ép trạng thái DB thành thành công.",actor.CorrelationId);
        }
    }

    // CMP-16: OperationsService
    public class OperationsService : IOperationsService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public OperationsService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<ConfigInfo>> SaveConfigAsync(ConfigRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string id = Guid.NewGuid().ToString("N");
            string hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.PayloadJson))).ToLowerInvariant();
            string now = DateTime.UtcNow.ToString("o");

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO ConfigVersion (id, kind, versionNo, payloadJson, contentHash, createdUtc)
VALUES (@id, @kind, (SELECT COALESCE(MAX(versionNo),0)+1 FROM ConfigVersion WHERE kind=@kind), @payload, @hash, @now);";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@kind", request.Kind);
            cmd.Parameters.AddWithValue("@payload", request.PayloadJson);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@now", now);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<ConfigInfo>.Ok(new ConfigInfo(id, request.Kind, 1, hash), actor.CorrelationId);
        }

        public async Task<OperationResult<AuditPage>> QueryAuditAsync(AuditQuery query, ActorContext actor, CancellationToken ct = default)
        {
            var list = new List<AuditItemDto>();
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT sequenceNo, actorSid, action, objectType, objectId, resultCode, correlationId, createdUtc FROM AuditEvent ORDER BY sequenceNo DESC LIMIT @lim;";
            cmd.Parameters.AddWithValue("@lim", query.Limit > 0 ? query.Limit : 50);

            using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                list.Add(new AuditItemDto(
                    reader.GetInt64(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetString(3),
                    reader.GetString(4),
                    reader.GetString(5),
                    reader.GetString(6),
                    reader.GetString(7)
                ));
            }

            return OperationResult<AuditPage>.Ok(new AuditPage(list, list.Count), actor.CorrelationId);
        }

        public async Task<OperationResult<BackupInfo>> BackupAsync(BackupRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string backupId = Guid.NewGuid().ToString("N");
            string zipName = $"SACD_Backup_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{backupId[..6]}.zip";
            string targetZip = Path.Combine(request.DestinationDirectory, zipName);
            string? dir = Path.GetDirectoryName(targetZip);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            string workspace = Path.GetFullPath(_dbContext.WorkspaceDirectory).TrimEnd(Path.DirectorySeparatorChar)+Path.DirectorySeparatorChar;
            if(Path.GetFullPath(targetZip).StartsWith(workspace,StringComparison.OrdinalIgnoreCase))
                return OperationResult<BackupInfo>.Fail(ErrorCodes.StorageFailure,"Chọn thư mục sao lưu nằm ngoài kho dữ liệu.",actor.CorrelationId);
            string snapshot = Path.Combine(Path.GetTempPath(),"SACD_Backup_"+backupId);
            Directory.CreateDirectory(snapshot);
            try
            {
                using var source = _dbContext.CreateConnection();
                using var destination = new SqliteConnection(new SqliteConnectionStringBuilder {DataSource=Path.Combine(snapshot,"sacd_workspace.db"),Pooling=false}.ToString());
                destination.Open();source.BackupDatabase(destination);
                using var objects = destination.CreateCommand();objects.CommandText="SELECT hash FROM FileObject";
                using var reader = objects.ExecuteReader();
                Directory.CreateDirectory(Path.Combine(snapshot,"objects"));
                while(await reader.ReadAsync(ct))
                {
                    string objectHash=reader.GetString(0);
                    _=new Sha256Hash(objectHash);
                    string original=Path.Combine(_dbContext.WorkspaceDirectory,"objects",objectHash);
                    using(var input=File.OpenRead(original))
                    {
                        string actual=Convert.ToHexString(await SHA256.HashDataAsync(input,ct)).ToLowerInvariant();
                        if(actual!=objectHash)throw new InvalidDataException("Tệp gốc không khớp mã băm khi sao lưu.");
                    }
                    File.Copy(original,Path.Combine(snapshot,"objects",objectHash));
                }
                reader.Close();destination.Close();
                ZipFile.CreateFromDirectory(snapshot,targetZip,CompressionLevel.Optimal,false);
            }
            finally { Directory.Delete(snapshot,true); }

            string hash;
            using (var sha = SHA256.Create())
            await using (var fs = File.OpenRead(targetZip))
            {
                byte[] b = await sha.ComputeHashAsync(fs, ct);
                hash = BitConverter.ToString(b).Replace("-", "").ToLowerInvariant();
            }

            string now = DateTime.UtcNow.ToString("o");
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO BackupRecord (id, manifestHash, relativeOrSelectedPath, state, drillState, actorSid, createdUtc)
VALUES (@id, @hash, @path, 'Verified', 'NotRun', @sid, @now);";
            cmd.Parameters.AddWithValue("@id", backupId);
            cmd.Parameters.AddWithValue("@hash", hash);
            cmd.Parameters.AddWithValue("@path", targetZip);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@now", now);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<BackupInfo>.Ok(new BackupInfo(
                backupId, targetZip, hash, new FileInfo(targetZip).Length, "Verified", now
            ), actor.CorrelationId);
        }

        public Task<OperationResult<RestoreInfo>> RestoreAsync(RestoreRequest request, ActorContext actor, CancellationToken ct = default)
            => WorkspaceRecovery.RestoreAsync(request, actor, ct);
    }

    // CMP-17: AiAssistant (Optional, default disabled)
    public class AiAssistant : IAiAssistant
    {
        public bool IsEnabled => false; // AI mặc định tắt theo BR-40 và tài liệu đề tài

        public Task<OperationResult<AiProposal>> ProposeAsync(AiRequest request, ActorContext actor, CancellationToken ct = default)
        {
            if (!IsEnabled)
            {
                return Task.FromResult(OperationResult<AiProposal>.Fail(
                    ErrorCodes.NotConfigured,
                    "Module AI cục bộ mặc định tắt. Hệ thống hoạt động độc lập hoàn toàn với dữ liệu trên máy mà không cần AI.",
                    actor.CorrelationId
                ));
            }

            return Task.FromResult(OperationResult<AiProposal>.Ok(new AiProposal(
                ProposalText: "Đề xuất tóm tắt nghiệp vụ.",
                ExtractedReferences: Array.Empty<string>(),
                ConfidenceScore: 0.95,
                Available: true
            ), actor.CorrelationId));
        }
    }

    // CMP-18: DirectoryService
    public class DirectoryService : IDirectoryService
    {
        private readonly SqliteDatabaseContext _dbContext;

        public DirectoryService(SqliteDatabaseContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<OperationResult<DirectoryVersionInfo>> PublishAsync(DirectoryRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string id = Guid.NewGuid().ToString("N");
            string logicalId = $"DIR-{request.Kind}-{request.Code}";
            string now = DateTime.UtcNow.ToString("o");

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO DirectoryEntryVersion (id, logicalId, versionNo, kind, code, name, parentVersionId, active, createdUtc)
VALUES (@id, @logId, (SELECT COALESCE(MAX(versionNo),0)+1 FROM DirectoryEntryVersion WHERE kind=@kind AND code=@code), @kind, @code, @name, @parent, @active, @now);";
            cmd.Parameters.AddWithValue("@id", id);
            cmd.Parameters.AddWithValue("@logId", logicalId);
            cmd.Parameters.AddWithValue("@kind", request.Kind);
            cmd.Parameters.AddWithValue("@code", request.Code);
            cmd.Parameters.AddWithValue("@name", request.Name);
            cmd.Parameters.AddWithValue("@parent", (object?)request.ParentVersionId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@active", request.Active ? 1 : 0);
            cmd.Parameters.AddWithValue("@now", now);
            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<DirectoryVersionInfo>.Ok(new DirectoryVersionInfo(
                id, logicalId, 1, request.Kind, request.Code, request.Name, request.Active
            ), actor.CorrelationId);
        }

        public async Task<OperationResult<bool>> RetireAsync(string entryId, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE DirectoryEntryVersion SET active=0 WHERE id=@id;";
            cmd.Parameters.AddWithValue("@id", entryId);
            int rows = await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<bool>.Ok(rows > 0, actor.CorrelationId);
        }
    }
}
