using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using AppInventory.Models;
using System.Web;

namespace AppInventory.Controllers
{
    public class ChartController : ApiController
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        [Route("chart/appScope")]
        public ChartData GetAppScopeSummary()
        {
            var results = db.AppCIs.Where(a => a.App.ScopeID != null && !(a.Deleted.HasValue && a.Deleted.Value)).GroupBy(a => new { ScopeID = a.App.ScopeID, Scope = a.App.AppScope.Scope }).OrderBy(s => s.Key.ScopeID)
                .Select(s => new { ScopeID = s.Key.ScopeID, Scope = s.Key.Scope, Count = s.Count() });

            return new ChartData()
            {
                Label = "Application Count",
                Labels = results.Select(a => a.Scope).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/AppCI?ScopeID=" + a.ScopeID).ToArray<string>()
            };
        }

        [Route("chart/appStatus")]
        public ChartData GetAppStatusSummary()
        {
            var results = db.AppCIs.Where(a => a.StatusID != null && !(a.Deleted.HasValue && a.Deleted.Value)).GroupBy(a => new { StatusID = a.StatusID, Status = a.AppStatu.Status }).OrderBy(s => s.Key.StatusID)
                .Select(s => new { StatusID = s.Key.StatusID, Status = s.Key.Status, Count = s.Count() });

            return new ChartData()
            {
                Label = "Application Count",
                Labels = results.Select(a => a.Status).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/AppCI?StatusID=" + a.StatusID).ToArray<string>()
            };
        }

        [Route("chart/appDisposition")]
        public ChartData GetAppDispositionSummary()
        {
            var results = db.AppCIs.Where(a => a.DispositionID != null && !(a.Deleted.HasValue && a.Deleted.Value)).GroupBy(a => new { DispositionID = a.DispositionID, Disposition = a.AppDisposition.Disposition }).OrderBy(s => s.Key.DispositionID)
                .Select(s => new { DispositionID = s.Key.DispositionID, Disposition = s.Key.Disposition, Count = s.Count() });

            return new ChartData()
            {
                Label = "Application Count",
                Labels = results.Select(a => a.Disposition).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/AppCI?DispositionID=" + a.DispositionID).ToArray<string>()
            };
        }

