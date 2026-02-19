using GateHub.Client.Configuration;
using GateHub.Shared.Models;
using Microsoft.Extensions.Logging;

namespace GateHub.Client.Services;

/// <summary>
/// 初回起動時に対話的に設定を入力させるセットアップウィザード
/// </summary>
public sealed class SetupWizard(ILogger<SetupWizard> logger) : ISetupWizard {

    /// <inheritdoc />
    public ClientSettings RunSetup() {
        Console.WriteLine();
        Console.WriteLine("╔══════════════════════════════════════════╗");
        Console.WriteLine("║    GateHub - Initial Setup               ║");
        Console.WriteLine("╚══════════════════════════════════════════╝");
        Console.WriteLine();
        Console.WriteLine("初回セットアップを実行します。");
        Console.WriteLine("以下の情報を入力してください。");
        Console.WriteLine();

        // サーバーURL
        var serverUrl = Prompt("サーバーURL", "http://192.168.1.100:5123");

        // APIトークン
        var apiToken = Prompt("APIトークン", "change-me-token-1");

        // PCSX2パス
        var defaultPcsx2Path = OperatingSystem.IsWindows()
            ? @"C:\Program Files\PCSX2\pcsx2-qt.exe"
            : "/Applications/PCSX2.app/Contents/MacOS/PCSX2";
        var pcsx2Path = Prompt("PCSX2実行ファイルのパス", defaultPcsx2Path);

        // メモリーカードパス
        var defaultMcdPath = OperatingSystem.IsWindows()
            ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "PCSX2", "memcards", "Mcd001")
            : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Library", "Application Support", "PCSX2", "memcards", "Mcd001");
        var memoryCardPath = Prompt("メモリーカードディレクトリのパス (Folder Mcd)", defaultMcdPath);

        // ゲーム登録
        var games = new List<GameInfo>();
        Console.WriteLine();
        Console.WriteLine("ゲームを登録します（空Enterで終了）:");

        while (true) {
            Console.WriteLine();
            Console.Write($"ゲーム{games.Count + 1} タイトル (空Enterで終了): ");
            var title = Console.ReadLine()?.Trim();
            if (string.IsNullOrEmpty(title))
                break;

            Console.Write("  ISOファイルのパス: ");
            var isoPath = Console.ReadLine()?.Trim() ?? "";

            Console.Write("  ゲームID (例: SLPM-65888): ");
            var gameId = Console.ReadLine()?.Trim() ?? "";

            games.Add(new GameInfo(title, isoPath, gameId));
            logger.LogInformation("ゲーム登録: {Title} ({GameId})", title, gameId);
        }

        if (games.Count == 0) {
            Console.WriteLine("Note: ゲームが未登録です。後から設定ファイルを編集して追加できます。");
            games.Add(new GameInfo("サンプルゲーム", "/path/to/game.iso", "SLPM-00000"));
        }

        var settings = new ClientSettings {
            ServerUrl = serverUrl,
            ApiToken = apiToken,
            Pcsx2Path = pcsx2Path,
            MemoryCardPath = memoryCardPath,
            Games = games
        };

        Console.WriteLine();
        Console.WriteLine("セットアップが完了しました。");

        return settings;
    }

    /// <summary>
    /// デフォルト値付きのプロンプトを表示し、ユーザー入力を取得する
    /// </summary>
    private static string Prompt(string label, string defaultValue) {
        Console.Write($"{label} [{defaultValue}]: ");
        var input = Console.ReadLine()?.Trim();
        return string.IsNullOrEmpty(input) ? defaultValue : input;
    }
}
