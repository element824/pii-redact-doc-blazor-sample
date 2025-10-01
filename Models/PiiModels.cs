namespace PiiRedactionApp.Models;

public class PiiJobRequest
{
    public string DisplayName { get; set; } = string.Empty;
    public AnalysisInput AnalysisInput { get; set; } = new();
    public List<PiiTask> Tasks { get; set; } = new();
}

public class AnalysisInput
{
    public List<DocumentInput> Documents { get; set; } = new();
}

public class DocumentInput
{
    public string Language { get; set; } = "en-US";
    public string Id { get; set; } = string.Empty;
    public DocumentSource Source { get; set; } = new();
    public DocumentTarget Target { get; set; } = new();
}

public class DocumentSource
{
    public string Location { get; set; } = string.Empty;
}

public class DocumentTarget
{
    public string Location { get; set; } = string.Empty;
}

public class PiiTask
{
    public string Kind { get; set; } = "PiiEntityRecognition";
    public string TaskName { get; set; } = string.Empty;
    public PiiParameters Parameters { get; set; } = new();
}

public class PiiParameters
{
    public RedactionPolicy RedactionPolicy { get; set; } = new();
    public List<string> PiiCategories { get; set; } = new();
    public bool ExcludeExtractionData { get; set; } = false;
}

public class RedactionPolicy
{
    public string PolicyKind { get; set; } = "entityMask";
}

public class PiiJobResponse
{
    public string JobId { get; set; } = string.Empty;
    public string LastUpdatedDateTime { get; set; } = string.Empty;
    public string CreatedDateTime { get; set; } = string.Empty;
    public string ExpirationDateTime { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public string DisplayName { get; set; } = string.Empty;
    public TaskProgress Tasks { get; set; } = new();
}

public class TaskProgress
{
    public int Completed { get; set; }
    public int Failed { get; set; }
    public int InProgress { get; set; }
    public int Total { get; set; }
}

public class UploadResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string JobId { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string? ErrorDetails { get; set; }
}

public class PiiJobResult
{
    public string JobId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public List<PiiTaskResult> Tasks { get; set; } = new();
}

public class PiiTaskResult
{
    public string TaskName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public PiiTaskResults Results { get; set; } = new();
}

public class PiiTaskResults
{
    public List<PiiDocumentResult> Documents { get; set; } = new();
}

public class PiiDocumentResult
{
    public string Id { get; set; } = string.Empty;
    public string RedactedText { get; set; } = string.Empty;
    public List<PiiEntity> Entities { get; set; } = new();
    public DocumentTarget? Target { get; set; }
}

public class PiiEntity
{
    public string Text { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string SubCategory { get; set; } = string.Empty;
    public double ConfidenceScore { get; set; }
    public int Offset { get; set; }
    public int Length { get; set; }
}