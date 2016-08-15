using AppInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace AppInventory.Controllers
{
    public class ReportsController : Controller
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        public ActionResult DeptAppDrillDown()
        {
            return View();
        }

        public ActionResult DeptUserDrillDown()
        {
            return View();
        }
    }
}