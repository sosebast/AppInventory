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
    public class UserInfoesController : Controller
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        // GET: UserInfoes
        public ActionResult Index(int? page, string sortOrder, string currentFilter, string searchString, int? deploymentID, string dept, string deptFilter)
        {
            
            ViewBag.CurrentSort = sortOrder;
            ViewBag.DeptSortParm = String.IsNullOrEmpty(sortOrder) ? "login_desc" : "";
            ViewBag.LoginIDSortParm = sortOrder == "dept" ? "dept_desc" : "dept";
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.CurrentDeploymentID = deploymentID;
            ViewBag.CurrentDeptFilter = deptFilter;
            ViewBag.CurrentDept = dept;
            
            if (searchString == null)
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;


            IEnumerable<UserInfo> results = null;
            if (deploymentID != null)
            {
                DeploymentBatch batch = db.DeploymentBatches.Find(deploymentID);
                ViewBag.CurrentDep = batch.Name;

                if (!string.IsNullOrEmpty(dept))
                    results = batch.DeploymentBatchUsers.Where(a => a.UserInfo.Dept == dept).Select(u => u.UserInfo).Distinct();
                else
                    results = batch.DeploymentBatchUsers.Select(u => u.UserInfo).Distinct();
            }
            else
                results = db.UserInfoes.Where(a => (a.Dept != null && a.Dept != "NULL") || a.UserOwnerships.Count != 0);

            if (!string.IsNullOrEmpty(deptFilter))
                if (deptFilter.Contains(','))
                    foreach (string item in deptFilter.Split(new char[] { ',' }))
                        if (results == null)
                            results = results.Where(a => a.Dept.StartsWith(item));
                        else
                            results = results.Union(results.Where(a => a.Dept.StartsWith(item)));
                else
                    results = results.Where(a => a.Dept.StartsWith(deptFilter));

            if (!String.IsNullOrEmpty(searchString))
                results = results.Where(a => a.FullName.Contains(searchString) || a.Dept.Contains(searchString) || a.UserName.Contains(searchString));



            switch (sortOrder)
            {
                case "login_desc":
                    results = results.OrderByDescending(a => a.UserName);
                    break;
                case "dept":
                    results = results.OrderBy(a => a.Dept);
                    break;
                case "dept_desc":
                    results = results.OrderByDescending(a => a.Dept);
                    break;
                case "name":
                    results = results.OrderBy(a => a.FullName);
                    break;
                case "name_desc":
                    results = results.OrderByDescending(a => a.FullName);
                    break;
                default:
                    results = results.OrderBy(a => a.UserName);
                    break;
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            return View(results.ToList().ToPagedList(pageNumber, pageSize));
        }

        // GET: UserInfoes/Details/5
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            UserInfo userInfo = db.UserInfoes.Find(id);
            ViewBag.TotalAppCount = db.vUserAppDtls.Where(a => a.UserID == id && !(a.AppCI.Deleted.HasValue && a.AppCI.Deleted.Value)).Select(a => a.AppCI).ToList().Count();
            ViewBag.RTGAppCount = db.vUserAppDtls.Where(a => a.UserID == id && !(a.AppCI.Deleted.HasValue && a.AppCI.Deleted.Value) && a.AppCI.StatusID != null && a.AppCI.AppStatu.Status == "Complete").Select(a => a.AppCI).ToList().Count();
            if (ViewBag.TotalAppCount != 0)
                ViewBag.RTG = ViewBag.RTGAppCount * 100 / ViewBag.TotalAppCount;
            else
                ViewBag.RTG = 0;

            if (userInfo == null)
            {
                return HttpNotFound();
            }
            return View(userInfo);
        }

        //// GET: UserInfoes/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: UserInfoes/Create
        //// To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        //// more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create([Bind(Include = "UserID,UserName,DistinguishedName,FullName,Title,EmployeeID,Dept,CostCenter,Email,Address,City,State,ZipCode,Country,Region,ManagerUserID,OwnerUserID,Migrated,Deleted,DeletedOn,ModDt,RowVersion,ModUser")] UserInfo userInfo)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        db.UserInfoes.Add(userInfo);
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }

        //    return View(userInfo);
        //}

        // GET: UserInfoes/Edit/5
        //public ActionResult Edit(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    UserInfo userInfo = db.UserInfoes.Find(id);
        //    if (userInfo == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userInfo);
        //}

        // POST: UserInfoes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit([Bind(Include = "UserID,UserName,DistinguishedName,FullName,Title,EmployeeID,Dept,CostCenter,Email,Address,City,State,ZipCode,Country,Region")] UserInfo userInfo)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        userInfo.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
        //        userInfo.ModDt = DateTime.Now;

        //        db.Entry(userInfo).State = EntityState.Modified;
        //        db.SaveChanges();
        //        return RedirectToAction("Index");
        //    }
        //    return View(userInfo);
        //}

        //// GET: UserInfoes/Delete/5
        //public ActionResult Delete(string id)
        //{
        //    if (id == null)
        //    {
        //        return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
        //    }
        //    UserInfo userInfo = db.UserInfoes.Find(id);
        //    if (userInfo == null)
        //    {
        //        return HttpNotFound();
        //    }
        //    return View(userInfo);
        //}

        //// POST: UserInfoes/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public ActionResult DeleteConfirmed(string id)
        //{
        //    UserInfo userInfo = db.UserInfoes.Find(id);
        //    db.UserInfoes.Remove(userInfo);
        //    db.SaveChanges();
        //    return RedirectToAction("Index");
        //}

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
