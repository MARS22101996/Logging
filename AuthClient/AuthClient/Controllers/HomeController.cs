using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace AuthClient.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            _logger.LogInformation("Start page is successfully loaded");

            return View();
        }

        public IActionResult Admin()
        {
            ViewData["Message"] = "Hello, Admin!";

            _logger.LogInformation("Admin page is successfully loaded");

            return View();
        }

        public IActionResult Users()
        {
            ViewData["Message"] = "Hello, users!";

            _logger.LogInformation("Users page is successfully loaded");

            return View();
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}
