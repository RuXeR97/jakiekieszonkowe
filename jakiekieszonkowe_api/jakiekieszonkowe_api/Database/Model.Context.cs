﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class DatabaseEntities : DbContext
    {
        public DatabaseEntities()
            : base("name=DatabaseEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Child> Children { get; set; }
        public virtual DbSet<City> Cities { get; set; }
        public virtual DbSet<Comment_city> Comments_city { get; set; }
        public virtual DbSet<Comment_country> Comments_country { get; set; }
        public virtual DbSet<Comment_province> Comments_province { get; set; }
        public virtual DbSet<Education_stage> Education_stages { get; set; }
        public virtual DbSet<History_city> History_cities { get; set; }
        public virtual DbSet<History_province> History_provinces { get; set; }
        public virtual DbSet<Information_notification> Information_notifications { get; set; }
        public virtual DbSet<Payout_period> Payout_periods { get; set; }
        public virtual DbSet<Pocket_money_option> Pocket_money_options { get; set; }
        public virtual DbSet<Province> Provinces { get; set; }
        public virtual DbSet<Reminder_notification> Reminder_notifications { get; set; }
        public virtual DbSet<User> Users { get; set; }
    }
}
