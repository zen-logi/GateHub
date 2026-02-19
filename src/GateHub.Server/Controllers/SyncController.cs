using GateHub.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GateHub.Server.Controllers;

/// <summary>
/// セーブデータ同期APIのコントローラー
/// マニフェスト取得・ファイルPull/Push・削除を提供する
/// </summary>
[ApiController]
[Route("api/sync")]
[Authorize]
public class SyncController(
    IManifestService manifestService,
    IStorageService storageService,
    ILogger<SyncController> logger) : ControllerBase {

    /// <summary>
    /// サーバー側の現在のファイルマニフェストを取得する
    /// </summary>
    [HttpGet("manifest")]
    public async Task<IActionResult> GetManifestAsync(CancellationToken cancellationToken) {
        var manifest = await manifestService.GenerateManifestAsync(cancellationToken);
        return Ok(manifest);
    }

    /// <summary>
    /// 指定パスのファイルをダウンロードする
    /// </summary>
    /// <param name="path">ファイルの相対パス</param>
    [HttpGet("files/{**path}")]
    public IActionResult GetFile(string path) {
        var stream = storageService.GetFileStream(path);
        if (stream is null)
            return NotFound(new { message = $"ファイルが見つからない: {path}" });

        return File(stream, "application/octet-stream", enableRangeProcessing: true);
    }

    /// <summary>
    /// 指定パスにファイルをアップロードする
    /// If-Matchヘッダーによる競合検知に対応する
    /// </summary>
    /// <param name="path">ファイルの相対パス</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    [HttpPost("files/{**path}")]
    public async Task<IActionResult> UploadFileAsync(string path, CancellationToken cancellationToken) {
        var expectedHash = Request.Headers.IfMatch.FirstOrDefault()?.Trim('"');

        logger.LogInformation("ファイルアップロード要求: {Path} (クライアント: {Client})",
            path, User.Identity?.Name ?? "unknown");

        var result = await storageService.SaveFileAtomicallyAsync(
            path, Request.Body, expectedHash, cancellationToken);

        if (!result.Success)
            return StatusCode(500, result);

        if (result.ConflictDetected)
            return StatusCode(209, result); // 209 = 競合検知だが更新は成功

        return Ok(result);
    }

    /// <summary>
    /// 指定パスのファイルを削除する
    /// </summary>
    /// <param name="path">ファイルの相対パス</param>
    [HttpDelete("files/{**path}")]
    public IActionResult DeleteFile(string path) {
        logger.LogInformation("ファイル削除要求: {Path} (クライアント: {Client})",
            path, User.Identity?.Name ?? "unknown");

        var result = storageService.DeleteFile(path);
        return result.Success ? Ok(result) : StatusCode(500, result);
    }
}
