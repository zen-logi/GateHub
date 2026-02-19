using GateHub.Shared.Models;

namespace GateHub.Client.Services;

/// <summary>
/// ゲーム選択UIサービスのインタフェース
/// </summary>
public interface IGameSelector {
    /// <summary>
    /// ユーザーにゲーム一覧を提示し選択させる
    /// </summary>
    /// <param name="games">選択可能なゲーム一覧</param>
    /// <returns>選択されたゲーム。終了が選択された場合はnull</returns>
    GameInfo? SelectGame(IReadOnlyList<GameInfo> games);
}
