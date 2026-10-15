using FluentAssertions;
using SkillForge.Core.Exceptions;
using SkillForge.Core.Services;

namespace SkillForge.Tests;

public class BookingValidationTests
{
    [Fact]
    public void Rejects_past_slot()
    {
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var start = now.AddMinutes(-1);
        var end = now.AddHours(1);

        var act = () => BookingRules.ValidateSlotRange(start, end, now);

        act.Should().Throw<DomainException>()
            .WithMessage("*past*");
    }

    [Fact]
    public void Rejects_end_not_after_start()
    {
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var start = now.AddHours(2);
        var end = start;

        var act = () => BookingRules.ValidateSlotRange(start, end, now);

        act.Should().Throw<DomainException>()
            .WithMessage("*earlier*");
    }

    [Fact]
    public void Accepts_valid_future_utc_slot()
    {
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        var start = now.AddHours(1);
        var end = now.AddHours(2);

        var act = () => BookingRules.ValidateSlotRange(start, end, now);

        act.Should().NotThrow();
    }

    [Fact]
    public void Detects_overlapping_slots()
    {
        var existing = new[]
        {
            new SlotWindow(
                new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc))
        };

        var candidate = new SlotWindow(
            new DateTime(2026, 10, 20, 10, 30, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 20, 11, 30, 0, DateTimeKind.Utc));

        var act = () => BookingRules.EnsureNoOverlap(candidate, existing);

        act.Should().Throw<DomainException>()
            .WithMessage("*overlap*");
    }

    [Fact]
    public void Allows_adjacent_non_overlapping_slots()
    {
        var existing = new[]
        {
            new SlotWindow(
                new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc),
                new DateTime(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc))
        };

        var candidate = new SlotWindow(
            new DateTime(2026, 10, 20, 11, 0, 0, DateTimeKind.Utc),
            new DateTime(2026, 10, 20, 12, 0, 0, DateTimeKind.Utc));

        var act = () => BookingRules.EnsureNoOverlap(candidate, existing);

        act.Should().NotThrow();
    }

    [Fact]
    public void EnsureSlotBookable_rejects_already_booked()
    {
        var start = new DateTime(2026, 10, 20, 10, 0, 0, DateTimeKind.Utc);
        var now = new DateTime(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);

        var act = () => BookingRules.EnsureSlotBookable(isBooked: true, start, now);

        act.Should().Throw<DomainException>()
            .WithMessage("*already booked*");
    }

    [Fact]
    public void Rejects_local_datetime_kind()
    {
        var now = DateTime.UtcNow;
        var start = DateTime.SpecifyKind(now.AddHours(1), DateTimeKind.Local);
        var end = DateTime.SpecifyKind(now.AddHours(2), DateTimeKind.Local);

        var act = () => BookingRules.ValidateSlotRange(start, end, now);

        act.Should().Throw<DomainException>()
            .WithMessage("*UTC*");
    }
}
