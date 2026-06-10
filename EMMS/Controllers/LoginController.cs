using System.Web.Mvc;

namespace EMMS.Controllers
{
    public class LoginController : Controller
    {
        public ActionResult Index()
        {
            return View("Login");
        }

        public ActionResult Register()
        {
            return View();
        }
    }
}