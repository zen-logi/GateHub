using System.Security.Cryptography;
using GateHub.Shared.Utilities;

namespace GateHub.Tests.Shared;

public class HashHelperTests {

    /// <summary>
    /// 正常なファイルのSHA256ハッシュを正しく計算できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess1() {
        // Arrange
        var tempFile = Path.GetTempFileName();
        try {
            var content = "Hello GateHub"u8.ToArray();
            await File.WriteAllBytesAsync(tempFile, content);

            // 期待値を計算
            var expectedHash = Convert.ToHexStringLower(SHA256.HashData(content));

            // Act
            var result = await HashHelper.ComputeFileHashAsync(tempFile);

            // Assert
            await Assert.That(result).IsEqualTo(expectedHash);
        }
        finally {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// 空ファイルのハッシュを正しく計算できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessEmptyFile() {
        var tempFile = Path.GetTempFileName();
        try {
            var expectedHash = Convert.ToHexStringLower(SHA256.HashData([]));

            var result = await HashHelper.ComputeFileHashAsync(tempFile);

            await Assert.That(result).IsEqualTo(expectedHash);
        }
        finally {
            File.Delete(tempFile);
        }
    }

    /// <summary>
    /// 同一内容のファイルは同じハッシュを返すことを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess2() {
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        try {
            var content = "identical content"u8.ToArray();
            await File.WriteAllBytesAsync(tempFile1, content);
            await File.WriteAllBytesAsync(tempFile2, content);

            var hash1 = await HashHelper.ComputeFileHashAsync(tempFile1);
            var hash2 = await HashHelper.ComputeFileHashAsync(tempFile2);

            await Assert.That(hash1).IsEqualTo(hash2);
        }
        finally {
            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }
    }

    /// <summary>
    /// 異なる内容のファイルは異なるハッシュを返すことを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess3() {
        var tempFile1 = Path.GetTempFileName();
        var tempFile2 = Path.GetTempFileName();
        try {
            await File.WriteAllBytesAsync(tempFile1, "Content A"u8.ToArray());
            await File.WriteAllBytesAsync(tempFile2, "Content B"u8.ToArray());

            var hash1 = await HashHelper.ComputeFileHashAsync(tempFile1);
            var hash2 = await HashHelper.ComputeFileHashAsync(tempFile2);

            await Assert.That(hash1).IsNotEqualTo(hash2);
        }
        finally {
            File.Delete(tempFile1);
            File.Delete(tempFile2);
        }
    }

    /// <summary>
    /// ハッシュ値が64文字の小文字16進文字列であることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessFormat() {
        var tempFile = Path.GetTempFileName();
        try {
            await File.WriteAllBytesAsync(tempFile, "test"u8.ToArray());

            var result = await HashHelper.ComputeFileHashAsync(tempFile);

            // 64文字の16進文字列（SHA256 = 256bit = 32byte = 64hex chars）
            await Assert.That(result.Length).IsEqualTo(64);
            await Assert.That(result).IsEqualTo(result.ToLowerInvariant());
        }
        finally {
            File.Delete(tempFile);
        }
    }
}
