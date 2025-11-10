using Microsoft.AspNetCore.Mvc;
using VrsAuditApplication.Models;

namespace VrsAuditApplication.Controllers
{
    public class PendingCashupController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly DbService _dbService;

        public PendingCashupController(IConfiguration configuration, DbService dbService)
        {
            _configuration = configuration;
            _dbService = dbService;
        }

        [HttpGet]
        public IActionResult Index(DateTime? operationDay, string shift, string userId)
        {
            // Fetch data from DB service
            List<PendingCashupDetail> pendingCashupDetails = _dbService.GetPendingCashupDetails();

            // Optional filtering
            if (operationDay.HasValue)
            {
                pendingCashupDetails = pendingCashupDetails
                    .Where(x => x.OperationDay.Date == operationDay.Value.Date)
                    .ToList();
            }
            if (!string.IsNullOrEmpty(shift))
            {
                pendingCashupDetails = pendingCashupDetails
                    .Where(x => x.Shift.Equals(shift, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
            if (!string.IsNullOrEmpty(userId))
            {
                pendingCashupDetails = pendingCashupDetails
                    .Where(x => x.UserID == userId || x.UserName == userId)
                    .ToList();
            }

            return View(pendingCashupDetails);
        }


    }
}
