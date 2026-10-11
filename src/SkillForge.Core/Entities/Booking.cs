using SkillForge.Core.Enums;

namespace SkillForge.Core.Entities;

public class Booking : BaseEntity
{
    public Guid SlotId { get; set; }
    public Guid CandidateProfileId { get; set; }
    public string? MeetingLink { get; set; }
    public BookingStatus Status { get; set; } = BookingStatus.Pending;

    public AvailabilitySlot Slot { get; set; } = null!;
    public CandidateProfile CandidateProfile { get; set; } = null!;
    public Scorecard? Scorecard { get; set; }
    public ScoreRecord? ScoreRecord { get; set; }
}
