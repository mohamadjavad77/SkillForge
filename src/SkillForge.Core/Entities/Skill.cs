namespace SkillForge.Core.Entities;

public class Skill : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public User? CreatedByUser { get; set; }
    public ICollection<CandidateSkill> CandidateSkills { get; set; } = new List<CandidateSkill>();
    public ICollection<InterviewerSkill> InterviewerSkills { get; set; } = new List<InterviewerSkill>();
    public ICollection<ScorecardSkillAssessment> ScorecardAssessments { get; set; } = new List<ScorecardSkillAssessment>();
}
