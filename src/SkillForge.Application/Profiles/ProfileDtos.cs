using SkillForge.Core.Enums;

namespace SkillForge.Application.Profiles;

public sealed record MeProfileResponse(
    Guid UserId,
    string Email,
    IReadOnlyList<string> Roles,
    CandidateProfileDto? Candidate,
    InterviewerProfileDto? Interviewer);

public sealed record CandidateProfileDto(
    Guid Id,
    string FullName,
    string? Headline,
    string? Bio,
    string? ResumeFileUrl,
    IReadOnlyList<CandidateSkillDto> Skills);

public sealed record CandidateSkillDto(
    Guid SkillId,
    string SkillName,
    ProficiencyLevel SelfProficiencyLevel);

public sealed record InterviewerProfileDto(
    Guid Id,
    string? Bio,
    decimal TrustScore,
    bool IsVerified,
    IReadOnlyList<InterviewerSkillDto> Skills);

public sealed record InterviewerSkillDto(
    Guid SkillId,
    string SkillName);

public sealed record UpdateCandidateProfileRequest(
    string FullName,
    string? Headline,
    string? Bio,
    string? ResumeFileUrl,
    IReadOnlyList<CandidateSkillInput> Skills);

public sealed record CandidateSkillInput(
    Guid SkillId,
    ProficiencyLevel SelfProficiencyLevel);

public sealed record UpdateInterviewerProfileRequest(
    string? Bio,
    IReadOnlyList<Guid> SkillIds);
