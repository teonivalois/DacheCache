using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace DacheCache.Sample.Web.Models {
    
    [Table("people")]
    public class Person {

        [Key, Column("id")]
        public Guid ID { get; set; }

        [Column("birth_date"), Required]
        public DateTime BirthDate { get; set; }

        [Column("name"), Required(AllowEmptyStrings = false)]
        public string Name { get; set; }

    }
}