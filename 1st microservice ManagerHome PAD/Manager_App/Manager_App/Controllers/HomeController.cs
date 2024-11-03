using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;

namespace Manager_App.Controllers
{
    //[Authorize]  
    public class HomeController : Controller
    {
        // Основная страница
        public IActionResult Index()
        {
            return RedirectToAction("ManagerHome");
        }

        public IActionResult ManagerHome()
        {
            return View(); 
        }
    }
}
