//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace jakiekieszonkowe_api.Database
{
    using System;
    using System.Collections.Generic;
    
    public partial class Child
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Child()
        {
            this.Reminder_notification = new HashSet<Reminder_notification>();
            this.Pocket_money_option = new HashSet<Pocket_money_option>();
        }
    
        public int Id_child { get; set; }
        public string First_name { get; set; }
        public Nullable<System.DateTime> Date_of_birth { get; set; }
        public System.DateTime Date_of_payout { get; set; }
        public int Id_user { get; set; }
        public int Id_education_stage { get; set; }
        public int Id_city { get; set; }
        public int Id_payout_period { get; set; }
        public decimal Current_amount_of_money { get; set; }
    
        public virtual City City { get; set; }
        public virtual Education_stage Education_stage { get; set; }
        public virtual Payout_period Payout_period { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Reminder_notification> Reminder_notification { get; set; }
        public virtual User User { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Pocket_money_option> Pocket_money_option { get; set; }
    }
}
