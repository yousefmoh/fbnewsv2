﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FBNews
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class FBNewsDbEntities : DbContext
    {
        public FBNewsDbEntities()
            : base("name=FBNewsDbEntities")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<FBNew> FBNews { get; set; }
        public virtual DbSet<page> pages { get; set; }
        public virtual DbSet<keyword> keywords { get; set; }
        public virtual DbSet<criteria> criterias { get; set; }
    }
}
