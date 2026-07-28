using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Verity.Insurance.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("User context not available.");

    public string UserUid =>
        User.FindFirstValue("useruid")
        ?? throw new UnauthorizedAccessException("User UID not found.");

    public string? Role =>
        User.FindFirstValue(ClaimTypes.Role);
}