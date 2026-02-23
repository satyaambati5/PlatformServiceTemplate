using PlatformServiceTemplate.Domain.Common;
using PlatformServiceTemplate.Domain.Enums;

namespace PlatformServiceTemplate.Domain.Entities;

public sealed class WorkItem : BaseAuditableEntity
{
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public WorkItemStatus Status { get; private set; } = WorkItemStatus.New;
    public Guid CreatedByUserId { get; private set; }

    private WorkItem() { }

    public WorkItem(string title, string? description, Guid createdByUserId)
    {
        SetTitle(title);
        Description = description;
        CreatedByUserId = createdByUserId;
    }

    public void SetTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }

        Title = title.Trim();
    }

    public void SetDescription(string? description)
    {
        Description = description?.Trim();
    }

    public void SetStatus(WorkItemStatus status)
    {
        Status = status;
    }
}
