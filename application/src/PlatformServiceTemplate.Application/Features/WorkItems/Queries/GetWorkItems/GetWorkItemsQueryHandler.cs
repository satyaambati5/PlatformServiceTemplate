using MediatR;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Models;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Queries.GetWorkItems;

public sealed class GetWorkItemsQueryHandler(IWorkItemRepository workItemRepository) : IRequestHandler<GetWorkItemsQuery, IReadOnlyCollection<WorkItemDto>>
{
    public async Task<IReadOnlyCollection<WorkItemDto>> Handle(GetWorkItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await workItemRepository.ListAsync(cancellationToken);
        return items.Select(x => new WorkItemDto(x.Id, x.Title, x.Description, x.Status, x.CreatedAt)).ToArray();
    }
}
