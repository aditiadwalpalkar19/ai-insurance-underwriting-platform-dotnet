using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Verity.Insurance.Api.Contracts;
using Verity.Insurance.Api.Infrastructure;
using Verity.Insurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace Verity.Insurance.Api.Controllers;

[ApiController]
public sealed class AuthController(PostgresDatabase db, IConfiguration config) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<TokenResponse>> Login(LoginRequest credentials)
    {
        await using var connection = await db.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT users_uid, password_hash, role FROM users WHERE email = @email", connection);
        command.Parameters.AddWithValue("email", credentials.Email);
        await using var reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync() || !BCrypt.Net.BCrypt.Verify(credentials.Password, reader.GetString(1)))
            return Unauthorized(new { detail = "Invalid credentials" });
        var uid = reader.GetString(0); var role = reader.GetString(2);
        var jwt = config.GetSection("Jwt");
        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["SecretKey"]!));
        var token = new JwtSecurityToken(jwt["Issuer"], jwt["Audience"],
            [new Claim("useruid", uid), new Claim(ClaimTypes.Role, role)],
            expires: DateTime.UtcNow.AddMinutes(int.Parse(jwt["ExpiryMinutes"] ?? "60")),
            signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));
        return new TokenResponse(new JwtSecurityTokenHandler().WriteToken(token), Role: role);
    }
}
