using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MessengerApp1.Data;
using MessengerApp1.Models;
using Microsoft.AspNetCore.Authorization;

namespace MessengerApp1.Controllers
{
    [Authorize]
    public class MessagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MessagesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Chat(string id)
        {
            var currentUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(id) || id == currentUserId) return RedirectToAction("Index", "Group");

            var receiver = await _context.Users.FindAsync(id);
            if (receiver == null) return NotFound();

            var messages = await _context.Messages
                .Where(m => (m.SenderId == currentUserId && m.ReceiverId == id) ||
                            (m.SenderId == id && m.ReceiverId == currentUserId))
                .OrderBy(m => m.SentAt) // Используем SentAt
                .ToListAsync();

            ViewBag.Receiver = receiver;
            return View(messages);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(string receiverId, string content)
        {
            var senderId = _userManager.GetUserId(User);
            if (string.IsNullOrWhiteSpace(content)) return RedirectToAction("Chat", new { id = receiverId });

            var message = new Message
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Text = content,      // Используем Text
                SentAt = DateTime.Now // Используем SentAt
            };

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return RedirectToAction("Chat", new { id = receiverId });
        }
    }
}