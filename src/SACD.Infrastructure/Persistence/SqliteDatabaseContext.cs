using System;
using System.IO;
using System.Threading;
using Microsoft.Data.Sqlite;
using SACD.Application.Common;
using SACD.Domain.Rules;

namespace SACD.Infrastructure.Persistence
{
    public class SqliteDatabaseContext : IDisposable
    {
        private readonly string _workspaceDirectory;
        private readonly string _dbFilePath;
        private readonly string _lockFilePath;
        private FileStream? _lockStream;
        private readonly object _connectionSync = new();

        public string WorkspaceDirectory => _workspaceDirectory;
        public string DatabasePath => _dbFilePath;

        public SqliteDatabaseContext(string workspaceDirectory)
        {
            if (string.IsNullOrWhiteSpace(workspaceDirectory))
                throw new ArgumentException("Workspace directory cannot be empty.", nameof(workspaceDirectory));

            _workspaceDirectory = Path.GetFullPath(workspaceDirectory);
            Directory.CreateDirectory(_workspaceDirectory);

            _dbFilePath = Path.Combine(_workspaceDirectory, "sacd_workspace.db");
            _lockFilePath = Path.Combine(_workspaceDirectory, "sacd.lock");
        }

        /// <summary>
        /// BR-01: Acquire exclusive single-writer file lock on the workspace.
        /// </summary>
        public bool TryAcquireWriterLock()
        {
            if (_lockStream != null) return true;

            try
            {
                _lockStream = new FileStream(_lockFilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
                return true;
            }
            catch (IOException)
            {
                return false;
            }
        }

        public void ReleaseWriterLock()
        {
            if (_lockStream != null)
            {
                _lockStream.Dispose();
                _lockStream = null;
                try { if (File.Exists(_lockFilePath)) File.Delete(_lockFilePath); } catch { }
            }
        }

        public SqliteConnection CreateConnection()
        {
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = _dbFilePath,
                Mode = SqliteOpenMode.ReadWriteCreate,
                ForeignKeys = true,
                Cache = SqliteCacheMode.Shared
            };

            var connection = new SqliteConnection(builder.ConnectionString);
            connection.Open();

            using var cmd = connection.CreateCommand();
            cmd.CommandText = "PRAGMA foreign_keys=ON; PRAGMA journal_mode=WAL;";
            cmd.ExecuteNonQuery();

            return connection;
        }

        public void EnsureInitialized()
        {
            lock (_connectionSync)
            {
                using var conn = CreateConnection();
                DatabaseInitializer.InitializeDatabase(conn);
            }
        }

        public void Dispose()
        {
            ReleaseWriterLock();
        }
    }
}
