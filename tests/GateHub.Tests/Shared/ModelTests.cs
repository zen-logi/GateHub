using GateHub.Shared.Models;

namespace GateHub.Tests.Shared;

public class ModelTests {

    /// <summary>
    /// FileManifestEntryのプロパティが正しく設定されることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess1() {
        var timestamp = DateTime.UtcNow;
        var entry = new FileManifestEntry("save/game.bin", "abc123", 1024, timestamp);

        await Assert.That(entry.RelativePath).IsEqualTo("save/game.bin");
        await Assert.That(entry.Hash).IsEqualTo("abc123");
        await Assert.That(entry.Size).IsEqualTo(1024);
        await Assert.That(entry.LastModifiedUtc).IsEqualTo(timestamp);
    }

    /// <summary>
    /// SyncManifestがエントリリストとタイムスタンプを正しく保持することを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess2() {
        var entries = new List<FileManifestEntry> {
            new("file1.bin", "hash1", 100, DateTime.UtcNow),
            new("file2.bin", "hash2", 200, DateTime.UtcNow)
        };
        var generatedAt = DateTime.UtcNow;

        var manifest = new SyncManifest(entries, generatedAt);

        await Assert.That(manifest.Entries).HasCount().EqualTo(2);
        await Assert.That(manifest.GeneratedAtUtc).IsEqualTo(generatedAt);
    }

    /// <summary>
    /// SyncResultが正常結果を正しく表現できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess3() {
        var result = new SyncResult(true, "completed");

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.Message).IsEqualTo("completed");
        await Assert.That(result.ConflictDetected).IsFalse();
    }

    /// <summary>
    /// SyncResultが競合検知結果を正しく表現できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessConflict() {
        var result = new SyncResult(true, "conflict resolved", true);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.ConflictDetected).IsTrue();
    }

    /// <summary>
    /// GameInfoのプロパティが正しく設定されることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess4() {
        var game = new GameInfo("FFX", "/path/to/ffx.iso", "SLPM-65888");

        await Assert.That(game.Title).IsEqualTo("FFX");
        await Assert.That(game.IsoPath).IsEqualTo("/path/to/ffx.iso");
        await Assert.That(game.GameId).IsEqualTo("SLPM-65888");
    }
}
