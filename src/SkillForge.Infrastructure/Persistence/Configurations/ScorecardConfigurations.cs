using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillForge.Core.Entities;

namespace SkillForge.Infrastructure.Persistence.Configurations;

public class ScorecardConfiguration : IEntityTypeConfiguration<Scorecard>
{
    public void Configure(EntityTypeBuilder<Scorecard> builder)
    {
        builder.ToTable("Scorecards");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.IsLocked).HasDefaultValue(false);
        builder.HasIndex(x => x.BookingId).IsUnique();

        builder.HasOne(x => x.InterviewerProfile)
            .WithMany(x => x.Scorecards)
            .HasForeignKey(x => x.InterviewerProfileId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Feedback)
            .WithOne(x => x.Scorecard)
            .HasForeignKey<Feedback>(x => x.ScorecardId);
    }
}

public class ScorecardSkillAssessmentConfiguration : IEntityTypeConfiguration<ScorecardSkillAssessment>
{
    public void Configure(EntityTypeBuilder<ScorecardSkillAssessment> builder)
    {
        builder.ToTable("ScorecardSkillAssessments");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ProficiencyLevel).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Notes).HasMaxLength(2000);

        builder.HasIndex(x => new { x.ScorecardId, x.SkillId }).IsUnique();

        builder.HasOne(x => x.Scorecard)
            .WithMany(x => x.SkillAssessments)
            .HasForeignKey(x => x.ScorecardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Skill)
            .WithMany(x => x.ScorecardAssessments)
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
{
    public void Configure(EntityTypeBuilder<Feedback> builder)
    {
        builder.ToTable("Feedbacks");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StrengthsJson).IsRequired();
        builder.Property(x => x.AreasForImprovementJson).IsRequired();
        builder.Property(x => x.Verdict).HasConversion<int>();
        builder.Property(x => x.OverallComment).HasMaxLength(4000);

        builder.HasIndex(x => x.ScorecardId).IsUnique();
    }
}

public class ScoreRecordConfiguration : IEntityTypeConfiguration<ScoreRecord>
{
    public void Configure(EntityTypeBuilder<ScoreRecord> builder)
    {
        builder.ToTable("ScoreRecords");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SkillScoresJson).IsRequired();
        builder.Property(x => x.VerdictScore).IsRequired();
        builder.Property(x => x.OverallScore).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_ScoreRecords_OverallScore",
            "\"OverallScore\" >= 0 AND \"OverallScore\" <= 100"));

        builder.HasIndex(x => x.BookingId).IsUnique();
        builder.HasIndex(x => x.CandidateProfileId);
        builder.HasIndex(x => x.CreatedAtUtc);

        // Immutable: disallow updates at schema level is not possible; enforced in SaveChanges.
        builder.Property(x => x.UpdatedAtUtc).IsRequired(false);

        builder.HasOne(x => x.CandidateProfile)
            .WithMany(x => x.ScoreRecords)
            .HasForeignKey(x => x.CandidateProfileId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
