namespace SkillForge.Application.Profiles;

public interface IProfileService
{
    Task<MeProfileResponse> GetMeAsync(CancellationToken cancellationToken = default);
    Task<CandidateProfileDto> UpdateCandidateAsync(UpdateCandidateProfileRequest request, CancellationToken cancellationToken = default);
    Task<InterviewerProfileDto> UpdateInterviewerAsync(UpdateInterviewerProfileRequest request, CancellationToken cancellationToken = default);
}
