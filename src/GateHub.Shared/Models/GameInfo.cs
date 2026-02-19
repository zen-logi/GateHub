namespace GateHub.Shared.Models;

/// <summary>
/// PCSX2で起動するゲームの情報を表すレコード
/// </summary>
/// <param name="Title">ゲームタイトル（表示名）</param>
/// <param name="IsoPath">ISOファイルの絶対パス</param>
/// <param name="GameId">ゲームID（例: SLPM-65888）</param>
public record GameInfo(
    string Title,
    string IsoPath,
    string GameId);
