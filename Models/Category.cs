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

        [Required(ErrorMessage ="You must key in Category Name")]
        [Display(Name = "Category Name")]
        [StringLength(60, ErrorMessage = "The Category Name between 3 - 60 characters!", MinimumLength = 3)]
        public string CategoryName { get; set; }

        [Display(Name = "Parent Category")]
        public string ParentCategory { get; set; }

        [StringLength(1000, ErrorMessage = "Description cannot have more than 100 characters!")]
        public string Description { get; set; }

    }
}
