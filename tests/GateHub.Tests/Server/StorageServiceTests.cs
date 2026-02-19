using GateHub.Server.Configuration;
using GateHub.Server.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace GateHub.Tests.Server;

public class StorageServiceTests {
    private string _storagePath = null!;
    private StorageService _service = null!;

    [Before(Test)]
    public void Setup() {
        _storagePath = Path.Combine(Path.GetTempPath(), "GateHub_Test_" + Guid.NewGuid().ToString("N")[..8]);
        Directory.CreateDirectory(_storagePath);

        var settings = Options.Create(new ServerSettings {
            StoragePath = _storagePath,
            ApiTokens = ["test-token"]
        });
        _service = new StorageService(settings, NullLogger<StorageService>.Instance);
    }

    [After(Test)]
    public void Cleanup() {
        if (Directory.Exists(_storagePath))
            Directory.Delete(_storagePath, recursive: true);
    }

    /// <summary>
    /// 新規ファイルをアトミックに保存できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess1() {
        using var content = new MemoryStream("Hello World"u8.ToArray());

        var result = await _service.SaveFileAtomicallyAsync("test.bin", content, null);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(File.Exists(Path.Combine(_storagePath, "test.bin"))).IsTrue();
    }

    /// <summary>
    /// 保存後に一時ファイル(.tmp)が残らないことを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess2() {
        using var content = new MemoryStream("data"u8.ToArray());

        await _service.SaveFileAtomicallyAsync("file.bin", content, null);

        var tmpExists = File.Exists(Path.Combine(_storagePath, "file.bin.tmp"));
        await Assert.That(tmpExists).IsFalse();
    }

    /// <summary>
    /// ハッシュ一致時に競合なしで正常更新できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess3() {
        // 初回保存
        var data = "original"u8.ToArray();
        using (var s = new MemoryStream(data))
            await _service.SaveFileAtomicallyAsync("data.bin", s, null);

        // 現在のハッシュを取得
        var hash = await _service.GetFileHashAsync("data.bin");

        // ハッシュ一致で更新
        using var newContent = new MemoryStream("updated"u8.ToArray());
        var result = await _service.SaveFileAtomicallyAsync("data.bin", newContent, hash);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.ConflictDetected).IsFalse();
    }

    /// <summary>
    /// ハッシュ不一致時にconflictファイルが生成されることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessConflict() {
        // 初回保存
        using (var s = new MemoryStream("original"u8.ToArray()))
            await _service.SaveFileAtomicallyAsync("data.bin", s, null);

        // 不正なハッシュで上書き試行
        using var newContent = new MemoryStream("conflicting"u8.ToArray());
        var result = await _service.SaveFileAtomicallyAsync("data.bin", newContent, "wrong-hash");

        await Assert.That(result.Success).IsTrue();
        await Assert.That(result.ConflictDetected).IsTrue();

        // .conflict.ファイルが生成されていることを確認
        var conflictFiles = Directory.GetFiles(_storagePath, "*.conflict.*");
        await Assert.That(conflictFiles).HasCount().EqualTo(1);
    }

    /// <summary>
    /// 存在するファイルのStreamを取得できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess4() {
        var filePath = Path.Combine(_storagePath, "existing.bin");
        File.WriteAllBytes(filePath, "content"u8.ToArray());

        using var stream = _service.GetFileStream("existing.bin");

        await Assert.That(stream).IsNotNull();
    }

    /// <summary>
    /// 存在しないファイルに対してnullを返すことを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessNotFound() {
        var stream = _service.GetFileStream("nonexistent.bin");

        await Assert.That(stream).IsNull();
    }

    /// <summary>
    /// 存在するファイルを正常に削除できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccess5() {
        var filePath = Path.Combine(_storagePath, "to-delete.bin");
        File.WriteAllBytes(filePath, "data"u8.ToArray());

        var result = _service.DeleteFile("to-delete.bin");

        await Assert.That(result.Success).IsTrue();
        await Assert.That(File.Exists(filePath)).IsFalse();
    }

    /// <summary>
    /// 存在しないファイルの削除でもSuccessを返すことを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessDeleteMissing() {
        var result = _service.DeleteFile("missing.bin");

        await Assert.That(result.Success).IsTrue();
    }

    /// <summary>
    /// パストラバーサル攻撃を拒否することを検証する
    /// </summary>
    [Test]
    public async Task TestFailurePathTraversal() {
        await Assert.That(() => _service.GetFileStream("../../etc/passwd")).Throws<UnauthorizedAccessException>();
    }

    /// <summary>
    /// サブディレクトリ階層を含むパスにファイルを保存できることを検証する
    /// </summary>
    [Test]
    public async Task TestSuccessNestedDirectory() {
        using var content = new MemoryStream("nested"u8.ToArray());

        var result = await _service.SaveFileAtomicallyAsync("sub/dir/file.bin", content, null);

        await Assert.That(result.Success).IsTrue();
        await Assert.That(File.Exists(Path.Combine(_storagePath, "sub", "dir", "file.bin"))).IsTrue();
    }
}
