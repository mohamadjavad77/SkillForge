using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillForge.Core.Entities;

namespace SkillForge.Infrastructure.Persistence.Configurations;

public class AvailabilitySlotConfiguration : IEntityTypeConfiguration<AvailabilitySlot>
{
    public void Configure(EntityTypeBuilder<AvailabilitySlot> builder)
    {
        builder.ToTable("AvailabilitySlots");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StartUtc).IsRequired();
        builder.Property(x => x.EndUtc).IsRequired();
        builder.Property(x => x.IsBooked).HasDefaultValue(false);

        builder.HasIndex(x => new { x.InterviewerProfileId, x.StartUtc, x.EndUtc });

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_AvailabilitySlots_UtcRange",
            "\"EndUtc\" > \"StartUtc\""));

        builder.HasOne(x => x.InterviewerProfile)
            .WithMany(x => x.AvailabilitySlots)
            .HasForeignKey(x => x.InterviewerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Booking)
            .WithOne(x => x.Slot)
            .HasForeignKey<Booking>(x => x.SlotId);
    }
}

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.MeetingLink).HasMaxLength(1000);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(x => x.SlotId).IsUnique();

        builder.HasOne(x => x.CandidateProfile)
            .WithMany(x => x.Bookings)
            .HasForeignKey(x => x.CandidateProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Scorecard)
            .WithOne(x => x.Booking)
            .HasForeignKey<Scorecard>(x => x.BookingId);

        builder.HasOne(x => x.ScoreRecord)
            .WithOne(x => x.Booking)
            .HasForeignKey<ScoreRecord>(x => x.BookingId);
    }
}
