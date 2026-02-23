using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Options;
using PlatformServiceTemplate.Infrastructure.Auth;
using PlatformServiceTemplate.Infrastructure.Identity;
using PlatformServiceTemplate.Infrastructure.Persistence;
using PlatformServiceTemplate.Infrastructure.Security;
using System.Text;

namespace PlatformServiceTemplate.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection("Authentication:Jwt"));
        services.Configure<RefreshTokenOptions>(configuration.GetSection("Authentication:RefreshToken"));
        services.Configure<ApiKeyOptions>(configuration.GetSection("Authentication:ApiKey"));
        services.Configure<BasicAuthOptions>(configuration.GetSection("Authentication:Basic"));
        services.Configure<OAuthOptions>(configuration.GetSection("Authentication:OAuth"));
        services.Configure<CertificateAuthOptions>(configuration.GetSection("Authentication:Certificate"));
        services.Configure<HmacOptions>(configuration.GetSection("Authentication:Hmac"));

        services.AddOptions<JwtOptions>().Bind(configuration.GetSection("Authentication:Jwt")).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<RefreshTokenOptions>().Bind(configuration.GetSection("Authentication:RefreshToken")).ValidateDataAnnotations().ValidateOnStart();
        services.AddOptions<HmacOptions>().Bind(configuration.GetSection("Authentication:Hmac")).ValidateDataAnnotations().ValidateOnStart();

        var connectionString = configuration["ConnectionStrings:DefaultConnection"] ?? configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Database connection string is not configured.");
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString);
            // PostgreSQL alternative:
            // options.UseNpgsql(connectionString);
        });

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var jwtOptions = configuration.GetSection("Authentication:Jwt").Get<JwtOptions>() ?? new JwtOptions();
        var oauthOptions = configuration.GetSection("Authentication:OAuth").Get<OAuthOptions>() ?? new OAuthOptions();

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        });

        authBuilder.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SecretKey)),
                ClockSkew = TimeSpan.Zero
            };
        });

        if (oauthOptions.IsEnabled)
        {
            authBuilder.AddJwtBearer("Oidc", options =>
            {
                options.Authority = oauthOptions.Authority;
                options.Audience = oauthOptions.Audience;
                options.RequireHttpsMetadata = true;
            });
        }

        authBuilder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>("ApiKey", _ => { });
        authBuilder.AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", _ => { });
        authBuilder.AddScheme<AuthenticationSchemeOptions, HmacAuthenticationHandler>("Hmac", _ => { });
        authBuilder.AddCertificate("Certificate", options =>
        {
            options.AllowedCertificateTypes = CertificateTypes.All;
            options.Events = new CertificateAuthenticationEvents
            {
                OnCertificateValidated = context =>
                {
                    var validator = context.HttpContext.RequestServices.GetRequiredService<ICertificateValidator>();
                    if (!validator.IsAllowed(context.ClientCertificate))
                    {
                        context.Fail("Certificate is not allowed.");
                    }

                    return Task.CompletedTask;
                }
            };
        });

        services.AddHttpContextAccessor();
        services.AddScoped<IWorkItemRepository, WorkItemRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<IRefreshTokenStore, RefreshTokenStore>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<ITokenService, TokenService>();
        services.AddSingleton<IApiKeyValidator, ApiKeyValidator>();
        services.AddSingleton<IHmacValidator, HmacValidator>();
        services.AddSingleton<ICertificateValidator, CertificateValidator>();

        return services;
    }
}
