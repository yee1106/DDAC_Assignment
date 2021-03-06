using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class Advertisement
    {
        public int ID { get; set; }

        [Required(ErrorMessage = "You must key in description!")]
        [StringLength(512, ErrorMessage = "The description between 3 - 512 characters!", MinimumLength = 3)]
        public string Description { get; set; }

        [Required(ErrorMessage = "You must key in advertisor name!")]
        [StringLength(80, ErrorMessage = "The description between 3 - 80 characters!", MinimumLength = 3)]
        [Display(Name = "Advertisor Name / Company")]
        public string Advertiser { get; set; }

        [Required(ErrorMessage = "You must select position!")]
        public string Position { get; set; }

        [Required(ErrorMessage = "You must select category!")]
        [Display(Name = "Where placing ads")]
        public string Category { get; set; }

        [Display(Name = "Image Path")]
        //[Required(ErrorMessage = "You must select an image!")]
        public string ImagePath { get; set; }

        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        public string Visibility { get; set; }

        [Display(Name = "Duration (days)")]
        [Required(ErrorMessage = "You must key in duration!")]
        public int Duration { get; set; }

    }
}
