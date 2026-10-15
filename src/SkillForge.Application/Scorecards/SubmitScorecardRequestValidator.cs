using FluentValidation;

namespace SkillForge.Application.Scorecards;

public sealed class SubmitScorecardRequestValidator : AbstractValidator<SubmitScorecardRequest>
{
    public SubmitScorecardRequestValidator()
    {
        RuleFor(x => x.Assessments).NotNull();
        RuleForEach(x => x.Assessments).ChildRules(a =>
        {
            a.RuleFor(x => x.SkillId).NotEmpty();
            a.RuleFor(x => x.ProficiencyLevel).IsInEnum();
            a.RuleFor(x => x.Notes).MaximumLength(2000).When(x => x.Notes is not null);
        });

        RuleFor(x => x.Strengths).NotNull();
        RuleFor(x => x.AreasForImprovement).NotNull();
        RuleFor(x => x.Verdict).IsInEnum();
        RuleFor(x => x.OverallComment)
            .MaximumLength(4000)
            .When(x => x.OverallComment is not null);
    }
}
