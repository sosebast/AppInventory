using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppInventory.Models
{
    public class DeploymentBatchUserUpdate
    {
        public int DeploymentBatchUserID { get; set; }
        public string DeploymentStatus { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class DeploymentBatchDeptUpdate
    {
        public int DeploymentBatchID { get; set; }
        public string[] Dept { get; set; }
        public bool UpdateAll { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class DeploymentBactchDeptSummary
    {
        public int DeploymentBatchID { get; set; }
        public string Dept { get; set; }
        public int Count { get; set; }
        public int UserCount { get; set; }
        public int AppCount { get; set; }
        public int RTG { get; set; }
        public int PercentageComplete { get; set; }
    }
}