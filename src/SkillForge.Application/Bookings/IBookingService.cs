using SkillForge.Application.Scorecards;

namespace SkillForge.Application.Bookings;

public interface IBookingService
{
    Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<ScorecardDto> GetScorecardAsync(Guid bookingId, CancellationToken cancellationToken = default);
    Task<ScorecardDto> SubmitScorecardAsync(Guid bookingId, SubmitScorecardRequest request, CancellationToken cancellationToken = default);
}
