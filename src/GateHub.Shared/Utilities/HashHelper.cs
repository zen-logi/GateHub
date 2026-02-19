using System.Security.Cryptography;

namespace GateHub.Shared.Utilities;

/// <summary>
/// SHA256ハッシュ計算のユーティリティクラス
/// </summary>
public static class HashHelper {
    /// <summary>
    /// 指定ファイルのSHA256ハッシュを小文字16進文字列で返す
    /// </summary>
    /// <param name="filePath">ハッシュを計算するファイルの絶対パス</param>
    /// <param name="cancellationToken">キャンセルトークン</param>
    /// <returns>SHA256ハッシュ値（小文字16進文字列）</returns>
    public static async Task<string> ComputeFileHashAsync(string filePath, CancellationToken cancellationToken = default) {
        using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192, useAsync: true);
        var hashBytes = await SHA256.HashDataAsync(stream, cancellationToken);
        return Convert.ToHexStringLower(hashBytes);
    }
}
