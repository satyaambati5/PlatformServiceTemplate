namespace PlatformServiceTemplate.Application.Common.Options;

public sealed class ApiKeyOptions
{
    public bool IsEnabled { get; set; } = true;
    public string HeaderName { get; set; } = "X-Api-Key";
    public string[] AllowedKeys { get; set; } = [];
}
