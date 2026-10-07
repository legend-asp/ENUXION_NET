using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp1.Data;
using Microsoft.AspNetCore.Identity;
using MessengerApp1.Models;
using Microsoft.AspNetCore.Authorization;

namespace MessengerApp1.Controllers
{
    // Модель данных для объединенного хаба (как в Discord / TG)
    public class GroupIndexViewModel
    {
        public List<ApplicationUser> DirectNodes { get; set; } = new();
        public List<Group> UserGroups { get; set; } = new();
    }

    [Authorize]
    public class GroupController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GroupController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ХАБ: Список P2P-диалогов и Групповых кластеров
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var currentUserId = _userManager.GetUserId(User);

            // 1. Личные P2P контакты
            var contactIds = await _context.Messages
                .Where(m => m.SenderId == currentUserId || m.ReceiverId == currentUserId)
                .Select(m => m.SenderId == currentUserId ? m.ReceiverId : m.SenderId)
                .Distinct()
                .ToListAsync();

            var directNodes = await _context.Users
                .Where(u => contactIds.Contains(u.Id))
                .ToListAsync();

            // 2. Группы, в которых состоит пользователь (или все группы)
            var userGroupIds = await _context.GroupMembers
                .Where(gm => gm.UserId == currentUserId)
                .Select(gm => gm.GroupId)
                .ToListAsync();

            var userGroups = await _context.Groups
                .Where(g => userGroupIds.Contains(g.Id))
                .Include(g => g.Messages)
                .ToListAsync();

            var model = new GroupIndexViewModel
            {
                DirectNodes = directNodes,
                UserGroups = userGroups
            };

            return View(model);
        }

        // СТРАНИЦА СОЗДАНИЯ ГРУППЫ (GET)
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // СОЗДАНИЕ ГРУППЫ В БАЗЕ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(string groupName)
        {
            if (string.IsNullOrWhiteSpace(groupName))
            {
                ModelState.AddModelError("", "Cluster name cannot be empty");
                return View();
            }

            var currentUserId = _userManager.GetUserId(User);

            // 1. Создаем группу
            var group = new Group
            {
                Name = groupName.Trim()
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            // 2. Автоматически добавляем создателя в участники
            var member = new GroupMember
            {
                GroupId = group.Id,
                UserId = currentUserId
            };
            _context.GroupMembers.Add(member);
            await _context.SaveChangesAsync();

            // 3. Отправляем приветственное системное сообщение
            var welcomeMsg = new GroupMessage
            {
                GroupId = group.Id,
                SenderId = currentUserId,
                Text = "[SYSTEM_INITIALIZED]: Cluster uplink established.",
                Timestamp = DateTime.Now
            };
            _context.GroupMessages.Add(welcomeMsg);
            await _context.SaveChangesAsync();

            // Сразу переходим в чат созданной группы
            return RedirectToAction("Chat", new { id = group.Id });
        }

        // ЧАТ ГРУППЫ (GET)
        [HttpGet]
        public async Task<IActionResult> Chat(int id)
        {
            var currentUserId = _userManager.GetUserId(User);

            var group = await _context.Groups
                .Include(g => g.Messages)
                    .ThenInclude(m => m.Sender)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (group == null) return NotFound();

            // Проверяем членство: если пользователя нет в группе, подключаем его
            var isMember = await _context.GroupMembers
                .AnyAsync(gm => gm.GroupId == id && gm.UserId == currentUserId);

            if (!isMember)
            {
                _context.GroupMembers.Add(new GroupMember
                {
                    GroupId = id,
                    UserId = currentUserId
                });
                await _context.SaveChangesAsync();
            }

            // Загружаем участников для боковой панели
            ViewBag.MembersCount = await _context.GroupMembers.CountAsync(gm => gm.GroupId == id);
            ViewBag.CurrentUserId = currentUserId;

            return View(group);
        }

        // ОТПРАВКА СООБЩЕНИЯ В ГРУППУ (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendMessage(int groupId, string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                var currentUserId = _userManager.GetUserId(User);

                var msg = new GroupMessage
                {
                    GroupId = groupId,
                    SenderId = currentUserId,
                    Text = text.Trim(),
                    Timestamp = DateTime.Now
                };

                _context.GroupMessages.Add(msg);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Chat", new { id = groupId });
        }
    }
}