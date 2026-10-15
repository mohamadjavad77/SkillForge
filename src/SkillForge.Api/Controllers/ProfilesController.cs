using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillForge.Api.Validation;
using SkillForge.Application.Profiles;

namespace SkillForge.Api.Controllers;

[ApiController]
[Route("api/profiles")]
[Authorize]
public sealed class ProfilesController : ControllerBase
{
    private readonly IProfileService _profileService;
    private readonly IValidator<UpdateCandidateProfileRequest> _candidateValidator;
    private readonly IValidator<UpdateInterviewerProfileRequest> _interviewerValidator;

    public ProfilesController(
        IProfileService profileService,
        IValidator<UpdateCandidateProfileRequest> candidateValidator,
        IValidator<UpdateInterviewerProfileRequest> interviewerValidator)
    {
        _profileService = profileService;
        _candidateValidator = candidateValidator;
        _interviewerValidator = interviewerValidator;
    }

    /// <summary>
    /// Get the current user's profile(s).
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(MeProfileResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<MeProfileResponse>> GetMe(CancellationToken cancellationToken)
    {
        var result = await _profileService.GetMeAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update the current candidate profile and self-declared skills.
    /// </summary>
    [HttpPut("candidate")]
    [Authorize(Roles = "Candidate")]
    [ProducesResponseType(typeof(CandidateProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CandidateProfileDto>> UpdateCandidate(
        [FromBody] UpdateCandidateProfileRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationHelper.ValidateAsync(_candidateValidator, request, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await _profileService.UpdateCandidateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Update the current interviewer profile and expertise skills.
    /// </summary>
    [HttpPut("interviewer")]
    [Authorize(Roles = "Interviewer")]
    [ProducesResponseType(typeof(InterviewerProfileDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<InterviewerProfileDto>> UpdateInterviewer(
        [FromBody] UpdateInterviewerProfileRequest request,
        CancellationToken cancellationToken)
    {
        var validation = await ValidationHelper.ValidateAsync(_interviewerValidator, request, cancellationToken);
        if (validation is not null)
        {
            return validation;
        }

        var result = await _profileService.UpdateInterviewerAsync(request, cancellationToken);
        return Ok(result);
    }
}
