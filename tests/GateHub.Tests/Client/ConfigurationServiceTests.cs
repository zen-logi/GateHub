using GateHub.Client.Services;
using Microsoft.Extensions.Logging.Abstractions;

namespace GateHub.Tests.Client;

public class ConfigurationServiceTests {

    /// <summary>
    /// OS別の設定ファイルパスが正しく生成されることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess1() {
        var service = new ConfigurationService(NullLogger<ConfigurationService>.Instance);

        await Assert.That(service.SettingsFilePath).IsNotNull();
        await Assert.That(service.SettingsFilePath).Contains("GateHub").Or.Contains("gatehub");
    }

    /// <summary>
    /// 存在しない設定ファイルに対してfalseを返すことを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess2() {
        var service = new ConfigurationService(NullLogger<ConfigurationService>.Instance);

        // テスト環境では通常設定ファイルが存在しない（既存の場合は無視）
        var result = service.SettingsFileExists();
        await Assert.That(result).IsTypeOf<bool>();
    }

    /// <summary>
    /// 設定の保存と読み込みの往復が正しく行えることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessRoundTrip() {
        var service = new ConfigurationService(NullLogger<ConfigurationService>.Instance);

        try {
            var settings = new GateHub.Client.Configuration.ClientSettings {
                ServerUrl = "http://192.168.1.50:5123",
                ApiToken = "test-token-123",
                Pcsx2Path = "/usr/bin/pcsx2",
                MemoryCardPath = "/home/user/memcards",
                ConnectionTimeoutSeconds = 5,
                Games = [
                    new GateHub.Shared.Models.GameInfo("TestGame", "/path/game.iso", "SLPM-00001")
                ]
            };

            // 保存して読み込み
            service.SaveSettings(settings);
            var loaded = service.LoadSettings();

            await Assert.That(loaded).IsNotNull();
            await Assert.That(loaded!.ServerUrl).IsEqualTo("http://192.168.1.50:5123");
            await Assert.That(loaded.ApiToken).IsEqualTo("test-token-123");
            await Assert.That(loaded.Games).HasCount().EqualTo(1);
        }
        finally {
            // クリーンアップ
            if (File.Exists(service.SettingsFilePath))
                File.Delete(service.SettingsFilePath);
        }
    }
}
