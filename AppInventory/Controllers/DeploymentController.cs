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
using PagedList;
using Newtonsoft.Json;

namespace AppInventory.Controllers
{
    public class DeploymentController : Controller
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        // GET: /Deployment/
        public ActionResult Index(int? page)
        {
            var results = db.DeploymentBatches.OrderByDescending(a => a.PlannedDt) ;

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            return View(results.ToList().ToPagedList(pageNumber, pageSize));
        }

        // GET: /Deployment/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeploymentBatch deploymentbatch = db.DeploymentBatches.Find(id);
            if (deploymentbatch == null)
            {
                return HttpNotFound();
            }

            var deptUsers = deploymentbatch.DeploymentBatchUsers.Select(b => b.UserID).ToList();
            var deptApps = db.vUserAppDtls.Where(a => deptUsers.Contains(a.UserInfo.UserID) && !(a.AppCI.Deleted.HasValue && a.AppCI.Deleted.Value)).Select(va => new { Dept = va.UserInfo.Dept, AppCI = va.AppCI }).Distinct().ToList();

            var results = deploymentbatch.DeploymentBatchUsers.Select(u => new { UserID = u.UserID, Dept = u.UserInfo.Dept, MachineID = u.Win7MachineID, Status = u.DeploymentStatu }).ToList();

            ViewBag.BatchDeptApps = results.GroupBy(u => u.Dept)
                .Select(d => new DeploymentBactchDeptSummary
                {
                    Dept = d.Key,
                    Count = d.Count(),
                    UserCount = d.Select(a => a.UserID).Distinct().Count(),
                    AppCount = deptApps.Where(v => v.Dept == d.Key).Count(),
                    RTG = deptApps.Where(v => v.Dept == d.Key && v.AppCI.StatusID != null && v.AppCI.AppStatu.Status == "Complete").Count() * 100 / deptApps.Where(v => v.Dept == d.Key).Count(),
                    PercentageComplete = d.Where(a => a.Status.Status == "Complete" || a.Status.Status == "Ignore").Count() * 100 / d.Count()
                }).OrderBy(u => u.Dept).ToList();

            ViewBag.TotalAppCount = deptApps.Select(a => a.AppCI).Distinct().Count();

            ViewBag.TotalRTG = deptApps.Where(a => a.AppCI.StatusID != null && a.AppCI.AppStatu.Status == "Complete").Select(a => a.AppCI.AppCIID).Distinct().Count() * 100 / ViewBag.TotalAppCount;

            ViewBag.TotalComplete = deploymentbatch.DeploymentBatchUsers.Where(a => a.DeploymentStatu.Status == "Complete" || a.DeploymentStatu.Status == "Ignore").Count() * 100 / deploymentbatch.DeploymentBatchUsers.Count();

