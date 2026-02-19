namespace GateHub.Shared.Models;

/// <summary>
/// ファイルマニフェスト全体を表すレコード
/// サーバー・クライアント間で交換されるファイル状態リスト
/// </summary>
/// <param name="Entries">ファイルエントリのコレクション</param>
/// <param name="GeneratedAtUtc">マニフェスト生成日時（UTC）</param>
public record SyncManifest(
    IReadOnlyList<FileManifestEntry> Entries,
    DateTime GeneratedAtUtc);
