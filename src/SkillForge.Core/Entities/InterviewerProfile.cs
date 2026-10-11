namespace SkillForge.Core.Entities;

public class InterviewerProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string? Bio { get; set; }
    public decimal TrustScore { get; set; } = 100m;
    public bool IsVerified { get; set; }

    public User User { get; set; } = null!;
    public ICollection<InterviewerSkill> Skills { get; set; } = new List<InterviewerSkill>();
    public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();
    public ICollection<Scorecard> Scorecards { get; set; } = new List<Scorecard>();
}
