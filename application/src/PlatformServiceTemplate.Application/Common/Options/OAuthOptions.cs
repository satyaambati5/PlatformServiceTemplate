namespace PlatformServiceTemplate.Application.Common.Options;

public sealed class OAuthOptions
{
    public bool IsEnabled { get; set; }
    public string Authority { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
}
