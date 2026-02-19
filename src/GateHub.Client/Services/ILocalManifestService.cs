using GateHub.Shared.Models;

namespace GateHub.Client.Services;

/// <summary>
/// ローカルのメモリーカードディレクトリからマニフェストを生成するサービスのインタフェース
/// </summary>
public interface ILocalManifestService {
    /// <summary>
    /// ローカルディレクトリを走査しマニフェストを生成する
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>ローカルファイルマニフェスト</returns>
    Task<SyncManifest> GenerateLocalManifestAsync(CancellationToken cancellationToken = default);
}
