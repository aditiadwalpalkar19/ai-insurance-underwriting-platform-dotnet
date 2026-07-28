using Verity.Insurance.Api.Contracts;
using Verity.Insurance.Api.Infrastructure;
using Verity.Insurance.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Verity.Insurance.Api.Controllers;

[ApiController, Authorize(Roles = "UNDERWRITER")]
public sealed class AnalysisController(PostgresDatabase db) : ControllerBase
{
    [HttpGet("analysis/{subNumber}")]
    public async Task<ActionResult<List<AIAnalysisResponse>>> Get(string subNumber)
    {
        await using var connection = await db.OpenAsync();
        await using var command = new NpgsqlCommand("SELECT risk_summary,recommendation,risk_level,risk_score,risk_factors,missing_information FROM ai_analysis WHERE submission_uid=(SELECT submissions_uid FROM submissions WHERE submission_number=@number)", connection);
        command.Parameters.AddWithValue("number", subNumber);
        await using var row = await command.ExecuteReaderAsync();
        if (!await row.ReadAsync()) return NotFound(new { detail = "No analysis found for this submission" });
        return new List<AIAnalysisResponse> { new(row.GetString(0), row.GetString(1), row.GetString(2), row.GetInt32(3), row.GetString(4), row.IsDBNull(5) ? null : row.GetString(5)) };
    }
}
