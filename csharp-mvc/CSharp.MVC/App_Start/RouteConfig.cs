using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CSharp.MVC
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "SignIn",
                url: "sign-in/{action}/{id}",
                defaults: new { controller = "Idp", action = "SignIn", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "SignOut",
                url: "sign-out/{action}/{id}",
                defaults: new { controller = "Idp", action = "SignOut", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}