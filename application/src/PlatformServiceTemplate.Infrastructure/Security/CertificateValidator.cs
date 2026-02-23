using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Options;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Options;

namespace PlatformServiceTemplate.Infrastructure.Security;

public sealed class CertificateValidator(IOptions<CertificateAuthOptions> options) : ICertificateValidator
{
    private readonly CertificateAuthOptions _options = options.Value;

    public bool IsAllowed(X509Certificate2? certificate)
    {
        if (!_options.IsEnabled || certificate is null)
        {
            return false;
        }

        return _options.AllowedThumbprints.Contains(certificate.Thumbprint, StringComparer.OrdinalIgnoreCase);
    }
}
