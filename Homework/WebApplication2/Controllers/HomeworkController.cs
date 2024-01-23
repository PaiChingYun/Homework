using Microsoft.AspNetCore.Mvc;

namespace WebApplication2.Controllers
{
    public class HomeworkController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
