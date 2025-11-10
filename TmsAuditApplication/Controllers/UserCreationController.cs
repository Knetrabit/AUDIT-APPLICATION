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
            return View();
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

            // TODO: Save logic later (database or file)
            return Json(new { success = true, message = "User created successfully!" });
        }


    }
}
