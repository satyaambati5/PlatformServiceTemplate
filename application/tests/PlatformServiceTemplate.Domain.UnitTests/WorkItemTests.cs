using PlatformServiceTemplate.Domain.Entities;
using PlatformServiceTemplate.Domain.Enums;

namespace PlatformServiceTemplate.Domain.UnitTests;

public sealed class WorkItemTests
{
    [Fact]
    public void Constructor_WithValidData_SetsDefaults()
    {
        var item = new WorkItem("Task A", "Desc", Guid.NewGuid());
        Assert.Equal("Task A", item.Title);
        Assert.Equal(WorkItemStatus.New, item.Status);
    }

    [Fact]
    public void SetTitle_WithInvalidTitle_Throws()
    {
        var item = new WorkItem("Task A", null, Guid.NewGuid());
        Assert.Throws<ArgumentException>(() => item.SetTitle(" "));
    }
}
