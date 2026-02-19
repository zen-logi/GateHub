using System.Runtime.InteropServices;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using GateHub.Client.Configuration;
using Microsoft.Extensions.Logging;

namespace GateHub.Client.Services;

/// <summary>
/// OS別のユーザー設定ファイルを管理するサービス
/// Windows: %APPDATA%/GateHub/settings.json
/// macOS/Linux: ~/.config/gatehub/settings.json
/// </summary>
public sealed class ConfigurationService(ILogger<ConfigurationService> logger) : IConfigurationService {
    private static readonly JsonSerializerOptions JsonOptions = new() {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.Create(UnicodeRanges.All),
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <inheritdoc />
    public string SettingsFilePath { get; } = GetSettingsFilePath();

    /// <inheritdoc />
    public bool SettingsFileExists() => File.Exists(SettingsFilePath);

    /// <inheritdoc />
    public ClientSettings? LoadSettings() {
        if (!File.Exists(SettingsFilePath))
            return null;

        try {
            var json = File.ReadAllText(SettingsFilePath);
            var settings = JsonSerializer.Deserialize<ClientSettings>(json, JsonOptions);
            logger.LogInformation("設定ファイルを読み込み: {Path}", SettingsFilePath);
            return settings;
        }
        catch (Exception ex) {
            logger.LogError(ex, "設定ファイルの読み込みに失敗: {Path}", SettingsFilePath);
            return null;
        }
    }

    /// <inheritdoc />
    public void SaveSettings(ClientSettings settings) {
        var directory = Path.GetDirectoryName(SettingsFilePath);
        if (directory is not null)
            Directory.CreateDirectory(directory);

        var json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(SettingsFilePath, json);
        logger.LogInformation("設定ファイルを保存: {Path}", SettingsFilePath);
    }

    /// <summary>
    /// OS別の設定ファイルパスを取得する
    /// </summary>
    private static string GetSettingsFilePath() {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "GateHub", "settings.json");
        }

        // macOS / Linux
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".config", "gatehub", "settings.json");
    }
}
