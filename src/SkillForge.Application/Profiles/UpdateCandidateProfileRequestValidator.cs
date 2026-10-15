using FluentValidation;

namespace SkillForge.Application.Profiles;

public sealed class UpdateCandidateProfileRequestValidator : AbstractValidator<UpdateCandidateProfileRequest>
{
    public UpdateCandidateProfileRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Headline)
            .MaximumLength(300)
            .When(x => x.Headline is not null);

        RuleFor(x => x.Bio)
            .MaximumLength(4000)
            .When(x => x.Bio is not null);

        RuleFor(x => x.ResumeFileUrl)
            .MaximumLength(1000)
            .When(x => x.ResumeFileUrl is not null);

        RuleFor(x => x.Skills)
            .NotNull();

        RuleForEach(x => x.Skills).ChildRules(skill =>
        {
            skill.RuleFor(s => s.SkillId).NotEmpty();
            skill.RuleFor(s => s.SelfProficiencyLevel).IsInEnum();
        });
    }
}
