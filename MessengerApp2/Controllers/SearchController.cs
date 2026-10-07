using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MessengerApp1.Data; // Проверь, что путь к папке Data верный
using MessengerApp1.Models;
using System.Linq;
using System.Threading.Tasks;

namespace MessengerApp1.Controllers
{
    // 1. Контроллер ОБЯЗАН наследоваться от Controller, чтобы видеть View и ViewBag
    public class SearchController : Controller
    {
        private readonly ApplicationDbContext _context;

        // 2. Инжектим контекст базы данных через конструктор
        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 3. Метод Index для поиска
        public async Task<IActionResult> Index(string searchString)
        {
            // Если строка пустая — ничего не ищем, показываем пустой экран (сканер в простое)
            if (string.IsNullOrWhiteSpace(searchString))
            {
                ViewBag.SearchPerformed = false;
                return View(new List<ApplicationUser>());
            }

            ViewBag.SearchPerformed = true;

            // Ищем только по UserName (Handle), как в Telegram
            var users = await _context.Users
                .Where(u => u.UserName.Contains(searchString))
                .ToListAsync();

            return View(users);
        }
    }
}