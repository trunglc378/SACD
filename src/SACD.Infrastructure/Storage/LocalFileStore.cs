using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using SACD.Domain.Entities;
using SACD.Domain.ValueObjects;

namespace SACD.Infrastructure.Storage
{
    public interface IFileStore
    {
        Task<FileObjectEntity> StoreFileAsync(string sourceFilePath, CancellationToken ct = default);
        Task<FileObjectEntity> StoreStreamAsync(Stream contentStream, string originalExtension, CancellationToken ct = default);
        string GetObjectAbsolutePath(string hash);
        bool ObjectExists(string hash);
        Stream OpenRead(string hash);
        string StagingDirectory { get; }
        string ObjectsDirectory { get; }
    }

    public class LocalFileStore : IFileStore
    {
        private readonly string _objectsDir;
        private readonly string _stagingDir;

        public string ObjectsDirectory => _objectsDir;
        public string StagingDirectory => _stagingDir;

        public LocalFileStore(string workspaceDirectory)
        {
            _objectsDir = Path.Combine(workspaceDirectory, "objects");
            _stagingDir = Path.Combine(workspaceDirectory, "staging");

            Directory.CreateDirectory(_objectsDir);
            Directory.CreateDirectory(_stagingDir);
        }

        public async Task<FileObjectEntity> StoreFileAsync(string sourceFilePath, CancellationToken ct = default)
        {
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException($"Không tìm thấy tệp nguồn: {sourceFilePath}", sourceFilePath);

            using var stream = File.OpenRead(sourceFilePath);
            return await StoreStreamAsync(stream, Path.GetExtension(sourceFilePath), ct);
        }

        public async Task<FileObjectEntity> StoreStreamAsync(Stream contentStream, string originalExtension, CancellationToken ct = default)
        {
            // Stage first
            string stagingTempFile = Path.Combine(_stagingDir, $"{Guid.NewGuid():N}.tmp");
            string hashString;
            long totalBytes = 0;

            try
            {
                using (var sha256 = SHA256.Create())
                await using (var targetStream = new FileStream(stagingTempFile, FileMode.Create, FileAccess.Write, FileShare.None, 81920, true))
                {
                    byte[] buffer = new byte[81920];
                    int read;
                    while ((read = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) > 0)
                    {
                        await targetStream.WriteAsync(buffer, 0, read, ct);
                        sha256.TransformBlock(buffer, 0, read, null, 0);
                        totalBytes += read;
                        if (totalBytes > 50L * 1024 * 1024)
                            throw new InvalidDataException("Tệp vượt giới hạn 50 MiB.");
                    }
                    sha256.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                    hashString = BitConverter.ToString(sha256.Hash!).Replace("-", "").ToLowerInvariant();
                }

                _ = new Sha256Hash(hashString); // Validate 64 chars

                string relativeObjPath = Path.Combine("objects", hashString);
                string finalPath = Path.Combine(_objectsDir, hashString);

                if (!File.Exists(finalPath))
                {
                    File.Move(stagingTempFile, finalPath, true);
                }
                else
                {
                    File.Delete(stagingTempFile);
                }

                string mediaType = GetMediaTypeFromExtension(originalExtension);

                return new FileObjectEntity
                {
                    Hash = hashString,
                    RelativePath = relativeObjPath,
                    SizeBytes = totalBytes,
                    MediaType = mediaType,
                    CreatedUtc = DateTime.UtcNow.ToString("o")
                };
            }
            finally
            {
                if (File.Exists(stagingTempFile))
                {
                    try { File.Delete(stagingTempFile); } catch { }
                }
            }
        }

        public string GetObjectAbsolutePath(string hash)
        {
            return Path.Combine(_objectsDir, new Sha256Hash(hash).Value);
        }

        public bool ObjectExists(string hash)
        {
            return File.Exists(GetObjectAbsolutePath(hash));
        }

        public Stream OpenRead(string hash)
        {
            string path = GetObjectAbsolutePath(hash);
            if (!File.Exists(path))
                throw new FileNotFoundException($"Object with hash {hash} does not exist in store.");

            return File.OpenRead(path);
        }

        private static string GetMediaTypeFromExtension(string ext)
        {
            return ext.ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".tif" or ".tiff" => "image/tiff",
                ".zip" => "application/zip",
                _ => "application/octet-stream"
            };
        }
    }
}
