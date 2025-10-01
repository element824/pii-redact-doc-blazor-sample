using PiiRedactionApp.Models;
using System.Text.Json;

namespace PiiRedactionApp.Services;

public interface IPiiDetectionService
{
    Task<string> SubmitPiiJobAsync(string fileName, string sourceUrl, string targetUrl);
    Task<PiiJobResponse> GetJobStatusAsync(string jobId);
    Task<bool> IsJobCompleteAsync(string jobId);
    Task<string> GetJobResultsAsync(string jobId);
    Task<string?> GetRedactedDocumentUrlAsync(string jobId);
}

public class PiiDetectionService : IPiiDetectionService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PiiDetectionService> _logger;

    public PiiDetectionService(HttpClient httpClient, IConfiguration configuration, ILogger<PiiDetectionService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<string> SubmitPiiJobAsync(string fileName, string sourceSasUrl, string targetSasUrl)
    {
        try
        {
            var endpoint = _configuration["Azure:LanguageEndpoint"];
            var apiKey = _configuration["Azure:LanguageKey"];
            var apiVersion = _configuration["Azure:ApiVersion"];

            if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Azure Language service configuration is missing");
            }

            var url = $"{endpoint}/language/analyze-documents/jobs?api-version={apiVersion}";

            var request = new PiiJobRequest
            {
                DisplayName = $"PII Detection for {fileName}",
                AnalysisInput = new AnalysisInput
                {
                    Documents = new List<DocumentInput>
                    {
                        new DocumentInput
                        {
                            Id = Guid.NewGuid().ToString(),
                            Language = "en-US",
                            Source = new DocumentSource { Location = sourceSasUrl },
                            Target = new DocumentTarget { Location = targetSasUrl }
                        }
                    }
                },
                Tasks = new List<PiiTask>
                {
                    new PiiTask
                    {
                        Kind = "PiiEntityRecognition",
                        TaskName = "PiiTask",
                        Parameters = new PiiParameters
                        {
                            RedactionPolicy = new RedactionPolicy { PolicyKind = "entityMask" },
                            PiiCategories = new List<string>
                            {
                                "Person",
                                "Organization", 
                                "Email",
                                "USSocialSecurityNumber",
                                "CreditCardNumber",
                                "PhoneNumber",
                                "Address",
                                "DateTime",
                                "IPAddress",
                                "URL"
                            },
                            ExcludeExtractionData = false
                        }
                    }
                }
            };

            var jsonContent = JsonSerializer.Serialize(request, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", apiKey);

            var content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                if (response.Headers.TryGetValues("operation-location", out var locations))
                {
                    var operationLocation = locations.First();
                    var jobId = ExtractJobIdFromOperationLocation(operationLocation);
                    
                    _logger.LogInformation("Successfully submitted PII job for file {FileName}. Job ID: {JobId}", fileName, jobId);
                    return jobId;
                }
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to submit PII job. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
            throw new HttpRequestException($"Failed to submit PII job: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting PII job for file {FileName}", fileName);
            throw;
        }
    }

    public async Task<PiiJobResponse> GetJobStatusAsync(string jobId)
    {
        try
        {
            var endpoint = _configuration["Azure:LanguageEndpoint"];
            var apiVersion = _configuration["Azure:ApiVersion"];
            var url = $"{endpoint}/language/analyze-documents/jobs/{jobId}?api-version={apiVersion}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                var jobResponse = JsonSerializer.Deserialize<PiiJobResponse>(jsonContent, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return jobResponse ?? throw new InvalidOperationException("Failed to deserialize job response");
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to get job status for {JobId}. Status: {StatusCode}, Content: {Content}", jobId, response.StatusCode, errorContent);
            throw new HttpRequestException($"Failed to get job status: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job status for {JobId}", jobId);
            throw;
        }
    }

    public async Task<bool> IsJobCompleteAsync(string jobId)
    {
        try
        {
            var status = await GetJobStatusAsync(jobId);
            return status.Status.Equals("succeeded", StringComparison.OrdinalIgnoreCase) || 
                   status.Status.Equals("failed", StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if job {JobId} is complete", jobId);
            return false;
        }
    }

    public async Task<string> GetJobResultsAsync(string jobId)
    {
        try
        {
            var endpoint = _configuration["Azure:LanguageEndpoint"];
            var apiVersion = _configuration["Azure:ApiVersion"];
            var url = $"{endpoint}/language/analyze-documents/jobs/{jobId}?api-version={apiVersion}";

            var response = await _httpClient.GetAsync(url);
            
            if (response.IsSuccessStatusCode)
            {
                var jsonContent = await response.Content.ReadAsStringAsync();
                return jsonContent;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to get job results for {JobId}. Status: {StatusCode}, Content: {Content}", jobId, response.StatusCode, errorContent);
            throw new HttpRequestException($"Failed to get job results: {response.StatusCode} - {errorContent}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting job results for {JobId}", jobId);
            throw;
        }
    }

    public async Task<string?> GetRedactedDocumentUrlAsync(string jobId)
    {
        try
        {
            var jsonContent = await GetJobResultsAsync(jobId);
            using var document = JsonDocument.Parse(jsonContent);
            var root = document.RootElement;

            // Navigate through the JSON structure to find the redacted document URL
            if (root.TryGetProperty("tasks", out var tasks) &&
                tasks.TryGetProperty("items", out var items) &&
                items.GetArrayLength() > 0)
            {
                var firstTask = items[0];
                if (firstTask.TryGetProperty("results", out var results) &&
                    results.TryGetProperty("documents", out var documents) &&
                    documents.GetArrayLength() > 0)
                {
                    var firstDoc = documents[0];
                    if (firstDoc.TryGetProperty("targets", out var targets) &&
                        targets.ValueKind == JsonValueKind.Array)
                    {
                        // Look for PDF file in the targets array
                        foreach (var target in targets.EnumerateArray())
                        {
                            if (target.TryGetProperty("location", out var location))
                            {
                                var locationUrl = location.GetString();
                                if (!string.IsNullOrEmpty(locationUrl) && 
                                    (locationUrl.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ||
                                     locationUrl.EndsWith(".docx", StringComparison.OrdinalIgnoreCase)))
                                {
                                    _logger.LogInformation("Found redacted document URL for job {JobId}: {DocumentUrl}", jobId, locationUrl);
                                    return locationUrl;
                                }
                            }
                        }
                    }
                }
            }

            _logger.LogWarning("No redacted document URL found for job {JobId}", jobId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting redacted document URL for job {JobId}", jobId);
            return null;
        }
    }

    private static string ExtractJobIdFromOperationLocation(string operationLocation)
    {
        var uri = new Uri(operationLocation);
        var segments = uri.Segments;
        return segments[^1].TrimEnd('/');
    }
}