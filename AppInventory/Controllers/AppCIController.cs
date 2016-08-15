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
using Newtonsoft.Json;
using AppInventory.CustomFilters;
using System.Collections;

namespace AppInventory.Controllers
{
    public class AppCIController : Controller
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        public ActionResult Index(string sortOrder, string currentFilter, string searchString, int? page, int? statusID, int? dispositionID, string appModel, int? scopeID, string dept, string deptHL, string deptFilter, int? deploymentID, string userID, bool? showDeleted, int? pkgStatusID)
        {
            SetListViewBags();

            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentDept = dept;
            ViewBag.CurrentDeptHL = deptHL;
            ViewBag.CurrentDeptFilter = deptFilter;
            ViewBag.CurrentDeploymentID = deploymentID;
            ViewBag.CurrentUserID = userID;
            ViewBag.ShowDeleted = showDeleted;
            
            if (searchString != null && statusID != null && dispositionID != null && appModel != null && scopeID != null)
                page = 1;

            if (searchString == null)
                searchString = currentFilter;
            
            IQueryable<AppCI> apps = GetQueryFilteredResults(dept, deptHL, deptFilter, deploymentID, userID);
            apps = GetFormFilteredResults(apps, searchString, statusID, dispositionID, appModel, scopeID, showDeleted, pkgStatusID);
            apps = GetSortedResults(apps, sortOrder);

            int pageSize = 50;
            int pageNumber = (page ?? 1);
            return View(apps.ToPagedList(pageNumber, pageSize));
        }

        private void SetListViewBags()
        {
            ViewBag.Disposition = new SelectList(db.AppDispositions, "AppDispositionID", "Disposition");
            ViewBag.Status = new SelectList(db.AppStatus, "AppStatusID", "Status");
            ViewBag.Scope = new SelectList(db.AppScopes, "AppScopeID", "Scope");
            ViewBag.CMModel = new SelectList(new string[] { "Application", "Package" });
            ViewBag.PkgStatus = new SelectList(db.AppPkgStatus, "AppPkgStatusID", "Status");
        }

        private IQueryable<AppCI> GetQueryFilteredResults(string dept, string deptHL, string deptFilter, int? deploymentID, string userID)
        {
            IQueryable<AppCI> apps = null;

            if (deploymentID != null)
            {
                DeploymentBatch batch = db.DeploymentBatches.Find(deploymentID);
                ViewBag.CurrentDep = batch.Name;

                var deptUsers = batch.DeploymentBatchUsers.Select(b => b.UserID).ToList();
                var deptApps = db.vUserAppDtls.Where(a => deptUsers.Contains(a.UserInfo.UserID)).Select(va => new { Dept = va.UserInfo.Dept, AppCI = va.AppCI }).Distinct();

                if (!string.IsNullOrEmpty(dept))
                    apps = deptApps.Where(a => a.Dept == dept).Select(u => u.AppCI).Distinct();
                else
                    if (!string.IsNullOrEmpty(deptFilter))
                        if (deptFilter.Contains(','))
                            foreach (string item in deptFilter.Split(new char[] { ',' }))
                                if (apps == null)
                                    apps = deptApps.Where(a => a.Dept.StartsWith(item)).Select(a => a.AppCI).Distinct();
                                else
                                    apps = apps.Union(deptApps.Where(a => a.Dept.StartsWith(item)).Select(a => a.AppCI).Distinct());
                        else
                            apps = deptApps.Where(a => a.Dept.StartsWith(deptFilter)).Select(a => a.AppCI).Distinct();
                    else
                        apps = deptApps.Select(u => u.AppCI).Distinct(); ;
            }
            else if (!string.IsNullOrEmpty(userID))
            {
                apps = db.vUserAppDtls.Where(a => a.UserID == userID).Select(a => a.AppCI);
                UserInfo user = db.UserInfoes.Find(userID);
                ViewBag.CurrentUser = user.UserName;
            }
            else
                if (string.IsNullOrEmpty(dept))
                    if (string.IsNullOrEmpty(deptHL))
                        if (string.IsNullOrEmpty(deptFilter))
                            apps = db.AppCIs;
                        else
                            if (deptFilter.Contains(','))
                                foreach (string item in deptFilter.Split(new char[] { ',' }))
                                    if (apps == null)
                                        apps = db.vDeptAppDtls.Where(a => a.Dept.StartsWith(item) && a.DeptHL != null).Select(a => a.AppCI).Distinct();
                                    else
                                        apps = apps.Union(db.vDeptAppDtls.Where(a => a.Dept.StartsWith(item) && a.DeptHL != null).Select(a => a.AppCI).Distinct());
                            else
                                apps = db.vDeptAppDtls.Where(a => a.Dept.StartsWith(deptFilter) && a.DeptHL != null).Select(a => a.AppCI).Distinct();
                    else
                        if (string.IsNullOrEmpty(deptFilter))
                            apps = db.vDeptAppDtls.Where(a => a.DeptHL == deptHL).Select(a => a.AppCI).Distinct();
                        else
                            if (deptFilter.Contains(','))
                                foreach (string item in deptFilter.Split(new char[] { ',' }))
                                    if (apps == null)
                                        apps = db.vDeptAppDtls.Where(a => a.Dept.StartsWith(item) && a.DeptHL == deptHL).Select(a => a.AppCI).Distinct();
                                    else
                                        apps = apps.Union(db.vDeptAppDtls.Where(a => a.Dept.StartsWith(item) && a.DeptHL == deptHL).Select(a => a.AppCI).Distinct());
                            else
                                apps = db.vDeptAppDtls.Where(a => a.Dept.StartsWith(deptFilter) && a.DeptHL == deptHL).Select(a => a.AppCI).Distinct();
                else
                    apps = db.vDeptAppDtls.Where(a => a.Dept == dept && a.DeptHL != null).Select(a => a.AppCI).Distinct();

            return apps;
        }

