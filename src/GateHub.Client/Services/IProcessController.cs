using GateHub.Shared.Models;

namespace GateHub.Client.Services;

/// <summary>
/// PCSX2プロセスの起動・終了待機を行うサービスのインタフェース
/// </summary>
public interface IProcessController {
    /// <summary>
    /// PCSX2を起動し、プロセス終了まで待機する
    /// </summary>
    /// <param name="game">起動するゲーム情報</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>プロセスの終了コード</returns>
    Task<int> LaunchAndWaitAsync(GameInfo game, CancellationToken cancellationToken = default);
}
