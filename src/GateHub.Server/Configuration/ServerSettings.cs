namespace GateHub.Server.Configuration;

/// <summary>
/// サーバー設定のオプションクラス
/// appsettings.jsonの"GateHub"セクションにバインドされる
/// </summary>
public sealed class ServerSettings {
    /// <summary>
    /// 設定セクション名
    /// </summary>
    public const string SectionName = "GateHub";

    /// <summary>
    /// セーブデータの保存先ディレクトリパス
    /// </summary>
    public required string StoragePath { get; set; }

    /// <summary>
    /// 許可されたAPIトークンのリスト
    /// </summary>
    public required List<string> ApiTokens { get; set; }
}
