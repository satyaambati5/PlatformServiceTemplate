using Microsoft.Extensions.Options;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Options;

namespace PlatformServiceTemplate.Infrastructure.Security;

public sealed class ApiKeyValidator(IOptions<ApiKeyOptions> options) : IApiKeyValidator
{
    private readonly ApiKeyOptions _options = options.Value;

    public bool IsValid(string? key)
    {
        if (!_options.IsEnabled)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        return _options.AllowedKeys.Contains(key);
    }
}
