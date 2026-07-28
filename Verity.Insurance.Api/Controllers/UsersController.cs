using Verity.Insurance.Api.Contracts;
using Verity.Insurance.Api.Common;
using Verity.Insurance.Api.Infrastructure;
using Verity.Insurance.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Verity.Insurance.Api.Controllers;

[ApiController]
public sealed class UsersController(PostgresDatabase db) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<UserResponse>> Register(UserCreate user)
    {
        await using var connection = await db.OpenAsync();
        await using var command = new NpgsqlCommand("INSERT INTO users (users_uid,email,password_hash,role) VALUES (@uid,@email,@password,@role) RETURNING email,role,created_at", connection);
        command.Parameters.AddWithValue("uid", IdentifierGenerator.New()); command.Parameters.AddWithValue("email", user.Email);
        command.Parameters.AddWithValue("password", BCrypt.Net.BCrypt.HashPassword(user.PasswordHash)); command.Parameters.AddWithValue("role", user.Role);
        await using var row = await command.ExecuteReaderAsync();
        if (!await row.ReadAsync()) return Problem();
        return StatusCode(201, new UserResponse(row.GetString(0), row.GetString(1), row.GetDateTime(2)));
    }

    [HttpGet("fetch/{role}")]
    public async Task<ActionResult<List<UserResponse>>> Fetch(string role)
    {
        var users = new List<UserResponse>();
        await using var connection = await db.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT email,role,created_at FROM users WHERE role = @role", connection);
        command.Parameters.AddWithValue("role", role.ToUpperInvariant());
        await using var rows = await command.ExecuteReaderAsync();
        while (await rows.ReadAsync()) users.Add(new(rows.GetString(0), rows.GetString(1), rows.GetDateTime(2)));
        return users.Count == 0 ? NotFound(new { detail = "No records found" }) : users;
    }
}
