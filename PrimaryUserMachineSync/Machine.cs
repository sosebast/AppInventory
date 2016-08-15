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
    
    public partial class Machine
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Machine()
        {
            this.UserMachines = new HashSet<UserMachine>();
        }
    
        public string MachineID { get; set; }
        public string Name { get; set; }
        public string DistinguishedName { get; set; }
        public string Manufacturer { get; set; }
        public string Model { get; set; }
        public string SerialNumber { get; set; }
        public string AssetTagNumber { get; set; }
        public string Type { get; set; }
        public Nullable<int> OSID { get; set; }
        public System.DateTime ModDt { get; set; }
        public byte[] RowVersion { get; set; }
        public string ModUser { get; set; }
    
        public virtual O O { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<UserMachine> UserMachines { get; set; }
    }
}