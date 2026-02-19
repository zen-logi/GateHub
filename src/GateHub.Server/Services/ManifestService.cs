using GateHub.Server.Configuration;
using GateHub.Shared.Models;
using GateHub.Shared.Utilities;
using Microsoft.Extensions.Options;

namespace GateHub.Server.Services;

/// <summary>
/// ストレージディレクトリを走査しファイルマニフェストを生成するサービス
/// </summary>
public sealed class ManifestService(
    IOptions<ServerSettings> settings,
    ILogger<ManifestService> logger) : IManifestService {

    /// <inheritdoc />
    public async Task<SyncManifest> GenerateManifestAsync(CancellationToken cancellationToken = default) {
        var storagePath = settings.Value.StoragePath;
        if (!Directory.Exists(storagePath)) {
            Directory.CreateDirectory(storagePath);
            logger.LogInformation("ストレージディレクトリを作成: {Path}", storagePath);
            return new SyncManifest([], DateTime.UtcNow);
        }

        var files = Directory.GetFiles(storagePath, "*", SearchOption.AllDirectories);
        var entries = new List<FileManifestEntry>(files.Length);

        foreach (var file in files) {
            cancellationToken.ThrowIfCancellationRequested();

            // .tmpファイルと.conflictファイルはマニフェストから除外
            if (file.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase) ||
                file.Contains(".conflict.", StringComparison.OrdinalIgnoreCase))
                continue;

            var relativePath = Path.GetRelativePath(storagePath, file)
                .Replace('\\', '/');
            var hash = await HashHelper.ComputeFileHashAsync(file, cancellationToken);
            var fileInfo = new FileInfo(file);

            entries.Add(new FileManifestEntry(
                relativePath,
                hash,
                fileInfo.Length,
                fileInfo.LastWriteTimeUtc));
        }

        logger.LogInformation("マニフェスト生成完了: {Count}ファイル", entries.Count);
        return new SyncManifest(entries, DateTime.UtcNow);
    }
}
