using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using VrsAuditApplication.Models;

namespace VrsAuditApplication.Controllers
{
    public class CashupDeclarationController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly DbService _dbService;

        public CashupDeclarationController(IConfiguration configuration, DbService dbService)
        {
            _configuration = configuration;
            _dbService = dbService;
        }

        public IActionResult Index()
        {

            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Index", "Home");

            return View();
        }


        [HttpGet]
        public IActionResult DeclareCashup()
        {
            return View(); // Looks for Views/CashupDeclaration/DeclareCashup.cshtml
        }

        [HttpGet]
        public IActionResult GetShifts(string user, int bagNumber)
        {
            var shifts = _dbService.GetShiftDetails(user, bagNumber);
            return Json(shifts);
        }

        [HttpGet]
        public IActionResult GetPersonDetails(string userId)
        {
            var person = _dbService.GetPersonDetails(userId);
            return Json(person);
        }

        [HttpPost]
        public IActionResult Index(DeclareCashupModel model)
        {
            if (string.IsNullOrWhiteSpace(model.BagNumber))
                return Json(new { success = false, message = "Please enter bag number." });

            if (!Regex.IsMatch(model.BagNumber.Trim(), @"^\d+$"))
                return Json(new { success = false, message = "Only digits are allowed." });

            if (string.IsNullOrEmpty(model.SelectedShift))
                return Json(new { success = false, message = "Please select shift first." });

            var (success, message) = _dbService.ValidateCashup(model);
            return Json(new { success, message });
            //return Json(new { success = true, message = "Cashup declaration successful." });
        }
       

        [HttpPost]
        public IActionResult SaveCashupDeclaration([FromBody] CashupRequest request)
        {
            try
            {
                int result = _dbService.SaveCashupDeclaration(request.UserName, request.BagNumber, request.Shift, request.OperationDay, request.Cash, request.NEFT_CHEQUE_CARD_Amount);

                int totalCash = request.Cash.TotalAmount();
                int totalCoins = request.Cash.TotalCoinsAmount();
                return Ok(new { Success = result > 0, ResultCode = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        


    }
}
