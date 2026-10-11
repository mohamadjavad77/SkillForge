namespace SkillForge.Core.Entities;

public class AvailabilitySlot : BaseEntity
{
    public Guid InterviewerProfileId { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public bool IsBooked { get; set; }

    public InterviewerProfile InterviewerProfile { get; set; } = null!;
    public Booking? Booking { get; set; }
}
