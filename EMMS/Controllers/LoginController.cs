using Application.Interfaces;
using Application.Services;
using Infrastructure.Repositories;
using Shared.Common;
using Shared.DTOs;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EMMS.Controllers
{
    public class LoginController : Controller
    {
        private readonly IUserService _userService;

        public LoginController()
        {
            _userService = new UserService(new UserRepository());
        }

        public ActionResult Index()
        {
            if (Session["UserId"] != null)
                return RedirectToAction("Index", "Home");

            return View("Login");
        }

        [HttpPost]
        public async Task<ActionResult> SignIn(LoginRequestDto model)
        {
            var result = await _userService.LoginAsync(model);

            if (result.Success && result.Data != null)
            {
                Session["UserId"] = result.Data.UserID;
                Session["UserName"] = result.Data.UserName;
                Session["Role"] = result.Data.Role;
                Session["CurrentRole"] = result.Data.CurrentRole;
            }

            return Json(result);
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.RemoveAll();
            Session.Abandon();

            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                var sessionCookie = new HttpCookie("ASP.NET_SessionId", string.Empty)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };

                Response.Cookies.Add(sessionCookie);
            }

            return RedirectToAction("Index", "Login");
        }

        public ActionResult Register()
        {
            return View();
        }
    }
}
