using System.Security.Claims;
using System.Text.Json;
using Verity.Insurance.Api.Common;
using Verity.Insurance.Api.Contracts;
using Verity.Insurance.Api.Infrastructure;
using Verity.Insurance.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;

namespace Verity.Insurance.Api.Controllers;

[ApiController, Authorize]
public sealed class SubmissionsController(PostgresDatabase db, IRiskAnalysisService riskAnalysis) : ControllerBase
{
    [HttpGet("search")]
    public async Task<ActionResult<List<SubmissionResponse>>> Search()
    {
        var results = new List<SubmissionResponse>();
        var isBroker = User.IsInRole("BROKER");
        await using var connection = await db.OpenAsync();
        var sql = "SELECT submission_number,submission_type,submission_status,insured_firstname,insured_lastname,insured_age,insured_code,insured_email_address,insured_address_1,insured_city,submission_summary FROM submissions s INNER JOIN submission_details sd ON sd.submission_reference_uid=s.submissions_uid" + (isBroker ? " WHERE s.created_by=@user" : "");
        await using var command = new NpgsqlCommand(sql, connection);
        if (isBroker) command.Parameters.AddWithValue("user", UserUid());
        await using var rows = await command.ExecuteReaderAsync();
        while (await rows.ReadAsync()) results.Add(new(rows.GetString(0), rows.GetString(1), rows.GetString(2), rows.GetString(3), rows.GetString(4), rows.GetInt32(5), rows.GetInt32(6), rows.GetString(7), NullableString(rows, 8), NullableString(rows, 9), NullableString(rows, 10)));
        return results;
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(SubmissionCreate submission)
    {
        await using var connection = await db.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            var number = await NextSubmissionNumber(connection, transaction);
            var submissionUid = IdentifierGenerator.New();
            await Execute(connection, transaction, "INSERT INTO submissions (submissions_uid,submission_type,submission_summary,submission_status,submission_number,created_by) VALUES (@uid,@type,@summary,'SUBMITTED',@number,@user)",
                ("uid", submissionUid), ("type", submission.SubmissionType), ("summary", (object?)submission.SubmissionSummary ?? DBNull.Value), ("number", number), ("user", UserUid()));
            var d = submission.SubmissionDetails;
            await Execute(connection, transaction, "INSERT INTO submission_details (submission_details_uid,submission_reference_uid,insured_firstname,insured_lastname,insured_age,insured_code,insured_email_address,insured_address_1,insured_city,created_by) VALUES (@uid,@reference,@first,@last,@age,@code,@email,@address,@city,@user)",
                ("uid", IdentifierGenerator.New()), ("reference", submissionUid), ("first", d.InsuredFirstname), ("last", d.InsuredLastname), ("age", d.InsuredAge), ("code", d.InsuredCode), ("email", d.InsuredEmailAddress), ("address", (object?)d.InsuredAddress1 ?? DBNull.Value), ("city", (object?)d.InsuredCity ?? DBNull.Value), ("user", UserUid()));
            var analysis = await riskAnalysis.AnalyzeAsync(d);
            await Execute(connection, transaction, "INSERT INTO ai_analysis (ai_analysis_uid,submission_uid,risk_score,risk_level,risk_summary,risk_factors,recommendation,model_used,validation_status,ai_analysis_created_by) VALUES (@uid,@submission,@score,@level,@summary,@factors,@recommendation,@model,'COMPLETED',@user)",
                ("uid", IdentifierGenerator.New()), ("submission", submissionUid), ("score", analysis.RiskScore), ("level", analysis.RiskLevel), ("summary", analysis.RiskSummary), ("factors", analysis.RiskFactors), ("recommendation", analysis.Recommendation), ("model", "llama-3.3-70b-versatile"), ("user", UserUid()));
            await Execute(connection, transaction, "INSERT INTO audit_log (audit_log_uid,submission_uid,action,old_status,new_status,audit_log_created_by,audit_log_updated_by) VALUES (@uid,@submission,'Submission Created',NULL,'Submission Created',@user,@user)", ("uid", IdentifierGenerator.New()), ("submission", submissionUid), ("user", UserUid()));
            await transaction.CommitAsync();
            return StatusCode(201, new { message = "submission got created. underwriter will review it", details = submission });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Problem($"Issues with submission creation. Please revisit the details: {ex.Message}", statusCode: 500);
        }
    }

    [HttpPatch("update/{subNumber}"), Authorize(Roles = "UNDERWRITER")]
    public async Task<IActionResult> Update(string subNumber, SubmissionStatusUpdate update)
    {
        var allowed = new[] { "APPROVED", "REJECTED", "MANUAL_REVIEW", "PENDING_INFO" };
        if (!allowed.Contains(update.SubmissionStatus)) return BadRequest(new { detail = "Invalid submission status" });
        await using var connection = await db.OpenAsync(); await using var transaction = await connection.BeginTransactionAsync();
        await using var find = new NpgsqlCommand("SELECT submissions_uid,submission_status FROM submissions WHERE submission_number=@number", connection, transaction);
        find.Parameters.AddWithValue("number", subNumber); await using var item = await find.ExecuteReaderAsync();
        if (!await item.ReadAsync()) return NotFound(new { detail = "Submission not found" });
        var uid = item.GetString(0); var old = item.GetString(1); await item.CloseAsync();
        await Execute(connection, transaction, "UPDATE submissions SET submission_status=@status WHERE submissions_uid=@uid", ("status", update.SubmissionStatus), ("uid", uid));
        await Execute(connection, transaction, "INSERT INTO audit_log (audit_log_uid,submission_uid,action,old_status,new_status) VALUES (@id,@uid,'Status Updated',@old,@new)", ("id", IdentifierGenerator.New()), ("uid", uid), ("old", old), ("new", update.SubmissionStatus));
        await transaction.CommitAsync(); return Ok(new { submission_number = subNumber, submission_status = update.SubmissionStatus });
    }

    private string UserUid() => User.FindFirstValue("useruid") ?? throw new UnauthorizedAccessException();
    private static string? NullableString(NpgsqlDataReader reader, int i) => reader.IsDBNull(i) ? null : reader.GetString(i);
    private static async Task Execute(NpgsqlConnection c, NpgsqlTransaction t, string sql, params (string, object)[] p) { await using var cmd = new NpgsqlCommand(sql, c, t); foreach (var (name, value) in p) cmd.Parameters.AddWithValue(name, value); await cmd.ExecuteNonQueryAsync(); }
    private static async Task<string> NextSubmissionNumber(NpgsqlConnection c, NpgsqlTransaction t) { await using var select = new NpgsqlCommand("SELECT CONCAT(prefix,first_number) FROM unique_number_generator WHERE module_name='SUBMISSIONS' FOR UPDATE", c, t); var n = (string?)await select.ExecuteScalarAsync() ?? throw new InvalidOperationException("SUBMISSIONS number generator is missing."); await Execute(c, t, "UPDATE unique_number_generator SET first_number=first_number+increment WHERE module_name='SUBMISSIONS'"); return n; }
}
