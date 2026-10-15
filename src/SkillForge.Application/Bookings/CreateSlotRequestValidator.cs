using FluentValidation;

namespace SkillForge.Application.Bookings;

public sealed class CreateSlotRequestValidator : AbstractValidator<CreateSlotRequest>
{
    public CreateSlotRequestValidator()
    {
        RuleFor(x => x.StartUtc)
            .Must(dt => dt.Kind != DateTimeKind.Local)
            .WithMessage("StartUtc must be UTC.");

        RuleFor(x => x.EndUtc)
            .Must(dt => dt.Kind != DateTimeKind.Local)
            .WithMessage("EndUtc must be UTC.")
            .GreaterThan(x => x.StartUtc)
            .WithMessage("EndUtc must be after StartUtc.");
    }
}
