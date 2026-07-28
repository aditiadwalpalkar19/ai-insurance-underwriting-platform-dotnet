using System.ComponentModel.DataAnnotations;

namespace Verity.Insurance.Api.Contracts;

public record UserCreate([param: EmailAddress] string Email, string PasswordHash, string Role);
public record UserResponse(string Email, string Role, DateTime CreatedAt);
