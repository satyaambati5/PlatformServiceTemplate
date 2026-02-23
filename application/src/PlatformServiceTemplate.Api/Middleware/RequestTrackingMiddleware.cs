using System.Diagnostics;
using System.Security.Claims;

namespace PlatformServiceTemplate.Api.Middleware;

public sealed class RequestTrackingMiddleware(RequestDelegate next, ILogger<RequestTrackingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

        logger.LogInformation(
            "Request started {Method} {Path} user={UserId} correlation={CorrelationId}",
            context.Request.Method,
            context.Request.Path,
            userId,
            context.TraceIdentifier);

        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Request failed {Method} {Path} user={UserId} correlation={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                userId,
                context.TraceIdentifier);
            throw;
        }
        finally
        {
            stopwatch.Stop();
            logger.LogInformation(
                "Request completed {Method} {Path} status={StatusCode} elapsedMs={ElapsedMs} user={UserId} correlation={CorrelationId}",
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds,
                userId,
                context.TraceIdentifier);
        }
    }
}
