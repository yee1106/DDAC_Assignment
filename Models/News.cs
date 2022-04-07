using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    //table to keep the news details
    public class News
    {
        public int ID { get; set; }

        public string Title { get; set; }
        public string Content { get; set; }
        public string Actor { get; set; }

        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }
        public string Category { get; set; }

        [Display(Name = "Parent Category")]
        public string ParentCategory { get; set; }

        public string Status { get; set; }

    }
}
