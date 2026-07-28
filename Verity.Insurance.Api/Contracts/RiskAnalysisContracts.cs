namespace Verity.Insurance.Api.Contracts;

public record AIAnalysisResponse(string RiskSummary, string Recommendation, string RiskLevel, int RiskScore,
    string RiskFactors, string? MissingInformation);
