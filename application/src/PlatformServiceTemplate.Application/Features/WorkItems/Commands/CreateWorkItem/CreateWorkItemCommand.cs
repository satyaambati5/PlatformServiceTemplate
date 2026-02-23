using MediatR;
using PlatformServiceTemplate.Application.Common.Models;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Commands.CreateWorkItem;

public sealed record CreateWorkItemCommand(string Title, string? Description) : IRequest<WorkItemDto>;
