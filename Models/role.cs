using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace DDAC_Assignment.Models
{
    public class role
    {
        public enum Roles
        {
            Admin,
            User,
            Staff
        }
        public class RoleSelector
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public bool Selected { get; set; }
        }

        [Required(ErrorMessage = "You must key in the role before submit!")]
        [Display(Name = "Role")]
        [StringLength(256, ErrorMessage = "You must key in the chars between 1 to 30 chars!", MinimumLength = 1)]
        [DataType(DataType.Text)]
        public string Name { get; set; }
    }

    public class RoleClaimsViewModel
    {
        public string Type { get; set; }
        public string Value { get; set; }
        public bool Selected { get; set; }
    }

}
