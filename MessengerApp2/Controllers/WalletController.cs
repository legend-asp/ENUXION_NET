using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MessengerApp1.Data;
using MessengerApp1.Models;
using System;
using System.Threading.Tasks;

namespace MessengerApp1.Controllers
{
    public class WalletController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public WalletController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Главная страница кошелька
        // Главная страница кошелька
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            // Подгружаем последние 10 транзакций
            var history = await _context.Messages
                .Where(m => (m.SenderId == user.Id || m.ReceiverId == user.Id) && m.Text.StartsWith("[SYSTEM_TRANSFER]"))
                .OrderByDescending(m => m.SentAt)
                .Take(10)
                .ToListAsync();

            ViewBag.Transfers = history;

            return View(user);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Transfer(string receiverId, decimal amount)
        {
            var senderId = _userManager.GetUserId(User);
            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    var sender = await _context.Users.FirstOrDefaultAsync(u => u.Id == senderId);
                    var receiver = await _context.Users.FirstOrDefaultAsync(u => u.Id == receiverId);

                    if (sender == null || receiver == null || amount <= 0 || sender.Balance < amount)
                        return Json(new { success = false, message = "Ошибка: недостаточно средств" });

                    sender.Balance -= amount;
                    receiver.Balance += amount;

                    _context.Messages.Add(new Message
                    {
                        SenderId = senderId,
                        ReceiverId = receiverId,
                        Text = $"[SYSTEM_TRANSFER]: {amount:F2}",
                        SentAt = DateTime.Now
                    });

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // Возвращаем успех и новое значение баланса
                    return Json(new
                    {
                        success = true,
                        newBalance = sender.Balance.ToString("N2", System.Globalization.CultureInfo.InvariantCulture)
                    });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    return Json(new { success = false });
                }
            }
        }
    }
}