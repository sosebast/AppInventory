//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace PrimaryUserMachineSync
{
    using System;
    using System.Collections.Generic;
    
    public partial class UserMachine
    {
        public int UserMachineID { get; set; }
        public string UserID { get; set; }
        public string MachineID { get; set; }
        public Nullable<bool> IsPrimaryMachine { get; set; }
        public System.DateTime ModDt { get; set; }
        public byte[] RowVersion { get; set; }
        public string ModUser { get; set; }
    
        public virtual Machine Machine { get; set; }
        public virtual UserInfo UserInfo { get; set; }
    }
}
