using FluentAssertions;
using SkillForge.Core.Enums;
using SkillForge.Core.Services;

namespace SkillForge.Tests;

public class ScoringEngineTests
{
    [Fact]
    public void All_Expert_with_StrongHire_equals_100()
    {
        var result = ScoringEngine.Calculate(new ScoringInput(
            new[] { ProficiencyLevel.Expert, ProficiencyLevel.Expert, ProficiencyLevel.Expert },
            Verdict.StrongHire));

        result.SkillScore.Should().Be(100);
        result.VerdictScore.Should().Be(100);
        result.OverallScore.Should().Be(100);
    }

    [Fact]
    public void Mixed_levels_compute_weighted_overall()
    {
        // Average proficiency = (2 + 4) / 2 = 3 → SkillScore = 75
        // Verdict Hire = 75
        // Overall = round(0.7*75 + 0.3*75) = 75
        var result = ScoringEngine.Calculate(new ScoringInput(
            new[] { ProficiencyLevel.Intermediate, ProficiencyLevel.Expert },
            Verdict.Hire));

        result.SkillScore.Should().Be(75);
        result.VerdictScore.Should().Be(75);
        result.OverallScore.Should().Be(75);
    }

    [Fact]
    public void No_skills_falls_back_to_verdict_score()
    {
        var result = ScoringEngine.Calculate(new ScoringInput(
            Array.Empty<ProficiencyLevel>(),
            Verdict.LeaningHire));

        result.SkillScore.Should().Be(50);
        result.VerdictScore.Should().Be(50);
        result.OverallScore.Should().Be(50);
    }

    [Fact]
    public void Overall_score_is_clamped_between_0_and_100()
    {
        var result = ScoringEngine.Calculate(new ScoringInput(
            new[] { ProficiencyLevel.Novice },
            Verdict.NoHire));

        // SkillScore = 25, Verdict = 15 → Overall = round(0.7*25 + 0.3*15) = round(17.5+4.5)=22
        result.OverallScore.Should().BeInRange(0, 100);
        result.OverallScore.Should().Be(22);
    }

    [Fact]
    public void Only_NA_levels_fall_back_to_verdict()
    {
        var result = ScoringEngine.Calculate(new ScoringInput(
            new[] { ProficiencyLevel.NA, ProficiencyLevel.NA },
            Verdict.StrongHire));

        result.SkillScore.Should().Be(100);
        result.OverallScore.Should().Be(100);
    }
}
