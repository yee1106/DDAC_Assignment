using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DDAC_Assignment.Data;

namespace DDAC_Assignment.Models
{
    
    public class User
    {

        public string Id { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "You must key in the user full name before submit!")]
        [Display(Name = "User Full Name")]
        [StringLength(256, ErrorMessage = "You must key in full name between 5 to 256 chars!", MinimumLength = 5)]
        [DataType(DataType.Text)]
        public string FullName { get; set; }

        public IEnumerable<string> Roles { get; set; }

        [Display(Name = "User Role")]
        public List<role.RoleSelector> roleSelectors { get; set; }

    }
}
