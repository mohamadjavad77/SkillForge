namespace SkillForge.Core.Entities;

public class InterviewerSkill : BaseEntity
{
    public Guid InterviewerProfileId { get; set; }
    public Guid SkillId { get; set; }

    public InterviewerProfile InterviewerProfile { get; set; } = null!;
    public Skill Skill { get; set; } = null!;
}
