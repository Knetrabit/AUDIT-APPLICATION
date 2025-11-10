using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using VrsAuditApplication.Models;
using System.Configuration;
using CommonComponent;

namespace VrsAuditApplication.Controllers
{
    public class HomeController : Controller
    {
        private readonly DbService _dbService;

        public HomeController(DbService dbService)
        {
            this._dbService = dbService;
        }

        public IActionResult Index()
        {
            //_dbService.FetchData();
            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        // ✅ Login Form Submission
        [HttpPost]
        public IActionResult Login(string username, string password)
        {

            // Hardcoded validation
            //if (username == "38000066" && password == "1234")
            //{
               
            //    HttpContext.Session.SetString("User", username);
            //    return RedirectToAction("Index", "FastTag");             
            //}

            bool isVerified = VerifyUserCredentials(username, password);
            if (isVerified)
            {
                HttpContext.Session.SetString("User", username);
                return RedirectToAction("Index", "FastTag");
            }

            ViewBag.Message = "Invalid username or password.";
            return View("Index");
        }

 
        public Boolean VerifyUserCredentials(string username, string password)
        {
             SystemUser user = _dbService.GetUserCredential(username);
             string userPass = Decryption_Data(user.Password, user.SecurityKey);

             if(userPass != password)
             {
                 ViewBag.Message = "Invalid username or password.";
                 return false;
            }   
            return true;
        }

        public string Decryption_Data(string strPWD, string strKey)
        {

            string strDecryptedString;
            Encryption.Data objDecryptedData;
            Encryption.Symmetric objSym = new Encryption.Symmetric(Encryption.Symmetric.Provider.TripleDES, true);
            Encryption.Data objkey = new Encryption.Data();
            Encryption.Data objPWD = new Encryption.Data();
            objPWD.Base64 = strPWD;
            objkey.Base64 = strKey;
            objDecryptedData = objSym.Decrypt(objPWD, objkey);
            strDecryptedString = objDecryptedData.ToString();
            return strDecryptedString;

            throw new Exception("The method or operation is not implemented.");
        }

        [HttpPost]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            var test = HttpContext.Session.GetString("User"); // Should be null
            return RedirectToAction("Index", "Home");
        }


    }
}
