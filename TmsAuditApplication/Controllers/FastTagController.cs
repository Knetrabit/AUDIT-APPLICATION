using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using VrsAuditApplication.Models;


namespace VrsAuditApplication.Controllers
{
    public class FastTagController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly DbService _dbService;
        private readonly MediaHelper _mediaHelper;

        public FastTagController(IConfiguration configuration, DbService dbService, MediaHelper mediaHelper)
        {
            _configuration = configuration;
            _dbService = dbService;
            _mediaHelper = mediaHelper;
        }

        public IActionResult Index()
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Index", "Home"); 

            return View(); 
        }


        [HttpGet]
        public IActionResult Index(DateTime? fromDate, DateTime? toDate, string laneType, string tagClass, bool isManual, bool isViolation = false)
        {
            var user = HttpContext.Session.GetString("User");
            if (string.IsNullOrEmpty(user))
                return RedirectToAction("Index", "Home");
            
            List<object> transactions = _dbService.GetFastTagTransactionData(fromDate, toDate, laneType, tagClass, isManual, isViolation);
            return View(transactions);

        }

        [HttpGet]
        public JsonResult GetMedia(string transactionNumber, string shiftNumber, string paymentMeansType)
        {
            var media = _mediaHelper.ShowAllMedia(transactionNumber, shiftNumber);
            return Json(media);
        }


        [HttpGet]
        public ActionResult AuditorAccept(string transactionId, string isManual, int avcClassId)
        {
            // Example: process the data
            // You can parse isManual to bool if needed
            bool manualFlag = string.Equals(isManual, "Yes", StringComparison.OrdinalIgnoreCase);

            _dbService.AuditorAcceptUpdate(transactionId, manualFlag, avcClassId);

            return RedirectToAction("Index");
        }

        // 🔹 1) GET — Show popup if manual
        [HttpGet]
        public ActionResult AuditorRejectPopup(string transactionId, string tagClass)
        {
            // Trigger the JS popup in view
            ViewBag.TransactionId = transactionId;
            ViewBag.TagClass = tagClass;
            return View("RejectPopup"); // View contains popup UI
        }

        // 🔹 2) POST — Process rejection (called from popup OK or direct case)
        [HttpGet]
        public ActionResult AuditorRejectProcess(string transactionId, string isMannual, int avcClassId)
        {
            bool manualFlag = string.Equals(isMannual, "Yes", StringComparison.OrdinalIgnoreCase);

            _dbService.AuditorRejectUpdate(transactionId, manualFlag, avcClassId);

            return RedirectToAction("Index");
        }

        // 🔹 3) Direct reject (non-manual case)
        [HttpGet]
        public ActionResult AuditorRejectDirect(string transactionId, int avcClassId)
        {
            _dbService.AuditorRejectUpdate(transactionId, true, avcClassId);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult SIAccept(string transactionId, string isManual, string bankClassId)
        {
            bool manualFlag = string.Equals(isManual, "Yes", StringComparison.OrdinalIgnoreCase);

            //_dbService.AuditorAcceptUpdate(transactionId, manualFlag, bankClassId);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult SIReject(string transactionId, string isManual, string bankClassId)
        {
            bool manualFlag = string.Equals(isManual, "Yes", StringComparison.OrdinalIgnoreCase);

            //_dbService.AuditorAcceptUpdate(transactionId, manualFlag, bankClassId);

            return RedirectToAction("Index");
        }



    }

}
