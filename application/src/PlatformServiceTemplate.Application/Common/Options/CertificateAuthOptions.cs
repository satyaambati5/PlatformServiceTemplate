namespace PlatformServiceTemplate.Application.Common.Options;

public sealed class CertificateAuthOptions
{
    public bool IsEnabled { get; set; }
    public string[] AllowedThumbprints { get; set; } = [];
}
