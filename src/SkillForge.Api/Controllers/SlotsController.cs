using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Api.Validation;
using SkillForge.Application.Bookings;

namespace SkillForge.Api.Controllers;

[ApiController]
[Route("api/slots")]
public sealed class SlotsController : ControllerBase
{
    private readonly ISlotService _slotService;
    private readonly IValidator<CreateSlotRequest> _createValidator;

    public SlotsController(ISlotService slotService, IValidator<CreateSlotRequest> createValidator)
    {
        _slotService = slotService;
        _createValidator = createValidator;
    }

    /// <summary>
    /// Create an availability slot (Interviewer only). Times must be UTC.
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Interviewer")]
    [ProducesResponseType(typeof(SlotDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SlotDto>> Create(
        [FromBody] CreateSlotRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationHelper.ValidateAsync(_createValidator, request, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await _slotService.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// List bookable future slots that are not yet booked.
    /// </summary>
    [HttpGet("available")]
    [Authorize]
    [ProducesResponseType(typeof(IReadOnlyList<SlotDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SlotDto>>> GetAvailable(CancellationToken cancellationToken)
    {
        var items = await _slotService.GetAvailableAsync(cancellationToken);
        return Ok(items);
    }
}
