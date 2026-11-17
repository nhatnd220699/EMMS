using EMMS.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;
using System.Web.SessionState;

namespace EMMS
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        //protected void Application_PostAuthorizeRequest()
        //{
        //    var path = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

        //    if (path.Equals("~/api", StringComparison.OrdinalIgnoreCase)
        //        || path.StartsWith("~/api/", StringComparison.OrdinalIgnoreCase))
        //    {
        //        HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
        //    }
        //}

        protected void Application_BeginRequest()
        {
            string lang = "en-US"; // default

            HttpCookie cookie = HttpContext.Current.Request.Cookies["lang"];

            if (cookie != null && !string.IsNullOrEmpty(cookie.Value))
            {
                lang = cookie.Value;
            }

            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lang);

            System.Threading.Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang);
        }


        protected void Application_Error()
        {
            var ex = Server.GetLastError();

            if (ex is HttpException httpEx &&
                httpEx.GetHttpCode() == 404)
            {
                // API
                if (Request.Path.StartsWith("/api/",
                    StringComparison.OrdinalIgnoreCase))
                {
                    Server.ClearError();

                    Response.Clear();
                    Response.StatusCode = 404;
                    Response.TrySkipIisCustomErrors = true;

                    return;
                }

                // MVC
                Response.Clear();
                Server.ClearError();

                Response.StatusCode = 404;

                Server.TransferRequest("~/Error/NotFound");
            }
        }

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];

            if (authCookie == null)
                return;

            try
            {
                var authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                if (authTicket == null || authTicket.Expired)
                    return;

                var userInfo = JsonConvert.DeserializeObject<UserInfo>(
                    authTicket.UserData
                );

                if (userInfo == null)
                    return;

                var roles = string.IsNullOrWhiteSpace(userInfo.Role)
                   ? new string[0]
                   : new[] { userInfo.Role };

                var identity = new FormsIdentity(authTicket);

                var principal = new GenericPrincipal(identity, roles);

                HttpContext.Current.User = principal;
                Thread.CurrentPrincipal = principal;
            }
            catch
            {
                FormsAuthentication.SignOut();
            }
        }
    }
}