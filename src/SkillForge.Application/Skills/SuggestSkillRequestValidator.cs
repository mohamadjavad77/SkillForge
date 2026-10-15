using FluentValidation;

namespace SkillForge.Application.Skills;

public sealed class SuggestSkillRequestValidator : AbstractValidator<SuggestSkillRequest>
{
    public SuggestSkillRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(128);

        RuleFor(x => x.Category)
            .NotEmpty()
            .MaximumLength(64);
    }
}
