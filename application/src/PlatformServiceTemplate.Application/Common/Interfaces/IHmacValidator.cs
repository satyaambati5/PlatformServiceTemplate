namespace PlatformServiceTemplate.Application.Common.Interfaces;

public interface IHmacValidator
{
    Task<bool> IsValidAsync(string? signature, string? timestamp, string payload, CancellationToken cancellationToken);
}
