using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Verity.Insurance.Api.Contracts;

namespace Verity.Insurance.Api.Services;

public sealed class GroqRiskAnalysisService : IRiskAnalysisService
{
    private readonly HttpClient _client;
    private readonly IConfiguration _configuration;
    private readonly ILogger<GroqRiskAnalysisService> _logger;

    public GroqRiskAnalysisService(
        HttpClient client,
        IConfiguration configuration,
        ILogger<GroqRiskAnalysisService> logger)
    {
        _client = client;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<AIAnalysisResponse> AnalyzeAsync(SubmissionDetail details)
    {
        var apiKey = _configuration["Groq:ApiKey"];

        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Groq API Key is missing.");

        var model = _configuration["Groq:Model"] ?? "llama-3.3-70b-versatile";

        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            "https://api.groq.com/openai/v1/chat/completions");

        request.Headers.Authorization =
            new AuthenticationHeaderValue("Bearer", apiKey);

        request.Content = new StringContent(
            JsonSerializer.Serialize(new
            {
                model,
                temperature = 0.3,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = BuildPrompt(details)
                    }
                }
            }),
            Encoding.UTF8,
            "application/json");

        _logger.LogInformation(
            "Sending underwriting request for {FirstName} {LastName}",
            details.InsuredFirstname,
            details.InsuredLastname);

        using var response = await _client.SendAsync(request);

        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync();

        using var groqDocument = JsonDocument.Parse(responseContent);

        var aiContent = groqDocument.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();

        if (string.IsNullOrWhiteSpace(aiContent))
            throw new InvalidOperationException("Groq returned an empty response.");

        var json = ExtractJsonObject(aiContent);

        using var analysis = JsonDocument.Parse(json);

        var root = analysis.RootElement;

        _logger.LogInformation("Risk analysis completed successfully.");

        return new AIAnalysisResponse(
            ScalarText(root.GetProperty("risk_summary")),
            ScalarText(root.GetProperty("recommendation")),
            ScalarText(root.GetProperty("risk_level")),
            root.GetProperty("risk_score").GetInt32(),
            root.GetProperty("risk_factors").GetRawText(),
            root.TryGetProperty("missing_information", out var missing)
                ? ScalarText(missing)
                : null);
    }

    private static string BuildPrompt(SubmissionDetail details)
    {
        return $@"
You are an insurance underwriting assistant.

Assess the following applicant and respond ONLY with valid JSON.

Applicant Name:
{details.InsuredFirstname} {details.InsuredLastname}

Age:
{details.InsuredAge}

Email:
{details.InsuredEmailAddress}

Address:
{details.InsuredAddress1}, {details.InsuredCity}

Return ONLY this JSON format:

{{
  ""risk_score"": 0,
  ""risk_level"": ""LOW"",
  ""risk_summary"": """",
  ""risk_factors"": [],
  ""recommendation"": ""APPROVE"",
  ""missing_information"": """"
}}

Rules:
- risk_score must be between 0 and 100.
- risk_level must be LOW, MEDIUM, or HIGH.
- recommendation must be APPROVE, MANUAL_REVIEW, or REJECT.
- Do not include markdown.
- Do not wrap the JSON in ```json.
- Return only the JSON object.
";
    }

    private static string ExtractJsonObject(string content)
    {
        var start = content.IndexOf('{');
        var end = content.LastIndexOf('}');

        if (start < 0 || end <= start)
            throw new InvalidOperationException("Groq response did not contain a valid JSON object.");

        return content[start..(end + 1)];
    }

    private static string ScalarText(JsonElement value)
    {
        return value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty
            : value.GetRawText();
    }
}