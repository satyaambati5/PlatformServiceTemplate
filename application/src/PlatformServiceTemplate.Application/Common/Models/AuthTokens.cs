namespace PlatformServiceTemplate.Application.Common.Models;

public sealed record AuthTokens(string AccessToken, string RefreshToken, DateTimeOffset AccessTokenExpiresAt);
