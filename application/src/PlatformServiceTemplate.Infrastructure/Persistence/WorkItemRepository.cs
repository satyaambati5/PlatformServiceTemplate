using Microsoft.EntityFrameworkCore;
using PlatformServiceTemplate.Application.Common.Interfaces;
using PlatformServiceTemplate.Domain.Entities;

namespace PlatformServiceTemplate.Infrastructure.Persistence;

public sealed class WorkItemRepository(ApplicationDbContext dbContext) : IWorkItemRepository
{
    public Task AddAsync(WorkItem workItem, CancellationToken cancellationToken)
        => dbContext.WorkItems.AddAsync(workItem, cancellationToken).AsTask();

    public Task<WorkItem?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        => dbContext.WorkItems.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<List<WorkItem>> ListAsync(CancellationToken cancellationToken)
        => dbContext.WorkItems.OrderByDescending(x => x.CreatedAt).ToListAsync(cancellationToken);
}
