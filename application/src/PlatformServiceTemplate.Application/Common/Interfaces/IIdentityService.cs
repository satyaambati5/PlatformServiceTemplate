using PlatformServiceTemplate.Application.Common.Models;

namespace PlatformServiceTemplate.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<(bool Succeeded, string[] Errors)> RegisterAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthenticatedUser?> ValidateCredentialsAsync(string email, string password, CancellationToken cancellationToken);
    Task<AuthenticatedUser?> GetUserByIdAsync(Guid userId, CancellationToken cancellationToken);
}
