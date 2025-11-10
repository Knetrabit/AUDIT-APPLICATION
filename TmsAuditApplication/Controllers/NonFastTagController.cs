

using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using VrsAuditApplication.Models;

namespace VrsAuditApplication.Controllers
{
    public class NonFastTagController : Controller
    {
        private readonly DbService _dbService;
        private readonly MediaHelper _mediaHelper;

        public NonFastTagController(DbService dbService, MediaHelper mediaHelper)
        {
            _dbService = dbService;
            _mediaHelper = mediaHelper;
        }


        public IActionResult Index()
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Index", "Home"); // Should go here if session is cleared

            return View(); 
        }

        [HttpGet]
        public IActionResult Index(DateTime? fromDate, DateTime? toDate, int lane, string shiftType, string category, string user, string subCat)
        {
            var user1 = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user1))
                return RedirectToAction("Index", "Home"); // Should go here if session is cleared

            // Replace this sample data with your DB logic
            List<NonFastTagTransaction> transactions = _dbService.GetNonFastTagTransactionData(category, subCat, 0, 0, fromDate, toDate, user, lane, shiftType);

            return View(transactions); // ✅ Pass to view
        }


        [HttpGet]
        public JsonResult GetMedia(string transactionNumber, string shiftNumber, string paymentMeansType)
        {
            var media = _mediaHelper.ShowAllMedia(transactionNumber, shiftNumber);
            return Json(media);
        }


        [HttpGet]
        public JsonResult AcceptTC_NonFastTag(int plazaID,
                                      string transactionNumber,
                                      string shiftNumber,
                                      string validator,
                                      string exemptionID,
                                      string tccClass,
                                      string avcClass,
                                      string validatorClass,
                                      string fareID,
                                      string paymentMeansType,
                                      string shiftvalidationMode,
                                      string remark)
        {
            var user1 = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user1))
                return Json(new { success = false, message = "Session expired. Please log in again." });

            // Validate remarks for certain conditions
            if ((paymentMeansType == "4" || paymentMeansType == "5" ||
                 string.Equals(shiftvalidationMode, "True", StringComparison.OrdinalIgnoreCase))
                && string.IsNullOrWhiteSpace(remark))
            {
                return Json(new { success = false, message = "Please enter a remark." });
            }

            try
            {
                _dbService.UpdateAcceptTC_NonFastTag(plazaID, transactionNumber, shiftNumber, validator,
                                                     exemptionID, tccClass, avcClass, validatorClass,
                                                     fareID, paymentMeansType, shiftvalidationMode, remark);

                return Json(new { success = true, message = "Transaction TC accepted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpGet]
        public JsonResult Audit_NonFastTag(
                                    int plazaID,
                                    string transactionNumber,
                                    string shiftNumber,
                                    string exemptionID,
                                    string tccClass,
                                    string avcClass,
                                    string validatorClass,
                                    string fareID,
                                    string paymentMeansType,
                                    string shiftvalidationMode,
                                    string subClass,
                                    string violationType,
                                    string remark)
        {
            var user1 = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user1))
                return Json(new { success = false, message = "Session expired. Please log in again." });

            // Perform audit checks before update
            var auditResult = Audit_Check(paymentMeansType, avcClass, exemptionID, tccClass, shiftvalidationMode, remark);
            if (!auditResult.success)
                return Json(auditResult);

            try
            {
                _dbService.UpdateAudit_NonFastTag(
                    plazaID, transactionNumber, shiftNumber, exemptionID, tccClass, avcClass, validatorClass,
                    fareID, paymentMeansType, shiftvalidationMode, subClass, violationType, remark);

                return Json(new { success = true, message = "Audit successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        private static (bool success, string message) Audit_Check(
                                                string paymentMeansType,
                                                string avcClass,
                                                string exemptionID,
                                                string tcClass,
                                                string shiftValidationMode,
                                                string remark)
        {
            // Mandatory remark check
            if ((paymentMeansType == "4" || paymentMeansType == "5" ||
                string.Equals(shiftValidationMode, "True", StringComparison.OrdinalIgnoreCase)) &&
                string.IsNullOrWhiteSpace(remark))
            {
                return (false, "Please enter a remark.");
            }

            // Convoy exemption check
            if (paymentMeansType == "4" && exemptionID == "1")
            {
                if (avcClass != tcClass)
                    return (false, "Convoy not accepted: AVC class mismatch with TC class.");
            }

            // Local commercial check
            if (paymentMeansType == "16" && avcClass != tcClass)
                return (false, "Local Commercial not accepted: AVC class mismatch with TC class.");

            // No issues → return success
            return (true, "Audit check passed.");
        }


        [HttpGet]
        public JsonResult VoilationAudit_NonFastTag(
                                   int plazaID,
                                    string transactionNumber,
                                    string shiftNumber,
                                    string plateNumber,
                                    string vehicleClass,
                                    string shiftvalidationMode,
                                    string subClass,
                                    string violationType,
                                    string remark)
        {
            try
            {
                _dbService.UpdateViolationTxnAudit_NonFastTag(
                    plazaID, transactionNumber, shiftNumber, plateNumber,vehicleClass, 
                    shiftvalidationMode, subClass, violationType, remark);

                return Json(new { success = true, message = "Audit successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


        [HttpGet]
        public JsonResult AcceptAVC_NonFastTag(int plazaID,
                                    string transactionNumber,
                                    string shiftNumber,
                                    string exemptionID,
                                    string tccClass,
                                    string avcClass,
                                    string validatorClass,
                                    string fareID,
                                    string paymentMeansType,
                                    string shiftvalidationMode,
                                    string subClass,
                                    string violationType,
                                    string remark)
        {
            var user1 = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user1))
                return Json(new { success = false, message = "Session expired. Please log in again." });

            // Validate remarks for certain conditions
            if ((paymentMeansType == "4" || paymentMeansType == "5" ||
                 string.Equals(shiftvalidationMode, "True", StringComparison.OrdinalIgnoreCase))
                && string.IsNullOrWhiteSpace(remark))
            {
                return Json(new { success = false, message = "Please enter a remark." });
            }

            try
            {
                _dbService.AvcAudit_NonFastTag(
                   plazaID, transactionNumber, shiftNumber, exemptionID, tccClass, avcClass, validatorClass,
                   fareID, paymentMeansType, shiftvalidationMode, subClass, violationType, remark);

                return Json(new { success = true, message = "Transaction AVC accepted successfully." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Error: {ex.Message}" });
            }
        }


    }

}
