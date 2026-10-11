using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SkillForge.Application.Auth;
using SkillForge.Core.Entities;
using SkillForge.Core.Enums;
using SkillForge.Core.Exceptions;
using SkillForge.Core.Interfaces;
using SkillForge.Infrastructure.Persistence;

namespace SkillForge.Infrastructure.Identity;

public sealed class AuthService : IAuthService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly JwtOptions _jwtOptions;

    public AuthService(
        ApplicationDbContext db,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtOptions> jwtOptions)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _jwtOptions = jwtOptions.Value;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            throw new DomainException("Email and password are required.");
        }

        if (request.InitialRole is not (UserRole.Candidate or UserRole.Interviewer))
        {
            throw new DomainException("InitialRole must be Candidate or Interviewer.");
        }

        if (request.Password.Length < 8)
        {
            throw new DomainException("Password must be at least 8 characters.");
        }

        var email = request.Email.Trim().ToLowerInvariant();

        if (await _db.Users.AnyAsync(u => u.Email == email, cancellationToken))
        {
            throw new DomainException("A user with this email already exists.");
        }

        var user = new User
        {
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            IsActive = true,
            Roles =
            {
                new UserRoleAssignment { Role = request.InitialRole }
            }
        };

        if (request.InitialRole == UserRole.Candidate)
        {
            user.CandidateProfile = new CandidateProfile
            {
                FullName = email.Split('@')[0]
            };
        }
        else
        {
            user.InterviewerProfile = new InterviewerProfile
            {
                TrustScore = 100m,
                IsVerified = false
            };
        }

        _db.Users.Add(user);
        await _db.SaveChangesAsync(cancellationToken);

        return CreateAuthResponse(user, user.Roles.Select(r => r.Role));
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _db.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new DomainException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new DomainException("User account is inactive.");
        }

        return CreateAuthResponse(user, user.Roles.Select(r => r.Role));
    }

    private AuthResponse CreateAuthResponse(User user, IEnumerable<UserRole> roles)
    {
        var roleList = roles.Distinct().ToList();
        var token = _jwtTokenGenerator.GenerateToken(user, roleList);

        return new AuthResponse(
            user.Id,
            user.Email,
            roleList.Select(r => r.ToString()).ToList(),
            token,
            DateTime.UtcNow.AddMinutes(_jwtOptions.ExpirationMinutes));
    }
}
