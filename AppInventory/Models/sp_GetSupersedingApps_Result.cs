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
    
    public partial class sp_GetSupersedingApps_Result
    {
        public Nullable<int> AppSupersedenceID { get; set; }
        public Nullable<int> AppCIID { get; set; }
        public Nullable<int> SupersedesAppCIID { get; set; }
        public Nullable<System.DateTime> ModDt { get; set; }
        public byte[] RowVersion { get; set; }
        public string ModUser { get; set; }
    }
}
