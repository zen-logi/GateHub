using GateHub.Client.Configuration;
using GateHub.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GateHub.Client.Services;

/// <summary>
/// マニフェスト比較による差分同期エンジン
/// サーバーとローカルのマニフェストを比較し、差分ファイルのPull/Pushを実行する
/// </summary>
public sealed class SyncEngine(
    ISyncApiClient apiClient,
    ILocalManifestService localManifestService,
    IOptions<ClientSettings> settings,
    ILogger<SyncEngine> logger) : ISyncEngine {

    /// <inheritdoc />
    public async Task<int> PullAsync(CancellationToken cancellationToken = default) {
        logger.LogInformation("Pull同期を開始");
        var serverManifest = await apiClient.GetManifestAsync(cancellationToken);
        var localManifest = await localManifestService.GenerateLocalManifestAsync(cancellationToken);

        var pullTargets = DetectPullTargets(serverManifest, localManifest);
        if (pullTargets.Count == 0) {
            logger.LogInformation("Pull対象なし（ローカルは最新）");
            return 0;
        }

        logger.LogInformation("Pull対象: {Count}ファイル", pullTargets.Count);
        var memoryCardPath = settings.Value.MemoryCardPath;

        foreach (var entry in pullTargets) {
            await apiClient.DownloadFileAsync(entry.RelativePath, memoryCardPath, cancellationToken);
        }

        // サーバーに存在しないローカルファイルの削除
        var deleteTargets = DetectLocalOnlyFiles(serverManifest, localManifest);
        foreach (var entry in deleteTargets) {
            var localPath = Path.Combine(memoryCardPath, entry.RelativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(localPath)) {
                File.Delete(localPath);
                logger.LogInformation("ローカルファイル削除（サーバーに存在しない）: {Path}", entry.RelativePath);
            }
        }

        return pullTargets.Count + deleteTargets.Count;
    }

    /// <inheritdoc />
    public async Task<int> PushAsync(CancellationToken cancellationToken = default) {
        logger.LogInformation("Push同期を開始");
        var serverManifest = await apiClient.GetManifestAsync(cancellationToken);
        var localManifest = await localManifestService.GenerateLocalManifestAsync(cancellationToken);

        var pushTargets = DetectPushTargets(serverManifest, localManifest);
        if (pushTargets.Count == 0) {
            logger.LogInformation("Push対象なし（サーバーは最新）");
            return 0;
        }

        logger.LogInformation("Push対象: {Count}ファイル", pushTargets.Count);
        var memoryCardPath = settings.Value.MemoryCardPath;

        foreach (var (entry, expectedHash) in pushTargets) {
            await apiClient.UploadFileAsync(entry.RelativePath, memoryCardPath, expectedHash, cancellationToken);
        }

        return pushTargets.Count;
    }

    /// <summary>
    /// サーバーに存在しローカルに存在しない、またはハッシュが異なるファイルを抽出する
    /// </summary>
    private static List<FileManifestEntry> DetectPullTargets(SyncManifest server, SyncManifest local) {
        var localLookup = local.Entries.ToDictionary(e => e.RelativePath, e => e.Hash, StringComparer.OrdinalIgnoreCase);
        var targets = new List<FileManifestEntry>();

        foreach (var serverEntry in server.Entries) {
            if (!localLookup.TryGetValue(serverEntry.RelativePath, out var localHash) ||
                !string.Equals(localHash, serverEntry.Hash, StringComparison.OrdinalIgnoreCase))
                targets.Add(serverEntry);
        }

        return targets;
    }

    /// <summary>
    /// ローカルに存在しサーバーに存在しないファイルを抽出する（Pull時の削除対象）
    /// </summary>
    private static List<FileManifestEntry> DetectLocalOnlyFiles(SyncManifest server, SyncManifest local) {
        var serverPaths = new HashSet<string>(server.Entries.Select(e => e.RelativePath), StringComparer.OrdinalIgnoreCase);
        return local.Entries.Where(e => !serverPaths.Contains(e.RelativePath)).ToList();
    }

    /// <summary>
    /// ローカルで変更された（ハッシュが異なる）または新規のファイルを抽出する
    /// </summary>
    private static List<(FileManifestEntry Entry, string? ExpectedHash)> DetectPushTargets(SyncManifest server, SyncManifest local) {
        var serverLookup = server.Entries.ToDictionary(e => e.RelativePath, e => e.Hash, StringComparer.OrdinalIgnoreCase);
        var targets = new List<(FileManifestEntry, string?)>();

        foreach (var localEntry in local.Entries) {
            if (serverLookup.TryGetValue(localEntry.RelativePath, out var serverHash)) {
                if (!string.Equals(serverHash, localEntry.Hash, StringComparison.OrdinalIgnoreCase))
                    targets.Add((localEntry, serverHash));
            }
            else {
                // サーバーに存在しない新規ファイル
                targets.Add((localEntry, null));
            }
        }

        return targets;
    }
}
