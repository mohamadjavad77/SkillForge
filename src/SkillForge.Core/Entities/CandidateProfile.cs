namespace SkillForge.Core.Entities;

public class CandidateProfile : BaseEntity
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string? Headline { get; set; }
    public string? Bio { get; set; }
    public string? ResumeFileUrl { get; set; }

    public User User { get; set; } = null!;
    public ICollection<CandidateSkill> Skills { get; set; } = new List<CandidateSkill>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<ScoreRecord> ScoreRecords { get; set; } = new List<ScoreRecord>();
}
