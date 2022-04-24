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

        [Required(ErrorMessage ="You must key in title!")]
        [StringLength(256, ErrorMessage ="The title between 6 - 256 characters!", MinimumLength = 6)]
        public string Title { get; set; }

        [Required(ErrorMessage = "You must key in content!")]
        public string Content { get; set; }

        [StringLength(100, ErrorMessage = "The title between 3 - 100 characters!", MinimumLength = 3)]
        public string Actor { get; set; }

        [Display(Name = "Published Date")]
        //[DataType(DataType.Date)]
        [Required(ErrorMessage = "You must key in Published Date!")]
        public DateTime PublishedDate { get; set; }

        [Required(ErrorMessage = "You must select a category!")]
        public string Category { get; set; }

        [Display(Name = "Parent Category")]
        public string ParentCategory { get; set; }

        public bool Status { get; set; }

        [Display(Name = "Image Path")]
        public string ImagePath { get; set; }

        public DateTime LastUpdated { get; set; }

        [Required]
        public string Visibility { get; set; }

        //public int CommentsCount
    }
}
