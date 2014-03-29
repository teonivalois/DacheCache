using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Web;

namespace DacheCache.Sample.Web.Models.Context {
    public class AppDataContext : DbContext {

        public DbSet<Customer> Customers { get; set; }

        static AppDataContext() {
            Database.SetInitializer<AppDataContext>(null);
        }

        public AppDataContext()
            : base("connection") {
        }

    }
}