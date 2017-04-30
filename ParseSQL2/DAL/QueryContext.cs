using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using ParseSQL2.Models;

namespace ParseSQL2.DAL
{
    public class QueryContext : DbContext
        {
        public QueryContext() : base("DefaultConnection")
        {
           Database.SetInitializer<QueryContext>(new CreateDatabaseIfNotExists<QueryContext>());
        }
        public DbSet<QueryMaster> Query { get; set; }
    }
}



