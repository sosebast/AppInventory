using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppInventory.Models
{
    public class AppCreateNew : App
    {
        public string CMID { get; set; }
        public string CMModel { get; set; }
        public string DeploymentTool { get; set; }
        public string DeploymentGroup { get; set; }
        public Nullable<int> StatusID { get; set; }
        public Nullable<int> DispositionID { get; set; }
        public Nullable<bool> RebootReq { get; set; }
        public Nullable<bool> LogonReq { get; set; }
        public string Notes { get; set; }
        public string LicensingInfo { get; set; }
    }
}