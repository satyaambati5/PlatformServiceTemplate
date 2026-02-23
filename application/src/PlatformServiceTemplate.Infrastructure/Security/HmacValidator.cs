using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Options;

namespace PlatformServiceTemplate.Infrastructure.Security;

public sealed class HmacValidator(IOptions<HmacOptions> options) : IHmacValidator
{
    private readonly HmacOptions _options = options.Value;

    public Task<bool> IsValidAsync(string? signature, string? timestamp, string payload, CancellationToken cancellationToken)
    {
        if (!_options.IsEnabled || string.IsNullOrWhiteSpace(signature) || string.IsNullOrWhiteSpace(timestamp))
        {
            return Task.FromResult(false);
        }

        if (!long.TryParse(timestamp, out var unix))
        {
            return Task.FromResult(false);
        }

        var issued = DateTimeOffset.FromUnixTimeSeconds(unix);
        var age = Math.Abs((DateTimeOffset.UtcNow - issued).TotalSeconds);
        if (age > _options.MaxRequestAgeSeconds)
        {
            return Task.FromResult(false);
        }

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SecretKey));
        var expected = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes($"{timestamp}.{payload}"))).ToLowerInvariant();
        return Task.FromResult(signature.Equals(expected, StringComparison.OrdinalIgnoreCase));
    }
}
