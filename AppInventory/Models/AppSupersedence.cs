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
    
    public partial class AppSupersedence
    {
        public int AppSupersedenceID { get; set; }
        public int AppCIID { get; set; }
        public int SupersedesAppCIID { get; set; }
        public System.DateTime ModDt { get; set; }
        public byte[] RowVersion { get; set; }
        public string ModUser { get; set; }
    
        public virtual AppCI AppCI { get; set; }
        public virtual AppCI AppCI1 { get; set; }
    }
}
