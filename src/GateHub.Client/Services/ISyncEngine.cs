namespace GateHub.Client.Services;

/// <summary>
/// マニフェスト比較による差分同期エンジンのインタフェース
/// </summary>
public interface ISyncEngine {
    /// <summary>
    /// サーバーとの差分を検出しPull同期を実行する（プレイ前）
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>同期されたファイル数</returns>
    Task<int> PullAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// ローカル変更をサーバーにPush同期する（プレイ後）
    /// </summary>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>同期されたファイル数</returns>
    Task<int> PushAsync(CancellationToken cancellationToken = default);
}
