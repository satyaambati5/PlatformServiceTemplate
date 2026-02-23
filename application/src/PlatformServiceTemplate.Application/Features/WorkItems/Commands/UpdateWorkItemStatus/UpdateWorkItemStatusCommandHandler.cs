using MediatR;
using PlatformServiceTemplate.Application.Common.Exceptions;
using PlatformServiceTemplate.Application.Common.Interfaces;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Commands.UpdateWorkItemStatus;

public sealed class UpdateWorkItemStatusCommandHandler(IWorkItemRepository workItemRepository, IUnitOfWork unitOfWork) : IRequestHandler<UpdateWorkItemStatusCommand>
{
    public async Task Handle(UpdateWorkItemStatusCommand request, CancellationToken cancellationToken)
    {
        var workItem = await workItemRepository.GetByIdAsync(request.Id, cancellationToken);
        if (workItem is null)
        {
            throw new NotFoundException($"Work item '{request.Id}' was not found.");
        }

        workItem.SetStatus(request.Status);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
