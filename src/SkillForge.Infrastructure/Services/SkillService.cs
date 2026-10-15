using Microsoft.EntityFrameworkCore;
using SkillForge.Application.Abstractions;
using SkillForge.Application.Skills;
using SkillForge.Core.Entities;
using SkillForge.Core.Exceptions;
using SkillForge.Core.Services;
using SkillForge.Infrastructure.Persistence;

namespace SkillForge.Infrastructure.Services;

public sealed class SkillService : ISkillService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public SkillService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<SkillDto>> SearchAsync(string? query, CancellationToken cancellationToken = default)
    {
        var q = _db.Skills.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query))
        {
            var normalized = SkillNormalizer.Normalize(query);
            q = q.Where(s =>
                s.NormalizedName.Contains(normalized) ||
                s.Name.Contains(query.Trim()) ||
                s.Category.Contains(query.Trim()));
        }

        return await q
            .OrderByDescending(s => s.IsApproved)
            .ThenBy(s => s.Name)
            .Take(50)
            .Select(s => new SkillDto(s.Id, s.Name, s.Category, s.IsApproved))
            .ToListAsync(cancellationToken);
    }

    public async Task<SkillDto> SuggestAsync(SuggestSkillRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureAuthenticated();

        var name = request.Name.Trim();
        var normalized = SkillNormalizer.Normalize(name);
        var category = request.Category.Trim();

        var existing = await _db.Skills
            .FirstOrDefaultAsync(s => s.NormalizedName == normalized, cancellationToken);

        if (existing is not null)
        {
            return ToDto(existing);
        }

        var skill = new Skill
        {
            Name = name,
            NormalizedName = normalized,
            Category = category,
            IsApproved = false,
            CreatedByUserId = _currentUser.UserId
        };

        _db.Skills.Add(skill);
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(skill);
    }

    public async Task<SkillDto> ApproveAsync(Guid skillId, CancellationToken cancellationToken = default)
    {
        var skill = await _db.Skills.FirstOrDefaultAsync(s => s.Id == skillId, cancellationToken)
            ?? throw new KeyNotFoundException("Skill not found.");

        skill.IsApproved = true;
        await _db.SaveChangesAsync(cancellationToken);
        return ToDto(skill);
    }

    private void EnsureAuthenticated()
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Authentication required.");
        }
    }

    private static SkillDto ToDto(Skill skill) =>
        new(skill.Id, skill.Name, skill.Category, skill.IsApproved);
}
