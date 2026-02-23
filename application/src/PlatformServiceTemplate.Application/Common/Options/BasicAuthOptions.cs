namespace PlatformServiceTemplate.Application.Common.Options;

public sealed class BasicAuthOptions
{
    public bool IsEnabled { get; set; }
    public string Username { get; set; } = "legacy";
    public string Password { get; set; } = "legacy-change-me";
}