        [Route("chart/deployment")]
        public ChartData GetDeploymentSummary()
        {
            var results = db.DeploymentBatchUsers.GroupBy(a => new { ID = a.DeploymentBatchID, Name = a.DeploymentBatch.Name, Date = a.DeploymentBatch.PlannedDt }).OrderByDescending(s => s.Key.Date).Take(10).OrderBy(s => s.Key.Date)
                .Select(s => new { ID = "/Deployment/Details/" + s.Key.ID, Name = (String.IsNullOrEmpty(s.Key.Name) ? "NA" : s.Key.Name), Count = s.Count() });

            return new ChartData()
            {
                Label = "Deployments",
                Labels = results.Select(a => a.Name).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => a.ID).ToArray<string>()
            };
        }

        [Route("chart/appModel")]
        public ChartData GetAppModelSummary()
        {
            var results = db.AppCIs.Where(a => a.CMModel != null && !(a.Deleted.HasValue && a.Deleted.Value)).GroupBy(a => a.CMModel)
                .Select(s => new { Model = s.Key, Count = s.Count() });

            return new ChartData()
            {
                Label = "Application Count",
                Labels = results.Select(a => a.Model).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/AppCI?AppModel=" + a.Model).ToArray<string>()
            };
        }

        [Route("chart/deptAppSummary")]
        public ChartData GetDeptAppSummary()
        {
            int limitCount = 0;

            var apps = db.vDeptAppDtls.Where(a => !string.IsNullOrEmpty(a.DeptHL) && !(a.AppCI.Deleted.HasValue && a.AppCI.Deleted.Value)).Select(s => new { Dept = s.DeptHL, AppCIID = s.AppCIID }).Distinct().ToList();

            var results = apps.GroupBy(a => a.Dept).Select(a => new { Dept = a.Key, Count = a.Count() });

            results = results.Where(a => a.Count >= limitCount).OrderBy(a => a.Dept);

            return new ChartData()
            {
                Label = "App Count",
                Labels = results.Select(a => a.Dept).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "javascript:showDeptHLAppLandscape('" + a.Dept + "')").ToArray<string>(),
                SumTotal = apps.Select(a => a.AppCIID).Distinct().Count()
            };
        }

        [Route("chart/deptUserSummary")]
        public ChartData GetDeptUserummary()
        {
            int limitCount = 0;
            var users = db.UserMachines.Where(a => !string.IsNullOrEmpty(a.UserInfo.DeptHL) && a.Machine.O.Name == "Win 7")
                .GroupBy(a => a.UserInfo.DeptHL)
                .Select(s => new
                {
                    Dept = s.Key,
                    Count = s.Count()
                });

            var results = users.Where(a => a.Count >= limitCount).OrderBy(a => a.Dept);

            return new ChartData()
            {
                Label = "Deployment Count",
                Labels = results.Select(a => a.Dept).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "javascript:showDeptHLUser('" + a.Dept + "')").ToArray<string>(),
                SumTotal = results.Sum(a => a.Count)
            };
        }

        [Route("chart/deptHLAppLandscape")]
        public ChartData GetDeptHLAppLandscape(string deptHL)
        {
            var apps = db.vDeptAppDtls.Where(d => d.DeptHL == deptHL && !(d.AppCI.Deleted.HasValue && d.AppCI.Deleted.Value)).Select(s => new { Dept = s.Dept, AppCIID = s.AppCIID }).Distinct().ToList();

            var results = apps.GroupBy(a => a.Dept).Select(a => new { Dept = a.Key, Count = a.Count() }).OrderBy(a => a.Dept);

            return new ChartData()
            {
                Label = "App Count",
                Labels = results.Select(a => a.Dept).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/AppCI?Dept=" + HttpUtility.UrlEncode(a.Dept)).ToArray<string>(),
                SumTotal = apps.Select(a => a.AppCIID).Distinct().Count()
            };
        }

        [Route("chart/deptAppFiltered")]
        public ChartData GetDeptAppFiltered(string dept)
        {
            List<string> depts = null;
            if (dept.Contains(','))
            {
                depts = new List<string>();
                foreach (string item in dept.Split(new char[]{','}))
                {
                    depts.AddRange(db.UserInfoes.Where(a => a.Dept.StartsWith(item)).Select(a => a.Dept).Distinct().ToList());
                }
            }
            else
                depts = db.UserInfoes.Where(a => a.Dept.StartsWith(dept)).Select(a => a.Dept).Distinct().ToList();


            var apps = db.vDeptAppDtls.Where(d => depts.Contains(d.Dept) && d.DeptHL != null && !(d.AppCI.Deleted.HasValue && d.AppCI.Deleted.Value)).Select(s => new { Dept = s.Dept, AppCIID = s.AppCIID }).Distinct().ToList();

            var results = apps.GroupBy(a => a.Dept).Select(a => new { Dept = a.Key, Count = a.Count() }).OrderBy(a => a.Dept);

            return new ChartData()
            {
                Label = "App Count",
                Labels = results.Select(a => a.Dept).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/AppCI?Dept=" + HttpUtility.UrlEncode(a.Dept)).ToArray<string>(),
                SumTotal = apps.Select(a => a.AppCIID).Distinct().Count()
            };
        }

        [Route("chart/deptHLUser")]
        public ChartData GetDeptHLUserDeployments(string deptHL)
        {
            var results = db.UserMachines.Where(a => a.UserInfo.DeptHL == deptHL).GroupBy(a => a.UserInfo.Dept)
                .Select(s => new
                {
                    Dept = s.Key,
                    Count = s.Count()
                }).ToList().OrderBy(a => a.Dept);

            
            return new ChartData()
            {
                Label = "Deployment Count",
                Labels = results.Select(a => a.Dept).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/UserMachines?Dept=" + HttpUtility.UrlEncode(a.Dept)).ToArray<string>(),
                SumTotal = results.Sum(a => a.Count)
            };
        }


        [Route("chart/deptUserFiltered")]
        public ChartData GetDeptUserFiltered(string dept)
        {
            IQueryable<UserMachine> ums = null;
            if (dept.Contains(','))
                foreach (string item in dept.Split(new char[] { ',' }))
                    if (ums == null)
                        ums = db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(item) && a.UserInfo.DeptHL != null);
                    else
                        ums = ums.Union(db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(item) && a.UserInfo.DeptHL != null));
            else
                ums = db.UserMachines.Where(a => a.UserInfo.Dept.StartsWith(dept) && a.UserInfo.DeptHL != null);
            
            var results = ums.GroupBy(a => a.UserInfo.Dept).Select(s => new
                {
                    Dept = s.Key,
                    Count = s.Count()
                }).ToList().OrderBy(a => a.Dept);


            return new ChartData()
            {
                Label = "Deployment Count",
                Labels = results.Select(a => a.Dept).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/UserMachines?DeptFilter=" + HttpUtility.UrlEncode(a.Dept)).ToArray<string>(),
                SumTotal = results.Sum(a => a.Count)
            };
        }


        [Route("chart/appPkgStatus")]
        public ChartData GetAppPkgStatusSummary()
        {
            var results = db.AppCIs.Where(a => a.PkgStatusID != null && !(a.Deleted.HasValue && a.Deleted.Value)).GroupBy(a => new { StatusID = a.PkgStatusID, Status = a.AppPkgStatu.Status }).OrderBy(s => s.Key.StatusID)
                .Select(s => new { StatusID = s.Key.StatusID, Status = s.Key.Status, Count = s.Count() });

            return new ChartData()
            {
                Label = "Application Count",
                Labels = results.Select(a => a.Status).ToArray<string>(),
                Values = results.Select(a => a.Count).ToArray<int>(),
                Filters = results.Select(a => "/AppCI?PkgStatusID=" + a.StatusID).ToArray<string>()
            };
        }

    }
}