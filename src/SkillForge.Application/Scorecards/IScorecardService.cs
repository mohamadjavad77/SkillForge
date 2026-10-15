namespace SkillForge.Application.Scorecards;

public interface IScorecardService
{
    Task<IReadOnlyList<CandidateScoreHistoryItem>> GetCandidateHistoryAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken = default);

    Task<VerifyScoreResponse> VerifyAsync(Guid bookingId, CancellationToken cancellationToken = default);
}
