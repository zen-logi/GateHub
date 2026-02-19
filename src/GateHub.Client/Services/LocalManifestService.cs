using GateHub.Client.Configuration;
using GateHub.Shared.Models;
using GateHub.Shared.Utilities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GateHub.Client.Services;

/// <summary>
/// ローカルのメモリーカードディレクトリを走査しマニフェストを生成するサービス
/// </summary>
public sealed class LocalManifestService(
    IOptions<ClientSettings> settings,
    ILogger<LocalManifestService> logger) : ILocalManifestService {

    /// <inheritdoc />
    public async Task<SyncManifest> GenerateLocalManifestAsync(CancellationToken cancellationToken = default) {
        var memoryCardPath = settings.Value.MemoryCardPath;
        if (!Directory.Exists(memoryCardPath)) {
            Directory.CreateDirectory(memoryCardPath);
            logger.LogInformation("メモリーカードディレクトリを作成: {Path}", memoryCardPath);
            return new SyncManifest([], DateTime.UtcNow);
        }

        var files = Directory.GetFiles(memoryCardPath, "*", SearchOption.AllDirectories);
        var entries = new List<FileManifestEntry>(files.Length);

        foreach (var file in files) {
            cancellationToken.ThrowIfCancellationRequested();

            // 一時ファイルは除外
            if (file.EndsWith(".tmp", StringComparison.OrdinalIgnoreCase))
                continue;

            var relativePath = Path.GetRelativePath(memoryCardPath, file)
                .Replace('\\', '/');
            var hash = await HashHelper.ComputeFileHashAsync(file, cancellationToken);
            var fileInfo = new FileInfo(file);

            entries.Add(new FileManifestEntry(
                relativePath,
                hash,
                fileInfo.Length,
                fileInfo.LastWriteTimeUtc));
        }

        logger.LogInformation("ローカルマニフェスト生成完了: {Count}ファイル", entries.Count);
        return new SyncManifest(entries, DateTime.UtcNow);
    }
}
