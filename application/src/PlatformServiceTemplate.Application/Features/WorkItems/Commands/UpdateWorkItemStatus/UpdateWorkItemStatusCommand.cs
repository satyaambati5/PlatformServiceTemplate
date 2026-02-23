using MediatR;
using PlatformServiceTemplate.Domain.Enums;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Commands.UpdateWorkItemStatus;

public sealed record UpdateWorkItemStatusCommand(Guid Id, WorkItemStatus Status) : IRequest;
