using PlatformServiceTemplate.Application.Features.WorkItems.Commands.CreateWorkItem;

namespace PlatformServiceTemplate.Application.UnitTests;

public sealed class CreateWorkItemCommandValidatorTests
{
    [Fact]
    public async Task Validate_WithEmptyTitle_Fails()
    {
        var validator = new CreateWorkItemCommandValidator();
        var result = await validator.ValidateAsync(new CreateWorkItemCommand(string.Empty, "desc"));
        Assert.False(result.IsValid);
    }

    [Fact]
    public async Task Validate_WithValidInput_Passes()
    {
        var validator = new CreateWorkItemCommandValidator();
        var result = await validator.ValidateAsync(new CreateWorkItemCommand("Task", "desc"));
        Assert.True(result.IsValid);
    }
}
