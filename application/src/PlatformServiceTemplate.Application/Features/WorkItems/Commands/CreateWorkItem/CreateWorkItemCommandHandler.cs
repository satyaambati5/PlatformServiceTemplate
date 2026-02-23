using MediatR;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Application.Common.Models;
using PlatformServiceTemplate.Domain.Entities;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Commands.CreateWorkItem;

public sealed class CreateWorkItemCommandHandler(
    IWorkItemRepository workItemRepository,
    IUnitOfWork unitOfWork,
    ICurrentUserService currentUserService) : IRequestHandler<CreateWorkItemCommand, WorkItemDto>
{
    public async Task<WorkItemDto> Handle(CreateWorkItemCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? Guid.Empty;
        var entity = new WorkItem(request.Title, request.Description, userId);

        await workItemRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new WorkItemDto(entity.Id, entity.Title, entity.Description, entity.Status, entity.CreatedAt);
    }
}
