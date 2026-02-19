using GateHub.Shared.Models;
using Microsoft.Extensions.Logging;

namespace GateHub.Client.Services;

/// <summary>
/// コンソール上でゲーム一覧を表示しユーザーに選択させるサービス
/// </summary>
public sealed class GameSelector(ILogger<GameSelector> logger) : IGameSelector {

    /// <inheritdoc />
    public GameInfo? SelectGame(IReadOnlyList<GameInfo> games) {
        if (games.Count == 0) {
            logger.LogError("ゲームが設定されていない");
            Console.WriteLine("Error: appsettings.json にゲームが設定されていません。");
            return null;
        }

        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║       GateHub - PCSX2 Save Sync         ║");
        Console.WriteLine("╠══════════════════════════════════════════╣");

        for (var i = 0; i < games.Count; i++) {
            var title = games[i].Title;
            var line = $"  {i + 1}. {title}";
            Console.WriteLine($"║{line.PadRight(42)}║");
        }

        Console.WriteLine($"║{"  0. 終了".PadRight(40)}║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();

        while (true) {
            Console.Write($"ゲームを選択 [0-{games.Count}]: ");
            var input = Console.ReadLine();

            if (int.TryParse(input, out var selection)) {
                if (selection == 0) {
                    logger.LogInformation("ユーザーが終了を選択");
                    return null;
                }

                if (selection >= 1 && selection <= games.Count) {
                    var selected = games[selection - 1];
                    logger.LogInformation("ゲーム選択: {Title} ({GameId})", selected.Title, selected.GameId);
                    return selected;
                }
            }

            Console.WriteLine("無効な入力です。もう一度入力してください。");
        }
    }
}
