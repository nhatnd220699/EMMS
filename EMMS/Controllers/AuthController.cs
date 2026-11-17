using Application.Interfaces;
using Application.Services;
using EMMS.Filters;
using EMMS.Models;
using Infrastructure.Repositories;
using Newtonsoft.Json;
using Shared.Common;
using Shared.DTOs;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

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


        //[HttpGet]
        //[Route("login")]
        //public ActionResult Index()
        //{
        //    if (Session["UserId"] != null)
        //        return RedirectToAction("Index", "Home");

        //    return View("Login");
        //}

        [HttpGet]
        [AllowAnonymous]
        [Route("login")]
        public ActionResult Index(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
                return RedirectToAction("Index", "Home");

            ViewBag.returnUrl = returnUrl;
            return View("Login");
        }

        //[HttpPost]
        //public async Task<ActionResult> SignIn(LoginRequestDto model)
        //{
        //    var result = await _userService.LoginAsync(model);

        //    if (result.Success && result.Data != null)
        //    {
        //        var userId = (result.Data.UserID ?? string.Empty).Trim();

        //        if (string.Equals(userId, LongSessionUserId, StringComparison.OrdinalIgnoreCase))
        //        {
        //            Session.Timeout = LongSessionTimeoutMinutes;
        //        }

        //        Session["UserId"] = result.Data.UserID;
        //        Session["UserName"] = result.Data.UserName;
        //        Session["Role"] = result.Data.Role;
        //        Session["CurrentRole"] = result.Data.CurrentRole;

        //        return Json(ApiResponseNew<LoginResultDto>.Ok(result.Data, result.Message));
        //    }

        //    return Json(ApiResponseNew<string>.Fail(result.Message, result.ErrorCode));
        //}

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(
            string type,
            string username,
            string password,
            string returnUrl)
        {

            try
            {
                LoginRequestDto model = new LoginRequestDto
                {
                    UserId = username,
                    Password = password,
                    Type = type
                };
                var result = await _userService.LoginAsync(model);
                if (result is null || !result.Success || result.Data is null)
                {
                    ViewBag.ReturnUrl = returnUrl;
                    ViewBag.Error = "The account information or password is incorrect.";
                    return View("Login");
                    //return Json(ApiResponseNew<string>.Fail(result.Message, result.ErrorCode));
                }

                var data = result?.Data;

                UserInfo userInfo = new UserInfo
                {
                    BU = data.BU,
                    CurrentRole = data.CurrentRole,
                    Customer = data.Customer,
                    Dept = data.Dept,
                    Email = data.Email,
                    Role = data.Role,
                    RoleId = data.RoleId,
                    Type = data.Type,
                    UserID = data.UserID,
                    UserName = data.UserName,
                };

                var userData = JsonConvert.SerializeObject(userInfo);

                var ticket = new FormsAuthenticationTicket(
                    1,
                    userInfo.UserName, // User.Identity.Name
                    DateTime.Now,
                    DateTime.Now.AddMinutes(FormsAuthentication.Timeout.TotalMinutes),
                    false,
                    userData);

                string encryptedTicket = FormsAuthentication.Encrypt(ticket);

                var cookie = new HttpCookie(
                    FormsAuthentication.FormsCookieName,
                    encryptedTicket)
                {
                    HttpOnly = true,
                    Secure = Request.IsSecureConnection,
                    Path = FormsAuthentication.FormsCookiePath
                };

                Response.Cookies.Add(cookie);

                if (!string.IsNullOrEmpty(returnUrl)
                    && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
                //return Json(ApiResponseNew<LoginResultDto>.Ok(result.Data, result.Message));
            }
            catch   
            {
                ViewBag.Error = "An unexpected error occurred. Please try again later.";
                return View("Login");
                //return Json(ApiResponseNew<string>.Fail("result.Message", "result.ErrorCode"));

            }
        }

        //public ActionResult Logout()
        //{
        //    Session.Clear();
        //    Session.Abandon();

        //    if (Request.Cookies["ASP.NET_SessionId"] != null)
        //    {
        //        var sessionCookie = new HttpCookie("ASP.NET_SessionId", string.Empty)
        //        {
        //            Expires = DateTime.Now.AddDays(-1)
        //        };

        //        Response.Cookies.Add(sessionCookie);
        //    }

        //    return RedirectToAction("Index", "Auth");
        //}

        [AppAuthorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();

            Session.Clear();
            Session.Abandon();

            var cookie = new HttpCookie(FormsAuthentication.FormsCookieName, "")
            {
                Expires = DateTime.Now.AddYears(-1)
            };

            Response.Cookies.Add(cookie);

            return RedirectToAction("Index", "Auth");
        }

        public ActionResult Register()
        {
            return View();
        }

        [ApiAuthorize]
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
