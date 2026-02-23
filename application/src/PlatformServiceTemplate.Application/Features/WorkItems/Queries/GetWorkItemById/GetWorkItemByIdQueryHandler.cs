using MediatR;
using PlatformServiceTemplate.Application.Common.Exceptions;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Models;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Queries.GetWorkItemById;

public sealed class GetWorkItemByIdQueryHandler(IWorkItemRepository workItemRepository) : IRequestHandler<GetWorkItemByIdQuery, WorkItemDto>
{
    public async Task<WorkItemDto> Handle(GetWorkItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await workItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (item is null)
        {
            throw new NotFoundException($"Work item '{request.Id}' was not found.");
        }

        return new WorkItemDto(item.Id, item.Title, item.Description, item.Status, item.CreatedAt);
    }
}
