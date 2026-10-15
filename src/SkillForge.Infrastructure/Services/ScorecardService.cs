using Microsoft.EntityFrameworkCore;
using SkillForge.Application.Scorecards;
using SkillForge.Infrastructure.Persistence;

namespace SkillForge.Infrastructure.Services;

public sealed class ScorecardService : IScorecardService
{
    private readonly ApplicationDbContext _db;

    public ScorecardService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<CandidateScoreHistoryItem>> GetCandidateHistoryAsync(
        Guid candidateProfileId,
        CancellationToken cancellationToken = default)
    {
        var exists = await _db.CandidateProfiles.AnyAsync(p => p.Id == candidateProfileId, cancellationToken);
        if (!exists)
        {
            throw new KeyNotFoundException("Candidate profile not found.");
        }

        return await _db.ScoreRecords
            .AsNoTracking()
            .Where(r => r.CandidateProfileId == candidateProfileId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .Select(r => new CandidateScoreHistoryItem(
                r.BookingId,
                r.Id,
                r.OverallScore,
                r.VerdictScore,
                r.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<VerifyScoreResponse> VerifyAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.ScoreRecord)
            .Include(b => b.Scorecard)!.ThenInclude(s => s!.Feedback)
            .Include(b => b.Scorecard)!.ThenInclude(s => s!.SkillAssessments).ThenInclude(a => a.Skill)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Booking not found.");

        var record = booking.ScoreRecord
            ?? throw new KeyNotFoundException("No verified score exists for this booking.");

        var scorecard = booking.Scorecard
            ?? throw new KeyNotFoundException("Scorecard not found for this booking.");

        if (!scorecard.IsLocked || scorecard.Feedback is null || scorecard.SubmittedAtUtc is null)
        {
            throw new KeyNotFoundException("No verified score exists for this booking.");
        }

        var verdict = scorecard.Feedback.Verdict;

        return new VerifyScoreResponse(
            booking.Id,
            record.OverallScore,
            record.VerdictScore,
            verdict.ToString(),
            scorecard.SkillAssessments
                .Select(a => new SkillAssessmentDto(
                    a.SkillId,
                    a.Skill.Name,
                    a.ProficiencyLevel,
                    a.Notes))
                .ToList(),
            scorecard.SubmittedAtUtc.Value);
    }
}
