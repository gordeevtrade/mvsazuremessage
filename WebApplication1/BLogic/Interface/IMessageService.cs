using WebApplication1.Entity;

namespace WebApplication1.BLogic.Interface
{
    public interface IMessageService
    {
        Task<List<UserMessage>> LoadUserMessages(string userId);

        Task SaveUserMessage(string userId, string message);

        Task<List<UserMessage>> LoadAllMessages();
    }
}