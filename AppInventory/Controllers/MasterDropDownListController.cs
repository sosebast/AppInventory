using AppInventory.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace AppInventory.Controllers
{
    public class MasterDropDownListController : ApiController
    {
        private AppInventoryEntities db = new AppInventoryEntities();

        [Route("master/appStatus")]
        public object[] GetAppStatus()
        {
            var results = db.AppStatus.Select(a => new { value = a.AppStatusID, text = a.Status });
            return results.ToArray();            
        }

        [Route("master/appDisposition")]
        public object[] GetAppDisposition()
        {
            var results = db.AppDispositions.Select(a => new { value = a.AppDispositionID, text = a.Disposition });
            return results.ToArray();
        }

        [Route("master/deploymentStatus")]
        public object[] GetDeploymentStatus()
        {
            var results = db.DeploymentStatus.Select(a => new { value = a.DeploymentStatusID, text = a.Status });
            return results.ToArray();
        }

        [Route("master/appPkgStatus")]
        public object[] GetAppPkgStatus()
        {
            var results = db.AppPkgStatus.Select(a => new { value = a.AppPkgStatusID, text = a.Status });
            return results.ToArray();
        }
        
    }
}
