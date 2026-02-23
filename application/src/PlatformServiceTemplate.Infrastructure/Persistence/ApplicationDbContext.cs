using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PlatformServiceTemplate.Domain.Entities;
using PlatformServiceTemplate.Infrastructure.Identity;

namespace PlatformServiceTemplate.Infrastructure.Persistence;

public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<WorkItem> WorkItems => Set<WorkItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.HasDefaultSchema("app");
        builder.Entity<ApplicationUser>().ToTable("Users", "identity");
        builder.Entity<IdentityRole<Guid>>().ToTable("Roles", "identity");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles", "identity");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims", "identity");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins", "identity");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims", "identity");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens", "identity");

        builder.Entity<WorkItem>(entity =>
        {
            entity.ToTable("WorkItems", "app");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.HasQueryFilter(x => !x.IsDeleted);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens", "identity");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TokenHash).HasMaxLength(512).IsRequired();
            entity.HasIndex(x => x.TokenHash).IsUnique();
        });
    }
}
