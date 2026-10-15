namespace SkillForge.Application.Skills;

public sealed record SkillDto(
    Guid Id,
    string Name,
    string Category,
    bool IsApproved);

public sealed record SuggestSkillRequest(
    string Name,
    string Category);

public sealed record SkillSearchResult(
    IReadOnlyList<SkillDto> Items);
