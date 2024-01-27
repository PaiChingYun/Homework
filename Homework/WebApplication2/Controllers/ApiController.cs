using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication2.Models;
using WebApplication2.Models.Dtos;

namespace WebApplication2.Controllers
{
    public class ApiController : Controller
    {
        private readonly MyDBContext _dbContext;
        private readonly IWebHostEnvironment _host;

        public ApiController(MyDBContext dbContext, IWebHostEnvironment host)
        {
            _dbContext = dbContext;
            _host = host;
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

        public IActionResult CheckEmail(string email/*, string email, int age*/)
        {
            //if (string.IsNullOrEmpty(name))
            //{
            //    name = "guest";
            //}
            //return Content($"Hello {name}. You are {age} years old.!!");

            var isExist = _dbContext.Members.Any(m => m.Email == email);
            return Content(isExist.ToString());
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(Member member, IFormFile Avatar)
        {
            string fileName = "empty.jpg";
            if (Avatar != null)
            {
                fileName = Avatar.FileName;
            }

            //取得檔案上傳的實際路徑
            string uploadPath = Path.Combine(_host.WebRootPath, "haha", fileName);
            //檔案上傳
            using (var fileStream = new FileStream(uploadPath, FileMode.Create))
            {
                Avatar?.CopyTo(fileStream);
            }

            //轉成二進位
            byte[]? imgByte = null;
            using (var memoryStream = new MemoryStream())
            {
                Avatar?.CopyTo(memoryStream);
                imgByte = memoryStream.ToArray();
            }

            member.FileName = fileName;
            member.FileData = imgByte;

            //密碼加鹽          
            // 設定 PBKDF2 參數
            int iterations = 10000;
            int numBytesRequested = 32;
            //產生鹽
            byte[] salt = GenerateRandomSalt();
            // 使用 PBKDF2 加密
            byte[] hashedPassword = KeyDerivation.Pbkdf2(member.Password, salt, KeyDerivationPrf.HMACSHA512, iterations, numBytesRequested);

            // salt 和 Password 可以被儲存為位元組陣列或轉換成十六進位字串
            member.Salt = Convert.ToBase64String(salt);
            member.Password = Convert.ToBase64String(hashedPassword);

            //新增
            _dbContext.Members.Add(member);
            _dbContext.SaveChanges();
            return Content("存檔成功", "text/plain", System.Text.Encoding.UTF8);
        }

            //return Content("新增成功");

            // return Content($"Hello {_user.Name}, {_user.Age}歲了,電子郵件是{_user.Email}");
            //return Content($"{_user.Avatar?.FileName}-{_user.Avatar?.Length}-{_user.Avatar?.ContentType}");
        //}

        // 產生鹽

        private static byte[] GenerateRandomSalt()
        {
            byte[] salt = new byte[16]; // 16位元組的加鹽
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        //public IActionResult RegisterTest(UserDto _user)
        //{
        //    //if (string.IsNullOrEmpty(_user.Name))
        //    //{
        //    //    _user.Name = "Guest";
        //    //}
        //    //return Content($"Hello {_user.Name}. You are {_user.Age} years old.!! Your email is {_user.Email}");

        //    string fileName = "empty.jpg";
        //    if(_user.Avatar!= null)
        //    {
        //        fileName= _user.Avatar.FileName;
        //    }

        //    string uploadPath = Path.Combine(_host.WebRootPath, "images", fileName);
        //    using(var fileStream=new FileStream(uploadPath, FileMode.Create))
        //    {
        //        _user.Avatar?.CopyTo(fileStream);
        //    }


        //    return Content($"{_user.Avatar?.FileName}-{_user.Avatar?.Length}-{_user.Avatar?.ContentType}");
        //}

        public IActionResult Spots()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Spots([FromBody]SearchDto _search) 
        {
            // 根據分類編號讀取景點資料
            var spots= _search.categoryId==0? _dbContext.SpotImagesSpots: _dbContext.SpotImagesSpots.Where(s=> s.CategoryId==_search.categoryId);

            // 根據關鍵字搜尋
            
            if(!string.IsNullOrEmpty(_search.keyword))
            { spots=spots.Where(s => s.SpotTitle.Contains(_search.keyword) || s.SpotDescription.Contains(_search.keyword));
            }

            switch (_search.sortBy)
            {
                case "spotTitle":
                    spots= _search.sortType == "asc"? spots.OrderBy(s=>s.SpotTitle):spots.OrderByDescending(s=>s.SpotTitle); 
                    break;

                default:
                    spots = _search.sortType == "asc" ? spots.OrderBy(s => s.SpotId): spots.OrderByDescending(s => s.SpotId);
                    break;
            }

            // 分頁
            int TotalCount = spots.Count();// 搜尋出來共幾筆
            int pageSize = _search.pageSize?? 9;// 每頁多少筆資料
            int TotalPages= (int)Math.Ceiling((decimal)TotalCount/pageSize);// 計算出總共有幾頁
            int page = _search.page ?? 1;// 第幾頁

            // 取出分頁資料
            spots = spots.Skip((page - 1) * pageSize).Take(pageSize);

            // 設計要回傳的資料格式
            SpotsPagingDto spotsPaging = new SpotsPagingDto();
            spotsPaging.TotalPages = TotalPages;
            spotsPaging.SpotsResult=spots.ToList();
            return Json(spotsPaging);
        }

        public IActionResult SpotsTitle(string keyword)
        {
            var spots = _dbContext.Spots.Where(s => s.SpotTitle.Contains(keyword))
                               .Select(s => s.SpotTitle).Take(8);
            return Json(spots);
        }

        public IActionResult Categories()
        {
            var categoryName= _dbContext.Categories.Select(c=> c.CategoryName);
            return Json(categoryName);
        }
    }
}
