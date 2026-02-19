using GateHub.Shared.Models;

namespace GateHub.Server.Services;

/// <summary>
/// ファイルの読み書き・アトミック更新・競合退避を行うストレージサービスのインタフェース
/// </summary>
public interface IStorageService {
    /// <summary>
    /// 指定パスのファイルをストリームとして取得する
    /// </summary>
    /// <param name="relativePath">相対パス</param>
    /// <returns>ファイルストリーム。ファイルが存在しない場合はnull</returns>
    FileStream? GetFileStream(string relativePath);

    /// <summary>
    /// ファイルをアトミックに保存する
    /// 競合検知時はコンフリクトファイルとして退避する
    /// </summary>
    /// <param name="relativePath">相対パス</param>
    /// <param name="content">ファイル内容のストリーム</param>
    /// <param name="expectedHash">期待される既存ファイルのハッシュ（競合検知用、新規の場合はnull）</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>同期結果</returns>
    Task<SyncResult> SaveFileAtomicallyAsync(string relativePath, Stream content, string? expectedHash, CancellationToken cancellationToken = default);

    /// <summary>
    /// 指定パスのファイルを削除する
    /// </summary>
    /// <param name="relativePath">相対パス</param>
    /// <returns>同期結果</returns>
    SyncResult DeleteFile(string relativePath);

    /// <summary>
    /// 指定パスのファイルのSHA256ハッシュを取得する
    /// </summary>
    /// <param name="relativePath">相対パス</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>ハッシュ文字列。ファイルが存在しない場合はnull</returns>
    Task<string?> GetFileHashAsync(string relativePath, CancellationToken cancellationToken = default);
}
