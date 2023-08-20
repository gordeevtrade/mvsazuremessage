using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using WebApplication1.BLogic.Interface;
using WebApplication1.Entity;

namespace WebApplication1.BLogic
{
    public class MessageService : IMessageService
    {
        private const int _MaxUserMessages = 10;
        private const int _MaxAllMessages = 20;
        private AzureBlobStorageService _blobStorage;

        public MessageService(AzureBlobStorageService blobStorageService)
        {
            _blobStorage = blobStorageService;
        }

        public async Task SaveUserMessage(string userId, string message)
        {
            var userMessage = new UserMessage { UserId = userId, Message = message, Timestamp = DateTime.UtcNow };
            await SaveMessageInStorage(userMessage);
        }

        public async Task<List<UserMessage>> LoadAllMessages()
        {
            var allMessages = await GetAllMessagesFromStorage();
            await DeleteOldMessagesFromStorage(allMessages, _MaxAllMessages);
            return allMessages;
        }

        public async Task<List<UserMessage>> LoadUserMessages(string userId)
        {
            return await _blobStorage.LoadMessages(userIdFilter => userIdFilter == userId);
        }

        private async Task<List<UserMessage>> GetAllMessagesFromStorage()
        {
            return await _blobStorage.LoadMessages();
        }

        private async Task SaveMessageInStorage(UserMessage userMessage)
        {
            DateTime timestamp = DateTime.UtcNow;
            userMessage.Timestamp = timestamp;

            string jsonMessage = JsonConvert.SerializeObject(userMessage);
            string blobName = GetBlobName(userMessage.UserId, timestamp);

            CloudBlobContainer container = _blobStorage.GetBlobContainer();
            if (container != null)
            {
                CloudBlockBlob blob = await _blobStorage.GetBlobReferenceAsync(container, blobName);
                await blob.UploadTextAsync(jsonMessage);

                var userMessages = await LoadUserMessages(userMessage.UserId);
                await DeleteOldMessagesFromStorage(userMessages, _MaxUserMessages);
            }
        }

        private async Task DeleteOldMessagesFromStorage(List<UserMessage> messages, int maxCount)
        {
            if (messages.Count <= maxCount)
                return;

            var messagesToDelete = messages.OrderBy(m => m.Timestamp).Take(messages.Count - maxCount);

            CloudBlobContainer container = _blobStorage.GetBlobContainer();
            if (container != null)
            {
                foreach (var message in messagesToDelete)
                {
                    string blobName = GetBlobName(message.UserId, message.Timestamp);
                    CloudBlockBlob blob = await _blobStorage.GetBlobReferenceAsync(container, blobName);
                    await blob.DeleteIfExistsAsync();
                }
            }
        }

        private string GetBlobName(string userId, DateTime timestamp)
        {
            return "message_" + userId + "_" + timestamp.ToString("yyyyMMddHHmmss");
        }
    }
}