using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using AppInventory.Models;
using System.Net;
using System.Data.Entity;
using AppInventory.CustomFilters;

namespace AppInventory.Controllers
{
    public class AppController : Controller
    {
        // GET: App
        private AppInventoryEntities db = new AppInventoryEntities();

        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            App app = db.Apps.Find(id);

            if (app == null)
            {
                return HttpNotFound();
            }

            return View(app);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Edit([Bind(Include = "AppID, Name,Description,Language,BeastID,AppDeskID,TechnicalOwner,BusinessOwner,PrimaryTester,SecondaryTester,LicenseRequired, LicenseOwner,ModDt, Rowversion")] App app)
        {
            if (ModelState.IsValid)
            {
                var excluded = new[] { "Manufacturer", "Version", "ScopeID", "Deleted", "DeletedOn" };

                app.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                app.ModDt = DateTime.Now;

                var entry = db.Entry(app);

                entry.State = EntityState.Modified;
                foreach (var name in excluded)
                    entry.Property(name).IsModified = false;

                db.SaveChanges();

                AppCI appci = db.AppCIs.First(a => a.App.AppID == app.AppID);

                if (appci != null)
                    return RedirectToAction("Details", "AppCI", new { id = app.AppCIs.FirstOrDefault().AppCIID });
                else
                    return RedirectToAction("Index", "AppCI");
            }
            return View(app);
        }
    }
}