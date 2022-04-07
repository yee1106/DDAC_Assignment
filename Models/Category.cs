using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace DDAC_Assignment.Models
{
    public class Category
    {
        
        public int ID { get; set; }

        [Display(Name = "Category Name")]
        public string CategoryName { get; set; }

        [Display(Name = "Parent Category")]
        public string ParentCategory { get; set; }

        public string Description { get; set; }

    }
}
