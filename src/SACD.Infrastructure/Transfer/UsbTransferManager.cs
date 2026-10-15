using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using SACD.Domain.Entities;
using SACD.Infrastructure.Storage;

namespace SACD.Infrastructure.Transfer
{
    public record TransferManifest(
        string PackageId,
        string OriginWorkspaceId,
        int SchemaVersion,
        string CreatedUtc,
        IReadOnlyList<TransferManifestItem> Items
    );

    public record TransferManifestItem(
        string RelativePath,
        string EntityType,
        string OriginEntityId,
        string OriginVersion,
        string ContentHash,
        long SizeBytes
    );

    public interface IUsbTransferManager
    {
        Task<(string ZipPath, string ManifestHash, TransferPackageEntity Package, IReadOnlyList<TransferItemEntity> Items)>
            CreatePackageAsync(string destinationZipPath, string workspaceId, IReadOnlyList<DocumentEntity> documents, IFileStore fileStore, CancellationToken ct = default);

        Task<(TransferManifest Manifest, string ManifestHash, bool HashValid)>
            InspectPackageAsync(string packageZipPath, CancellationToken ct = default);
    }

    public class UsbTransferManager : IUsbTransferManager
    {
        public async Task<(string ZipPath, string ManifestHash, TransferPackageEntity Package, IReadOnlyList<TransferItemEntity> Items)>
            CreatePackageAsync(string destinationZipPath, string workspaceId, IReadOnlyList<DocumentEntity> documents, IFileStore fileStore, CancellationToken ct = default)
        {
            string packageId = Guid.NewGuid().ToString("N");
            string? dir = Path.GetDirectoryName(destinationZipPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

            if (File.Exists(destinationZipPath)) File.Delete(destinationZipPath);

            var manifestItems = new List<TransferManifestItem>();
            var transferItems = new List<TransferItemEntity>();

            using (var zipStream = new FileStream(destinationZipPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (var doc in documents)
                {
                    string objPath = fileStore.GetObjectAbsolutePath(doc.ObjectHash);
                    if (File.Exists(objPath))
                    {
                        var fi = new FileInfo(objPath);
                        string entryName = $"objects/{doc.ObjectHash}";
                        archive.CreateEntryFromFile(objPath, entryName, CompressionLevel.Optimal);

                        manifestItems.Add(new TransferManifestItem(
                            RelativePath: entryName,
                            EntityType: "Document",
                            OriginEntityId: doc.Id,
                            OriginVersion: doc.CurrentRevisionId ?? "1",
                            ContentHash: doc.ObjectHash,
                            SizeBytes: fi.Length
                        ));

                        transferItems.Add(new TransferItemEntity
                        {
                            PackageId = packageId,
                            RelativePath = entryName,
                            SizeBytes = fi.Length,
                            ContentHash = doc.ObjectHash,
                            EntityType = "Document",
                            OriginEntityId = doc.Id,
                            OriginVersion = doc.CurrentRevisionId ?? "1"
                        });
                    }
                }

                // Create manifest
                var manifest = new TransferManifest(
                    PackageId: packageId,
                    OriginWorkspaceId: workspaceId,
                    SchemaVersion: 1,
                    CreatedUtc: DateTime.UtcNow.ToString("o"),
                    Items: manifestItems
                );

                string manifestJson = JsonSerializer.Serialize(manifest, new JsonSerializerOptions { WriteIndented = true });
                byte[] manifestBytes = Encoding.UTF8.GetBytes(manifestJson);

                var manifestEntry = archive.CreateEntry("manifest.json", CompressionLevel.Fastest);
                using (var entryStream = manifestEntry.Open())
                {
                    await entryStream.WriteAsync(manifestBytes, 0, manifestBytes.Length, ct);
                }
            }

            // Calculate manifest hash
            string manifestHash;
            using (var archiveToRead = ZipFile.OpenRead(destinationZipPath))
            {
                var entry = archiveToRead.GetEntry("manifest.json");
                if (entry == null) throw new InvalidOperationException("manifest.json missing in created package.");
                using var stream = entry.Open();
                using var sha256 = SHA256.Create();
                byte[] hashBytes = await sha256.ComputeHashAsync(stream, ct);
                manifestHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }

            var packageEntity = new TransferPackageEntity
            {
                Id = packageId,
                OriginWorkspaceId = workspaceId,
                SchemaVersion = 1,
                ManifestHash = manifestHash,
                State = "Exported",
                CreatedUtc = DateTime.UtcNow.ToString("o")
            };

            return (destinationZipPath, manifestHash, packageEntity, transferItems);
        }

        public async Task<(TransferManifest Manifest, string ManifestHash, bool HashValid)>
            InspectPackageAsync(string packageZipPath, CancellationToken ct = default)
        {
            if (!File.Exists(packageZipPath))
                throw new FileNotFoundException("Package zip file not found.", packageZipPath);

            using var archive = ZipFile.OpenRead(packageZipPath);
            var manifestEntry = archive.GetEntry("manifest.json");
            if (manifestEntry == null)
            {
                throw new InvalidDataException("Package does not contain manifest.json.");
            }

            string manifestJson;
            string manifestHash;
            using (var stream = manifestEntry.Open())
            {
                using var reader = new StreamReader(stream, Encoding.UTF8);
                manifestJson = await reader.ReadToEndAsync(ct);
            }

            using (var sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(manifestJson));
                manifestHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
            }

            var manifest = JsonSerializer.Deserialize<TransferManifest>(manifestJson)
                ?? throw new InvalidDataException("Invalid manifest format.");

            // Verify items hashes
            bool allHashesValid = true;
            foreach (var item in manifest.Items)
            {
                var entry = archive.GetEntry(item.RelativePath);
                if (entry == null)
                {
                    allHashesValid = false;
                    break;
                }

                using var itemStream = entry.Open();
                using var sha = SHA256.Create();
                byte[] itemHash = await sha.ComputeHashAsync(itemStream, ct);
                string computedHash = BitConverter.ToString(itemHash).Replace("-", "").ToLowerInvariant();
                if (!string.Equals(computedHash, item.ContentHash, StringComparison.OrdinalIgnoreCase))
                {
                    allHashesValid = false;
                    break;
                }
            }

            return (manifest, manifestHash, allHashesValid);
        }
    }
}
