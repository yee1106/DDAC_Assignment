using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace DDAC_Assignment.Areas.Identity.Data
{
    // Add profile data for application users by adding properties to the DDAC_AssignmentUser class
    public class DDAC_AssignmentUser : IdentityUser
    {
        [PersonalData]
        public string FullName { get; set; }

    }
}
