using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.Data.Sqlite;
using SACD.Application.Common;
using SACD.Application.Contracts;
using SACD.Domain.ValueObjects;

namespace SACD.Infrastructure.Services;

public static class WorkspaceRecovery
{
    public static string? CheckExistingOwner(string database, string sid, int schemaVersion)
    {
        if (!File.Exists(database)) return null;
        using var connection = new SqliteConnection(new SqliteConnectionStringBuilder
        { DataSource=database, Mode=SqliteOpenMode.ReadOnly, Pooling=false }.ToString());
        connection.Open();
        using var exists = connection.CreateCommand();
        exists.CommandText="SELECT COUNT(*) FROM sqlite_master WHERE name='Workspace' AND type='table';";
        if (Convert.ToInt64(exists.ExecuteScalar())==0) return "Kho đã tồn tại nhưng thiếu thông tin Workspace.";
        using var command=connection.CreateCommand();
        command.CommandText="SELECT ownerSid,schemaVersion FROM Workspace;";
        using var reader=command.ExecuteReader();
        if (!reader.Read()) return "Kho đã tồn tại nhưng chưa có chủ sở hữu.";
        if (!string.Equals(reader.GetString(0),sid,StringComparison.Ordinal)) return "Tài khoản Windows không phải chủ kho. Không tự đổi SID hoặc quyền thư mục.";
        if (reader.GetInt32(1)!=schemaVersion) return "Phiên bản schema chưa được hỗ trợ. Cần migration trước khi mở.";
        if (reader.Read()) return "Kho có nhiều bản ghi Workspace, cần kiểm tra dữ liệu.";
        return null;
    }

    public static async Task<OperationResult<RestoreInfo>> RestoreAsync(RestoreRequest request,ActorContext actor,CancellationToken ct)
    {
        var destination=Path.GetFullPath(request.NewWorkspaceDirectory).TrimEnd(Path.DirectorySeparatorChar);
        if (Directory.Exists(destination) || File.Exists(destination))
            return OperationResult<RestoreInfo>.Fail(ErrorCodes.StorageFailure,"Phục hồi cần thư mục đích mới, chưa tồn tại; không ghi đè kho đang dùng.",actor.CorrelationId);
        string parent=Path.GetDirectoryName(destination) ?? throw new ArgumentException("Thiếu thư mục cha.");
        Directory.CreateDirectory(parent);
        string staging=Path.Combine(parent,".sacd-restore-"+Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(staging);
        try
        {
            using (var archive=ZipFile.OpenRead(request.BackupZipPath))
            {
                if(archive.Entries.Count>10000) throw new InvalidDataException("Backup vượt 10.000 entry.");
                long total=0; var names=new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach(var entry in archive.Entries)
                {
                    ct.ThrowIfCancellationRequested();
                    string name=entry.FullName.Replace('\\','/');
                    if(!names.Add(name))throw new InvalidDataException("Backup có entry trùng.");
                    if(name=="objects/")continue;
                    if(name!="sacd_workspace.db" && !Regex.IsMatch(name,@"^objects/[a-f0-9]{64}$"))
                        throw new InvalidDataException("Backup có đường dẫn hoặc nội dung không được phép.");
                    if(entry.Length>1024L*1024*1024)throw new InvalidDataException("Entry quá lớn.");
                    string target=Path.Combine(staging,name.Replace('/',Path.DirectorySeparatorChar));
                    Directory.CreateDirectory(Path.GetDirectoryName(target)!);
                    await using var input=entry.Open();await using var output=new FileStream(target,FileMode.CreateNew);
                    byte[] buffer=new byte[81920];int read;
                    while((read=await input.ReadAsync(buffer,ct))>0)
                    {
                        total=checked(total+read);
                        if(total>1024L*1024*1024)throw new InvalidDataException("Backup giải nén vượt 1 GiB.");
                        await output.WriteAsync(buffer.AsMemory(0,read),ct);
                    }
                }
            }
            string database=Path.Combine(staging,"sacd_workspace.db");
            if(!File.Exists(database))throw new InvalidDataException("Thiếu cơ sở dữ liệu trong backup.");
            var accessError=CheckExistingOwner(database,actor.WindowsSid,1);
            if(accessError!=null)throw new InvalidDataException(accessError);
            string workspaceId;
            using(var connection=new SqliteConnection(new SqliteConnectionStringBuilder{DataSource=database,Mode=SqliteOpenMode.ReadOnly,Pooling=false}.ToString()))
            {
                connection.Open();using var check=connection.CreateCommand();check.CommandText="PRAGMA integrity_check;";
                if((string?)check.ExecuteScalar()!="ok")throw new InvalidDataException("SQLite integrity_check thất bại.");
                check.CommandText="PRAGMA foreign_key_check;";using(var reader=check.ExecuteReader())
                    if(reader.Read())throw new InvalidDataException("Backup có tham chiếu khóa ngoại không hợp lệ.");
                check.CommandText="SELECT id FROM Workspace LIMIT 1;";workspaceId=(string)check.ExecuteScalar()!;
                check.CommandText="SELECT hash,relativePath,sizeBytes FROM FileObject;";
                using var objects=check.ExecuteReader();
                while(objects.Read())
                {
                    string hash=new Sha256Hash(objects.GetString(0)).Value;
                    if(objects.GetString(1).Replace('\\','/')!="objects/"+hash)throw new InvalidDataException("Đường dẫn object không hợp lệ.");
                    string path=Path.Combine(staging,"objects",hash);
                    await using var file=File.OpenRead(path);
                    if(file.Length!=objects.GetInt64(2) || Convert.ToHexString(await SHA256.HashDataAsync(file,ct)).ToLowerInvariant()!=hash)
                        throw new InvalidDataException("Object thiếu, sai kích thước hoặc sai SHA256: "+hash);
                }
            }
            ct.ThrowIfCancellationRequested();
            Directory.Move(staging,destination);
            return OperationResult<RestoreInfo>.Ok(new(workspaceId,destination,true,"Đã kiểm tra chủ kho, schema, SQLite/FK và toàn bộ object trước khi phục hồi."),actor.CorrelationId);
        }
        catch(Exception ex) when(ex is IOException or InvalidDataException or SqliteException or ArgumentException)
        {
            return OperationResult<RestoreInfo>.Fail(ErrorCodes.PackageInvalid,ex.Message,actor.CorrelationId);
        }
        finally
        {
            if(Directory.Exists(staging) && Path.GetFullPath(staging).StartsWith(parent+Path.DirectorySeparatorChar,StringComparison.OrdinalIgnoreCase))
                Directory.Delete(staging,true);
        }
    }
}
