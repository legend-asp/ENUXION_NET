using MessengerApp1.Data;
using MessengerApp1.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MessengerApp1.Controllers
{
    [Authorize]
    public class ProfileController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProfileController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _context = context;
            _env = env;
        }

        // 1. ОТКРЫТИЕ НАСТРОЕК (СТРОГО [HttpGet])
        [HttpGet]
        [Route("Profile/Settings")]
        public async Task<IActionResult> Settings()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            return View("Index", new ProfileViewModel
            {
                BirthDate = user.BirthDate,
                PhotoPath = user.PhotoPath,
                Status = user.Status,
                About = user.About,
            });
        }

        // 2. СОХРАНЕНИЕ НАСТРОЕК (СТРОГО [HttpPost])
        [HttpPost]
        [Route("Profile/Settings")]
        public async Task<IActionResult> Settings(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
                return View("Index", model);

            user.BirthDate = model.BirthDate;
            user.Status = model.Status;
            user.About = model.About;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                // Валидация размера (макс 3 MB)
                if (model.Photo.Length > 3 * 1024 * 1024)
                {
                    ModelState.AddModelError("Photo", "Файл не должен превышать 3 МБ");
                    return View("Index", model);
                }

                // Валидация расширения
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var extension = Path.GetExtension(model.Photo.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("Photo", "Разрешены только форматы: JPG, PNG, WEBP");
                    return View("Index", model);
                }

                var avatarsFolder = Path.Combine(_env.WebRootPath, "avatars");
                Directory.CreateDirectory(avatarsFolder);

                // Удаление старого аватара
                if (!string.IsNullOrEmpty(user.PhotoPath))
                {
                    var oldPath = Path.Combine(_env.WebRootPath, user.PhotoPath.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        System.IO.File.Delete(oldPath);
                    }
                }

                // Сохранение нового аватара
                var fileName = $"{user.Id}_{Guid.NewGuid():N}{extension}";
                var fullPath = Path.Combine(avatarsFolder, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await model.Photo.CopyToAsync(stream);
                }

                user.PhotoPath = $"/avatars/{fileName}";
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction("View", new { id = user.Id });
        }

        // ПРОСМОТР ЛЮБОГО ПРОФИЛЯ
        [HttpGet]
        public async Task<IActionResult> View(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var currentUser = await _userManager.GetUserAsync(User);
            ViewBag.IsOwnProfile = currentUser?.Id == id;

            var roles = await _userManager.GetRolesAsync(user);
            ViewBag.Role = roles.FirstOrDefault() ?? "User";

            // Дружба
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => (f.UserId == currentUser.Id && f.FriendId == id) ||
                                          (f.UserId == id && f.FriendId == currentUser.Id));

            ViewBag.FriendshipStatus = "None";
            if (friendship != null)
            {
                if (friendship.IsAccepted) ViewBag.FriendshipStatus = "Accepted";
                else if (friendship.UserId == currentUser.Id) ViewBag.FriendshipStatus = "Sent";
                else ViewBag.FriendshipStatus = "Received";
            }

            var friendIds = await _context.Friendships
                .Where(f => f.IsAccepted && (f.UserId == id || f.FriendId == id))
                .Select(f => f.UserId == id ? f.FriendId : f.UserId)
                .ToListAsync();

            ViewBag.FriendsDetails = await _userManager.Users
                .Where(u => friendIds.Contains(u.Id))
                .ToListAsync();

            return View(user);
        }

        // ЗАЯВКИ В ДРУЗЬЯ
        [HttpGet]
        public async Task<IActionResult> Requests()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var requesterIds = await _context.Friendships
                .Where(f => f.FriendId == currentUser.Id && !f.IsAccepted)
                .Select(f => f.UserId)
                .ToListAsync();

            var requests = await _userManager.Users
                .Where(u => requesterIds.Contains(u.Id))
                .ToListAsync();

            return View(requests);
        }

        [HttpPost]
        public async Task<IActionResult> AddFriend(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var existing = await _context.Friendships.AnyAsync(f =>
                (f.UserId == currentUser.Id && f.FriendId == id) ||
                (f.UserId == id && f.FriendId == currentUser.Id));

            if (!existing)
            {
                _context.Friendships.Add(new Friendship { UserId = currentUser.Id, FriendId = id, IsAccepted = false });
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("View", new { id });
        }

        [HttpPost]
        public async Task<IActionResult> AcceptFriend(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => f.UserId == id && f.FriendId == currentUser.Id && !f.IsAccepted);

            if (friendship != null)
            {
                friendship.IsAccepted = true;
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Requests");
        }

        [HttpPost]
        public async Task<IActionResult> RemoveFriend(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var friendship = await _context.Friendships
                .FirstOrDefaultAsync(f => (f.UserId == currentUser.Id && f.FriendId == id) ||
                                          (f.UserId == id && f.FriendId == currentUser.Id));

            if (friendship != null)
            {
                _context.Friendships.Remove(friendship);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("View", new { id });
        }
    }
}