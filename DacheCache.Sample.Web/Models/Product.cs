using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DacheCache.Sample.Web.Models {
    public class Product {

        [Key, Required]
        public int ProductID { get; set; }

        [Required]
        public string ProductName { get; set; }
        public int? SupplierID { get; set; }
        public int? CategoryID { get; set; }
        public string QuantityPerUnit { get; set; }
        public decimal? UnitPrice { get; set; }
        public short? UnitsInStock { get; set; }
        public short? UnitsOnOrder { get; set; }
        public short? ReorderLevel { get; set; }
        public bool? Discontinued { get; set; }
        public DateTime? DiscontinuedDate { get; set; }

        [ForeignKey("SupplierID")]
        public virtual Supplier Supplier { get; set; }

        [ForeignKey("CategoryID")]
        public virtual Category Category { get; set; }
    }
}