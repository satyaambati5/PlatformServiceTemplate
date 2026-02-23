namespace PlatformServiceTemplate.Application.Common.Models;

public sealed record AuthenticatedUser(Guid Id, string Email, string UserName, IReadOnlyCollection<string> Roles);
