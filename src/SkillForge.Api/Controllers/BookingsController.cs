using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Api.Validation;
using SkillForge.Application.Bookings;
using SkillForge.Application.Scorecards;

namespace SkillForge.Api.Controllers;

[ApiController]
[Route("api/bookings")]
[Authorize]
public sealed class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly IValidator<CreateBookingRequest> _createValidator;
    private readonly IValidator<SubmitScorecardRequest> _submitValidator;

    public BookingsController(
        IBookingService bookingService,
        IValidator<CreateBookingRequest> createValidator,
        IValidator<SubmitScorecardRequest> submitValidator)
    {
        _bookingService = bookingService;
        _createValidator = createValidator;
        _submitValidator = submitValidator;
    }

    /// <summary>
    /// Book an available slot (Candidate only). Prevents double-book.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(BookingDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<BookingDto>> Create(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationHelper.ValidateAsync(_createValidator, request, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await _bookingService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get the scorecard for a booking (participant or admin).
    /// </summary>
    [HttpGet("{id:guid}/scorecard")]
    [ProducesResponseType(typeof(ScorecardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScorecardDto>> GetScorecard(Guid id, CancellationToken cancellationToken)
    {
        var result = await _bookingService.GetScorecardAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Submit and lock the scorecard (Interviewer for that booking only).
    /// </summary>
    [HttpPost("{id:guid}/scorecard")]
    [Authorize(Roles = "Interviewer")]
    [ProducesResponseType(typeof(ScorecardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScorecardDto>> SubmitScorecard(
        Guid id,
        [FromBody] SubmitScorecardRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationHelper.ValidateAsync(_submitValidator, request, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await _bookingService.SubmitScorecardAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
