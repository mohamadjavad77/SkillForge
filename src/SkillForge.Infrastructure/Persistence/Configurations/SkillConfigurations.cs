using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillForge.Core.Entities;

namespace SkillForge.Infrastructure.Persistence.Configurations;

public class SkillConfiguration : IEntityTypeConfiguration<Skill>
{
    public void Configure(EntityTypeBuilder<Skill> builder)
    {
        builder.ToTable("Skills");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).HasMaxLength(150).IsRequired();
        builder.Property(x => x.NormalizedName).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Category).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsApproved).HasDefaultValue(false);

        builder.HasIndex(x => x.NormalizedName).IsUnique();

        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

public class CandidateSkillConfiguration : IEntityTypeConfiguration<CandidateSkill>
{
    public void Configure(EntityTypeBuilder<CandidateSkill> builder)
    {
        builder.ToTable("CandidateSkills");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.SelfProficiencyLevel).HasConversion<string>().HasMaxLength(32);

        builder.HasIndex(x => new { x.CandidateProfileId, x.SkillId }).IsUnique();

        builder.HasOne(x => x.CandidateProfile)
            .WithMany(x => x.Skills)
            .HasForeignKey(x => x.CandidateProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Skill)
            .WithMany(x => x.CandidateSkills)
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class InterviewerSkillConfiguration : IEntityTypeConfiguration<InterviewerSkill>
{
    public void Configure(EntityTypeBuilder<InterviewerSkill> builder)
    {
        builder.ToTable("InterviewerSkills");
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.InterviewerProfileId, x.SkillId }).IsUnique();

        builder.HasOne(x => x.InterviewerProfile)
            .WithMany(x => x.Skills)
            .HasForeignKey(x => x.InterviewerProfileId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Skill)
            .WithMany(x => x.InterviewerSkills)
            .HasForeignKey(x => x.SkillId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
