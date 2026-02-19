namespace GateHub.Shared.Models;

/// <summary>
/// ファイル同期操作の結果を表すレコード
/// </summary>
/// <param name="Success">操作が成功したかどうか</param>
/// <param name="Message">結果メッセージ</param>
/// <param name="ConflictDetected">競合が検知されたかどうか</param>
public record SyncResult(
    bool Success,
    string Message,
    bool ConflictDetected = false);
