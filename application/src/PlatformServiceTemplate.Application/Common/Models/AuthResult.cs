namespace PlatformServiceTemplate.Application.Common.Models;

public sealed record AuthResult(AuthenticatedUser User, AuthTokens Tokens);