        private IQueryable<AppCI> GetFormFilteredResults(IQueryable<AppCI> apps, string searchString, int? statusID, int? dispositionID, string appModel, int? scopeID, bool? showDeleted, int? pkgStatusID)
        {
            ViewBag.CurrentFilter = searchString;

            if (!String.IsNullOrEmpty(searchString))
                apps = apps.Where(a => a.App.Name.Contains(searchString) || a.CMID.Contains(searchString) || a.App.Manufacturer.Contains(searchString));

            if (statusID != null)
                apps = apps.Where(a => a.StatusID == statusID);
            ViewBag.CurrentStatusID = statusID;

            if (dispositionID != null)
                apps = apps.Where(a => a.DispositionID == dispositionID);
            ViewBag.CurrentDispositionID = dispositionID;


            if (scopeID != null)
                apps = apps.Where(a => a.App.ScopeID == scopeID);
            ViewBag.CurrentScopeID = scopeID;

            if (!String.IsNullOrEmpty(appModel))
                apps = apps.Where(a => a.CMModel == appModel);
            ViewBag.CurrentCMModel = appModel;

            if (showDeleted != null && showDeleted.Value)
                apps = apps.Where(a => (a.Deleted.HasValue && a.Deleted.Value));
            else
                apps = apps.Where(a => !a.Deleted.HasValue || !(a.Deleted.HasValue && a.Deleted.Value));

            if (pkgStatusID != null)
                apps = apps.Where(a => a.PkgStatusID == pkgStatusID);
            ViewBag.CurrentPkgStatusID = pkgStatusID;

            return apps;
        }

        private IQueryable<AppCI> GetSortedResults(IQueryable<AppCI> apps, string sortOrder)
        {
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
            ViewBag.CMIDSortParm = sortOrder == "cmid" ? "cmid_desc" : "cmid";
            ViewBag.ManufacturerSortParm = sortOrder == "manufacturer" ? "manufacturer_desc" : "manufacturer";

            switch (sortOrder)
            {
                case "name_desc":
                    apps = apps.OrderByDescending(a => a.App.Name);
                    break;
                case "cmid":
                    apps = apps.OrderBy(a => a.CMID);
                    break;
                case "cmid_desc":
                    apps = apps.OrderByDescending(a => a.CMID);
                    break;
                case "manufacturer":
                    apps = apps.OrderBy(a => a.App.Manufacturer);
                    break;
                case "manufacturer_desc":
                    apps = apps.OrderByDescending(a => a.App.Manufacturer);
                    break;
                default:
                    apps = apps.OrderBy(a => a.App.Name);
                    break;
            }
            return apps;
        }

        // GET: /AppCI/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppCI appci = db.AppCIs.Find(id);
            if (appci == null)
            {
                return HttpNotFound();
            }

