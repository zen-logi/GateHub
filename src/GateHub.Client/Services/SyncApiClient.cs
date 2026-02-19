using System.Net.Http.Json;
using GateHub.Shared.Models;
using Microsoft.Extensions.Logging;

namespace GateHub.Client.Services;

/// <summary>
/// HttpClientベースのサーバーAPI通信クライアント
/// </summary>
public sealed class SyncApiClient(
    HttpClient httpClient,
    ILogger<SyncApiClient> logger) : ISyncApiClient {

    /// <inheritdoc />
    public async Task<SyncManifest> GetManifestAsync(CancellationToken cancellationToken = default) {
        var manifest = await httpClient.GetFromJsonAsync<SyncManifest>(
            "api/sync/manifest", cancellationToken);
        return manifest ?? throw new InvalidOperationException("マニフェストの取得に失敗した");
    }

    /// <inheritdoc />
    public async Task DownloadFileAsync(string relativePath, string localBasePath, CancellationToken cancellationToken = default) {
        var encodedPath = Uri.EscapeDataString(relativePath).Replace("%2F", "/");
        using var response = await httpClient.GetAsync(
            $"api/sync/files/{encodedPath}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();

        var localPath = Path.Combine(localBasePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        var directory = Path.GetDirectoryName(localPath);
        if (directory is not null)
            Directory.CreateDirectory(directory);

        // アトミック書き込み: .tmp → リネーム
        var tempPath = $"{localPath}.tmp";
        try {
            using (var contentStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, useAsync: true)) {
                await contentStream.CopyToAsync(fileStream, cancellationToken);
            }

            File.Move(tempPath, localPath, overwrite: true);
            logger.LogInformation("ダウンロード完了: {Path}", relativePath);
        }
        catch {
            if (File.Exists(tempPath))
                File.Delete(tempPath);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<SyncResult> UploadFileAsync(
        string relativePath, string localBasePath, string? expectedHash, CancellationToken cancellationToken = default) {
        var localPath = Path.Combine(localBasePath, relativePath.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(localPath))
            return new SyncResult(false, $"ローカルファイルが見つからない: {relativePath}");

        using var fileStream = new FileStream(localPath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, useAsync: true);
        using var content = new StreamContent(fileStream);

        var encodedPath = Uri.EscapeDataString(relativePath).Replace("%2F", "/");
        using var request = new HttpRequestMessage(HttpMethod.Post, $"api/sync/files/{encodedPath}") {
            Content = content
        };

        // 競合検知用のIf-Matchヘッダー
        if (expectedHash is not null)
            request.Headers.IfMatch.Add(new System.Net.Http.Headers.EntityTagHeaderValue($"\"{expectedHash}\""));

        using var response = await httpClient.SendAsync(request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<SyncResult>(cancellationToken);

        if (result is not null) {
            logger.LogInformation("アップロード完了: {Path} (結果: {Message})", relativePath, result.Message);
            return result;
        }

        return new SyncResult(false, $"レスポンスの解析に失敗した (StatusCode: {response.StatusCode})");
    }

    /// <inheritdoc />
    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default) {
        try {
            var manifest = await httpClient.GetFromJsonAsync<SyncManifest>(
                "api/sync/manifest", cancellationToken);
            return manifest is not null;
        }
        catch (Exception ex) {
            logger.LogWarning("サーバー接続確認失敗: {Message}", ex.Message);
            return false;
        }
    }
}
