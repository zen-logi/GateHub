using GateHub.Client.Configuration;

namespace GateHub.Client.Services;

/// <summary>
/// ユーザー設定ファイルの読み込み・保存・初回セットアップを行うサービスのインタフェース
/// </summary>
public interface IConfigurationService {
    /// <summary>
    /// ユーザー設定ファイルのパスを取得する
    /// </summary>
    string SettingsFilePath { get; }

    /// <summary>
    /// ユーザー設定ファイルが存在するか確認する
    /// </summary>
    bool SettingsFileExists();

    /// <summary>
    /// ユーザー設定を読み込む
    /// </summary>
    /// <returns>読み込んだ設定。ファイルが存在しない場合はnull</returns>
    ClientSettings? LoadSettings();

    /// <summary>
    /// ユーザー設定をファイルに保存する
    /// </summary>
    /// <param name="settings">保存する設定</param>
    void SaveSettings(ClientSettings settings);
}
