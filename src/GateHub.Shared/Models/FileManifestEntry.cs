namespace GateHub.Shared.Models;

/// <summary>
/// ファイルマニフェストの個別エントリを表すレコード
/// 相対パス・ハッシュ・サイズ・更新日時でファイルの状態を一意に識別する
/// </summary>
/// <param name="RelativePath">ストレージルートからの相対パス（正規化済み）</param>
/// <param name="Hash">SHA256ハッシュ値（小文字16進文字列）</param>
/// <param name="Size">ファイルサイズ（バイト）</param>
/// <param name="LastModifiedUtc">最終更新日時（UTC）</param>
public record FileManifestEntry(
    string RelativePath,
    string Hash,
    long Size,
    DateTime LastModifiedUtc);
