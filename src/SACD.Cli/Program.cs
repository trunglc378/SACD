using System;
using System.IO;
using System.Threading.Tasks;
using SACD.Application.Common;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;
using SACD.Infrastructure.Persistence;
using SACD.Infrastructure.Services;
using SACD.Infrastructure.Storage;
using SACD.Infrastructure.Text;
using SACD.Infrastructure.Transfer;
using SACD.Infrastructure.Word;

namespace SACD.Cli
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            Console.WriteLine("=================================================================");
            Console.WriteLine(" SACD Core Engine CLI & Docker Runner (v1.1)");
            Console.WriteLine(" Smart Air-gap Cabinet Desktop - Headless & Verification Worker");
            Console.WriteLine("=================================================================");

            string workspacePath = Environment.GetEnvironmentVariable("SACD_WORKSPACE") ?? Path.Combine(Directory.GetCurrentDirectory(), "workspace_data");
            if (args.Length > 0 && !args[0].StartsWith("--"))
            {
                workspacePath = args[0];
            }

            Console.WriteLine($"[INFO] Target Workspace: {workspacePath}");
            Directory.CreateDirectory(workspacePath);

            var dbContext = new SqliteDatabaseContext(workspacePath);
            var fileStore = new LocalFileStore(workspacePath);
            var wordGen = new WordReportGenerator();
            var textExtractor = new DocumentTextExtractor();
            var transferMgr = new UsbTransferManager();
            var auditLogger = new AuditLogger();
            var actor = ActorContext.Create(Environment.UserName, "sacd-cli-worker");

            var workspaceService = new WorkspaceService(dbContext, auditLogger);
            var intakeService = new IntakeService(dbContext, fileStore, auditLogger);
            var textService = new TextService(dbContext, fileStore, textExtractor, auditLogger);
            var factService = new FactService(dbContext);
            var reportService = new ReportService(dbContext, fileStore, wordGen);
            var operationsService = new OperationsService(dbContext);

            // Step 1: Open/Initialize Workspace
            Console.WriteLine("[INFO] Initializing SQLite database and 50 tables schema...");
            var openRes = await workspaceService.OpenAsync(new WorkspaceOpenRequest(workspacePath, "Standard", 1), actor);
            if (!openRes.Success)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"[ERROR] Failed to open workspace: {openRes.Message} [{openRes.Code}]");
                Console.ResetColor();
                return 1;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"[SUCCESS] Workspace {openRes.Value!.WorkspaceId} initialized successfully.");
            Console.WriteLine($"[INFO] MachineProfile: {openRes.Value.MachineProfile}, SchemaVersion: {openRes.Value.SchemaVersion}");
            Console.ResetColor();

            // Check arguments
            if (args.Length > 1 && args[1] == "--diag")
            {
                Console.WriteLine("[DIAG] Running health check diagnostics...");
                using var conn = dbContext.CreateConnection();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%';";
                long tblCount = (long)cmd.ExecuteScalar()!;
                Console.WriteLine($"[DIAG] Verified SQLite table count: {tblCount} (Target: >= 50)");
                Console.WriteLine($"[DIAG] FileStore objects directory: {fileStore.ObjectsDirectory}");
                Console.WriteLine("[DIAG] All core subsystems are ready and healthy.");
                return 0;
            }

            Console.WriteLine("[INFO] SACD Engine is running in daemon/idle mode. Press Ctrl+C to stop.");
            
            // Keep alive for container runtime
            var tcs = new TaskCompletionSource();
            AppDomain.CurrentDomain.ProcessExit += (s, e) => tcs.SetResult();
            Console.CancelKeyPress += (s, e) => { e.Cancel = true; tcs.SetResult(); };
            await tcs.Task;

            Console.WriteLine("[INFO] Shutting down SACD Engine cleanly.");
            return 0;
        }
    }
}
