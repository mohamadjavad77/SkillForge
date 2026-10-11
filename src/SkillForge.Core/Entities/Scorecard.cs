namespace SkillForge.Core.Entities;

public class Scorecard : BaseEntity
{
    public Guid BookingId { get; set; }
    public Guid InterviewerProfileId { get; set; }
    public DateTime? SubmittedAtUtc { get; set; }
    public bool IsLocked { get; set; }

    public Booking Booking { get; set; } = null!;
    public InterviewerProfile InterviewerProfile { get; set; } = null!;
    public Feedback? Feedback { get; set; }
    public ICollection<ScorecardSkillAssessment> SkillAssessments { get; set; } = new List<ScorecardSkillAssessment>();
}
