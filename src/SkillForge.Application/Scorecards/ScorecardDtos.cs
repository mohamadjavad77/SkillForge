using SkillForge.Core.Enums;

namespace SkillForge.Application.Scorecards;

public sealed record SkillAssessmentInput(
    Guid SkillId,
    ProficiencyLevel ProficiencyLevel,
    string? Notes);

public sealed record SubmitScorecardRequest(
    IReadOnlyList<SkillAssessmentInput> Assessments,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> AreasForImprovement,
    Verdict Verdict,
    string? OverallComment);

public sealed record SkillAssessmentDto(
    Guid SkillId,
    string SkillName,
    ProficiencyLevel ProficiencyLevel,
    string? Notes);

public sealed record ScorecardDto(
    Guid Id,
    Guid BookingId,
    Guid InterviewerProfileId,
    bool IsLocked,
    DateTime? SubmittedAtUtc,
    Verdict? Verdict,
    string? OverallComment,
    IReadOnlyList<string> Strengths,
    IReadOnlyList<string> AreasForImprovement,
    IReadOnlyList<SkillAssessmentDto> Assessments,
    int? OverallScore,
    int? VerdictScore);

public sealed record CandidateScoreHistoryItem(
    Guid BookingId,
    Guid ScoreRecordId,
    int OverallScore,
    int VerdictScore,
    DateTime CreatedAtUtc);

public sealed record VerifyScoreResponse(
    Guid BookingId,
    int OverallScore,
    int VerdictScore,
    string Verdict,
    IReadOnlyList<SkillAssessmentDto> AssessedSkills,
    DateTime SubmittedAtUtc);
