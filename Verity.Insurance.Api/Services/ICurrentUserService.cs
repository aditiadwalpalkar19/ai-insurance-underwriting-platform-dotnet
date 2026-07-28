using System.Security.Claims;

namespace Verity.Insurance.Api.Services;

public interface ICurrentUserService
{
    string UserUid { get; }

    string? Role { get; }

    ClaimsPrincipal User { get; }
}