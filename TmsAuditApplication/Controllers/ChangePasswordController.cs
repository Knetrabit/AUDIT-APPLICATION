using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace VrsAuditApplication.Controllers
{
    public class ChangePasswordController : Controller
    {
        // GET: ChangePasswordController
        public ActionResult Index()
        {
            return View();
        }

        // GET: ChangePasswordController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ChangePasswordController/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: ChangePasswordController/Create
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

        // GET: ChangePasswordController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ChangePasswordController/Edit/5
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

        // GET: ChangePasswordController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ChangePasswordController/Delete/5
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
