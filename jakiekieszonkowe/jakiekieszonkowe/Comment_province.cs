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
    
    public partial class Comment_province
    {
        public int Id_comment_province { get; set; }
        public int Id_province { get; set; }
        public int Id_user { get; set; }
        public string Content { get; set; }
        public int Likes_amount { get; set; }
        public System.DateTime Creation_date { get; set; }
    
        public virtual Province Province { get; set; }
        public virtual User User { get; set; }
    }
}
