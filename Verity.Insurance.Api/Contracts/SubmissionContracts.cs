using System.ComponentModel.DataAnnotations;

namespace Verity.Insurance.Api.Contracts;

public record SubmissionDetail(string InsuredFirstname, string InsuredLastname, int InsuredAge, int InsuredCode,
    [param: EmailAddress] string InsuredEmailAddress, string? InsuredAddress1 = null, string? InsuredCity = null);
public record SubmissionCreate(string SubmissionType, SubmissionDetail SubmissionDetails, string? SubmissionSummary = null);
public record SubmissionResponse(string SubmissionNumber, string SubmissionType, string SubmissionStatus,
    string InsuredFirstname, string InsuredLastname, int InsuredAge, int InsuredCode, string InsuredEmailAddress,
    string? InsuredAddress1, string? InsuredCity, string? SubmissionSummary);
public record SubmissionStatusUpdate(string SubmissionStatus);
