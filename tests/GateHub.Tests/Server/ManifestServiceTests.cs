using GateHub.Server.Configuration;
using GateHub.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GateHub.Tests.Server;

public class ManifestServiceTests {
    private string _storagePath = null!;
    private ManifestService _service = null!;

    [Before(Test)]
    public void Setup() {
        _storagePath = Path.Combine(Path.GetTempPath(), "GateHub_Manifest_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_storagePath);

        var settings = Options.Create(new ServerSettings {
            StoragePath = _storagePath,
            ApiTokens = ["test-token"]
        });
        _service = new ManifestService(settings, NullLogger<ManifestService>.Instance);
    }

    [After(Test)]
    public void Cleanup() {
        if (Directory.Exists(_storagePath))
            Directory.Delete(_storagePath, recursive: true);
    }

    /// <summary>
    /// 空ディレクトリでは空のマニフェストを返すことを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessEmpty() {
        var manifest = await _service.GenerateManifestAsync();

        await Assert.That(manifest.Entries).HasCount().EqualTo(0);
    }

    /// <summary>
    /// 複数ファイルが正しくマニフェストに含まれることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess1() {
        File.WriteAllBytes(Path.Combine(_storagePath, "save1.bin"), "data1"u8.ToArray());
        File.WriteAllBytes(Path.Combine(_storagePath, "save2.bin"), "data2"u8.ToArray());

        var manifest = await _service.GenerateManifestAsync();

        await Assert.That(manifest.Entries).HasCount().EqualTo(2);
    }

    /// <summary>
    /// .tmpファイルがマニフェストから除外されることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessExcludeTmp() {
        File.WriteAllBytes(Path.Combine(_storagePath, "save.bin"), "data"u8.ToArray());
        File.WriteAllBytes(Path.Combine(_storagePath, "save.bin.tmp"), "temp"u8.ToArray());

        var manifest = await _service.GenerateManifestAsync();

        await Assert.That(manifest.Entries).HasCount().EqualTo(1);
        await Assert.That(manifest.Entries[0].RelativePath).IsEqualTo("save.bin");
    }

    /// <summary>
    /// .conflictファイルがマニフェストから除外されることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessExcludeConflict() {
        File.WriteAllBytes(Path.Combine(_storagePath, "save.bin"), "data"u8.ToArray());
        File.WriteAllBytes(Path.Combine(_storagePath, "save.bin.conflict.20260220120000"), "old"u8.ToArray());

        var manifest = await _service.GenerateManifestAsync();

        await Assert.That(manifest.Entries).HasCount().EqualTo(1);
    }

    /// <summary>
    /// サブディレクトリ内のファイルがスラッシュ区切りの相対パスで含まれることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessNestedDirectory() {
        var subDir = Path.Combine(_storagePath, "subdir");
        Directory.CreateDirectory(subDir);
        File.WriteAllBytes(Path.Combine(subDir, "nested.bin"), "nested"u8.ToArray());

        var manifest = await _service.GenerateManifestAsync();

        await Assert.That(manifest.Entries).HasCount().EqualTo(1);
        await Assert.That(manifest.Entries[0].RelativePath).IsEqualTo("subdir/nested.bin");
    }

    /// <summary>
    /// 存在しないストレージディレクトリが自動作成されることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessAutoCreateDirectory() {
        var nonExistentPath = Path.Combine(Path.GetTempPath(), "GateHub_NonExist_" + Guid.NewGuid().ToString("N")[..8]);
        try {
            var settings = Options.Create(new ServerSettings {
                StoragePath = nonExistentPath,
                ApiTokens = ["test-token"]
            });
            var service = new ManifestService(settings, NullLogger<ManifestService>.Instance);

            var manifest = await service.GenerateManifestAsync();

            await Assert.That(manifest.Entries).HasCount().EqualTo(0);
            await Assert.That(Directory.Exists(nonExistentPath)).IsTrue();
        }
        finally {
            if (Directory.Exists(nonExistentPath))
                Directory.Delete(nonExistentPath, recursive: true);
        }
    }
}
