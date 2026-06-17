using System.Web.Mvc;

namespace EMMS.Controllers
{
    public class HomeController : Controller
    {
        private bool IsLoggedIn()
        {
            return Session["UserId"] != null;
        }

        public ActionResult Index()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Auth");

            return View();
        }

        public ActionResult About()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Auth");

            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            if (!IsLoggedIn())
                return RedirectToAction("Index", "Auth");

            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}
