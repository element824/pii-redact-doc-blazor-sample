using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Azure.Storage.Blobs.Models;

namespace PiiRedactionApp.Services;

public interface IBlobStorageService
{
    Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName);
    Task<string> GenerateSasUrl(string containerName, string blobName, TimeSpan expiry);
    Task<string> GenerateContainerSasUrl(string containerName, TimeSpan expiry);
    Task<List<string>> ListBlobsAsync(string containerName, string prefix = "");
    Task<Stream> DownloadBlobAsync(string containerName, string blobName);
}

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(BlobServiceClient blobServiceClient, ILogger<BlobStorageService> logger)
    {
        _blobServiceClient = blobServiceClient;
        _logger = logger;
    }

    public async Task<string> UploadFileAsync(Stream fileStream, string fileName, string containerName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            var uniqueFileName = $"{Guid.NewGuid()}_{fileName}";
            var blobClient = containerClient.GetBlobClient(uniqueFileName);

            // Copy to memory stream to avoid BrowserFileStream limitations
            using var memoryStream = new MemoryStream();
            await fileStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            await blobClient.UploadAsync(memoryStream, overwrite: true);

            _logger.LogInformation("Successfully uploaded file {FileName} to container {ContainerName}", uniqueFileName, containerName);
            return uniqueFileName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file {FileName} to container {ContainerName}", fileName, containerName);
            throw;
        }
    }

    public Task<string> GenerateSasUrl(string containerName, string blobName, TimeSpan expiry)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            if (blobClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    BlobName = blobName,
                    Resource = "b",
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
                };

                sasBuilder.SetPermissions(BlobSasPermissions.Read);

                var sasUri = blobClient.GenerateSasUri(sasBuilder);
                return Task.FromResult(sasUri.ToString());
            }

            return Task.FromException<string>(new InvalidOperationException("Cannot generate SAS token. Ensure the BlobServiceClient is configured with account key credentials."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate SAS URL for blob {BlobName} in container {ContainerName}", blobName, containerName);
            return Task.FromException<string>(ex);
        }
    }

    public async Task<string> GenerateContainerSasUrl(string containerName, TimeSpan expiry)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync(PublicAccessType.None);

            if (containerClient.CanGenerateSasUri)
            {
                var sasBuilder = new BlobSasBuilder
                {
                    BlobContainerName = containerName,
                    Resource = "c",
                    ExpiresOn = DateTimeOffset.UtcNow.Add(expiry)
                };

                sasBuilder.SetPermissions(BlobContainerSasPermissions.Read | BlobContainerSasPermissions.Add | 
                                        BlobContainerSasPermissions.Create | BlobContainerSasPermissions.Write);

                var sasUri = containerClient.GenerateSasUri(sasBuilder);
                return sasUri.ToString();
            }

            throw new InvalidOperationException("Cannot generate SAS token. Ensure the BlobServiceClient is configured with account key credentials.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate container SAS URL for container {ContainerName}", containerName);
            throw;
        }
    }

    public async Task<List<string>> ListBlobsAsync(string containerName, string prefix = "")
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = new List<string>();

            await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
            {
                blobs.Add(blobItem.Name);
            }

            return blobs;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to list blobs in container {ContainerName} with prefix {Prefix}", containerName, prefix);
            throw;
        }
    }

    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
    {
        try
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var response = await blobClient.DownloadStreamingAsync();
            return response.Value.Content;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download blob {BlobName} from container {ContainerName}", blobName, containerName);
            throw;
        }
    }
}