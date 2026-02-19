using System.Security.Claims;
using System.Text.Encodings.Web;
using GateHub.Server.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace GateHub.Server.Authentication;

/// <summary>
/// X-Api-Tokenヘッダーベースの簡易認証ハンドラー
/// </summary>
public sealed class ApiTokenAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<ServerSettings> serverSettings) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder) {

    /// <summary>
    /// 認証スキーム名
    /// </summary>
    public const string SchemeName = "ApiToken";

    private const string TokenHeaderName = "X-Api-Token";

    /// <inheritdoc />
    protected override Task<AuthenticateResult> HandleAuthenticateAsync() {
        if (!Request.Headers.TryGetValue(TokenHeaderName, out var tokenValues)) {
            return Task.FromResult(AuthenticateResult.Fail("X-Api-Tokenヘッダーが見つからない"));
        }

        var token = tokenValues.ToString();
        if (!serverSettings.Value.ApiTokens.Contains(token)) {
            return Task.FromResult(AuthenticateResult.Fail("無効なAPIトークン"));
        }

        var claims = new[] { new Claim(ClaimTypes.Name, token) };
        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
