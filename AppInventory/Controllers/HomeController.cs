using AppInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppInventory.Controllers
{
    public class HomeController : Controller
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "BlackRock Application Inventory";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Desktop Engineering";

            return View();
        }
    }
}