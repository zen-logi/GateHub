using GateHub.Server.Configuration;
using GateHub.Shared.Models;
using GateHub.Shared.Utilities;
using Microsoft.Extensions.Options;

namespace GateHub.Server.Services;

/// <summary>
/// ファイルのアトミック保存・競合退避・削除を行うストレージサービス
/// </summary>
public sealed class StorageService(
    IOptions<ServerSettings> settings,
    ILogger<StorageService> logger) : IStorageService {

    /// <inheritdoc />
    public FileStream? GetFileStream(string relativePath) {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            return null;

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, useAsync: true);
    }

    /// <inheritdoc />
    public async Task<SyncResult> SaveFileAtomicallyAsync(
        string relativePath, Stream content, string? expectedHash, CancellationToken cancellationToken = default) {
        var fullPath = GetFullPath(relativePath);
        var directory = Path.GetDirectoryName(fullPath);
        if (directory is not null)
            Directory.CreateDirectory(directory);

        var conflictDetected = false;

        // 競合検知: 既存ファイルのハッシュが期待値と異なる場合は退避
        if (expectedHash is not null && File.Exists(fullPath)) {
            var currentHash = await HashHelper.ComputeFileHashAsync(fullPath, cancellationToken);
            if (!string.Equals(currentHash, expectedHash, StringComparison.OrdinalIgnoreCase)) {
                var conflictPath = $"{fullPath}.conflict.{DateTime.UtcNow:yyyyMMddHHmmss}";
                File.Copy(fullPath, conflictPath, overwrite: true);
                logger.LogWarning("競合検知: {Path} → {ConflictPath}", relativePath, conflictPath);
                conflictDetected = true;
            }
        }

        // 一時ファイルへ書き込み → アトミックリプレイス
        var tempPath = $"{fullPath}.tmp";
        try {
            using (var tempStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true)) {
                await content.CopyToAsync(tempStream, cancellationToken);
            }

            File.Move(tempPath, fullPath, overwrite: true);
            logger.LogInformation("ファイル保存完了: {Path}", relativePath);

            return new SyncResult(true,
                conflictDetected ? "競合を検知し退避後に更新した" : "保存完了",
                conflictDetected);
        }
        catch (Exception ex) {
            // ロールバック: 一時ファイルを削除
            if (File.Exists(tempPath))
                File.Delete(tempPath);

            logger.LogError(ex, "ファイル保存失敗: {Path}", relativePath);
            return new SyncResult(false, $"保存失敗: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public SyncResult DeleteFile(string relativePath) {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            return new SyncResult(true, "ファイルが存在しない");

        try {
            File.Delete(fullPath);
            logger.LogInformation("ファイル削除: {Path}", relativePath);
            return new SyncResult(true, "削除完了");
        }
        catch (Exception ex) {
            logger.LogError(ex, "ファイル削除失敗: {Path}", relativePath);
            return new SyncResult(false, $"削除失敗: {ex.Message}");
        }
    }

    /// <inheritdoc />
    public async Task<string?> GetFileHashAsync(string relativePath, CancellationToken cancellationToken = default) {
        var fullPath = GetFullPath(relativePath);
        if (!File.Exists(fullPath))
            return null;

        return await HashHelper.ComputeFileHashAsync(fullPath, cancellationToken);
    }

    /// <summary>
    /// 相対パスをストレージルートからの絶対パスへ変換し、パストラバーサルを検証する
    /// </summary>
    private string GetFullPath(string relativePath) {
        var normalized = relativePath.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(settings.Value.StoragePath, normalized));

        // パストラバーサル防止
        if (!fullPath.StartsWith(Path.GetFullPath(settings.Value.StoragePath), StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException($"不正なパス: {relativePath}");

        return fullPath;
    }
}
