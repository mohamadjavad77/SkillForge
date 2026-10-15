namespace SkillForge.Application.Skills;

public interface ISkillService
{
    Task<IReadOnlyList<SkillDto>> SearchAsync(string? query, CancellationToken cancellationToken = default);
    Task<SkillDto> SuggestAsync(SuggestSkillRequest request, CancellationToken cancellationToken = default);
    Task<SkillDto> ApproveAsync(Guid skillId, CancellationToken cancellationToken = default);
}
