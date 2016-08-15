using AppInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppInventory.Controllers
{
    public class SearchController : ApiController
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        [Route("search/depUserMachineDept")]
        [HttpPost]
        public object[] GetDploymentUserMachineDeptSearchResults(DeploymentBatchDeptUpdate jsonParam)
        {
            string term = jsonParam.Dept[0];
            var currentDepts = db.DeploymentBatchUsers.Where(a => a.DeploymentBatchID == jsonParam.DeploymentBatchID).Select(a => a.UserInfo.Dept).Distinct();
            var currentDepsComplete = db.DeploymentBatchUsers.Where(a => a.DeploymentStatu.Status == "Complete" || a.DeploymentStatu.Status == "Ignore");
            var results = db.UserMachines.Where(u => !currentDepts.Contains(u.UserInfo.Dept)
                        && u.UserInfo.Dept.StartsWith(term)
                        && u.UserInfo.Dept != null && u.UserInfo.DeptHL != null
                        && !currentDepsComplete.Select(a => a.Win7MachineID).Contains(u.MachineID)
                        && !currentDepsComplete.Select(a => a.UserID).Contains(u.UserID)
                    ).Select(u => new { UserID = u.UserID, Dept = u.UserInfo.Dept, MachineID = u.MachineID }).ToList();

            var users = results.Select(b => b.UserID).ToArray();
            var deptApps = db.vUserAppDtls.Where(a => users.Contains(a.UserInfo.UserID) && !(a.AppCI.Deleted.HasValue && a.AppCI.Deleted.Value)).Select(va => new { Dept = va.UserInfo.Dept, AppCIID = va.AppCIID }).Distinct().ToList();

            var searchResults = results.GroupBy(u => u.Dept)
                .Select(d => new DeploymentBactchDeptSummary
                {
                    Dept = d.Key,
                    Count = d.Count(),
                    UserCount = d.Select(a => a.UserID).Distinct().Count(),
                    AppCount = deptApps.Where(v => v.Dept == d.Key).Count()
                }).OrderBy(u => u.Dept);

            return searchResults.ToArray();
        }
    }
}
