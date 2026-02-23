using FluentValidation;

namespace PlatformServiceTemplate.Application.Features.WorkItems.Commands.CreateWorkItem;

public sealed class CreateWorkItemCommandValidator : AbstractValidator<CreateWorkItemCommand>
{
    public CreateWorkItemCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}
