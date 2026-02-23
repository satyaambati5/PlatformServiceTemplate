using System.Text.Json;
using System.Threading.RateLimiting;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using PlatformServiceTemplate.Application;
using PlatformServiceTemplate.Application.Common.Exceptions;
using PlatformServiceTemplate.Infrastructure;
using PlatformServiceTemplate.Infrastructure.Identity;
using PlatformServiceTemplate.Infrastructure.Persistence;
using PlatformServiceTemplate.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    var logsDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "logs");
    Directory.CreateDirectory(logsDirectory);

    config.ReadFrom.Configuration(context.Configuration)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            Path.Combine(logsDirectory, "api-.log"),
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 14,
            shared: true);
});

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddProblemDetails();
builder.Services.AddHttpContextAccessor();
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: ["live"])
    .AddDbContextCheck<ApplicationDbContext>(tags: ["ready"]);

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("AuthPolicy", limiter =>
    {
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.PermitLimit = 15;
        limiter.QueueLimit = 0;
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("JwtPolicy", policy =>
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireAuthenticatedUser());
    options.AddPolicy("InternalPolicy", policy =>
        policy.AddAuthenticationSchemes("ApiKey", "Certificate").RequireAuthenticatedUser());
    options.AddPolicy("WebhookPolicy", policy =>
        policy.AddAuthenticationSchemes("Hmac").RequireAuthenticatedUser());
    options.AddPolicy("AdminPolicy", policy =>
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme).RequireRole("Admin"));
});

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("PlatformServiceTemplate.Api"))
    .WithTracing(trace =>
    {
        trace.AddAspNetCoreInstrumentation();
        trace.AddHttpClientInstrumentation();
        trace.AddOtlpExporter();
    });

var app = builder.Build();

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestTrackingMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exception = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>()?.Error;
        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            _ => StatusCodes.Status500InternalServerError
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Detail = exception?.Message,
            Status = statusCode
        };
        problem.Extensions["correlationId"] = context.TraceIdentifier;
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
    });
});

if (!app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
    foreach (var roleName in new[] { "User", "Admin" })
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            var createRoleResult = await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            if (!createRoleResult.Succeeded)
            {
                throw new InvalidOperationException($"Failed to create role '{roleName}': {string.Join(", ", createRoleResult.Errors.Select(x => x.Description))}");
            }
        }
    }
}

app.UseHttpsRedirection();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Platform Service Template API v1");
    options.RoutePrefix = "swagger";
});
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

var openApiEndpoint = app.MapOpenApi();
if (!app.Environment.IsDevelopment())
{
    openApiEndpoint.RequireAuthorization("AdminPolicy");
}

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live")
});
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});
app.MapControllers();

app.Run();

public partial class Program;
