using PlatformServiceTemplate.Domain.Enums;

namespace PlatformServiceTemplate.Application.Common.Models;

public sealed record WorkItemDto(Guid Id, string Title, string? Description, WorkItemStatus Status, DateTimeOffset CreatedAt);
