using System.Security.Cryptography.X509Certificates;

namespace PlatformServiceTemplate.Application.Common.Interfaces;

public interface ICertificateValidator
{
    bool IsAllowed(X509Certificate2? certificate);
}
