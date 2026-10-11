namespace SkillForge.Core.Entities;

/// <summary>
/// Immutable, append-only assessment outcome. Do not update after insert.
/// </summary>
public class ScoreRecord : BaseEntity
{
    public Guid CandidateProfileId { get; set; }
    public Guid BookingId { get; set; }
    public string SkillScoresJson { get; set; } = "{}";
    public int VerdictScore { get; set; }
    public int OverallScore { get; set; }

    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Booking Booking { get; set; } = null!;
}
