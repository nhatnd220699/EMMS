using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
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

        protected void Application_PostAuthorizeRequest()
        {
            var path = HttpContext.Current.Request.AppRelativeCurrentExecutionFilePath;

            if (path.Equals("~/api", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("~/api/", StringComparison.OrdinalIgnoreCase))
            {
                HttpContext.Current.SetSessionStateBehavior(SessionStateBehavior.Required);
            }
        }

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
    }
}