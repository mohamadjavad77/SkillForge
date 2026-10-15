using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Api.Validation;
using SkillForge.Application.Skills;

namespace SkillForge.Api.Controllers;

[ApiController]
[Route("api/skills")]
public sealed class SkillsController : ControllerBase
{
    private readonly ISkillService _skillService;
    private readonly IValidator<SuggestSkillRequest> _suggestValidator;

    public SkillsController(ISkillService skillService, IValidator<SuggestSkillRequest> suggestValidator)
    {
        _skillService = skillService;
        _suggestValidator = suggestValidator;
    }

    /// <summary>
    /// Search skills by name or category.
    /// </summary>
    [HttpGet("search")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(IReadOnlyList<SkillDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SkillDto>>> Search(
        [FromQuery] string? q,
        CancellationToken cancellationToken)
    {
        var items = await _skillService.SearchAsync(q, cancellationToken);
        return Ok(items);
    }

    /// <summary>
    /// Suggest a new skill (pending admin approval).
    /// </summary>
    [HttpPost("suggest")]
    [Authorize]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SkillDto>> Suggest(
        [FromBody] SuggestSkillRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationHelper.ValidateAsync(_suggestValidator, request, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await _skillService.SuggestAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Approve a suggested skill (Admin only).
    /// </summary>
    [HttpPatch("{id:guid}/approve")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(SkillDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SkillDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await _skillService.ApproveAsync(id, cancellationToken);
        return Ok(result);
    }
}
