using SkillForge.Core.Enums;

namespace SkillForge.Core.Entities;

public class Feedback : BaseEntity
{
    public Guid ScorecardId { get; set; }
    public string StrengthsJson { get; set; } = "[]";
    public string AreasForImprovementJson { get; set; } = "[]";
    public Verdict Verdict { get; set; }
    public string? OverallComment { get; set; }

    public Scorecard Scorecard { get; set; } = null!;
}
