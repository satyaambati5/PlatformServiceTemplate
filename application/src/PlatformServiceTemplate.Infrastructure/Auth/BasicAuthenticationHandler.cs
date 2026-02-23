using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PlatformServiceTemplate.Application.Common.Options;

namespace PlatformServiceTemplate.Infrastructure.Auth;

public sealed class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<BasicAuthOptions> basicOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!basicOptions.Value.IsEnabled)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Request.Headers.Authorization.ToString().StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var encoded = Request.Headers.Authorization.ToString()["Basic ".Length..].Trim();
        var credential = Encoding.UTF8.GetString(Convert.FromBase64String(encoded));
        var parts = credential.Split(':', 2);
        if (parts.Length != 2)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid basic auth header."));
        }

        var isValid = parts[0] == basicOptions.Value.Username && parts[1] == basicOptions.Value.Password;
        if (!isValid)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid basic auth credentials."));
        }

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, parts[0]), new Claim(ClaimTypes.Role, "LegacyClient") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(principal, Scheme.Name)));
    }
}
