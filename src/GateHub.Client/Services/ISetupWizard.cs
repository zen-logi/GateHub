using GateHub.Client.Configuration;
using GateHub.Shared.Models;

namespace GateHub.Client.Services;

/// <summary>
/// 初回起動時のインタラクティブセットアップウィザードのインタフェース
/// </summary>
public interface ISetupWizard {
    /// <summary>
    /// セットアップウィザードを実行し、ユーザーに設定を入力させる
    /// </summary>
    /// <returns>入力された設定</returns>
    ClientSettings RunSetup();
}
