﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class JakieKieszonkoweEntities : DbContext
    {
        public JakieKieszonkoweEntities()
            : base("name=JakieKieszonkoweEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Child> Child { get; set; }
        public virtual DbSet<City> City { get; set; }
        public virtual DbSet<Comment_city> Comment_city { get; set; }
        public virtual DbSet<Comment_country> Comment_country { get; set; }
        public virtual DbSet<Comment_province> Comment_province { get; set; }
        public virtual DbSet<Education_stage> Education_stage { get; set; }
        public virtual DbSet<History_city> History_city { get; set; }
        public virtual DbSet<History_province> History_province { get; set; }
        public virtual DbSet<Information_notification> Information_notification { get; set; }
        public virtual DbSet<Payout_period> Payout_period { get; set; }
        public virtual DbSet<Pocket_money_option> Pocket_money_option { get; set; }
        public virtual DbSet<Province> Province { get; set; }
        public virtual DbSet<Reminder_notification> Reminder_notification { get; set; }
        public virtual DbSet<User> User { get; set; }
    }
}
