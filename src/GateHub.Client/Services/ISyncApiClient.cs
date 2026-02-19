using GateHub.Shared.Models;

namespace GateHub.Client.Services;

/// <summary>
/// サーバーAPIとの通信を行うクライアントのインタフェース
/// </summary>
public interface ISyncApiClient {
    /// <summary>
    /// サーバーの現在のマニフェストを取得する
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>サーバー側マニフェスト</returns>
    Task<SyncManifest> GetManifestAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// サーバーからファイルをダウンロードしローカルに保存する
    /// </summary>
    /// <param name="relativePath">相対パス</param>
    /// <param name="localBasePath">ローカル保存先ベースディレクトリ</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    Task DownloadFileAsync(string relativePath, string localBasePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// ローカルファイルをサーバーにアップロードする
    /// </summary>
    /// <param name="relativePath">相対パス</param>
    /// <param name="localBasePath">ローカルベースディレクトリ</param>
    /// <param name="expectedHash">期待される既存ファイルのハッシュ（競合検知用、新規の場合はnull）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>同期結果</returns>
    Task<SyncResult> UploadFileAsync(string relativePath, string localBasePath, string? expectedHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// サーバーとの接続を確認する
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>接続可能な場合true</returns>
    Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default);
}
