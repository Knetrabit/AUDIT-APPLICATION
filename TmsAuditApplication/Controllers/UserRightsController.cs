using Microsoft.AspNetCore.Mvc;

namespace VrsAuditApplication.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using System.Linq;
    using VrsAuditApplication.Models;

    public class UserRightsController : Controller
    {

        private readonly DbService _dbService;
        public UserRightsController(DbService dbService)
        {
            _dbService = dbService;
        }


        public IActionResult Index()
        {
            var model = new AssignRightsViewModel
            {
                JobPositions = _dbService.GetJobPositions(),
                AssignedRights = _dbService.GetAllRights().Where(r => r.Id <= 5).ToList(),
                UnassignedRights = _dbService.GetAllRights().Where(r => r.Id > 5).ToList()
            };
            return View(model);
        }


        [HttpGet]
        public JsonResult GetRightsByPosition(int jobPositionId)
        {
            var rights = _dbService.GetAllRights();

            // Example: Only rights with even IDs assigned for demo
            var assigned = rights.Where(r => r.Id % 2 == 0).ToList();
            var unassigned = rights.Where(r => r.Id % 2 != 0).ToList();

            return Json(new { assigned, unassigned });
        }

        [HttpPost]
        public JsonResult SaveAssignedRights(int jobPositionId, List<int> rights)
        {
            if (jobPositionId == 0)
                return Json(new { success = false, message = "Job Position is required." });

            //// Remove all rights currently mapped to Job Position
            //var existingRights = _context.JobPositionRights
            //                            .Where(x => x.JobPositionId == jobPositionId)
            //                            .ToList();

            //_context.JobPositionRights.RemoveRange(existingRights);

            //// Add new rights mapping
            //foreach (var rightId in rights)
            //{
            //    _context.JobPositionRights.Add(new JobPositionRight
            //    {
            //        JobPositionId = jobPositionId,
            //        RightId = rightId
            //    });
            //}

            //_context.SaveChanges();

            return Json(new { success = true, message = "Rights assigned successfully." });
        }


    }

}
