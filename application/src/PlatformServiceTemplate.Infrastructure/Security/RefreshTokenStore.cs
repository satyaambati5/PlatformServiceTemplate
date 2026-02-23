using Microsoft.EntityFrameworkCore;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Domain.Entities;
using PlatformServiceTemplate.Infrastructure.Persistence;

namespace PlatformServiceTemplate.Infrastructure.Security;

public sealed class RefreshTokenStore(ApplicationDbContext dbContext) : IRefreshTokenStore
{
    public Task AddAsync(RefreshToken token, CancellationToken cancellationToken)
        => dbContext.RefreshTokens.AddAsync(token, cancellationToken).AsTask();

    public Task<RefreshToken?> GetByHashAsync(string tokenHash, CancellationToken cancellationToken)
        => dbContext.RefreshTokens.FirstOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken) => dbContext.SaveChangesAsync(cancellationToken);
}
