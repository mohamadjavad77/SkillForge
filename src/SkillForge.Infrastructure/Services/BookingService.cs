using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SkillForge.Application.Abstractions;
using SkillForge.Application.Bookings;
using SkillForge.Application.Scorecards;
using SkillForge.Core.Entities;
using SkillForge.Core.Enums;
using SkillForge.Core.Exceptions;
using SkillForge.Core.Services;
using SkillForge.Infrastructure.Persistence;

namespace SkillForge.Infrastructure.Services;

public sealed class BookingService : IBookingService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public BookingService(ApplicationDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<BookingDto> CreateAsync(CreateBookingRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated || !_currentUser.IsInRole(UserRole.Candidate))
        {
            throw new UnauthorizedAccessException("Candidate role is required.");
        }

        var candidate = await _db.CandidateProfiles
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new DomainException("Candidate profile not found for this user.");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var slot = await _db.AvailabilitySlots
            .FirstOrDefaultAsync(s => s.Id == request.SlotId, cancellationToken)
            ?? throw new KeyNotFoundException("Slot not found.");

        BookingRules.EnsureSlotBookable(slot.IsBooked, slot.StartUtc);

        // Atomic claim: only succeed if still unbooked.
        var claimed = await _db.AvailabilitySlots
            .Where(s => s.Id == slot.Id && !s.IsBooked)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(s => s.IsBooked, true),
                cancellationToken);

        if (claimed != 1)
        {
            throw new DomainException("Slot is already booked.");
        }

        slot.IsBooked = true;

        var booking = new Booking
        {
            SlotId = slot.Id,
            CandidateProfileId = candidate.Id,
            MeetingLink = request.MeetingLink?.Trim(),
            Status = BookingStatus.Confirmed,
            Scorecard = new Scorecard
            {
                InterviewerProfileId = slot.InterviewerProfileId,
                IsLocked = false
            }
        };

        _db.Bookings.Add(booking);

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            throw new DomainException("Slot is already booked.", ex);
        }

        return new BookingDto(
            booking.Id,
            booking.SlotId,
            booking.CandidateProfileId,
            booking.MeetingLink,
            booking.Status,
            slot.StartUtc,
            slot.EndUtc);
    }

    public async Task<ScorecardDto> GetScorecardAsync(Guid bookingId, CancellationToken cancellationToken = default)
    {
        if (!_currentUser.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Authentication required.");
        }

        var booking = await LoadBookingWithScorecardAsync(bookingId, cancellationToken);
        EnsureCanViewScorecard(booking);
        return MapScorecard(booking);
    }

    public async Task<ScorecardDto> SubmitScorecardAsync(
        Guid bookingId,
        SubmitScorecardRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!_currentUser.IsAuthenticated || !_currentUser.IsInRole(UserRole.Interviewer))
        {
            throw new UnauthorizedAccessException("Interviewer role is required.");
        }

        var interviewer = await _db.InterviewerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new DomainException("Interviewer profile not found for this user.");

        await using var tx = await _db.Database.BeginTransactionAsync(cancellationToken);

        var booking = await _db.Bookings
            .Include(b => b.Slot)
            .Include(b => b.Scorecard)!.ThenInclude(s => s!.SkillAssessments)
            .Include(b => b.Scorecard)!.ThenInclude(s => s!.Feedback)
            .Include(b => b.ScoreRecord)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken)
            ?? throw new KeyNotFoundException("Booking not found.");

        var scorecard = booking.Scorecard
            ?? throw new DomainException("Scorecard template not found for this booking.");

        if (scorecard.InterviewerProfileId != interviewer.Id)
        {
            throw new UnauthorizedAccessException("Only the interviewer for this booking can submit the scorecard.");
        }

        if (scorecard.IsLocked || booking.ScoreRecord is not null)
        {
            throw new DomainException("Scorecard is already submitted and locked.");
        }

        var skillIds = request.Assessments.Select(a => a.SkillId).Distinct().ToList();
        if (skillIds.Count != request.Assessments.Count)
        {
            throw new DomainException("Duplicate skill assessments are not allowed.");
        }

        Dictionary<Guid, string> skillNames = new();
        if (skillIds.Count > 0)
        {
            skillNames = await _db.Skills
                .Where(s => skillIds.Contains(s.Id))
                .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

            if (skillNames.Count != skillIds.Count)
            {
                throw new DomainException("One or more assessed skills do not exist.");
            }
        }

        if (scorecard.SkillAssessments.Count > 0)
        {
            _db.ScorecardSkillAssessments.RemoveRange(scorecard.SkillAssessments);
        }

        scorecard.SkillAssessments = request.Assessments
            .Select(a => new ScorecardSkillAssessment
            {
                ScorecardId = scorecard.Id,
                SkillId = a.SkillId,
                ProficiencyLevel = a.ProficiencyLevel,
                Notes = a.Notes
            })
            .ToList();

        var strengthsJson = JsonSerializer.Serialize(request.Strengths, JsonOptions);
        var areasJson = JsonSerializer.Serialize(request.AreasForImprovement, JsonOptions);

        if (scorecard.Feedback is null)
        {
            scorecard.Feedback = new Feedback
            {
                ScorecardId = scorecard.Id,
                StrengthsJson = strengthsJson,
                AreasForImprovementJson = areasJson,
                Verdict = request.Verdict,
                OverallComment = request.OverallComment
            };
        }
        else
        {
            scorecard.Feedback.StrengthsJson = strengthsJson;
            scorecard.Feedback.AreasForImprovementJson = areasJson;
            scorecard.Feedback.Verdict = request.Verdict;
            scorecard.Feedback.OverallComment = request.OverallComment;
        }

        var scoring = ScoringEngine.Calculate(new ScoringInput(
            request.Assessments.Select(a => a.ProficiencyLevel).ToList(),
            request.Verdict));

        var skillScoresPayload = request.Assessments.Select(a => new
        {
            skillId = a.SkillId,
            skillName = skillNames.GetValueOrDefault(a.SkillId, string.Empty),
            proficiencyLevel = a.ProficiencyLevel.ToString(),
            score = a.ProficiencyLevel == ProficiencyLevel.NA
                ? (double?)null
                : ((double)a.ProficiencyLevel / 4.0) * 100.0
        });

        booking.ScoreRecord = new ScoreRecord
        {
            CandidateProfileId = booking.CandidateProfileId,
            BookingId = booking.Id,
            SkillScoresJson = JsonSerializer.Serialize(skillScoresPayload, JsonOptions),
            VerdictScore = scoring.VerdictScore,
            OverallScore = scoring.OverallScore
        };

        scorecard.IsLocked = true;
        scorecard.SubmittedAtUtc = DateTime.UtcNow;
        booking.Status = BookingStatus.Completed;

        try
        {
            await _db.SaveChangesAsync(cancellationToken);
            await tx.CommitAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
        {
            await tx.RollbackAsync(cancellationToken);
            throw new DomainException("Scorecard is already submitted and locked.", ex);
        }

        // Reload skill names for response
        await _db.Entry(scorecard).Collection(s => s.SkillAssessments).Query()
            .Include(a => a.Skill)
            .LoadAsync(cancellationToken);

        return MapScorecard(booking, scoring.OverallScore, scoring.VerdictScore);
    }

    private async Task<Booking> LoadBookingWithScorecardAsync(Guid bookingId, CancellationToken cancellationToken)
    {
        var booking = await _db.Bookings
            .AsNoTracking()
            .Include(b => b.Slot)
            .Include(b => b.CandidateProfile)
            .Include(b => b.Scorecard)!.ThenInclude(s => s!.SkillAssessments).ThenInclude(a => a.Skill)
            .Include(b => b.Scorecard)!.ThenInclude(s => s!.Feedback)
            .Include(b => b.Scorecard)!.ThenInclude(s => s!.InterviewerProfile)
            .Include(b => b.ScoreRecord)
            .FirstOrDefaultAsync(b => b.Id == bookingId, cancellationToken);

        return booking ?? throw new KeyNotFoundException("Booking not found.");
    }

    private void EnsureCanViewScorecard(Booking booking)
    {
        var scorecard = booking.Scorecard
            ?? throw new DomainException("Scorecard not found for this booking.");

        var isCandidate = booking.CandidateProfile.UserId == _currentUser.UserId;
        var isInterviewer = scorecard.InterviewerProfile.UserId == _currentUser.UserId;
        var isAdmin = _currentUser.IsInRole(UserRole.Admin);

        if (!isCandidate && !isInterviewer && !isAdmin)
        {
            throw new UnauthorizedAccessException("You are not allowed to view this scorecard.");
        }
    }

    private static ScorecardDto MapScorecard(Booking booking, int? overallScore = null, int? verdictScore = null)
    {
        var scorecard = booking.Scorecard!;
        var feedback = scorecard.Feedback;

        var strengths = DeserializeStringList(feedback?.StrengthsJson);
        var areas = DeserializeStringList(feedback?.AreasForImprovementJson);

        overallScore ??= booking.ScoreRecord?.OverallScore;
        verdictScore ??= booking.ScoreRecord?.VerdictScore ?? (feedback is null ? null : (int)feedback.Verdict);

        return new ScorecardDto(
            scorecard.Id,
            scorecard.BookingId,
            scorecard.InterviewerProfileId,
            scorecard.IsLocked,
            scorecard.SubmittedAtUtc,
            feedback?.Verdict,
            feedback?.OverallComment,
            strengths,
            areas,
            scorecard.SkillAssessments
                .Select(a => new SkillAssessmentDto(
                    a.SkillId,
                    a.Skill?.Name ?? string.Empty,
                    a.ProficiencyLevel,
                    a.Notes))
                .ToList(),
            overallScore,
            verdictScore);
    }

    private static IReadOnlyList<string> DeserializeStringList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return Array.Empty<string>();
        }

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOptions) ?? new List<string>();
        }
        catch (JsonException)
        {
            return Array.Empty<string>();
        }
    }
}
