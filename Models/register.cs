using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class register
    {

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required(ErrorMessage = "You must key in the user full name before submit!")]
        [Display(Name = "User Full Name")]
        [StringLength(256, ErrorMessage = "You must key in the chars between 10 to 256 chars!", MinimumLength = 10)]
        [DataType(DataType.Text)]
        public string FullName { get; set; }


        [Display(Name = "User Role")]
        public string userroles { get; set; }

    }
}
