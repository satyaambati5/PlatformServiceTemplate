namespace PlatformServiceTemplate.Application.Common.Interfaces;

public interface ITokenService
{
    string CreateAccessToken(Guid userId, string email, IReadOnlyCollection<string> roles);
    string CreateRefreshToken();
    string HashToken(string token);
}
