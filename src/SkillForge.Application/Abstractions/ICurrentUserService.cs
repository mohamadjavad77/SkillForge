using SkillForge.Core.Enums;

namespace SkillForge.Application.Abstractions;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }
    Guid UserId { get; }
    IReadOnlyCollection<UserRole> Roles { get; }
    bool IsInRole(UserRole role);
}
