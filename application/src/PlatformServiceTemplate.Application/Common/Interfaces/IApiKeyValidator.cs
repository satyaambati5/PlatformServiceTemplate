namespace PlatformServiceTemplate.Application.Common.Interfaces;

public interface IApiKeyValidator
{
    bool IsValid(string? key);
}
