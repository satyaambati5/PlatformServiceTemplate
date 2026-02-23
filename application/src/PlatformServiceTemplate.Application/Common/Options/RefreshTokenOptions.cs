using System.ComponentModel.DataAnnotations;

namespace PlatformServiceTemplate.Application.Common.Options;

public sealed class RefreshTokenOptions
{
    [Range(1, 90)]
    public int ExpirationDays { get; set; } = 7;
}
