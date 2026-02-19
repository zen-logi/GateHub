using GateHub.Server.Authentication;
using GateHub.Server.Configuration;
using GateHub.Server.Services;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// 設定バインド
builder.Services.Configure<ServerSettings>(
    builder.Configuration.GetSection(ServerSettings.SectionName));

// DI登録
builder.Services.AddSingleton<IManifestService, ManifestService>();
builder.Services.AddSingleton<IStorageService, StorageService>();

// 認証設定
builder.Services.AddAuthentication(ApiTokenAuthHandler.SchemeName)
    .AddScheme<AuthenticationSchemeOptions, ApiTokenAuthHandler>(
        ApiTokenAuthHandler.SchemeName, null);
builder.Services.AddAuthorization();

builder.Services.AddControllers();
builder.Services.AddOpenApi();

// LAN内アクセスに限定するためHTTPSリダイレクトは無効化
var app = builder.Build();

if (app.Environment.IsDevelopment())
    app.MapOpenApi();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