            return View(deploymentbatch);
        }

        // GET: /Deployment/Create
        [AuthorizeRole(Roles = "Admin,Packager,Deployment")]
        public ActionResult Create()
        {
            return View();
        }

        // POST: /Deployment/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager,Deployment")]
        public ActionResult Create([Bind(Include = "DeploymentBatchID,Name,PlannedDt,DeploymentDt,Geo,Team")] DeploymentBatch deploymentbatch)
        {
            if (ModelState.IsValid)
            {
                deploymentbatch.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                deploymentbatch.ModDt = DateTime.Now;

                db.DeploymentBatches.Add(deploymentbatch);
                db.SaveChanges();
                return RedirectToAction("Edit/" + deploymentbatch.DeploymentBatchID);
            }

            return View(deploymentbatch);
        }

        // GET: /Deployment/Edit/5
        [AuthorizeRole(Roles = "Admin,Packager,Deployment")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeploymentBatch deploymentbatch = db.DeploymentBatches.Find(id);

            
            if (deploymentbatch == null)
            {
                return HttpNotFound();
            }

            var deptUsers = deploymentbatch.DeploymentBatchUsers.Select(b => b.UserID).ToList();
            var deptApps = db.vUserAppDtls.Where(a => deptUsers.Contains(a.UserInfo.UserID) && !(a.AppCI.Deleted.HasValue && a.AppCI.Deleted.Value)).Select(va => new { Dept = va.UserInfo.Dept, AppCIID = va.AppCIID }).Distinct().ToList();

            var results = deploymentbatch.DeploymentBatchUsers.Select(u => new { UserID = u.UserID, Dept = u.UserInfo.Dept, MachineID = u.Win7MachineID }).ToList();

            ViewBag.BatchDeptApps = results.GroupBy(u => u.Dept)
                .Select(d => new DeploymentBactchDeptSummary
                {
                    Dept = d.Key,
                    Count = d.Count(),
                    UserCount = d.Select(a => a.UserID).Distinct().Count(),
                    AppCount = deptApps.Where(v => v.Dept == d.Key).Count()
                }).OrderBy(u => u.Dept).ToList();

            ViewBag.TotalAppCount = deptApps.Select(a => a.AppCIID).Distinct().Count();

            return View(deploymentbatch);
        }

        // POST: /Deployment/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager,Deployment")]
        public ActionResult Edit([Bind(Include = "DeploymentBatchID,Name,PlannedDt,DeploymentDt,Geo,Team,RowVersion")] DeploymentBatch deploymentbatch)
        {
            if (ModelState.IsValid)
            {
                deploymentbatch.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                deploymentbatch.ModDt = DateTime.Now;

                db.Entry(deploymentbatch).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(deploymentbatch);
        }

        // GET: /Deployment/Delete/5
        [AuthorizeRole(Roles = "Admin,Packager,Deployment")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeploymentBatch deploymentbatch = db.DeploymentBatches.Find(id);
            if (deploymentbatch == null)
            {
                return HttpNotFound();
            }
            return View(deploymentbatch);
        }

        // POST: /Deployment/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager,Deployment")]
        public ActionResult DeleteConfirmed(int id)
        {
            DeploymentBatch deploymentbatch = db.DeploymentBatches.Find(id);
            db.DeploymentBatches.Remove(deploymentbatch);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Users(int? id, int? page, string sortOrder, string currentFilter, string searchString, string dept, string deptFilter)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            DeploymentBatch batch = db.DeploymentBatches.Find(id);

            IEnumerable<DeploymentBatchUser> results = batch.DeploymentBatchUsers;
            if (results == null)
            {
                return HttpNotFound();
            }


            ViewBag.CurrentSort = sortOrder;
            ViewBag.DeptSortParm = String.IsNullOrEmpty(sortOrder) ? "dept_desc" : "";
            ViewBag.LoginIDSortParm = sortOrder == "login" ? "login_desc" : "login";
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";
            ViewBag.CurrentDept = dept;
            ViewBag.CurrentDeptFilter = deptFilter;
            ViewBag.CurrentFilter = searchString;

            if (searchString == null)
                searchString = currentFilter;

            if (!String.IsNullOrEmpty(searchString))
                results = results.Where(a => a.UserInfo.FullName.Contains(searchString) || a.UserInfo.UserName.Contains(searchString) || a.Machine.Name.Contains(searchString));

            if (!String.IsNullOrEmpty(dept))
                results = results.Where(a => a.UserInfo.Dept == dept);
            else if(!string.IsNullOrEmpty(deptFilter))
                if (deptFilter.Contains(','))
                    foreach (string item in deptFilter.Split(new char[] { ',' }))
                        if (results == null)
                            results = results.Where(a => a.UserInfo.Dept.StartsWith(item) && a.UserInfo.DeptHL != null);
                        else
                            results = results.Union(results.Where(a => a.UserInfo.Dept.StartsWith(item) && a.UserInfo.DeptHL != null));
                else
                    results = results.Where(a => a.UserInfo.Dept.StartsWith(deptFilter) && a.UserInfo.DeptHL != null);

            

            
            switch (sortOrder)
            {
                case "dept":
                    results = results.OrderBy(a => a.UserInfo.Dept);
                    break;
                case "dept_desc":
                    results = results.OrderByDescending(a => a.UserInfo.Dept);
                    break;
                case "login":
                    results = results.OrderBy(a => a.UserInfo.UserName);
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
                    results = results.OrderBy(a => a.UserInfo.Country).ThenBy(a => a.UserInfo.City).ThenBy(a => a.UserInfo.UserName);
                    break;
            }

            ViewBag.DeploymentBatch = batch.Name;
            ViewBag.DeploymentBatchID = batch.DeploymentBatchID;

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            return View(results.ToList().ToPagedList(pageNumber, pageSize));
        }

        // GET: /Deployment/Details/5
        public ActionResult BatchUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            DeploymentBatchUser user = db.DeploymentBatchUsers.Find(id);

            ViewBag.TotalAppCount = db.vUserAppDtls.Where(a => a.UserID == user.UserID && !(a.AppCI.Deleted.HasValue && a.AppCI.Deleted.Value)).Select(a => a.AppCI).ToList().Count();
            ViewBag.RTGAppCount = db.vUserAppDtls.Where(a => a.UserID == user.UserID && !(a.AppCI.Deleted.HasValue && a.AppCI.Deleted.Value) && a.AppCI.StatusID != null && a.AppCI.AppStatu.Status == "Complete").Select(a => a.AppCI).ToList().Count();
            if (ViewBag.TotalAppCount != 0)
                ViewBag.RTG = ViewBag.RTGAppCount * 100 / ViewBag.TotalAppCount;
            else
                ViewBag.RTG = 0;

            if (user == null)
            {
                return HttpNotFound();
            }

            

            return View(user);
        }

        // GET: /Deployment/Edit/5
        [AuthorizeRole(Roles = "Admin,Packager,Deployment")]
        public ActionResult EditBatchUser(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            
            DeploymentBatchUser user = db.DeploymentBatchUsers.Find(id);
            ViewBag.DeploymentStaus = new SelectList(db.DeploymentStatus, "DeploymentStatusID", "Status", user.StatusID);


            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager,Deployment")]
        public ActionResult EditBatchUser([Bind(Include = "DeploymentBatchUserID, DeploymentBatchID,UserID, Win7MachineID,Win10MachineID,RowVersion")] DeploymentBatchUser user)
        {
            if (ModelState.IsValid)
            {
                user.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                user.ModDt = DateTime.Now;

                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Details", new { id = user.DeploymentBatchUserID });
            }
            return View(user);
        }

        [HttpPost]
        public JsonResult UpdateBatchUser(DeploymentBatchUserUpdate jsonParam)
        {
            string result = "0";
            try
            {
                if (!AuthorizeRole.IsRoleAuthorized(this.HttpContext, new List<string>() { "Packager", "Admin", "Deployment" }))
                {
                    result = JsonConvert.SerializeObject(new { Result = "0", Message = "You are not authorized to perform this action." });
                }
                else
                {
                    DeploymentBatchUser user = db.DeploymentBatchUsers.Find(jsonParam.DeploymentBatchUserID);

                    if (!user.RowVersion.SequenceEqual(jsonParam.RowVersion))
                    {
                        result = JsonConvert.SerializeObject(new { Result = "0", Message = "This row has been modified outside this window. Refersh the screen to get the new values." });
                    }
                    else
                    {

                        var excluded = new[] { "DeploymentBatchID", "UserID", "Win7MachineID", "Win10MachineID", "Comments" };

                        if (jsonParam.DeploymentStatus != null)
                            user.StatusID = db.DeploymentStatus.First(a => a.Status == jsonParam.DeploymentStatus).DeploymentStatusID;

                        user.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                        user.ModDt = DateTime.Now;

                        var entry = db.Entry(user);

                        entry.State = EntityState.Modified;
                        foreach (var name in excluded)
                            entry.Property(name).IsModified = false;

                        db.SaveChanges();
                        result = JsonConvert.SerializeObject(new { Result = "1", Message = "All good. Your changes have been updated!", RowVersion = user.RowVersion });
                    }
                }
            }
            catch (Exception)
            {
                result = JsonConvert.SerializeObject(new { Result = "0", Message = "Something went wrong during the save. Try again!" });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        [HttpPost]
        public JsonResult AddBatchDept(DeploymentBatchDeptUpdate jsonParam)
        {
            string result = "0";
            try
            {
                if (!AuthorizeRole.IsRoleAuthorized(this.HttpContext, new List<string>() { "Packager", "Admin", "Deployment" }))
                {
                    result = JsonConvert.SerializeObject(new { Result = "0", Message = "You are not authorized to perform this action." });
                }
                else
                {
                    DeploymentBatch bacth = db.DeploymentBatches.Find(jsonParam.DeploymentBatchID);

                    if (!bacth.RowVersion.SequenceEqual(jsonParam.RowVersion))
                    {
                        result = JsonConvert.SerializeObject(new { Result = "0", Message = "This row has been modified outside this window. Refersh the screen to get the new values." });
                    }
                    else
                    {

                        var currentDepsComplete = db.DeploymentBatchUsers.Where(a => a.DeploymentStatu.Status == "Complete" || a.DeploymentStatu.Status == "Ignore");
                        IEnumerable<UserMachine> userMachines = null;

                        string dept = jsonParam.Dept[0];

                        if (jsonParam.UpdateAll)
                        {
                            var currentDepts = db.DeploymentBatchUsers.Where(a => a.DeploymentBatchID == jsonParam.DeploymentBatchID).Select(a => a.UserInfo.Dept).Distinct();
                            userMachines = db.UserMachines.Where(u => u.UserInfo.Dept != null && u.UserInfo.DeptHL != null && !currentDepts.Contains(u.UserInfo.Dept)
                                && u.UserInfo.Dept.StartsWith(dept)
                                && !currentDepsComplete.Select(a => a.Win7MachineID).Contains(u.MachineID)
                                && !currentDepsComplete.Select(a => a.UserID).Contains(u.UserID)
                                );
                        }
                        else
                        {
                            userMachines = db.UserMachines.Where(u => jsonParam.Dept.Contains(u.UserInfo.Dept) && u.UserInfo.Dept != null && u.UserInfo.DeptHL != null
                                && !currentDepsComplete.Select(a => a.Win7MachineID).Contains(u.MachineID)
                                && !currentDepsComplete.Select(a => a.UserID).Contains(u.UserID)
                                );
                        }
                        var modUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                        int scheduledStatusID = db.DeploymentStatus.Where(a => a.Status == "Scheduled").FirstOrDefault().DeploymentStatusID;
                        foreach (var item in userMachines)
                        {
                            bacth.DeploymentBatchUsers.Add(new DeploymentBatchUser() { DeploymentBatchID = jsonParam.DeploymentBatchID, UserID = item.UserID
                                , Win7MachineID = item.MachineID, StatusID = scheduledStatusID, ModDt = DateTime.Now, ModUser = modUser });
                        }

                        bacth.ModUser = modUser;
                        bacth.ModDt = DateTime.Now;

                        var entry = db.Entry(bacth);

                        entry.State = EntityState.Modified;

                        db.SaveChanges();
                        result = JsonConvert.SerializeObject(new { Result = "1", Message = "All good. Your changes have been updated!", RowVersion = bacth.RowVersion });
                    }
                }
            }
            catch (Exception)
            {
                result = JsonConvert.SerializeObject(new { Result = "0", Message = "Something went wrong during the save. Try again!" });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }



        [HttpPost]
        public JsonResult RemoveBatchDept(DeploymentBatchDeptUpdate jsonParam)
        {
            string result = "0";
            try
            {
                if (!AuthorizeRole.IsRoleAuthorized(this.HttpContext, new List<string>() { "Packager", "Admin", "Deployment" }))
                {
                    result = JsonConvert.SerializeObject(new { Result = "0", Message = "You are not authorized to perform this action." });
                }
                else
                {
                    DeploymentBatch bacth = db.DeploymentBatches.Find(jsonParam.DeploymentBatchID);
                    string dept = jsonParam.Dept[0];

                    if (!bacth.RowVersion.SequenceEqual(jsonParam.RowVersion))
                    {
                        result = JsonConvert.SerializeObject(new { Result = "0", Message = "This row has been modified outside this window. Refersh the screen to get the new values." });
                    }
                    else
                    {
                        var entry = db.Entry(bacth);

                        if (jsonParam.UpdateAll)
                        {
                            foreach (var item in bacth.DeploymentBatchUsers.ToList())
                                db.DeploymentBatchUsers.Remove(item);   
                        }
                        else
                        {
                            foreach (var item in bacth.DeploymentBatchUsers.Where(a => a.UserInfo.Dept == dept).ToList())
                                db.DeploymentBatchUsers.Remove(item);
                        }
                        bacth.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                        bacth.ModDt = DateTime.Now;
                        
                       
                        entry.State = EntityState.Modified;

                        db.SaveChanges();
                        result = JsonConvert.SerializeObject(new { Result = "1", Message = "All good. Your changes have been updated!", RowVersion = bacth.RowVersion });
                    }
                }
            }
            catch (Exception)
            {
                result = JsonConvert.SerializeObject(new { Result = "0", Message = "Something went wrong during the save. Try again!" });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
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
