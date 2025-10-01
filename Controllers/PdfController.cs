using Microsoft.AspNetCore.Mvc;
using PiiRedactionApp.Services;

namespace PiiRedactionApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PdfController : ControllerBase
{
    private readonly IBlobStorageService _blobStorageService;
    private readonly ILogger<PdfController> _logger;

    public PdfController(IBlobStorageService blobStorageService, ILogger<PdfController> logger)
    {
        _blobStorageService = blobStorageService;
        _logger = logger;
    }

    [HttpGet("view/{containerName}/{*blobName}")]
    public async Task<IActionResult> ViewPdf(string containerName, string blobName)
    {
        try
        {
            _logger.LogInformation("Attempting to retrieve PDF: Container={ContainerName}, Blob={BlobName}", containerName, blobName);
            
            // URL decode the blob name in case it contains special characters
            blobName = Uri.UnescapeDataString(blobName);
            
            _logger.LogInformation("Decoded blob name: {BlobName}", blobName);
            
            // Download the PDF content from blob storage
            var pdfStream = await _blobStorageService.DownloadBlobAsync(containerName, blobName);
            
            if (pdfStream == null)
            {
                _logger.LogWarning("PDF stream is null for: {ContainerName}/{BlobName}", containerName, blobName);
                return NotFound($"PDF not found: {containerName}/{blobName}");
            }

            _logger.LogInformation("Successfully retrieved PDF stream for: {ContainerName}/{BlobName}", containerName, blobName);
            
            // Return the PDF with proper content type and headers for inline viewing
            return File(pdfStream, "application/pdf", enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving PDF: {ContainerName}/{BlobName}. Error: {ErrorMessage}", containerName, blobName, ex.Message);
            return StatusCode(500, $"Error retrieving PDF: {ex.Message}");
        }
    }

    [HttpGet("download/{containerName}/{*blobName}")]
    public async Task<IActionResult> DownloadPdf(string containerName, string blobName)
    {
        try
        {
            // URL decode the blob name
            blobName = Uri.UnescapeDataString(blobName);
            
            // Download the PDF content from blob storage
            var pdfStream = await _blobStorageService.DownloadBlobAsync(containerName, blobName);
            
            if (pdfStream == null)
            {
                return NotFound("PDF not found");
            }

            // Return the PDF as a download
            return File(pdfStream, "application/pdf", $"{blobName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading PDF: {ContainerName}/{BlobName}", containerName, blobName);
            return StatusCode(500, $"Error downloading PDF: {ex.Message}");
        }
    }

    [HttpGet("test/{containerName}")]
    public async Task<IActionResult> TestConnection(string containerName)
    {
        try
        {
            var blobs = await _blobStorageService.ListBlobsAsync(containerName);
            return Ok(new { Container = containerName, BlobCount = blobs.Count, Blobs = blobs.Take(10) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error testing connection to container: {ContainerName}", containerName);
            return StatusCode(500, $"Error testing connection: {ex.Message}");
        }
    }
}