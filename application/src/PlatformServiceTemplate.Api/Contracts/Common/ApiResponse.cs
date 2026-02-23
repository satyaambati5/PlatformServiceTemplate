namespace PlatformServiceTemplate.Api.Contracts.Common;

public sealed record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    string? CorrelationId,
    IReadOnlyCollection<string>? Errors = null)
{
    public static ApiResponse<T> Ok(T? data, string message, string? correlationId) =>
        new(true, message, data, correlationId);

    public static ApiResponse<T> Fail(string message, string? correlationId, params string[] errors) =>
        new(false, message, default, correlationId, errors);
}
