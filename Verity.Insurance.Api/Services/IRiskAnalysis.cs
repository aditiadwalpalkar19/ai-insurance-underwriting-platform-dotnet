using Verity.Insurance.Api.Contracts;

namespace Verity.Insurance.Api.Services;

public interface IRiskAnalysisService
{
    Task<AIAnalysisResponse> AnalyzeAsync(SubmissionDetail details);
}