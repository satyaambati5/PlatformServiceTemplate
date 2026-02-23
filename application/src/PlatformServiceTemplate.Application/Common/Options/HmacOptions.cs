using System.ComponentModel.DataAnnotations;

namespace PlatformServiceTemplate.Application.Common.Options;

public sealed class HmacOptions
{
    public bool IsEnabled { get; set; } = true;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    public string SignatureHeaderName { get; set; } = "X-Signature-256";
    public string TimestampHeaderName { get; set; } = "X-Timestamp";
    public int MaxRequestAgeSeconds { get; set; } = 300;
}
