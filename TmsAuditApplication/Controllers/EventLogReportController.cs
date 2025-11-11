using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VrsAuditApplication.Controllers
{
    public class EventLogReportController : Controller
    {
        // GET: EventLogReportController
        public ActionResult Index()
        {
            return View();
        }

        // GET: EventLogReportController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: EventLogReportController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: EventLogReportController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EventLogReportController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: EventLogReportController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: EventLogReportController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: EventLogReportController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
