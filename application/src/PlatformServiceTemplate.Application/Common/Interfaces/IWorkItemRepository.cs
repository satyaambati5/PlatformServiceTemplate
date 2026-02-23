using PlatformServiceTemplate.Domain.Entities;

namespace PlatformServiceTemplate.Application.Common.Interfaces;

public interface IWorkItemRepository
{
    Task AddAsync(WorkItem workItem, CancellationToken cancellationToken);
    Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<WorkItem>> ListAsync(CancellationToken cancellationToken);
}
