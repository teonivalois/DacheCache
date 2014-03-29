using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace DacheCache.Sample.Web.Models.Context {
    public class AppDbContext : DbContext {

        public DbSet<Category> Categories { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Supplier> Suppliers { get; set; }

        static AppDbContext() {
            Database.SetInitializer<AppDbContext>(null);
        }

        public AppDbContext()
            : base("connection") {
        }

    }
}