            appci.AppSupersedences.Clear();
            foreach (var item in db.sp_GetSupersedingApps(appci.AppCIID))
            {
                appci.AppSupersedences.Add(db.AppSupersedences.Find(item.AppSupersedenceID));
            }
            return View(appci);
        }

        public ActionResult Users(int? id, int? page, string sortOrder, string currentFilter, string searchString)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            IEnumerable<vUserAppDtl> results = db.vUserAppDtls.Where(a => a.AppCIID == id);
            if (results == null)
            {
                return HttpNotFound();
            }


            ViewBag.CurrentSort = sortOrder;
            ViewBag.DeptSortParm = String.IsNullOrEmpty(sortOrder) ? "dept_desc" : "";
            ViewBag.LoginIDSortParm = sortOrder == "login" ? "login_desc" : "login";
            ViewBag.NameSortParm = sortOrder == "name" ? "name_desc" : "name";

            if (searchString == null)
                searchString = currentFilter;

            ViewBag.CurrentFilter = searchString;


            if (!String.IsNullOrEmpty(searchString))
                results = results.Where(a => a.UserInfo.FullName.Contains(searchString) || a.UserInfo.Dept.Contains(searchString) || a.UserInfo.UserName.Contains(searchString) || a.Machine.Name.Contains(searchString));



            switch (sortOrder)
            {
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
                    results = results.OrderBy(a => a.UserInfo.Dept);
                    break;
            }

            AppCI appCI = db.AppCIs.Find(id);
            ViewBag.ApplicationName = appCI.App.Name;
            ViewBag.AppCIID = id;

            int pageSize = 20;
            int pageNumber = (page ?? 1);

            return View(results.ToList().ToPagedList(pageNumber, pageSize));
        }

        // GET: /AppCI/Create
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Create()
        {
            ViewBag.DispositionID = new SelectList(db.AppDispositions, "AppDispositionID", "Disposition");
            ViewBag.StatusID = new SelectList(db.AppStatus, "AppStatusID", "Status");
            ViewBag.ScopeID = new SelectList(db.AppScopes, "AppScopeID", "Scope");
            ViewBag.CMModel = new SelectList(new string[] { "Application", "Package" });

            return View();
        }

        // POST: /AppCI/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Create([Bind(Include = "AppID,Name,Manufacturer,Version,ScopeID,CMID,CMModel,StatusID,DispositionID,Description,Language,AppDeskID,BeastID,DeploymentTool,DeploymentGroup,RebootReq,LogonReq,Notes,LicensingInfo")] AppCreateNew app)
        {

            if (ModelState.IsValid)
            {
                App appInfo = new App();
                db.Apps.Attach(appInfo);
                var appEntry = db.Entry(appInfo);
                appEntry.CurrentValues.SetValues(app);

                appInfo.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                appInfo.ModDt = DateTime.Now;

                AppCI appCI = new AppCI() {
                    CMID = app.CMID, CMModel = app.CMModel, StatusID = app.StatusID, DispositionID = app.DispositionID,  
                    DeploymentTool = app.DeploymentTool, Notes = app.Notes,
                    LicensingInfo = app.LicensingInfo, LogonReq = app.LogonReq, 
                    ModUser = appInfo.ModUser, ModDt = DateTime.Now 
                };


                appInfo.AppCIs.Add(appCI);

                db.Apps.Add(appInfo);

                db.SaveChanges();

                return RedirectToAction("Details", new { id = appCI.AppCIID });
            }
            ViewBag.DispositionID = new SelectList(db.AppDispositions, "AppDispositionID", "Disposition");
            ViewBag.StatusID = new SelectList(db.AppStatus, "AppStatusID", "Status");
            ViewBag.ScopeID = new SelectList(db.AppScopes, "AppScopeID", "Scope");
            ViewBag.CMModel = new SelectList(new string[] { "Application", "Package" });

            return View(app);
        }

        // GET: /AppCI/Edit/5
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppCI appci = db.AppCIs.Find(id);
            if (appci == null)
            {
                return HttpNotFound();
            }
            ViewBag.DispositionID = new SelectList(db.AppDispositions, "AppDispositionID", "Disposition", appci.DispositionID);
            ViewBag.StatusID = new SelectList(db.AppStatus, "AppStatusID", "Status", appci.StatusID);
            ViewBag.PkgStatusID = new SelectList(db.AppPkgStatus, "AppPkgStatusID", "Status", appci.PkgStatusID);
            return View(appci);
        }

        // POST: /AppCI/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Edit([Bind(Include = "AppCIID,DeploymentTool,StatusID,DispositionID,PkgStatusID,RebootReq,LogonReq,Notes,LicensingInfo,ModDt, Rowversion")] AppCI appci)
        {
            if (ModelState.IsValid)
            {
                //appci.ModDt = DateTime.Now;
                var excluded = new[] { "AppID", "CMID", "CMModel", "FolderPath", "DeploymentTool", "DeploymentGroup", "LicensingInfo", "Deleted", "DeletedOn" };

                appci.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                appci.ModDt = DateTime.Now;

                var entry = db.Entry(appci);


                entry.State = EntityState.Modified;
                foreach (var name in excluded)
                    entry.Property(name).IsModified = false;

                db.SaveChanges();
                return RedirectToAction("Details", new { id = appci.AppCIID });
            }
            ViewBag.DispositionID = new SelectList(db.AppDispositions, "AppDispositionID", "Disposition", appci.DispositionID);
            ViewBag.StatusID = new SelectList(db.AppStatus, "AppStatusID", "Status", appci.StatusID);
            ViewBag.PkgStatusID = new SelectList(db.AppPkgStatus, "AppPkgStatusID", "Status", appci.PkgStatusID);

            return View(appci);
        }

        [HttpPost]
        public JsonResult UpdateData(AppCIUpdate jsonParam)
        {
            string result = "0";
            try
            {
                if (!AuthorizeRole.IsRoleAuthorized(this.HttpContext, new List<string>() { "Packager", "Admin" }))
                {
                    result = JsonConvert.SerializeObject(new { Result = "0", Message = "You are not authorized to perform this action." });
                }
                else
                {
                    AppCI appci = db.AppCIs.First(a => a.AppCIID == jsonParam.AppCIID);

                    if (!appci.RowVersion.SequenceEqual(jsonParam.RowVersion))
                    {
                        result = JsonConvert.SerializeObject(new { Result = "0", Message = "This application has been modified outside this window. Refersh the screen to get the new values." });
                    }
                    else
                    {

                        var excluded = new[] { "AppID", "CMID", "CMModel", "FolderPath", "DeploymentTool", "DeploymentGroup", "RebootReq", "LogonReq", "Notes", "LicensingInfo", "Deleted", "DeletedOn" };

                        if (jsonParam.AppStatus != null)
                            appci.StatusID = db.AppStatus.First(a => a.Status == jsonParam.AppStatus).AppStatusID;
                        else
                            appci.StatusID = null;

                        if (jsonParam.AppDisposition != null)
                            appci.DispositionID = db.AppDispositions.First(a => a.Disposition == jsonParam.AppDisposition).AppDispositionID;
                        else
                            appci.DispositionID = null;

                        if (jsonParam.AppPkgStatus != null)
                            appci.PkgStatusID = db.AppPkgStatus.First(a => a.Status == jsonParam.AppPkgStatus).AppPkgStatusID;
                        else
                            appci.PkgStatusID = null;

                        appci.ModUser = User.Identity.Name.ToLower().Replace(@"blackrock\", "");
                        appci.ModDt = DateTime.Now;

                        var entry = db.Entry(appci);

                        entry.State = EntityState.Modified;
                        foreach (var name in excluded)
                            entry.Property(name).IsModified = false;

                        db.SaveChanges();
                        result = JsonConvert.SerializeObject(new { Result = "1", Message = "All good. Your changes have been updated to the inventory!", RowVersion = appci.RowVersion });
                    }
                }
            }
            catch (Exception)
            {
                result = JsonConvert.SerializeObject(new { Result = "0", Message = "Something went wrong during the save. Try again!" });
            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }


        // GET: /AppCI/Delete/5
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            AppCI appci = db.AppCIs.Find(id);
            if (appci == null)
            {
                return HttpNotFound();
            }
            return View(appci);
        }

        // POST: /AppCI/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [AuthorizeRole(Roles = "Admin,Packager")]
        public ActionResult DeleteConfirmed(int id)
        {
            AppCI appci = db.AppCIs.Find(id);
            db.AppCIs.Remove(appci);
            db.SaveChanges();
            return RedirectToAction("Index");
        }


        public ActionResult ChangeLog(int id, string sortOrder, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.CurrentFilter = searchString;

            AppCI app = db.AppCIs.Find(id);
            ViewBag.AppCIID = id;
            ViewBag.CurrentAppName = app.App.Name;

            var changeLog = app.AppCIChangeLogs.OrderByDescending(a => a.AppCIChangeLogID);

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(changeLog.ToPagedList(pageNumber, pageSize));
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
