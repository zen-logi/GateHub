using GateHub.Client;
using GateHub.Client.Configuration;
using GateHub.Client.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

// ユーザー設定ファイルの読み込みを追加（存在する場合のみ）
var configService = new ConfigurationService(
    Microsoft.Extensions.Logging.Abstractions.NullLogger<ConfigurationService>.Instance);
if (configService.SettingsFileExists()) {
    builder.Configuration.AddJsonFile(configService.SettingsFilePath, optional: true, reloadOnChange: false);
}

// 設定バインド（appsettings.json + ユーザー設定ファイルのマージ）
builder.Services.Configure<ClientSettings>(
    builder.Configuration.GetSection(ClientSettings.SectionName));

// HttpClient登録（APIトークンヘッダー付き）
// ユーザー設定ファイルからの読み込みを優先する
var clientSettings = new ClientSettings {
    ServerUrl = "http://localhost:5123",
    ApiToken = "",
    Pcsx2Path = "",
    MemoryCardPath = ""
};
builder.Configuration.GetSection(ClientSettings.SectionName).Bind(clientSettings);

// ユーザー設定ファイルからの設定で上書き
var userSettings = configService.LoadSettings();
if (userSettings is not null) {
    clientSettings = userSettings;
}

builder.Services.AddHttpClient<ISyncApiClient, SyncApiClient>((sp, client) => {
    client.BaseAddress = new Uri(clientSettings.ServerUrl);
    client.DefaultRequestHeaders.Add("X-Api-Token", clientSettings.ApiToken);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// DIサービス登録
builder.Services.AddSingleton<IConfigurationService, ConfigurationService>();
builder.Services.AddSingleton<ISetupWizard, SetupWizard>();
builder.Services.AddSingleton<ILocalManifestService, LocalManifestService>();
builder.Services.AddSingleton<ISyncEngine, SyncEngine>();
builder.Services.AddSingleton<IProcessController, ProcessController>();
builder.Services.AddSingleton<IGameSelector, GameSelector>();

// メインワークフロー
builder.Services.AddHostedService<SyncWorkflow>();

var host = builder.Build();
await host.RunAsync();
