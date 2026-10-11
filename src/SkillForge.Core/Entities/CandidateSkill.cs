using SkillForge.Core.Enums;

namespace SkillForge.Core.Entities;

public class CandidateSkill : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public Guid SkillId { get; set; }
    public ProficiencyLevel SelfProficiencyLevel { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
