//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace jakiekieszonkowe
{
    using System;
    using System.Collections.Generic;
    
    public partial class Information_notification
    {
        public int Id_information_notification { get; set; }
        public int Id_user { get; set; }
    
        public virtual User User { get; set; }
    }
}
