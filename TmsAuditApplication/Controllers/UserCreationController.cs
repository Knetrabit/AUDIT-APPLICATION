using Microsoft.AspNetCore.Mvc;
using VrsAuditApplication.Models;

namespace VrsAuditApplication.Controllers
{
    public class UserCreationController : Controller
    {
        private readonly DbService _dbService;
        public UserCreationController(DbService dbService)
        {
            _dbService = dbService;
        }
        public IActionResult Index()
        {
            ViewBag.CityList = _dbService.GetCities();
            return View();
        }

        //public IActionResult UserCreation()
        //{
        //    ViewBag.CityList = _dbService.GetCities();
        //    return View();
        //}

        [HttpGet]
        public JsonResult GetZipByCity(int cityCode)
        {
            var city = _dbService.GetCities().FirstOrDefault(x => x.CityCode == cityCode);
            return Json(city?.ZipCode ?? "");
        }

        [HttpGet]
        public JsonResult GetCityByZip(string zip)
        {
            var city = _dbService.GetCities().FirstOrDefault(x => x.ZipCode == zip);
            return Json(city != null ? new { city.CityCode, city.CityName } : null);
        }


        [HttpPost]
        public IActionResult SaveUser(UserModel userModel, IFormFile PdfDocument)
        {
            if (PdfDocument != null && PdfDocument.Length > 0)
            {
                using (var ms = new MemoryStream())
                {
                    PdfDocument.CopyTo(ms);
                    userModel.PdfDocument = Convert.ToBase64String(ms.ToArray());
                }
            }

            bool result = _dbService.UpdatePerson(userModel);

            // TODO: Save logic later (database or file)
            return Json(new { success = result, message = "User created successfully!" });
        }


    }
}
