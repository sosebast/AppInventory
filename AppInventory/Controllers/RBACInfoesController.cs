using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using AppInventory.Models;
using AppInventory.CustomFilters;

namespace AppInventory.Controllers
{
    public class RBACInfoesController : Controller
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        // GET: RBACInfoes
        [AuthorizeRole(Roles = "Admin")]
        public ActionResult Index()
        {
            return View(db.RBACInfoes.ToList());
        }

        // GET: RBACInfoes/Create
        [AuthorizeRole(Roles = "Admin")]
        public ActionResult Create()
        {
            ViewBag.ADGroups = new MultiSelectList(db.Groups, "GroupID", "Name");
            return View();
        }

        // POST: RBACInfoes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "RoleID,Name,Description,Groups")] RBACInfoGroup rBACInfoGroup)
        {
            if (ModelState.IsValid)
            {
                RBACInfo rBACInfo = new RBACInfo();
                db.RBACInfoes.Attach(rBACInfo);
                var rBACInfoEntry = db.Entry(rBACInfo);
                rBACInfoEntry.CurrentValues.SetValues(rBACInfoGroup);

                rBACInfo.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                rBACInfo.ModDt = DateTime.Now;

                foreach (var item in rBACInfoGroup.Groups)
                {
                    rBACInfo.RBACGroups.Add(new RBACGroup() { GroupID = item, RoleID = rBACInfo.RoleID, ModUser = rBACInfo.ModUser, ModDt = rBACInfo.ModDt });
                }

                db.RBACInfoes.Add(rBACInfo);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(rBACInfoGroup);
        }

        // GET: RBACInfoes/Edit/5
        [AuthorizeRole(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RBACInfo rBACInfo = db.RBACInfoes.Find(id);

            if (rBACInfo == null)
            {
                return HttpNotFound();
            }

            ViewBag.ADGroups = new MultiSelectList(db.Groups, "GroupID", "Name");
            RBACInfoGroup rbacWithGruops = new RBACInfoGroup(rBACInfo);
            return View(rbacWithGruops);
        }

        // POST: RBACInfoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "RoleID,Name,Description,Groups,ModDt,RowVersion,ModUser")] RBACInfoGroup rBACInfoGroup)
        {
            if (ModelState.IsValid)
            {
                RBACInfo rBACInfo = db.RBACInfoes.Where(s => s.RoleID == rBACInfoGroup.RoleID).Include(p => p.RBACGroups).FirstOrDefault();
                
                var rBACInfoEntry = db.Entry(rBACInfo);

                rBACInfoGroup.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                rBACInfoGroup.ModDt = DateTime.Now;
                
                rBACInfoEntry.CurrentValues.SetValues(rBACInfoGroup);
                

               
                foreach (var item in rBACInfo.RBACGroups.Where(s => !rBACInfoGroup.Groups.Contains(s.GroupID)).ToList())
                    db.RBACGroups.Remove(item);

                var existingItems = rBACInfo.RBACGroups.Select(s => s.GroupID);

                foreach (var item in rBACInfoGroup.Groups.Where(s => !existingItems.Contains(s)))
                {
                    rBACInfo.RBACGroups.Add(new RBACGroup() { GroupID = item, RoleID = rBACInfoGroup.RoleID, ModUser = rBACInfoGroup.ModUser, ModDt = rBACInfoGroup.ModDt });
                }

                db.Entry(rBACInfo).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(rBACInfoGroup);
        }

        // GET: RBACInfoes/Delete/5
        [AuthorizeRole(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            RBACInfo rBACInfo = db.RBACInfoes.Find(id);
            if (rBACInfo == null)
            {
                return HttpNotFound();
            }
            return View(rBACInfo);
        }

        // POST: RBACInfoes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            RBACInfo rBACInfo = db.RBACInfoes.Find(id);
            foreach (var item in rBACInfo.RBACGroups.ToList())
                db.RBACGroups.Remove(item);
            
            db.RBACInfoes.Remove(rBACInfo);
            db.SaveChanges();
            return RedirectToAction("Index");
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
