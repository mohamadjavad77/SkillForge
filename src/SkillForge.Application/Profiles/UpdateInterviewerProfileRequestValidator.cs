using FluentValidation;

namespace SkillForge.Application.Profiles;

public sealed class UpdateInterviewerProfileRequestValidator : AbstractValidator<UpdateInterviewerProfileRequest>
{
    public UpdateInterviewerProfileRequestValidator()
    {
        RuleFor(x => x.Bio)
            .MaximumLength(4000)
            .When(x => x.Bio is not null);

        RuleFor(x => x.SkillIds)
            .NotNull();

        RuleForEach(x => x.SkillIds)
            .NotEmpty();
    }
}
