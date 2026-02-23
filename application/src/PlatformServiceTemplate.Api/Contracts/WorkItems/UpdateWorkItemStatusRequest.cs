using PlatformServiceTemplate.Domain.Enums;

namespace PlatformServiceTemplate.Api.Contracts.WorkItems;

public sealed record UpdateWorkItemStatusRequest(WorkItemStatus Status);
