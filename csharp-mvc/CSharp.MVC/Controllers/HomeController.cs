using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CSharp.MVC.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewData["header"] = "Not signed in";
            ViewData["text"] = "Click the button below to sign in.";

            if (Session["signed_in"] != null) {
                ViewData["header"] = "Welcome!";
                ViewData["text"] = String.Format("Signed in as {0}.", Session["email"]);
            }

            return View();
        }
    }
}
