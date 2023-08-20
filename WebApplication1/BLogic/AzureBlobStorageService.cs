using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using WebApplication1.Entity;

namespace WebApplication1.BLogic
{
    public class AzureBlobStorageService
    {
        private string _storageConnectionString;
        private string _containerName;
        private readonly IConfiguration _configuration;

        public AzureBlobStorageService(IConfiguration configuration)
        {
            _configuration = configuration;
            _storageConnectionString = _configuration["AzureBlobStorage:StorageConnectionString"];
            _containerName = _configuration["AzureBlobStorage:ContainerName"];
        }

        public async Task<List<UserMessage>> LoadMessages(Func<string, bool> filter = null)
        {
            List<UserMessage> messages = new List<UserMessage>();

            if (CloudStorageAccount.TryParse(_storageConnectionString, out CloudStorageAccount storageAccount))
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                CloudBlobContainer container = blobClient.GetContainerReference(_containerName);

                BlobContinuationToken continuationToken = null;
                do
                {
                    var resultSegment = await container.ListBlobsSegmentedAsync("", true, BlobListingDetails.All, null, continuationToken, null, null);
                    continuationToken = resultSegment.ContinuationToken;

                    foreach (var blobItem in resultSegment.Results.OfType<CloudBlockBlob>())
                    {
                        try
                        {
                            string content = await blobItem.DownloadTextAsync();

                            if (!string.IsNullOrWhiteSpace(content))
                            {
                                UserMessage userMessage = JsonConvert.DeserializeObject<UserMessage>(content);

                                if (filter == null || filter(userMessage.UserId))
                                {
                                    messages.Add(userMessage);
                                }
                            }
                        }
                        catch (StorageException ex) when (ex.RequestInformation.ErrorCode.Equals("BlobNotFound"))
                        {
                            Console.WriteLine($"Blob {blobItem.Name} not found. Skipping.");
                        }
                    }
                }
                while (continuationToken != null);
            }

            return SortMessagesByTimestamp(messages);
        }

        public CloudBlobContainer GetBlobContainer()
        {
            if (CloudStorageAccount.TryParse(_storageConnectionString, out CloudStorageAccount storageAccount))
            {
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
                return blobClient.GetContainerReference(_containerName);
            }

            return null;
        }

        public async Task<CloudBlockBlob> GetBlobReferenceAsync(CloudBlobContainer container, string blobName)
        {
            await container.CreateIfNotExistsAsync();
            return container.GetBlockBlobReference(blobName);
        }

        private List<UserMessage> SortMessagesByTimestamp(List<UserMessage> messages)
        {
            return messages.OrderByDescending(m => m.Timestamp).ToList();
        }
    }
}