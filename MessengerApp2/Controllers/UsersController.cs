using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp1.Data;
using Microsoft.AspNetCore.Identity;
using MessengerApp1.Models;

namespace MessengerApp1.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> View(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.IsOwnProfile = currentUser?.Id == user.Id;
            ViewBag.Role = "OPERATOR";

            // Проверяем статус дружбы для отображения кнопок
            if (currentUser != null && currentUser.Id != user.Id)
            {
                var friendship = await _context.Friendships
                    .FirstOrDefaultAsync(f => (f.UserId == currentUser.Id && f.FriendId == id) ||
                                              (f.UserId == id && f.FriendId == currentUser.Id));

                if (friendship == null) ViewBag.FriendshipStatus = "None";
                else if (friendship.IsAccepted) ViewBag.FriendshipStatus = "Accepted";
                else ViewBag.FriendshipStatus = friendship.UserId == currentUser.Id ? "Sent" : "Received";
            }

            return View(user);
        }

        // МЕТОД: Добавить в друзья
        [HttpPost]
        public async Task<IActionResult> AddFriend(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null || currentUser.Id == id) return BadRequest();

            var existing = await _context.Friendships
                .AnyAsync(f => (f.UserId == currentUser.Id && f.FriendId == id) ||
                               (f.UserId == id && f.FriendId == currentUser.Id));

            if (!existing)
            {
                var friendship = new Friendship
                {
                    UserId = currentUser.Id,
                    FriendId = id,
                    IsAccepted = false // Ждем подтверждения
                };
                _context.Friendships.Add(friendship);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("View", new { id = id });
        }
        [HttpPost]
        public async Task<IActionResult> AcceptFriend(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == id && f.FriendId == currentUser.Id);

            if (friendship != null)
            {
                friendship.IsAccepted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("View", new { id = id });
        }

        // МЕТОД: Написать (редирект в чат)
        public IActionResult Chat(string id)
        {
            // Перенаправляем в контроллер сообщений (Messages), который мы создадим следующим шагом
            return RedirectToAction("Chat", "Messages", new { receiverId = id });
        }
    }
}