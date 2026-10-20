using System;
using System.Collections.Generic;
using System.IO;
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
    // CMP-01: WorkspaceService
    public class WorkspaceService : IWorkspaceService
    {
        private readonly SqliteDatabaseContext _dbContext;
        private readonly IAuditLogger _auditLogger;

        public WorkspaceService(SqliteDatabaseContext dbContext, IAuditLogger? auditLogger = null)
        {
            _dbContext = dbContext;
            _auditLogger = auditLogger ?? new AuditLogger();
        }

        public async Task<OperationResult<WorkspaceInfo>> OpenAsync(WorkspaceOpenRequest request, ActorContext actor, CancellationToken ct = default)
        {
            if (!_dbContext.TryAcquireWriterLock())
            {
                return OperationResult<WorkspaceInfo>.Fail(ErrorCodes.WorkspaceBusy, "Kho đang bị phiên làm việc khác khóa.", actor.CorrelationId);
            }

            var accessError = WorkspaceRecovery.CheckExistingOwner(_dbContext.DatabasePath, actor.WindowsSid, request.ExpectedSchemaVersion);
            if (accessError != null)
            {
                _dbContext.ReleaseWriterLock();
                return OperationResult<WorkspaceInfo>.Fail(ErrorCodes.AccessDenied, accessError, actor.CorrelationId);
            }
            _dbContext.EnsureInitialized();

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT id, ownerSid, machineProfile, schemaVersion, createdUtc FROM Workspace LIMIT 1;";
            using var reader = await cmd.ExecuteReaderAsync(ct);
            if (await reader.ReadAsync(ct))
            {
                var ws = new WorkspaceEntity
                {
                    Id = reader.GetString(0),
                    OwnerSid = reader.GetString(1),
                    MachineProfile = reader.GetString(2),
                    SchemaVersion = reader.GetInt32(3),
                    CreatedUtc = reader.GetString(4)
                };

                BusinessRuleValidator.ValidateWorkspace(ws, request.ExpectedSchemaVersion);

                await _auditLogger.LogAsync(conn, null, actor, "OpenWorkspace", "Workspace", ws.Id, null, "Success", ct);

                return OperationResult<WorkspaceInfo>.Ok(new WorkspaceInfo(
                    ws.Id, _dbContext.WorkspaceDirectory, ws.OwnerSid, ws.MachineProfile, ws.SchemaVersion, ws.CreatedUtc
                ), actor.CorrelationId);
            }

            // If empty, auto-initialize
            var initRes = await InitializeNewAsync(_dbContext.WorkspaceDirectory,
                request.Mode is "Intake" or "Management" or "Test" ? request.Mode : "Intake", actor, ct);
            return initRes;
        }

        public async Task<OperationResult<WorkspaceInfo>> InitializeNewAsync(string directoryPath, string machineProfile, ActorContext actor, CancellationToken ct = default)
        {
            if (!_dbContext.TryAcquireWriterLock())
            {
                return OperationResult<WorkspaceInfo>.Fail(ErrorCodes.WorkspaceBusy, "Kho đang bị phiên làm việc khác khóa.", actor.CorrelationId);
            }

            _dbContext.EnsureInitialized();

            var ws = new WorkspaceEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                OwnerSid = actor.WindowsSid,
                MachineProfile = machineProfile,
                SchemaVersion = 1,
                CreatedUtc = DateTime.UtcNow.ToString("o")
            };

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO Workspace (id, ownerSid, machineProfile, schemaVersion, createdUtc)
VALUES (@id, @ownerSid, @profile, @schema, @createdUtc);";
            cmd.Parameters.AddWithValue("@id", ws.Id);
            cmd.Parameters.AddWithValue("@ownerSid", ws.OwnerSid);
            cmd.Parameters.AddWithValue("@profile", ws.MachineProfile);
            cmd.Parameters.AddWithValue("@schema", ws.SchemaVersion);
            cmd.Parameters.AddWithValue("@createdUtc", ws.CreatedUtc);

            await cmd.ExecuteNonQueryAsync(ct);

            return OperationResult<WorkspaceInfo>.Ok(new WorkspaceInfo(
                ws.Id, directoryPath, ws.OwnerSid, ws.MachineProfile, ws.SchemaVersion, ws.CreatedUtc
            ), actor.CorrelationId);
        }

        public Task<OperationResult<bool>> CheckLockAsync(string directoryPath, CancellationToken ct = default)
        {
            string lockPath = Path.Combine(directoryPath, "sacd.lock");
            bool locked = false;
            try
            {
                using var stream = new FileStream(lockPath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            }
            catch (IOException)
            {
                locked = true;
            }
            return Task.FromResult(OperationResult<bool>.Ok(locked, Guid.NewGuid().ToString("N")));
        }
    }

    // CMP-02: IntakeService
    public class IntakeService : IIntakeService
    {
        private readonly SqliteDatabaseContext _dbContext;
        private readonly IFileStore _fileStore;
        private readonly IAuditLogger _auditLogger;

        public IntakeService(SqliteDatabaseContext dbContext, IFileStore fileStore, IAuditLogger? auditLogger = null)
        {
            _dbContext = dbContext;
            _fileStore = fileStore;
            _auditLogger = auditLogger ?? new AuditLogger();
        }

        public async Task<OperationResult<ImportBatchResult>> ImportAsync(ImportRequest request, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            string dossierId = request.DossierId ?? Guid.NewGuid().ToString("N");
            string businessCode = $"HS-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";

            if (request.DossierId == null)
            {
                using var cmdDossier = conn.CreateCommand();
                cmdDossier.Transaction = tx;
                cmdDossier.CommandText = @"
INSERT INTO Dossier (id, businessCode, state, rowVersion, receivedUtc)
VALUES (@id, @code, 'Received', 1, @now);";
                cmdDossier.Parameters.AddWithValue("@id", dossierId);
                cmdDossier.Parameters.AddWithValue("@code", businessCode);
                cmdDossier.Parameters.AddWithValue("@now", DateTime.UtcNow.ToString("o"));
                await cmdDossier.ExecuteNonQueryAsync(ct);
            }

            var itemResults = new List<ImportItemResult>();

            foreach (var path in request.FilePaths)
            {
                if (!File.Exists(path)) continue;

                // Validate signature
                try
                {
                    byte[] header = new byte[8];
                    using (var fs = File.OpenRead(path))
                    {
                        int read = fs.Read(header, 0, header.Length);
                        if (read >= 4)
                        {
                            BusinessRuleValidator.ValidateFileSignature(header, Path.GetExtension(path));
                        }
                    }
                }
                catch (DomainValidationException ex)
                {
                    return OperationResult<ImportBatchResult>.Fail(ex.ErrorCode, ex.Message, actor.CorrelationId);
                }

                var fileObj = await _fileStore.StoreFileAsync(path, ct);

                // Insert or ignore FileObject
                using var cmdFileObj = conn.CreateCommand();
                cmdFileObj.Transaction = tx;
                cmdFileObj.CommandText = @"
INSERT OR IGNORE INTO FileObject (hash, relativePath, sizeBytes, mediaType, createdUtc)
VALUES (@hash, @rel, @size, @media, @created);";
                cmdFileObj.Parameters.AddWithValue("@hash", fileObj.Hash);
                cmdFileObj.Parameters.AddWithValue("@rel", fileObj.RelativePath);
                cmdFileObj.Parameters.AddWithValue("@size", fileObj.SizeBytes);
                cmdFileObj.Parameters.AddWithValue("@media", fileObj.MediaType);
                cmdFileObj.Parameters.AddWithValue("@created", fileObj.CreatedUtc);
                await cmdFileObj.ExecuteNonQueryAsync(ct);

                // Check existing Document with same (dossierId, objectHash)
                using var cmdCheck = conn.CreateCommand();
                cmdCheck.Transaction = tx;
                cmdCheck.CommandText = "SELECT id FROM Document WHERE dossierId=@dossierId AND objectHash=@hash LIMIT 1;";
                cmdCheck.Parameters.AddWithValue("@dossierId", dossierId);
                cmdCheck.Parameters.AddWithValue("@hash", fileObj.Hash);
                var existingDocId = await cmdCheck.ExecuteScalarAsync(ct);

                string documentId = existingDocId != null ? existingDocId.ToString()! : Guid.NewGuid().ToString("N");
                bool isDedup = existingDocId != null;

                if (!isDedup)
                {
                    using var cmdDoc = conn.CreateCommand();
                    cmdDoc.Transaction = tx;
                    cmdDoc.CommandText = @"
INSERT INTO Document (id, dossierId, objectHash, originalName, sourceKind, sourceRef, status, createdUtc)
VALUES (@id, @dossierId, @hash, @name, @sourceKind, @sourceRef, 'Imported', @created);";
                    cmdDoc.Parameters.AddWithValue("@id", documentId);
                    cmdDoc.Parameters.AddWithValue("@dossierId", dossierId);
                    cmdDoc.Parameters.AddWithValue("@hash", fileObj.Hash);
                    cmdDoc.Parameters.AddWithValue("@name", Path.GetFileName(path));
                    cmdDoc.Parameters.AddWithValue("@sourceKind", request.SourceKind);
                    cmdDoc.Parameters.AddWithValue("@sourceRef", (object?)request.SourceRef ?? DBNull.Value);
                    cmdDoc.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("o"));
                    await cmdDoc.ExecuteNonQueryAsync(ct);
                }

                itemResults.Add(new ImportItemResult(
                    OriginalPath: path,
                    DocumentId: documentId,
                    ObjectHash: fileObj.Hash,
                    SizeBytes: fileObj.SizeBytes,
                    Deduplicated: isDedup,
                    Status: "Imported"
                ));
            }

            await _auditLogger.LogAsync(conn, tx, actor, "ImportDocuments", "Dossier", dossierId, null, "Success", ct);
            await tx.CommitAsync(ct);

            return OperationResult<ImportBatchResult>.Ok(new ImportBatchResult(
                dossierId, businessCode, itemResults
            ), actor.CorrelationId);
        }

        public async Task<OperationResult<ImportBatchResult>> ReceiveMailAsync(MailIntakeRequest request, ActorContext actor, CancellationToken ct = default)
        {
            return await ImportAsync(new ImportRequest(
                FilePaths: request.SelectedAttachmentPaths,
                DossierId: null,
                SourceKind: "Mail",
                SourceRef: $"Email from {request.Sender}: {request.Subject} ({request.ReceivedUtc})",
                LimitsConfigVersion: 1
            ), actor, ct);
        }
    }

    // CMP-03: TextService
    public class TextService : ITextService
    {
        private readonly SqliteDatabaseContext _dbContext;
        private readonly IFileStore _fileStore;
        private readonly IDocumentTextExtractor _textExtractor;
        private readonly IAuditLogger _auditLogger;

        public TextService(SqliteDatabaseContext dbContext, IFileStore fileStore, IDocumentTextExtractor textExtractor, IAuditLogger? auditLogger = null)
        {
            _dbContext = dbContext;
            _fileStore = fileStore;
            _textExtractor = textExtractor;
            _auditLogger = auditLogger ?? new AuditLogger();
        }

        public async Task<OperationResult<TextRevisionResult>> ExtractAsync(ExtractRequest request, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            using var cmdDoc = conn.CreateCommand();
            cmdDoc.Transaction = tx;
            cmdDoc.CommandText = "SELECT objectHash FROM Document WHERE id=@id LIMIT 1;";
            cmdDoc.Parameters.AddWithValue("@id", request.DocumentId);
            var objHashObj = await cmdDoc.ExecuteScalarAsync(ct);
            if (objHashObj == null)
            {
                return OperationResult<TextRevisionResult>.Fail(ErrorCodes.NotFound, "Không tìm thấy văn bản.", actor.CorrelationId);
            }

            string objHash = objHashObj.ToString()!;
            string physicalPath = _fileStore.GetObjectAbsolutePath(objHash);

            string revisionId = Guid.NewGuid().ToString("N");
            int versionNo = 1;

            using var cmdVer = conn.CreateCommand();
            cmdVer.Transaction = tx;
            cmdVer.CommandText = "SELECT COALESCE(MAX(versionNo), 0) + 1 FROM TextRevision WHERE documentId=@id;";
            cmdVer.Parameters.AddWithValue("@id", request.DocumentId);
            versionNo = Convert.ToInt32(await cmdVer.ExecuteScalarAsync(ct));

            using var cmdName = conn.CreateCommand();
            cmdName.Transaction = tx;
            cmdName.CommandText = "SELECT originalName FROM Document WHERE id=@id;";
            cmdName.Parameters.AddWithValue("@id", request.DocumentId);
            string originalName = (string)(await cmdName.ExecuteScalarAsync(ct))!;
            var blocks = await _textExtractor.ExtractBlocksAsync(physicalPath, revisionId, ct, Path.GetExtension(originalName));

            var partialExtraction = blocks.Any(b => b.LocatorJson.Contains("needsOcr"));
            // Insert TextRevision
            using var cmdRev = conn.CreateCommand();
            cmdRev.Transaction = tx;
            cmdRev.CommandText = @"
INSERT INTO TextRevision (id, documentId, versionNo, method, reviewState, createdUtc)
VALUES (@id, @docId, @ver, 'Extract', @reviewState, @created);";
            cmdRev.Parameters.AddWithValue("@reviewState", partialExtraction ? "Partial" : "NeedsReview");
            cmdRev.Parameters.AddWithValue("@id", revisionId);
            cmdRev.Parameters.AddWithValue("@docId", request.DocumentId);
            cmdRev.Parameters.AddWithValue("@ver", versionNo);
            cmdRev.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("o"));
            await cmdRev.ExecuteNonQueryAsync(ct);

            var blockDtos = new List<TextBlockDto>();
            foreach (var b in blocks)
            {
                using var cmdBlock = conn.CreateCommand();
                cmdBlock.Transaction = tx;
                cmdBlock.CommandText = @"
INSERT INTO TextBlock (id, revisionId, ordinal, locatorJson, extractedText, rawTextNfc, searchText, reviewed)
VALUES (@id, @revId, @ord, @loc, @ext, @raw, @search, 0);";
                cmdBlock.Parameters.AddWithValue("@id", b.Id);
                cmdBlock.Parameters.AddWithValue("@revId", revisionId);
                cmdBlock.Parameters.AddWithValue("@ord", b.Ordinal);
                cmdBlock.Parameters.AddWithValue("@loc", b.LocatorJson);
                cmdBlock.Parameters.AddWithValue("@ext", b.ExtractedText);
                cmdBlock.Parameters.AddWithValue("@raw", b.RawTextNfc);
                cmdBlock.Parameters.AddWithValue("@search", b.SearchText);
                await cmdBlock.ExecuteNonQueryAsync(ct);

                // Insert into TextSearch FTS5
                using var cmdFts = conn.CreateCommand();
                cmdFts.Transaction = tx;
                cmdFts.CommandText = @"
INSERT INTO TextSearch (documentId, revisionId, blockId, rawTextNfc, searchText)
VALUES (@docId, @revId, @blockId, @raw, @search);";
                cmdFts.Parameters.AddWithValue("@docId", request.DocumentId);
                cmdFts.Parameters.AddWithValue("@revId", revisionId);
                cmdFts.Parameters.AddWithValue("@blockId", b.Id);
                cmdFts.Parameters.AddWithValue("@raw", b.RawTextNfc);
                cmdFts.Parameters.AddWithValue("@search", b.SearchText);
                await cmdFts.ExecuteNonQueryAsync(ct);

                blockDtos.Add(new TextBlockDto(b.Id, b.Ordinal, b.LocatorJson, b.ExtractedText, b.RawTextNfc, 0));
            }

            // Update Document currentRevisionId
            using var cmdUpdate = conn.CreateCommand();
            cmdUpdate.Transaction = tx;
            cmdUpdate.CommandText = "UPDATE Document SET currentRevisionId=@revId WHERE id=@id;";
            cmdUpdate.Parameters.AddWithValue("@revId", revisionId);
            cmdUpdate.Parameters.AddWithValue("@id", request.DocumentId);
            await cmdUpdate.ExecuteNonQueryAsync(ct);

            // BR-32: Mark any ReportRevision referencing old revisions as stale
            using var cmdStale = conn.CreateCommand();
            cmdStale.Transaction = tx;
            cmdStale.CommandText = @"
UPDATE ReportRevision 
SET isStale = 1 
WHERE id IN (
    SELECT reportRevisionId FROM ReportSource WHERE sourceRevisionId IN (
        SELECT id FROM TextRevision WHERE documentId = @docId AND id <> @newRevId
    )
);";
            cmdStale.Parameters.AddWithValue("@docId", request.DocumentId);
            cmdStale.Parameters.AddWithValue("@newRevId", revisionId);
            await cmdStale.ExecuteNonQueryAsync(ct);

            await _auditLogger.LogAsync(conn, tx, actor, "ExtractText", "Document", request.DocumentId, revisionId, "Success", ct);
            await tx.CommitAsync(ct);

            return OperationResult<TextRevisionResult>.Ok(new TextRevisionResult(
                revisionId, request.DocumentId, versionNo, partialExtraction ? "Partial" : "NeedsReview", blockDtos
            ), actor.CorrelationId);
        }

        public Task<OperationResult<TextRevisionResult>> OcrAsync(OcrRequest request, ActorContext actor, CancellationToken ct = default)
            => Task.FromResult(OperationResult<TextRevisionResult>.Fail(ErrorCodes.NotConfigured,
                "Chưa cấu hình OCR cục bộ. Trang scan không được coi là đã đọc hoặc đã đối chiếu.", actor.CorrelationId));

        public async Task<OperationResult<TextRevisionResult>> SaveReviewAsync(TextReviewRequest request, ActorContext actor, CancellationToken ct = default)
        {
            using var conn = _dbContext.CreateConnection();
            using var tx = conn.BeginTransaction();

            string nowUtc = DateTime.UtcNow.ToString("o");

            using var current = conn.CreateCommand();
            current.Transaction = tx;
            current.CommandText = "SELECT currentRevisionId FROM Document WHERE id=@id;";
            current.Parameters.AddWithValue("@id", request.DocumentId);
            if ((await current.ExecuteScalarAsync(ct)) as string != request.ExpectedRevisionId)
                return OperationResult<TextRevisionResult>.Fail(ErrorCodes.RevisionConflict, "Nội dung đã có phiên bản mới. Hãy tải lại văn bản.", actor.CorrelationId);
            using var references = conn.CreateCommand();
            references.Transaction = tx;
            references.CommandText = @"SELECT
                (SELECT COUNT(*) FROM EvidenceRef WHERE revisionId=@id) +
                (SELECT COUNT(*) FROM FactVersion WHERE sourceRevisionId=@id) +
                (SELECT COUNT(*) FROM ReportSource WHERE sourceRevisionId=@id)";
            references.Parameters.AddWithValue("@id", request.ExpectedRevisionId);
            if (Convert.ToInt64(await references.ExecuteScalarAsync(ct)) > 0)
                return OperationResult<TextRevisionResult>.Fail(ErrorCodes.RevisionConflict,
                    "Phiên bản đã được dùng làm căn cứ. Trích xuất thành phiên bản mới để sửa, không sửa đè nguồn đã tham chiếu.", actor.CorrelationId);
            using var partial = conn.CreateCommand();
            partial.Transaction = tx;
            partial.CommandText = "SELECT COUNT(*) FROM TextBlock WHERE revisionId=@id AND locatorJson LIKE '%needsOcr%';";
            partial.Parameters.AddWithValue("@id", request.ExpectedRevisionId);
            if (Convert.ToInt64(await partial.ExecuteScalarAsync(ct)) > 0)
                return OperationResult<TextRevisionResult>.Fail(ErrorCodes.NeedsReview,
                    "Tài liệu có trang scan/chưa đọc được. Cần OCR trước khi xác nhận toàn văn.", actor.CorrelationId);
            using var countBlocks = conn.CreateCommand();
            countBlocks.Transaction = tx;
            countBlocks.CommandText = "SELECT id FROM TextBlock WHERE revisionId=@id;";
            countBlocks.Parameters.AddWithValue("@id", request.ExpectedRevisionId);
            var expectedBlocks = new HashSet<string>();
            using (var r = await countBlocks.ExecuteReaderAsync(ct))
                while (await r.ReadAsync(ct)) expectedBlocks.Add(r.GetString(0));
            if (!expectedBlocks.SetEquals(request.EditedBlocks.Select(b => b.Id)) || request.EditedBlocks.Count != expectedBlocks.Count)
                return OperationResult<TextRevisionResult>.Fail(ErrorCodes.InvalidEvidence, "Phải đối chiếu đầy đủ các đoạn của đúng phiên bản.", actor.CorrelationId);

            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"
UPDATE TextRevision 
SET reviewState='Reviewed', reviewerSid=@sid, reviewedUtc=@now 
WHERE id=@id;";
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@now", nowUtc);
            cmd.Parameters.AddWithValue("@id", request.ExpectedRevisionId);
            int rows = await cmd.ExecuteNonQueryAsync(ct);

            if (rows == 0)
            {
                return OperationResult<TextRevisionResult>.Fail(ErrorCodes.NotFound, "Không tìm thấy phiên bản nội dung văn bản.", actor.CorrelationId);
            }

            // Mark blocks reviewed
            foreach (var b in request.EditedBlocks)
            {
                using var cmdB = conn.CreateCommand();
                cmdB.Transaction = tx;
                cmdB.CommandText = @"
UPDATE TextBlock 
SET rawTextNfc=@raw, searchText=@search, reviewed=1
WHERE id=@id AND revisionId=@rev;";
                cmdB.Parameters.AddWithValue("@rev", request.ExpectedRevisionId);
                string nfc = VietnameseTextNormalizer.NormalizeToNfc(b.RawTextNfc);
                cmdB.Parameters.AddWithValue("@raw", nfc);
                cmdB.Parameters.AddWithValue("@search", VietnameseTextNormalizer.PrepareSearchText(nfc));
                cmdB.Parameters.AddWithValue("@id", b.Id);
                await cmdB.ExecuteNonQueryAsync(ct);
                using var fts = conn.CreateCommand();
                fts.Transaction = tx;
                fts.CommandText = "UPDATE TextSearch SET rawTextNfc=@raw, searchText=@search WHERE blockId=@id AND revisionId=@rev;";
                fts.Parameters.AddWithValue("@raw", nfc);
                fts.Parameters.AddWithValue("@search", VietnameseTextNormalizer.PrepareSearchText(nfc));
                fts.Parameters.AddWithValue("@id", b.Id);
                fts.Parameters.AddWithValue("@rev", request.ExpectedRevisionId);
                await fts.ExecuteNonQueryAsync(ct);
            }

            await _auditLogger.LogAsync(conn, tx, actor, "ReviewText", "TextRevision", request.ExpectedRevisionId, null, "Success", ct);
            await tx.CommitAsync(ct);

            return OperationResult<TextRevisionResult>.Ok(new TextRevisionResult(
                request.ExpectedRevisionId, request.DocumentId, 1, "Reviewed", request.EditedBlocks
            ), actor.CorrelationId);
        }
    }

    // CMP-04: MetadataService
    public class MetadataService : IMetadataService
    {
        private readonly SqliteDatabaseContext _dbContext;
        private readonly IAuditLogger _auditLogger;

        public MetadataService(SqliteDatabaseContext dbContext, IAuditLogger? auditLogger = null)
        {
            _dbContext = dbContext;
            _auditLogger = auditLogger ?? new AuditLogger();
        }

        public async Task<OperationResult<MetadataResult>> SaveMetadataAsync(MetadataRequest request, ActorContext actor, CancellationToken ct = default)
        {
            var meta = new MetadataVersionEntity
            {
                Id = Guid.NewGuid().ToString("N"),
                DossierId = request.DossierId,
                FieldKey = request.FieldKey,
                VersionNo = request.ExpectedVersion,
                ValueJson = request.TypedValueJson,
                Status = request.Status,
                EvidenceId = request.EvidenceId,
                ActorSid = actor.WindowsSid,
                CreatedUtc = DateTime.UtcNow.ToString("o")
            };

            BusinessRuleValidator.ValidateMetadataConfirmation(meta);

            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO MetadataVersion (id, dossierId, fieldKey, versionNo, valueJson, status, evidenceId, actorSid, createdUtc)
VALUES (@id, @dossierId, @fieldKey, @ver, @val, @status, @evId, @sid, @created);";
            cmd.Parameters.AddWithValue("@id", meta.Id);
            cmd.Parameters.AddWithValue("@dossierId", meta.DossierId);
            cmd.Parameters.AddWithValue("@fieldKey", meta.FieldKey);
            cmd.Parameters.AddWithValue("@ver", meta.VersionNo);
            cmd.Parameters.AddWithValue("@val", (object?)meta.ValueJson ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@status", meta.Status);
            cmd.Parameters.AddWithValue("@evId", (object?)meta.EvidenceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sid", meta.ActorSid);
            cmd.Parameters.AddWithValue("@created", meta.CreatedUtc);

            await cmd.ExecuteNonQueryAsync(ct);
            await _auditLogger.LogAsync(conn, null, actor, "SaveMetadata", "MetadataVersion", meta.Id, null, "Success", ct);

            return OperationResult<MetadataResult>.Ok(new MetadataResult(
                meta.DossierId, meta.FieldKey, meta.VersionNo, meta.Status, meta.ValueJson ?? "{}"
            ), actor.CorrelationId);
        }

        public async Task<OperationResult<string>> ClassifyAsync(ClassificationRequest request, ActorContext actor, CancellationToken ct = default)
        {
            string recordId = Guid.NewGuid().ToString("N");
            using var conn = _dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
INSERT INTO ClassificationRecord (id, dossierId, versionNo, classificationJson, milestoneDate, evidenceId, actorSid, createdUtc)
VALUES (@id, @dossierId, 1, @json, @milestone, @evId, @sid, @created);";
            cmd.Parameters.AddWithValue("@id", recordId);
            cmd.Parameters.AddWithValue("@dossierId", request.DossierId);
            cmd.Parameters.AddWithValue("@json", request.ClassificationJson);
            cmd.Parameters.AddWithValue("@milestone", (object?)request.MilestoneDate ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@evId", (object?)request.EvidenceId ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@sid", actor.WindowsSid);
            cmd.Parameters.AddWithValue("@created", DateTime.UtcNow.ToString("o"));

            await cmd.ExecuteNonQueryAsync(ct);
            await _auditLogger.LogAsync(conn, null, actor, "ClassifyDossier", "ClassificationRecord", recordId, null, "Success", ct);

            return OperationResult<string>.Ok(recordId, actor.CorrelationId);
        }
    }
}
