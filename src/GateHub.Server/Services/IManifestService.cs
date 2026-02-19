using GateHub.Shared.Models;

namespace GateHub.Server.Services;

/// <summary>
/// サーバー側マニフェスト生成サービスのインタフェース
/// </summary>
public interface IManifestService {
    /// <summary>
    /// ストレージディレクトリの現在のファイル状態からマニフェストを生成する
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>現在のファイルマニフェスト</returns>
    Task<SyncManifest> GenerateManifestAsync(CancellationToken cancellationToken = default);
}
