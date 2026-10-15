using Microsoft.EntityFrameworkCore;
using SkillForge.Application.Abstractions;
using SkillForge.Application.Profiles;
using SkillForge.Core.Entities;
using SkillForge.Core.Enums;
using SkillForge.Core.Exceptions;
using SkillForge.Infrastructure.Persistence;

namespace SkillForge.Infrastructure.Services;

public sealed class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public ProfileService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<MeProfileResponse> GetMeAsync(CancellationToken cancellationToken = default)
    {
        var user = await LoadCurrentUserAsync(cancellationToken);

        CandidateProfileDto? candidate = null;
        if (user.CandidateProfile is not null)
        {
            candidate = MapCandidate(user.CandidateProfile);
        }

        InterviewerProfileDto? interviewer = null;
        if (user.InterviewerProfile is not null)
        {
            interviewer = MapInterviewer(user.InterviewerProfile);
        }

        return new MeProfileResponse(
            user.Id,
            user.Email,
            user.Roles.Select(r => r.Role.ToString()).ToList(),
            candidate,
            interviewer);
    }

    public async Task<CandidateProfileDto> UpdateCandidateAsync(
        UpdateCandidateProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureRole(UserRole.Candidate);

        var user = await LoadCurrentUserAsync(cancellationToken);
        var profile = user.CandidateProfile
            ?? throw new DomainException("Candidate profile not found for this user.");

        var skillIds = request.Skills.Select(s => s.SkillId).Distinct().ToList();
        if (skillIds.Count != request.Skills.Count)
        {
            throw new DomainException("Duplicate skills are not allowed.");
        }

        if (skillIds.Count > 0)
        {
            var existingCount = await _db.Skills.CountAsync(s => skillIds.Contains(s.Id), cancellationToken);
            if (existingCount != skillIds.Count)
            {
                throw new DomainException("One or more skills do not exist.");
            }
        }

        profile.FullName = request.FullName.Trim();
        profile.Headline = request.Headline?.Trim();
        profile.Bio = request.Bio?.Trim();
        profile.ResumeFileUrl = request.ResumeFileUrl?.Trim();

        _db.CandidateSkills.RemoveRange(profile.Skills);
        profile.Skills = request.Skills
            .Select(s => new CandidateSkill
            {
                CandidateProfileId = profile.Id,
                SkillId = s.SkillId,
                SelfProficiencyLevel = s.SelfProficiencyLevel
            })
            .ToList();

        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(profile).Collection(p => p.Skills).Query()
            .Include(s => s.Skill)
            .LoadAsync(cancellationToken);

        return MapCandidate(profile);
    }

    public async Task<InterviewerProfileDto> UpdateInterviewerAsync(
        UpdateInterviewerProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        EnsureRole(UserRole.Interviewer);

        var user = await LoadCurrentUserAsync(cancellationToken);
        var profile = user.InterviewerProfile
            ?? throw new DomainException("Interviewer profile not found for this user.");

        var skillIds = request.SkillIds.Distinct().ToList();
        if (skillIds.Count != request.SkillIds.Count)
        {
            throw new DomainException("Duplicate skills are not allowed.");
        }

        if (skillIds.Count > 0)
        {
            var existingCount = await _db.Skills.CountAsync(s => skillIds.Contains(s.Id), cancellationToken);
            if (existingCount != skillIds.Count)
            {
                throw new DomainException("One or more skills do not exist.");
            }
        }

        profile.Bio = request.Bio?.Trim();

        _db.InterviewerSkills.RemoveRange(profile.Skills);
        profile.Skills = skillIds
            .Select(id => new InterviewerSkill
            {
                InterviewerProfileId = profile.Id,
                SkillId = id
            })
            .ToList();

        await _db.SaveChangesAsync(cancellationToken);

        await _db.Entry(profile).Collection(p => p.Skills).Query()
            .Include(s => s.Skill)
            .LoadAsync(cancellationToken);

        return MapInterviewer(profile);
    }

    private async Task<User> LoadCurrentUserAsync(CancellationToken cancellationToken)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Authentication required.");
        }

        var user = await _db.Users
            .Include(u => u.Roles)
            .Include(u => u.CandidateProfile)!.ThenInclude(p => p!.Skills).ThenInclude(s => s.Skill)
            .Include(u => u.InterviewerProfile)!.ThenInclude(p => p!.Skills).ThenInclude(s => s.Skill)
            .FirstOrDefaultAsync(u => u.Id == _currentUser.UserId, cancellationToken);

        return user ?? throw new UnauthorizedAccessException("User not found.");
    }

    private void EnsureRole(UserRole role)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Authentication required.");
        }

        if (!_currentUser.IsInRole(role))
        {
            throw new UnauthorizedAccessException($"Role {role} is required.");
        }
    }

    private static CandidateProfileDto MapCandidate(CandidateProfile profile) =>
        new(
            profile.Id,
            profile.FullName,
            profile.Headline,
            profile.Bio,
            profile.ResumeFileUrl,
            profile.Skills
                .Select(s => new CandidateSkillDto(s.SkillId, s.Skill.Name, s.SelfProficiencyLevel))
                .ToList());

    private static InterviewerProfileDto MapInterviewer(InterviewerProfile profile) =>
        new(
            profile.Id,
            profile.Bio,
            profile.TrustScore,
            profile.IsVerified,
            profile.Skills
                .Select(s => new InterviewerSkillDto(s.SkillId, s.Skill.Name))
                .ToList());
}
