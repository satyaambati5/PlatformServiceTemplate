using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace PlatformServiceTemplate.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory(IConfiguration configuration) : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString = configuration["ConnectionStrings:DefaultConnection"] ?? configuration.GetConnectionString("DefaultConnection");
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseSqlServer(connectionString);
        // PostgreSQL alternative:
        // optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=platform_service_template;Username=postgres;Password=postgres");
        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
