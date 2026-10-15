using SkillForge.Core.Enums;

namespace SkillForge.Core.Services;

public sealed record ScoringInput(
    IReadOnlyList<ProficiencyLevel> ProficiencyLevels,
    Verdict Verdict);

public sealed record ScoringResult(
    double SkillScore,
    int VerdictScore,
    int OverallScore);

public static class ScoringEngine
{
    public static ScoringResult Calculate(ScoringInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var verdictScore = (int)input.Verdict;
        var skillScore = CalculateSkillScore(input.ProficiencyLevels, verdictScore);
        var overall = (int)Math.Round(0.7 * skillScore + 0.3 * verdictScore, MidpointRounding.AwayFromZero);
        overall = Math.Clamp(overall, 0, 100);

        return new ScoringResult(skillScore, verdictScore, overall);
    }

    private static double CalculateSkillScore(IReadOnlyList<ProficiencyLevel> levels, int verdictScoreFallback)
    {
        if (levels is null || levels.Count == 0)
        {
            return verdictScoreFallback;
        }

        // Exclude NA from the average when other levels exist; if all NA, treat as no skills.
        var rated = levels.Where(l => l != ProficiencyLevel.NA).ToList();
        if (rated.Count == 0)
        {
            return verdictScoreFallback;
        }

        var average = rated.Average(l => (double)l);
        var skillScore = (average / 4.0) * 100.0;
        return Math.Clamp(skillScore, 0, 100);
    }
}
