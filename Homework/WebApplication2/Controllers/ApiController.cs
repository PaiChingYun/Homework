using Microsoft.AspNetCore.Mvc;
using WebApplication2.Models;

namespace WebApplication2.Controllers
{
    public class ApiController : Controller
    {
        private readonly MyDBContext _dbContext;

        public ApiController(MyDBContext dbContext)
        {
            _dbContext = dbContext;
        }
        public IActionResult Index()
        {
            System.Threading.Thread.Sleep(5000);
            return Content("<h2>hello world，哈囉你好嗎?</h2>", "text/html", System.Text.Encoding.UTF8);
        }

        public IActionResult Cities()
        {
            var cities= _dbContext.Addresses.Select(a => a.City).Distinct();
            return Json(cities);
        }

        public IActionResult Counties(string city)
        {
            var counties = _dbContext.Addresses.Where(a=> a.City==city).Select(a=> a.SiteId).Distinct();
            return Json(counties);
        }

        public IActionResult Roads(string county)
        {
            var roads = _dbContext.Addresses.Where(a => a.SiteId == county).Select(a => a.Road).Distinct();
            return Json(roads);
        }

        public IActionResult Avatar(int id=1)
        {
            Member? member = _dbContext.Members.Find(id);
            if(member != null) 
            {
                byte[] img = member.FileData;
                return File(img, "image/jpeg");
            }

            return NotFound();
        }

        public IActionResult First()
        {
            return View();
        }

        public IActionResult Address()
        {
            return View();
        }

        public IActionResult CheckAccount()
        {
            return View();
        }

        public IActionResult CheckAccountName(string name/*, string email, int age*/)
        {
            //if (string.IsNullOrEmpty(name))
            //{
            //    name = "guest";
            //}
            //return Content($"Hello {name}. You are {age} years old.!!");

            var isExist = _dbContext.Members.Any(m => m.Name == name);
            return Content(isExist.ToString());
        }
    }
}
