using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VrsAuditApplication.Controllers
{
    public class LaneMasterController : Controller
    {
        // GET: LaneMasterController
        public ActionResult Index()
        {
            return View();
        }

        // GET: LaneMasterController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: LaneMasterController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: LaneMasterController/Create
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

        // GET: LaneMasterController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: LaneMasterController/Edit/5
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

        // GET: LaneMasterController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: LaneMasterController/Delete/5
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
