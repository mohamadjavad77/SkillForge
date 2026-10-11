using SkillForge.Core.Enums;

namespace SkillForge.Core.Entities;

public class ScorecardSkillAssessment : BaseEntity
{
    public Guid ScorecardId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel ProficiencyLevel { get; set; }
    public string? Notes { get; set; }

    public Scorecard Scorecard { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
