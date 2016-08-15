using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AppInventory.Models;
using PagedList;
using AppInventory.CustomFilters;

namespace AppInventory.Controllers
{
    public class MachinesController : Controller
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        // GET: Machines
        public ActionResult Index(int? page, string sortOrder, string currentFilter, string searchString)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";

            if (searchString == null)
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            var results = db.Machines.Include(m => m.O);

            if (!String.IsNullOrEmpty(searchString))
                results = results.Where(a => a.Name.Contains(searchString) || a.Manufacturer.Contains(searchString) || a.Model.Contains(searchString) || a.SerialNumber.Contains(searchString) 
                    || a.AssetTagNumber.Contains(searchString) || (a.UserMachines.FirstOrDefault() != null && a.UserMachines.FirstOrDefault().UserInfo.UserName.Contains(searchString)));

            switch (sortOrder)
            {
                case "name":
                    results = results.OrderBy(a => a.Name);
                    break;
                case "name_desc":
                    results = results.OrderByDescending(a => a.Name);
                    break;
                default:
                    results = results.OrderBy(a => a.Name);
                    break;
            }

            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(results.ToPagedList(pageNumber, pageSize));
        }

        // GET: Machines/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Machine machine = db.Machines.Find(id);
            if (machine == null)
            {
                return HttpNotFound();
            }
            return View(machine);
        }

       
        // GET: Machines/Edit/5
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Edit(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Machine machine = db.Machines.Find(id);
            if (machine == null)
            {
                return HttpNotFound();
            }
            ViewBag.OSID = new SelectList(db.OS, "OSID", "Name", machine.OSID);
            return View(machine);
        }

        // POST: Machines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Edit([Bind(Include = "MachineID,Name,DistinguishedName,Manufacturer,Model,SerialNumber,AssetTagNumber,Type,OSID,RowVersion")] Machine machine)
        {
            if (ModelState.IsValid)
            {
                var excluded = new[] { "Manufacturer", "Version", "ScopeID", "Deleted", "DeletedOn" };

                db.Entry(machine).State = EntityState.Modified;
                machine.ModDt = DateTime.Now;
                machine.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.OSID = new SelectList(db.OS, "OSID", "Name", machine.OSID);
            return View(machine);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
