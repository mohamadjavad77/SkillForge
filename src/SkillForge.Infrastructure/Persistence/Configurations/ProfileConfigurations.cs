using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillForge.Core.Entities;

namespace SkillForge.Infrastructure.Persistence.Configurations;

public class CandidateProfileConfiguration : IEntityTypeConfiguration<CandidateProfile>
{
    public void Configure(EntityTypeBuilder<CandidateProfile> builder)
    {
        builder.ToTable("CandidateProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.FullName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Headline).HasMaxLength(300);
        builder.Property(x => x.Bio).HasMaxLength(4000);
        builder.Property(x => x.ResumeFileUrl).HasMaxLength(1000);

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}

public class InterviewerProfileConfiguration : IEntityTypeConfiguration<InterviewerProfile>
{
    public void Configure(EntityTypeBuilder<InterviewerProfile> builder)
    {
        builder.ToTable("InterviewerProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Bio).HasMaxLength(4000);
        builder.Property(x => x.TrustScore).HasPrecision(5, 2).HasDefaultValue(100m);
        builder.Property(x => x.IsVerified).HasDefaultValue(false);

        builder.HasIndex(x => x.UserId).IsUnique();
    }
}
