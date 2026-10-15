using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Application.Scorecards;

namespace SkillForge.Api.Controllers;

[ApiController]
[Route("api/scorecards")]
public sealed class ScorecardsController : ControllerBase
{
    private readonly IScorecardService _scorecardService;

    public ScorecardsController(IScorecardService scorecardService)
    {
        _scorecardService = scorecardService;
    }

    /// <summary>
    /// Get score history for a candidate profile.
    /// </summary>
    [HttpGet("candidate/{candidateProfileId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<CandidateScoreHistoryItem>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IReadOnlyList<CandidateScoreHistoryItem>>> GetCandidateHistory(
        Guid candidateProfileId,
        CancellationToken cancellationToken)
    {
        var items = await _scorecardService.GetCandidateHistoryAsync(candidateProfileId, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Public verify endpoint for a completed booking score.
    /// </summary>
    [HttpGet("verify/{bookingId:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(VerifyScoreResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VerifyScoreResponse>> Verify(Guid bookingId, CancellationToken cancellationToken)
    {
        var result = await _scorecardService.VerifyAsync(bookingId, cancellationToken);
        return Ok(result);
    }
}
