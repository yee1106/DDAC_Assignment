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
        [StringLength(512, ErrorMessage = "The description between 5 - 512 characters!", MinimumLength = 5)]
        public string Description { get; set; }

        [Required(ErrorMessage = "You must key in advertisor name!")]
        [StringLength(256, ErrorMessage = "The description between 5 - 80 characters!", MinimumLength = 5)]
        [Display(Name = "Advertisor Name / Company")]
        public string Advertiser { get; set; }

        [Required(ErrorMessage = "You must select position!")]
        public string Position { get; set; }

        [Display(Name = "Image Path")]
        public string ImagePath { get; set; }

        [Display(Name = "Published Date")]
        public DateTime PublishedDate { get; set; }

        public string Visibility { get; set; }

        public int Duration { get; set; }

    }
}
