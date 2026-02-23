namespace PlatformServiceTemplate.Api.Contracts.WorkItems;

public sealed record CreateWorkItemRequest(string Title, string? Description);
