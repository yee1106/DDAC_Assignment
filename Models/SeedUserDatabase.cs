using DDAC_Assignment.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DDAC_Assignment.Areas.Identity.Data;
using System.Linq;

namespace DDAC_Assignment.Models
{
    public class SeedUserDatabase
    {
        //Seed Roles
        public static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
        {
            
            await roleManager.CreateAsync(new IdentityRole(role.Roles.Admin.ToString()));
            await roleManager.CreateAsync(new IdentityRole(role.Roles.Staff.ToString()));
            await roleManager.CreateAsync(new IdentityRole(role.Roles.User.ToString()));
        }

        //Seed Default User
        public static async Task SeedAdminAsync(UserManager<DDAC_AssignmentUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var defaultUser = new DDAC_AssignmentUser
            {
                UserName = "admin@ddac.com",
                Email = "admin@ddac.com",
                FullName = "Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true
            };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, "DDACadmin123@");
                    await userManager.AddToRoleAsync(defaultUser, role.Roles.User.ToString());
                    await userManager.AddToRoleAsync(defaultUser, role.Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, role.Roles.Staff.ToString());
                }
            }
        }

    }
}


