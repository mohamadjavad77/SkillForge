using SkillForge.Core.Entities;
using SkillForge.Core.Enums;

namespace SkillForge.Core.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user, IEnumerable<UserRole> roles);
}
