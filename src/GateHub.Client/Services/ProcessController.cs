using System.Diagnostics;
using GateHub.Client.Configuration;
using GateHub.Shared.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GateHub.Client.Services;

/// <summary>
/// PCSX2プロセスの起動・終了待機を行うサービス
/// Windows / macOS 両対応
/// </summary>
public sealed class ProcessController(
    IOptions<ClientSettings> settings,
    ILogger<ProcessController> logger) : IProcessController {

    /// <inheritdoc />
    public async Task<int> LaunchAndWaitAsync(GameInfo game, CancellationToken cancellationToken = default) {
        var pcsx2Path = settings.Value.Pcsx2Path;
        if (!File.Exists(pcsx2Path)) {
            logger.LogError("PCSX2が見つからない: {Path}", pcsx2Path);
            throw new FileNotFoundException("PCSX2実行ファイルが見つからない", pcsx2Path);
        }

        // PCSX2の起動引数を構築
        var arguments = BuildArguments(game);
        logger.LogInformation("PCSX2を起動: {Path} {Args}", pcsx2Path, arguments);

        using var process = new Process {
            StartInfo = new ProcessStartInfo {
                FileName = pcsx2Path,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false
            },
            EnableRaisingEvents = true
        };

        process.Start();
        logger.LogInformation("PCSX2プロセス開始 (PID: {Pid})", process.Id);

        await process.WaitForExitAsync(cancellationToken);
        logger.LogInformation("PCSX2プロセス終了 (終了コード: {ExitCode})", process.ExitCode);

        return process.ExitCode;
    }

    /// <summary>
    /// ゲーム情報からPCSX2の起動引数を構築する
    /// </summary>
    private static string BuildArguments(GameInfo game) =>
        $"\"{game.IsoPath}\"";
}
