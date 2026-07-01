using Application.Interfaces;
using Application.Services;
using Infrastructure.Repositories;
using Shared.Common;
using Shared.DTOs;
using System;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace EMMS.Controllers
{
    [RoutePrefix("")]
    public class AuthController : Controller
    {
        private readonly IUserService _userService;

        // Dorian update logic Session Timeout
        private const string LongSessionUserId = "TV0001"; // User login for TV display
        private const int LongSessionTimeoutMinutes = 525600; // 365 days

        public AuthController()
        {
            _userService = new UserService(new UserRepository());
        }


        [HttpGet]
        [Route("login")]
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
                var userId = (result.Data.UserID ?? string.Empty).Trim();

                if (string.Equals(userId, LongSessionUserId, StringComparison.OrdinalIgnoreCase))
                {
                    Session.Timeout = LongSessionTimeoutMinutes;
                }

                Session["UserId"] = result.Data.UserID;
                Session["UserName"] = result.Data.UserName;
                Session["Role"] = result.Data.Role;
                Session["CurrentRole"] = result.Data.CurrentRole;

                return Json(ApiResponseNew<LoginResultDto>.Ok(result.Data, result.Message));
            }

            return Json(ApiResponseNew<string>.Fail(result.Message, result.ErrorCode));
        }

        public ActionResult Logout()
        {
            Session.Clear();
            Session.Abandon();

            if (Request.Cookies["ASP.NET_SessionId"] != null)
            {
                var sessionCookie = new HttpCookie("ASP.NET_SessionId", string.Empty)
                {
                    Expires = DateTime.Now.AddDays(-1)
                };

                Response.Cookies.Add(sessionCookie);
            }

            return RedirectToAction("Index", "Auth");
        }

        public ActionResult Register()
        {
            return View();
        }

        [HttpGet]
        [Route("change-password")]
        public ActionResult ChangePasssword()
        {
            return View("ChangePassword");
        }

        [HttpPost]
        public async Task<ActionResult> SaveChangePasssword(ChangePasswordDto model)
        {
            var result = await _userService.ChangePasswordAsync(model);

            if (!result.Success)
            {
                return Json(ApiResponseNew<string>.Fail(result.Message, result.ErrorCode));
            }

            return Json(ApiResponseNew<string>.Ok(result.Message, result.ErrorCode));
        }
    }
}
