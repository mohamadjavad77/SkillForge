using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SkillForge.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Get() => Ok(new { status = "healthy", service = "SkillForge.Api" });

    [HttpGet("secure")]
    [Authorize]
    public IActionResult GetSecure() => Ok(new
    {
        status = "authorized",
        user = User.Identity?.Name ?? User.FindFirst("email")?.Value,
        roles = User.FindAll(System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToArray()
    });
}
