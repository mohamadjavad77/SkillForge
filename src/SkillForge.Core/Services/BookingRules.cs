using SkillForge.Core.Exceptions;

namespace SkillForge.Core.Services;

public sealed record SlotWindow(DateTime StartUtc, DateTime EndUtc);

public static class BookingRules
{
    /// <summary>
    /// Normalizes API/EF datetimes to UTC. Rejects local Kind (ambiguous for scheduling).
    /// </summary>
    public static DateTime AsUtc(DateTime value, string paramName = "value")
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => throw new DomainException($"{paramName} must be UTC (DateTimeKind.Utc or Unspecified treated as UTC).")
        };
    }

    public static void ValidateSlotRange(DateTime startUtc, DateTime endUtc, DateTime? utcNow = null)
    {
        startUtc = AsUtc(startUtc, nameof(startUtc));
        endUtc = AsUtc(endUtc, nameof(endUtc));

        if (startUtc >= endUtc)
        {
            throw new DomainException("Slot StartUtc must be earlier than EndUtc.");
        }

        var now = AsUtc(utcNow ?? DateTime.UtcNow, nameof(utcNow));

        if (startUtc <= now)
        {
            throw new DomainException("Cannot create or book a slot that starts in the past.");
        }
    }

    public static void EnsureUtc(DateTime value, string paramName)
    {
        _ = AsUtc(value, paramName);
    }

    public static bool Overlaps(DateTime startA, DateTime endA, DateTime startB, DateTime endB)
    {
        return startA < endB && startB < endA;
    }

    public static void EnsureNoOverlap(SlotWindow candidate, IEnumerable<SlotWindow> existingSlots)
    {
        ArgumentNullException.ThrowIfNull(existingSlots);

        foreach (var existing in existingSlots)
        {
            if (Overlaps(candidate.StartUtc, candidate.EndUtc, existing.StartUtc, existing.EndUtc))
            {
                throw new DomainException("Slot overlaps an existing availability slot for this interviewer.");
            }
        }
    }

    public static void EnsureSlotBookable(bool isBooked, DateTime startUtc, DateTime? utcNow = null)
    {
        if (isBooked)
        {
            throw new DomainException("Slot is already booked.");
        }

        startUtc = AsUtc(startUtc, nameof(startUtc));
        var now = AsUtc(utcNow ?? DateTime.UtcNow, nameof(utcNow));

        if (startUtc <= now)
        {
            throw new DomainException("Cannot book a slot that starts in the past.");
        }
    }
}
