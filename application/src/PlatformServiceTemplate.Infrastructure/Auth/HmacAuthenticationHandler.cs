using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Options;

namespace PlatformServiceTemplate.Infrastructure.Auth;

public sealed class HmacAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IHmacValidator hmacValidator,
    IOptions<HmacOptions> hmacOptions)
    : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!hmacOptions.Value.IsEnabled)
        {
            return AuthenticateResult.NoResult();
        }

        var signature = Request.Headers[hmacOptions.Value.SignatureHeaderName].FirstOrDefault();
        var timestamp = Request.Headers[hmacOptions.Value.TimestampHeaderName].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(timestamp))
        {
            return AuthenticateResult.NoResult();
        }

        Request.EnableBuffering();
        using var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true);
        var body = await reader.ReadToEndAsync();
        Request.Body.Position = 0;

        var payload = $"{Request.Method}:{Request.Path}:{body}";
        var valid = await hmacValidator.IsValidAsync(signature, timestamp, payload, Context.RequestAborted);
        if (!valid)
        {
            return AuthenticateResult.Fail("Invalid HMAC signature.");
        }

        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, "webhook-client"), new Claim(ClaimTypes.Role, "WebhookClient") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        return AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(identity), Scheme.Name));
    }
}
