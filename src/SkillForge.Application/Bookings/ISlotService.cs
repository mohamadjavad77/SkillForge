namespace SkillForge.Application.Bookings;

public interface ISlotService
{
    Task<SlotDto> CreateAsync(CreateSlotRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<SlotDto>> GetAvailableAsync(CancellationToken cancellationToken = default);
}
