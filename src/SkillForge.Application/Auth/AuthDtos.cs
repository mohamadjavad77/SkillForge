using SkillForge.Core.Enums;

namespace SkillForge.Application.Auth;

public sealed record RegisterRequest(
    string Email,
    string Password,
    UserRole InitialRole);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record AuthResponse(
    Guid UserId,
    string Email,
    IReadOnlyList<string> Roles,
    string AccessToken,
    DateTime ExpiresAtUtc);
