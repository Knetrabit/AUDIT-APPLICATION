using Microsoft.AspNetCore.Mvc;
using VrsAuditApplication.Models;

namespace VrsAuditApplication.Controllers
{
    public class FloatDeclarationController : Controller
    {

        private readonly DbService _dbService;

        public FloatDeclarationController(DbService dbService)
        {
            this._dbService = dbService;
        }

        public IActionResult Index()
        {
            var model = new FloatDeclarationViewModel
            {
                ActiveUserIds = _dbService.GetActiveUserIds()
            };

            return View(model);
        }     


        [HttpPost]
        public IActionResult AssignFloat(string userId, int bagNumber, double floatAmount, DateTime operationDay, string shift)
        {
            //bool success = _dbService.InsertFloatAmount(userId, bagNumber, floatAmount, operationDay, shift);
            bool success = true;

            if (success)
                return Json(new { success = true, message = "Float assign inserted successfully." });
            else
                return Json(new { success = false, message = "Failed to insert float assign." });
        }


        [HttpPost]
        public IActionResult AddWimTopupFloat(string userId, int bagNumber, double topupAmount, DateTime operationDay, string shift)
        {
            //bool success = _dbService.InsertFloatAmountTopup(userId, bagNumber, topupAmount, operationDay, shift, 1);
            bool success = true;

            if (success)
                return Json(new { success = true, message = "Float top-up inserted successfully." });
            else
                return Json(new { success = false, message = "Failed to insert float top-up." });
        }


        [HttpPost]
        public IActionResult FloatDetails(DateTime operationDay, string userName)
        {
            var details = _dbService.GetFloatDetails(operationDay, userName);
            return PartialView("_FloatDetailsTable", details);
        }


        [HttpPost]
        public IActionResult CancelFloat(string userId, int bagNumber)
        {
            //bool success = _dbService.UpdateCancelFloat(userId, bagNumber);
            bool success = true;

            if (success)
                TempData["Message"] = "Cancel float inserted successfully.";
            else
                TempData["Error"] = "Failed to update cancel float.";

            return RedirectToAction("Index");
        }


    }
}
