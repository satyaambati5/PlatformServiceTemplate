using MediatR;
using PlatformServiceTemplate.Application.Common.Models;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Queries.GetWorkItemById;

public sealed record GetWorkItemByIdQuery(Guid Id) : IRequest<WorkItemDto>;
