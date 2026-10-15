using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.Sqlite;
using SACD.Application.Common;
using SACD.Application.Contracts;
using SACD.Domain.Entities;
using SACD.Domain.Rules;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Rpa;
using SACD.Infrastructure.Services;
using SACD.Infrastructure.Storage;
using SACD.Infrastructure.Text;
using SACD.Infrastructure.Transfer;
using SACD.Infrastructure.Word;
using Xunit;

namespace SACD.Tests
{
    public class SacdFullTestSuite : IDisposable
    {
        private readonly string _testWorkspaceDir;

        public SacdFullTestSuite()
        {
            _testWorkspaceDir = Path.Combine(Path.GetTempPath(), "SACD_Test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testWorkspaceDir);
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_testWorkspaceDir))
                {
                    Directory.Delete(_testWorkspaceDir, true);
                }
            }
            catch { }
        }

        [Fact]
        public void Database_ShouldInitializeAll50TablesAndEnforceForeignKeys()
        {
            using var dbContext = new SqliteDatabaseContext(_testWorkspaceDir);
            dbContext.EnsureInitialized();

            using var conn = dbContext.CreateConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
            long tableCount = (long)cmd.ExecuteScalar()!;

            // Should have 50 business tables + FTS tables
            Assert.True(tableCount >= 50, $"Expected at least 50 tables, got {tableCount}");

            // Verify foreign key pragma
            using var cmdPragma = conn.CreateCommand();
            cmdPragma.CommandText = "PRAGMA foreign_keys;";
            long fkEnabled = (long)cmdPragma.ExecuteScalar()!;
            Assert.Equal(1, fkEnabled);

            // Verify FK violation rejection (BR-01 / Invariants)
            using var cmdInvalidFk = conn.CreateCommand();
            cmdInvalidFk.CommandText = @"
INSERT INTO Document (id, dossierId, objectHash, originalName, sourceKind, status, createdUtc)
VALUES ('doc-1', 'non-existent-dossier', '0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef', 'test.pdf', 'File', 'Imported', datetime('now'));";

            Assert.Throws<SqliteException>(() => cmdInvalidFk.ExecuteNonQuery());
        }

        [Fact]
        public void Database_Fts5TextSearch_ShouldSupportVietnameseWithAndWithoutAccents()
        {
            using var dbContext = new SqliteDatabaseContext(_testWorkspaceDir);
            dbContext.EnsureInitialized();

            using var conn = dbContext.CreateConnection();
            using var cmdInsert = conn.CreateCommand();
            string raw = "UBND Tỉnh Lào Cai ban hành quyết định xử lý văn bản khẩn cấp.";
            string searchText = VietnameseTextNormalizer.PrepareSearchText(raw);

            cmdInsert.CommandText = @"
INSERT INTO TextSearch (documentId, revisionId, blockId, rawTextNfc, searchText)
VALUES ('doc-test', 'rev-test', 'blk-test', @raw, @search);";
            cmdInsert.Parameters.AddWithValue("@raw", raw);
            cmdInsert.Parameters.AddWithValue("@search", searchText);
            cmdInsert.ExecuteNonQuery();

            // Search with unaccented keyword
            using var cmdSearch = conn.CreateCommand();
            string keyword = VietnameseTextNormalizer.RemoveDiacritics("Lao Cai").Trim();
            cmdSearch.CommandText = "SELECT COUNT(*) FROM TextSearch WHERE TextSearch MATCH @term;";
            cmdSearch.Parameters.AddWithValue("@term", $"\"{keyword}\"*");
            long count = (long)cmdSearch.ExecuteScalar()!;

            Assert.True(count > 0, "FTS5 query with unaccented keyword 'Lao Cai' should match accented 'Lào Cai'.");
        }

        [Fact]
        public void Domain_ExactRatio_ShouldComputeExactDecimalsWithoutIntermediateRounding()
        {
            var ratio = new ExactRatio(1, 3);
            Assert.Equal("1/3", ratio.ToString());
            Assert.Equal(0.3333m, ratio.ToDecimal(4));
            Assert.Equal("33.33%", ratio.ToPercentageString(2));

            // BR-29: Denominator = 0 throws DivideByZeroException
            Assert.Throws<DivideByZeroException>(() => new ExactRatio(10, 0));
        }

        [Fact]
        public void Domain_DeadlineCalculator_ShouldEvaluateUrgencyAccurately()
        {
            var nowUtc = new DateTime(2026, 10, 15, 12, 0, 0, DateTimeKind.Utc);
            var warningWindow = TimeSpan.FromHours(24);

            // Overdue task
            var overdueStatus = DeadlineCalculator.Evaluate(
                "Date", "2026-10-14", null, null, nowUtc, warningWindow
            );
            Assert.Equal(DeadlineUrgency.Overdue, overdueStatus.Urgency);
            Assert.True(overdueStatus.IsOverdue);

            // Upcoming soon task (within 24 hours)
            var upcomingStatus = DeadlineCalculator.Evaluate(
                "Instant", null, "2026-10-15T18:00:00Z", null, nowUtc, warningWindow
            );
            Assert.Equal(DeadlineUrgency.UpcomingSoon, upcomingStatus.Urgency);
            Assert.False(upcomingStatus.IsOverdue);

            // On-time task (in 5 days)
            var onTimeStatus = DeadlineCalculator.Evaluate(
                "Date", "2026-10-20", null, null, nowUtc, warningWindow
            );
            Assert.Equal(DeadlineUrgency.OnTime, onTimeStatus.Urgency);
            Assert.False(onTimeStatus.IsOverdue);

            // Completed task
            var completedStatus = DeadlineCalculator.Evaluate(
                "Date", "2026-10-10", null, "2026-10-10T10:00:00Z", nowUtc, warningWindow
            );
            Assert.Equal(DeadlineUrgency.Completed, completedStatus.Urgency);
        }

        [Fact]
        public async Task FileStore_ShouldStoreFilesAndDeduplicateByHash()
        {
            var fileStore = new LocalFileStore(_testWorkspaceDir);
            string testContent = "Nội dung văn bản thử nghiệm SACD v1.1";
            string testFilePath = Path.Combine(_testWorkspaceDir, "sample.txt");
            await File.WriteAllTextAsync(testFilePath, testContent, Encoding.UTF8);

            var obj1 = await fileStore.StoreFileAsync(testFilePath);
            Assert.NotNull(obj1.Hash);
            Assert.Equal(64, obj1.Hash.Length);
            Assert.True(fileStore.ObjectExists(obj1.Hash));

            // Storing same file again produces same SHA256 hash
            var obj2 = await fileStore.StoreFileAsync(testFilePath);
            Assert.Equal(obj1.Hash, obj2.Hash);
        }

        [Fact]
        public async Task WordReportGenerator_ShouldCreateOpenXmlDocxWithValidSha256()
        {
            var wordGen = new WordReportGenerator();
            string outDocx = Path.Combine(_testWorkspaceDir, "TestReport.docx");

            var revision = new ReportRevisionEntity
            {
                Id = "rev-01",
                ReportId = "rep-01",
                VersionNo = 1,
                State = "Draft"
            };

            var sections = new List<ReportSectionEntity>
            {
                new() { Id = "s1", SectionKey = "I. Tình hình tiếp nhận", Ordinal = 1, Text = "Đã tiếp nhận 15 văn bản trong kỳ." },
                new() { Id = "s2", SectionKey = "II. Kết quả xử lý", Ordinal = 2, Text = "Đã xử lý đúng hạn 14/15 văn bản." }
            };

            var claims = new List<ClaimEntity>
            {
                new() { Id = "c1", Kind = "Factual", Text = "15 văn bản tiếp nhận", Checked = 1 },
                new() { Id = "c2", Kind = "Commentary", Text = "Tiến độ xử lý đạt yêu cầu", Checked = 1, CommentaryReason = "Căn cứ thống kê hạn" }
            };

            var (path, hash) = await wordGen.GenerateReportDocxAsync(
                outDocx, revision, sections, claims, Array.Empty<FactVersionEntity>(), "Draft"
            );

            Assert.True(File.Exists(path));
            Assert.Equal(64, hash.Length);
            Assert.True(new FileInfo(path).Length > 0);
        }

        [Fact]
        public async Task UsbTransferManager_ShouldCreatePackageAndVerifyManifestIntegrity()
        {
            var fileStore = new LocalFileStore(_testWorkspaceDir);
            string dummyFile = Path.Combine(_testWorkspaceDir, "transfer_test.txt");
            await File.WriteAllTextAsync(dummyFile, "Tài liệu chuyển giao USB bảo mật", Encoding.UTF8);
            var fileObj = await fileStore.StoreFileAsync(dummyFile);

            var doc = new DocumentEntity
            {
                Id = "doc-transfer-1",
                DossierId = "dos-1",
                ObjectHash = fileObj.Hash,
                OriginalName = "transfer_test.txt",
                CurrentRevisionId = "rev-1"
            };

            var transferMgr = new UsbTransferManager();
            string zipPath = Path.Combine(_testWorkspaceDir, "package.zip");

            var (createdZip, manifestHash, pkg, items) = await transferMgr.CreatePackageAsync(
                zipPath, "ws-origin-1", new[] { doc }, fileStore
            );

            Assert.True(File.Exists(createdZip));
            Assert.Equal(64, manifestHash.Length);
            Assert.Single(items);

            var (manifest, inspectedHash, hashValid) = await transferMgr.InspectPackageAsync(createdZip);
            Assert.True(hashValid);
            Assert.Equal(manifestHash, inspectedHash);
            Assert.Single(manifest.Items);
        }

        [Fact]
        public async Task RpaEngine_ShouldExecuteStepsAndRespectPauseForUser()
        {
            var engine = new RpaEngine();
            var run = new RpaRunEntity
            {
                Id = "run-01",
                ScriptVersionId = "ver-01",
                State = "Queued",
                CurrentStep = 0
            };

            var steps = new List<ScriptStepEntity>
            {
                new() { Ordinal = 1, Action = "WaitWindow", ParamsJson = "{}" },
                new() { Ordinal = 2, Action = "PauseForUser", ParamsJson = "{}" },
                new() { Ordinal = 3, Action = "Click", ParamsJson = "{}" }
            };

            bool pauseTriggered = false;
            var result = await engine.ExecuteScriptAsync(run, steps, new Dictionary<string, string>(), false, step =>
            {
                if (step.Action == "PauseForUser")
                {
                    pauseTriggered = true;
                    return Task.FromResult(true); // Resume
                }
                return Task.FromResult(false);
            });

            Assert.True(pauseTriggered, "PauseForUser must be called and triggered.");
            Assert.Equal("Succeeded", result.State);
            Assert.Equal(3, result.CurrentStep);
        }

        [Fact]
        public async Task WorkflowService_RowVersionConflict_ShouldBeDetectedAndPreventOverwrites()
        {
            using var dbContext = new SqliteDatabaseContext(_testWorkspaceDir);
            dbContext.EnsureInitialized();

            var service = new WorkflowService(dbContext);
            var actor = ActorContext.Create("S-1-TEST", "ws-01");

            // Create initial dossier
            using var conn = dbContext.CreateConnection();
            using var cmdDos = conn.CreateCommand();
            cmdDos.CommandText = "INSERT INTO Dossier (id, businessCode, state, receivedUtc) VALUES ('dossier-task-1', 'HS-TASK-01', 'Received', datetime('now'));";
            cmdDos.ExecuteNonQuery();

            // Create Task with RowVersion = 1
            var createRes = await service.SaveTaskAsync(new TaskRequest(
                DossierId: "dossier-task-1",
                TaskId: null,
                ExpectedRowVersion: 0,
                AssigneeVersionId: null,
                Name: "Nhiệm vụ soạn thảo báo cáo",
                DueKind: "Date",
                DueDate: "2026-10-30",
                DueUtc: null,
                EvidenceId: null
            ), actor);

            Assert.True(createRes.Success);
            string taskId = createRes.Value!.TaskId;

            // Update with wrong row version should fail with RevisionConflict (BR-18)
            var conflictRes = await service.SaveTaskAsync(new TaskRequest(
                DossierId: "dossier-task-1",
                TaskId: taskId,
                ExpectedRowVersion: 999, // Wrong version
                AssigneeVersionId: null,
                Name: "Nhiệm vụ cập nhật",
                DueKind: "Date",
                DueDate: "2026-10-31",
                DueUtc: null,
                EvidenceId: null
            ), actor);

            Assert.False(conflictRes.Success);
            Assert.Equal(ErrorCodes.RevisionConflict, conflictRes.Code);
        }
    }
}
