using Microsoft.AspNetCore.Mvc;

namespace VrsAuditApplication.Controllers
{
    public class CashupReportController : Controller
    {

        private readonly IConfiguration _configuration;
        private readonly DbService _dbService;

        public CashupReportController(IConfiguration configuration, DbService dbService)
        {
            _configuration = configuration;
            _dbService = dbService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult CashupConsolidatedReport(string operationDay, string shift, string tcid)
        {
            if (string.IsNullOrEmpty(operationDay))
                operationDay = DateTime.Now.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(shift))
                shift = "All";

            if (string.IsNullOrEmpty(tcid))
                tcid = "All";

            var data = _dbService.GetCashupConsolidatedReport(operationDay, shift, tcid);
            return View("Index", data);

        }

    }
}
