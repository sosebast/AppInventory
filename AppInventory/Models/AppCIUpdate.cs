using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AppInventory.Models
{
    public class AppCIUpdate
    {
       public int AppCIID { get; set; }
       public string AppStatus { get; set; }
       public string AppDisposition { get; set; }
       public string AppPkgStatus { get; set; }
       public byte[] RowVersion { get; set; }
     }
}