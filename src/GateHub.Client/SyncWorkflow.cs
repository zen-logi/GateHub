using GateHub.Client.Configuration;
using GateHub.Client.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GateHub.Client;

/// <summary>
/// メイン同期ワークフロー
/// セットアップ確認 → ゲーム選択 → Pull同期 → PCSX2起動 → 終了待機 → Push同期 の一連のフローを実行する
/// </summary>
public sealed class SyncWorkflow(
    IConfigurationService configService,
    ISetupWizard setupWizard,
    IGameSelector gameSelector,
    ISyncEngine syncEngine,
    ISyncApiClient apiClient,
    IProcessController processController,
    IOptions<ClientSettings> settings,
    ILogger<SyncWorkflow> logger,
    IHostApplicationLifetime lifetime) : BackgroundService {

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
        try {
            await RunWorkflowAsync(stoppingToken);
        }
        catch (OperationCanceledException) {
            logger.LogInformation("ワークフローがキャンセルされた");
        }
        catch (Exception ex) {
            logger.LogError(ex, "ワークフロー実行中にエラーが発生");
            Console.WriteLine($"Error: {ex.Message}");
        }
        finally {
            lifetime.StopApplication();
        }
    }

    /// <summary>
    /// 同期ワークフローのメインロジック
    /// </summary>
    private async Task RunWorkflowAsync(CancellationToken cancellationToken) {
        // 初回セットアップ確認
        var clientSettings = EnsureConfiguration();

        // ゲーム選択
        var selectedGame = gameSelector.SelectGame(clientSettings.Games);
        if (selectedGame is null)
            return;

        Console.WriteLine($"\n[{selectedGame.Title}] を起動します...\n");

        // サーバー接続確認 + Pull同期
        var isOnline = await TryConnectWithTimeoutAsync(clientSettings, cancellationToken);
        if (isOnline) {
            Console.WriteLine("サーバーに接続中... Pull同期を実行");
            var pullCount = await syncEngine.PullAsync(cancellationToken);
            Console.WriteLine($"Pull同期完了: {pullCount}ファイル同期\n");
        }
        else {
            // オフラインフォールバック
            Console.WriteLine("Warning: サーバーに接続できません。");
            Console.Write("ローカルデータで続行しますか? [Y/n]: ");
            var input = Console.ReadLine();
            if (input?.Trim().Equals("n", StringComparison.OrdinalIgnoreCase) == true) {
                Console.WriteLine("中断しました。");
                return;
            }

            Console.WriteLine("オフラインモードで続行します。\n");
        }

        // PCSX2起動・終了待機
        Console.WriteLine("PCSX2を起動しています...");
        var exitCode = await processController.LaunchAndWaitAsync(selectedGame, cancellationToken);
        Console.WriteLine($"\nPCSX2が終了しました (終了コード: {exitCode})\n");

        // Push同期
        if (isOnline || await TryConnectWithTimeoutAsync(clientSettings, cancellationToken)) {
            Console.WriteLine("Push同期を実行中...");
            var pushCount = await syncEngine.PushAsync(cancellationToken);
            Console.WriteLine($"Push同期完了: {pushCount}ファイル同期");
        }
        else {
            Console.WriteLine("Warning: サーバーに接続できないためPush同期をスキップしました。");
            Console.WriteLine("次回起動時にローカルの変更がサーバーに反映されます。");
        }

        Console.WriteLine("\n完了しました。");
    }

    /// <summary>
    /// 設定ファイルが存在しない場合はセットアップウィザードを実行し、設定を返す
    /// </summary>
    private ClientSettings EnsureConfiguration() {
        if (configService.SettingsFileExists()) {
            var loaded = configService.LoadSettings();
            if (loaded is not null) {
                Console.WriteLine($"設定ファイル: {configService.SettingsFilePath}");
                return loaded;
            }
        }

        // 初回セットアップ
        var newSettings = setupWizard.RunSetup();
        configService.SaveSettings(newSettings);
        Console.WriteLine($"\n設定を保存しました: {configService.SettingsFilePath}");
        return newSettings;
    }

    /// <summary>
    /// タイムアウト付きでサーバー接続を試みる
    /// </summary>
    private async Task<bool> TryConnectWithTimeoutAsync(ClientSettings clientSettings, CancellationToken cancellationToken) {
        using var timeoutCts = new CancellationTokenSource(
            TimeSpan.FromSeconds(clientSettings.ConnectionTimeoutSeconds));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(
            timeoutCts.Token, cancellationToken);

        try {
            return await apiClient.CheckConnectionAsync(linkedCts.Token);
        }
        catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested) {
            logger.LogWarning("サーバー接続タイムアウト ({Seconds}秒)", clientSettings.ConnectionTimeoutSeconds);
            return false;
        }
    }
}
