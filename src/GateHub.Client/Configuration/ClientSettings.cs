using GateHub.Shared.Models;

namespace GateHub.Client.Configuration;

/// <summary>
/// クライアント設定のオプションクラス
/// appsettings.jsonの"GateHub"セクションにバインドされる
/// </summary>
public sealed class ClientSettings {
    /// <summary>
    /// 設定セクション名
    /// </summary>
    public const string SectionName = "GateHub";

    /// <summary>
    /// サーバーのベースURL（例: http://192.168.1.100:5123）
    /// </summary>
    public required string ServerUrl { get; set; }

    /// <summary>
    /// サーバー認証用APIトークン
    /// </summary>
    public required string ApiToken { get; set; }

    /// <summary>
    /// PCSX2実行ファイルのパス
    /// </summary>
    public required string Pcsx2Path { get; set; }

    /// <summary>
    /// メモリーカードディレクトリ（Folder Mcd形式）のパス
    /// </summary>
    public required string MemoryCardPath { get; set; }

    /// <summary>
    /// サーバー接続タイムアウト（秒）
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 3;

    /// <summary>
    /// 起動可能なゲーム一覧
    /// </summary>
    public List<GameInfo> Games { get; set; } = [];
}
