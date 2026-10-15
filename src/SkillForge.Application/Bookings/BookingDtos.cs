using SkillForge.Core.Enums;

namespace SkillForge.Application.Bookings;

public sealed record CreateSlotRequest(
    DateTime StartUtc,
    DateTime EndUtc);

public sealed record SlotDto(
    Guid Id,
    Guid InterviewerProfileId,
    DateTime StartUtc,
    DateTime EndUtc,
    bool IsBooked);

public sealed record CreateBookingRequest(
    Guid SlotId,
    string? MeetingLink);

public sealed record BookingDto(
    Guid Id,
    Guid SlotId,
    Guid CandidateProfileId,
    string? MeetingLink,
    BookingStatus Status,
    DateTime StartUtc,
    DateTime EndUtc);
