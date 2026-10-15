using Microsoft.EntityFrameworkCore;
using SkillForge.Application.Abstractions;
using SkillForge.Application.Bookings;
using SkillForge.Core.Entities;
using SkillForge.Core.Enums;
using SkillForge.Core.Exceptions;
using SkillForge.Core.Services;
using SkillForge.Infrastructure.Persistence;

namespace SkillForge.Infrastructure.Services;

public sealed class SlotService : ISlotService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SlotService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<SlotDto> CreateAsync(CreateSlotRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated || !_currentUser.IsInRole(UserRole.Interviewer))
        {
            throw new UnauthorizedAccessException("Interviewer role is required.");
        }

        var startUtc = BookingRules.AsUtc(request.StartUtc, nameof(request.StartUtc));
        var endUtc = BookingRules.AsUtc(request.EndUtc, nameof(request.EndUtc));
        BookingRules.ValidateSlotRange(startUtc, endUtc);

        var interviewer = await _db.InterviewerProfiles
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new DomainException("Interviewer profile not found for this user.");

        var existing = await _db.AvailabilitySlots
            .AsNoTracking()
            .Where(s => s.InterviewerProfileId == interviewer.Id)
            .Select(s => new SlotWindow(s.StartUtc, s.EndUtc))
            .ToListAsync(cancellationToken);

        BookingRules.EnsureNoOverlap(new SlotWindow(startUtc, endUtc), existing);

        var slot = new AvailabilitySlot
        {
            InterviewerProfileId = interviewer.Id,
            StartUtc = startUtc,
            EndUtc = endUtc,
            IsBooked = false
        };

        _db.AvailabilitySlots.Add(slot);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(slot);
    }

    public async Task<IReadOnlyList<SlotDto>> GetAvailableAsync(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _db.AvailabilitySlots
            .AsNoTracking()
            .Where(s => !s.IsBooked && s.StartUtc > now)
            .OrderBy(s => s.StartUtc)
            .Select(s => new SlotDto(s.Id, s.InterviewerProfileId, s.StartUtc, s.EndUtc, s.IsBooked))
            .ToListAsync(cancellationToken);
    }

    private static SlotDto ToDto(AvailabilitySlot slot) =>
        new(slot.Id, slot.InterviewerProfileId, slot.StartUtc, slot.EndUtc, slot.IsBooked);
}
