using SkillForge.Core.Enums;

namespace SkillForge.Core.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<UserRoleAssignment> Roles { get; set; } = new List<UserRoleAssignment>();
    public CandidateProfile? CandidateProfile { get; set; }
    public InterviewerProfile? InterviewerProfile { get; set; }
}

public class UserRoleAssignment
{
    public Guid UserId { get; set; }
    public UserRole Role { get; set; }

    public User User { get; set; } = null!;
}
