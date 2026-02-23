using MediatR;
using PlatformServiceTemplate.Application.Common.Models;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Queries.GetWorkItems;

public sealed record GetWorkItemsQuery : IRequest<IReadOnlyCollection<WorkItemDto>>;
