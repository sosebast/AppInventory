//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace AppInventory.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class vUserAppDtl
    {
        public long UserAppDtlID { get; set; }
        public string UserID { get; set; }
        public int AppCIID { get; set; }
        public string MachineID { get; set; }
        public int AppID { get; set; }
        public System.DateTime ModDt { get; set; }
        public string ModUser { get; set; }
    
        public virtual App App { get; set; }
        public virtual AppCI AppCI { get; set; }
        public virtual Machine Machine { get; set; }
        public virtual UserInfo UserInfo { get; set; }
    }
}
