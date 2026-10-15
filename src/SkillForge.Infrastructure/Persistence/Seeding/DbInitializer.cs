using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SkillForge.Core.Entities;
using SkillForge.Core.Enums;
using SkillForge.Core.Interfaces;
using SkillForge.Core.Services;

namespace SkillForge.Infrastructure.Persistence.Seeding;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;
        var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger("DbInitializer");
        var db = sp.GetRequiredService<ApplicationDbContext>();
        var passwordHasher = sp.GetRequiredService<IPasswordHasher>();

        await db.Database.MigrateAsync(cancellationToken);

        await SeedAdminAsync(db, passwordHasher, logger, cancellationToken);
        await SeedSkillsAsync(db, logger, cancellationToken);
    }

    private static async Task SeedAdminAsync(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        const string adminEmail = "admin@skillforge.local";

        if (await db.Users.AnyAsync(u => u.Email == adminEmail, cancellationToken))
        {
            return;
        }

        var admin = new User
        {
            Email = adminEmail,
            PasswordHash = passwordHasher.Hash("Admin@SkillForge2026!"),
            IsActive = true,
            Roles =
            {
                new UserRoleAssignment { Role = UserRole.Admin }
            }
        };

        db.Users.Add(admin);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded default admin user {Email}", adminEmail);
    }

    private static async Task SeedSkillsAsync(
        ApplicationDbContext db,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        if (await db.Skills.AnyAsync(cancellationToken))
        {
            return;
        }

        var catalog = new (string Name, string Category)[]
        {
            // Software
            ("C#", "Software"),
            (".NET", "Software"),
            ("ASP.NET Core", "Software"),
            ("Entity Framework Core", "Software"),
            ("PostgreSQL", "Software"),
            ("SQL", "Software"),
            ("Docker", "Software"),
            ("Kubernetes", "Software"),
            ("React", "Software"),
            ("TypeScript", "Software"),
            ("Blazor", "Software"),
            ("Clean Architecture", "Software"),
            ("System Design", "Software"),
            ("REST APIs", "Software"),
            ("Microservices", "Software"),
            ("CI/CD", "Software"),
            ("Git", "Software"),
            ("Redis", "Software"),
            // Marketing
            ("SEO", "Marketing"),
            ("Content Strategy", "Marketing"),
            ("Google Ads", "Marketing"),
            ("Copywriting", "Marketing"),
            ("Email Marketing", "Marketing"),
            ("Social Media Marketing", "Marketing"),
            ("Marketing Analytics", "Marketing"),
            // Design
            ("Figma", "Design"),
            ("UI/UX", "Design"),
            ("Design Systems", "Design"),
            ("User Research", "Design"),
            ("Prototyping", "Design"),
            ("Accessibility", "Design"),
            // Product & HR
            ("Agile/Scrum", "Product & HR"),
            ("Product Discovery", "Product & HR"),
            ("Roadmapping", "Product & HR"),
            ("Talent Acquisition", "Product & HR"),
            ("KPI Setting", "Product & HR"),
            ("Stakeholder Management", "Product & HR"),
            // Finance & Sales
            ("Financial Modeling", "Finance & Sales"),
            ("B2B Sales", "Finance & Sales"),
            ("Negotiation", "Finance & Sales"),
            ("Account Management", "Finance & Sales"),
            ("Pricing Strategy", "Finance & Sales")
        };

        var skills = catalog.Select(item => new Skill
        {
            Name = item.Name,
            NormalizedName = SkillNormalizer.Normalize(item.Name),
            Category = item.Category,
            IsApproved = true
        }).ToList();

        db.Skills.AddRange(skills);
        await db.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Seeded {Count} skills", skills.Count);
    }
}
