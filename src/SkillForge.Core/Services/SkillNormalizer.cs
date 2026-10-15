namespace SkillForge.Core.Services;

public static class SkillNormalizer
{
    public static string Normalize(string name)
    {
        ArgumentNullException.ThrowIfNull(name);
        return name.Trim().ToUpperInvariant();
    }
}
