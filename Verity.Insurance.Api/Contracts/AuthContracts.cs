using System.ComponentModel.DataAnnotations;

namespace Verity.Insurance.Api.Contracts;

public record LoginRequest([param: EmailAddress] string Email, string Password);
public record TokenResponse(string AccessToken, string TokenType = "bearer", string? Role = null);
