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

namespace AppInventory.Controllers
{
    public class UserMachinesController : Controller
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        // GET: UserMachines
        public ActionResult Index(int? page, string sortOrder, string currentFilter, string searchString, string dept, string deptHL, string deptFilter)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentDept = dept;
            ViewBag.CurrentDeptHL = deptHL;
            ViewBag.CurrentDeptFilter = deptFilter;

            ViewBag.DeptSortParm = String.IsNullOrEmpty(sortOrder) ? "dept_desc" : "";
            ViewBag.LoginIDSortParm = sortOrder == "login" ? "login_desc" : "login";
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";

            if (searchString == null)
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;

            IEnumerable<UserMachine> results = null;
            if (string.IsNullOrEmpty(dept))
                if (string.IsNullOrEmpty(deptHL))
                    if (string.IsNullOrEmpty(deptFilter))
                        results = db.UserMachines;
                    else
                        if (deptFilter.Contains(','))
                            foreach (string item in deptFilter.Split(new char[] { ',' }))
                                if (results == null)
                                    results = db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(item) && a.UserInfo.DeptHL != null);
                                else
                                    results = results.Union(db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(item) && a.UserInfo.DeptHL != null));
                        else
                            results = db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(deptFilter) && a.UserInfo.DeptHL != null);
                else
                    if (string.IsNullOrEmpty(deptFilter))
                        results = db.UserMachines.Where(a => a.UserInfo.DeptHL == deptHL);
                    else
                        if (deptFilter.Contains(','))
                            foreach (string item in deptFilter.Split(new char[] { ',' }))
                                if (results == null)
                                    results = db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(item) && a.UserInfo.DeptHL == deptHL);
                                else
                                    results = results.Union(db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(item) && a.UserInfo.DeptHL == deptHL));
                        else
                            results = db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(deptFilter) && a.UserInfo.DeptHL == deptHL);
            else
                results = db.UserMachines.Where(a => a.UserInfo.Dept == dept);


            if (!String.IsNullOrEmpty(searchString))
                results = results.Where(a => a.Machine.Name.Contains(searchString) || a.UserInfo.FullName.Contains(searchString) || a.UserInfo.UserName.Contains(searchString));

            switch (sortOrder)
            {
                case "dept_desc":
                    results = results.OrderByDescending(a => a.UserInfo.Dept);
                    break;
                case "dept":
                    results = results.OrderBy(a => a.UserInfo.Dept);
                    break;
                case "login_desc":
                    results = results.OrderByDescending(a => a.UserInfo.UserName);
                    break;
                case "name":
                    results = results.OrderBy(a => a.UserInfo.FullName);
                    break;
                case "name_desc":
                    results = results.OrderByDescending(a => a.UserInfo.FullName);
                    break;
                default:
                    results = results.OrderBy(a => a.UserInfo.UserName);
                    break;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(results.ToPagedList(pageNumber, pageSize));
        }

        // GET: UserMachines/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserMachine userMachine = db.UserMachines.Find(id);
            if (userMachine == null)
            {
                return HttpNotFound();
            }
            return View(userMachine);
        }

        // GET: UserMachines/Create
        public ActionResult Create()
        {
            ViewBag.MachineID = new SelectList(db.Machines, "MachineID", "Name");
            ViewBag.UserID = new SelectList(db.UserInfoes, "UserID", "UserName");
            return View();
        }

        // POST: UserMachines/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "UserMachineID,UserID,MachineID,IsPrimaryMachine,ModDt,RowVersion,ModUser")] UserMachine userMachine)
        {
            if (ModelState.IsValid)
            {
                db.UserMachines.Add(userMachine);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.MachineID = new SelectList(db.Machines, "MachineID", "Name", userMachine.MachineID);
            ViewBag.UserID = new SelectList(db.UserInfoes, "UserID", "UserName", userMachine.UserID);
            return View(userMachine);
        }

        // GET: UserMachines/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserMachine userMachine = db.UserMachines.Find(id);
            if (userMachine == null)
            {
                return HttpNotFound();
            }
            ViewBag.MachineID = new SelectList(db.Machines, "MachineID", "Name", userMachine.MachineID);
            ViewBag.UserID = new SelectList(db.UserInfoes, "UserID", "UserName", userMachine.UserID);
            return View(userMachine);
        }

        // POST: UserMachines/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "UserMachineID,UserID,MachineID,IsPrimaryMachine,ModDt,RowVersion,ModUser")] UserMachine userMachine)
        {
            if (ModelState.IsValid)
            {
                db.Entry(userMachine).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.MachineID = new SelectList(db.Machines, "MachineID", "Name", userMachine.MachineID);
            ViewBag.UserID = new SelectList(db.UserInfoes, "UserID", "UserName", userMachine.UserID);
            return View(userMachine);
        }

        // GET: UserMachines/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserMachine userMachine = db.UserMachines.Find(id);
            if (userMachine == null)
            {
                return HttpNotFound();
            }
            return View(userMachine);
        }

        // POST: UserMachines/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            UserMachine userMachine = db.UserMachines.Find(id);
            db.UserMachines.Remove(userMachine);
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
