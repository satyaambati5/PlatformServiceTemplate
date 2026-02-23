using PlatformServiceTemplate.Domain.Entities;

namespace PlatformServiceTemplate.Application.Common.Interfaces;

public interface IRefreshTokenStore
{
    Task AddAsync(RefreshToken token, CancellationToken cancellationToken);
    Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken);
    Task SaveChangesAsync(CancellationToken cancellationToken);
}
