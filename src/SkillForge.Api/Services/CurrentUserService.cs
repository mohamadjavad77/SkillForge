using System.Security.Claims;
using SkillForge.Application.Abstractions;
using SkillForge.Core.Enums;

namespace SkillForge.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly Lazy<Guid> _userId;
    private readonly Lazy<IReadOnlyCollection<UserRole>> _roles;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _userId = new Lazy<Guid>(ResolveUserId);
        _roles = new Lazy<IReadOnlyCollection<UserRole>>(ResolveRoles);
    }

    public bool IsAuthenticated =>
        _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid UserId
    {
        get
        {
            if (!IsAuthenticated)
            {
                throw new UnauthorizedAccessException("Authentication required.");
            }

            return _userId.Value;
        }
    }

    public IReadOnlyCollection<UserRole> Roles => IsAuthenticated ? _roles.Value : Array.Empty<UserRole>();

    public bool IsInRole(UserRole role) => Roles.Contains(role);

    private Guid ResolveUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User
            ?? throw new UnauthorizedAccessException("Authentication required.");

        var value = principal.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(ClaimTypes.Name)
            ?? principal.FindFirstValue("sub");

        if (value is null || !Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("User id claim is missing or invalid.");
        }

        return userId;
    }

    private IReadOnlyCollection<UserRole> ResolveRoles()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal is null)
        {
            return Array.Empty<UserRole>();
        }

        return principal
            .FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Select(v => Enum.TryParse<UserRole>(v, ignoreCase: true, out var role) ? role : (UserRole?)null)
            .Where(r => r.HasValue)
            .Select(r => r!.Value)
            .Distinct()
            .ToList();
    }
}
