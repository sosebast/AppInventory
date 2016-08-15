using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppInventory.Models
{
    public class RBACInfoGroup : RBACInfo
    {
        public RBACInfoGroup() { }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public RBACInfoGroup(RBACInfo rbacInfo) 
        {
            this.RoleID = rbacInfo.RoleID;
            this.Name = rbacInfo.Name;
            this.Description = rbacInfo.Description;
            this.ModDt = rbacInfo.ModDt;
            this.RowVersion = rbacInfo.RowVersion;
            this.RBACGroups = rbacInfo.RBACGroups;
            this.ModUser = rbacInfo.ModUser;

            this.Groups = rbacInfo.RBACGroups.Select(s => s.GroupID).ToArray<string>();
        }

        public string[] Groups { get; set; }
    }
}