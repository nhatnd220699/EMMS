using System;
using System.Web;
using System.Web.Mvc;

public class LanguageController : Controller
{
    public ActionResult Change(string lang)
    {
        if (!string.IsNullOrEmpty(lang))
        {
            // Set the culture in a cookie
            HttpCookie cookie = new HttpCookie("lang");
            cookie.Value = lang;
            cookie.Expires = DateTime.Now.AddYears(1);
            Response.Cookies.Add(cookie);
        }

        return Redirect(Request.UrlReferrer.ToString());
    }
}