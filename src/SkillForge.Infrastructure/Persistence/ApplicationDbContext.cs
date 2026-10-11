using Microsoft.EntityFrameworkCore;
using SkillForge.Core.Entities;

namespace SkillForge.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<UserRoleAssignment> UserRoles => Set<UserRoleAssignment>();
    public DbSet<CandidateProfile> CandidateProfiles => Set<CandidateProfile>();
    public DbSet<InterviewerProfile> InterviewerProfiles => Set<InterviewerProfile>();
    public DbSet<Skill> Skills => Set<Skill>();
    public DbSet<CandidateSkill> CandidateSkills => Set<CandidateSkill>();
    public DbSet<InterviewerSkill> InterviewerSkills => Set<InterviewerSkill>();
    public DbSet<AvailabilitySlot> AvailabilitySlots => Set<AvailabilitySlot>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<Scorecard> Scorecards => Set<Scorecard>();
    public DbSet<ScorecardSkillAssessment> ScorecardSkillAssessments => Set<ScorecardSkillAssessment>();
    public DbSet<Feedback> Feedbacks => Set<Feedback>();
    public DbSet<ScoreRecord> ScoreRecords => Set<ScoreRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditTimestamps();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        ApplyAuditTimestamps();
        return base.SaveChanges();
    }

    private void ApplyAuditTimestamps()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                if (entry.Entity.CreatedAtUtc == default)
                {
                    entry.Entity.CreatedAtUtc = utcNow;
                }

                // ScoreRecord is append-only: never set UpdatedAtUtc
                if (entry.Entity is not ScoreRecord)
                {
                    entry.Entity.UpdatedAtUtc = null;
                }
            }
            else if (entry.State == EntityState.Modified && entry.Entity is not ScoreRecord)
            {
                entry.Entity.UpdatedAtUtc = utcNow;
            }
            else if (entry.State == EntityState.Modified && entry.Entity is ScoreRecord)
            {
                // Prevent mutations to immutable score history
                entry.State = EntityState.Unchanged;
            }
        }
    }
}
