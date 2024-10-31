using Azure.Storage.Blobs;
using System.IO;
using System.Threading.Tasks;

public class BlobService
{
    private readonly BlobServiceClient _blobServiceClient;

    public BlobService(string connectionString)
    {
        Console.WriteLine($"Connection String: {connectionString}"); 
        _blobServiceClient = new BlobServiceClient(connectionString);
    }


    public async Task UploadFileAsync(string containerName, Stream fileStream, string fileName)
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
        await containerClient.CreateIfNotExistsAsync();
        var blobClient = containerClient.GetBlobClient(fileName);
        await blobClient.UploadAsync(fileStream, overwrite: true);
    }

    public string GetBlobUrl(string containerName, string fileName)
    {
        var blobClient = _blobServiceClient.GetBlobContainerClient(containerName).GetBlobClient(fileName);
        return blobClient.Uri.ToString();
    }
}