using Microsoft.AspNetCore.Mvc;
using WebApplication1.BLogic.Interface;

namespace WebApplication1.Controllers
{
    public class MessageController : Controller
    {
        private readonly IMessageService _messageService;

        public MessageController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> Submit(string message)
        {
            string userId = GetOrCreateUserId();
            await _messageService.SaveUserMessage(userId, message);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> GetUserMessages()
        {
            string userId = GetOrCreateUserId();
            var messages = await _messageService.LoadUserMessages(userId);
            return Json(messages);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllMessages()
        {
            var messages = await _messageService.LoadAllMessages();
            return Json(messages);
        }

        private string GetOrCreateUserId()
        {
            string userId = Request.Cookies["UserId"];

            if (string.IsNullOrEmpty(userId))
            {
                userId = Guid.NewGuid().ToString();
                Response.Cookies.Append("UserId", userId);
            }

            return userId;
        }
    }
}