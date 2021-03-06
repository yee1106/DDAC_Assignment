using DDAC_Assignment.Data;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DDAC_Assignment.Areas.Identity.Data;
using System.Linq;
using System.Security.Claims;

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

        //Seed Default Admin
        public static async Task SeedAdminAsync(UserManager<DDAC_AssignmentUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var defaultUser = new DDAC_AssignmentUser
            {
                UserName = Configuration.default_admin.UserName,
                Email = Configuration.default_admin.Email,
                FullName = "Admin",
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                ProfilePicture = false
            };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);
                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, Configuration.default_admin.password);
                    await userManager.AddToRoleAsync(defaultUser, role.Roles.User.ToString());
                    await userManager.AddToRoleAsync(defaultUser, role.Roles.Admin.ToString());
                    await userManager.AddToRoleAsync(defaultUser, role.Roles.Staff.ToString());

                    // seeds permissions for user, staff, admin
                    await SeedClaimsForAdmin(roleManager);
                    await SeedClaimsForStaff(roleManager);
                    await SeedClaimsForUser(roleManager);
                }
                
            }
        }

        public async static Task SeedClaimsForAdmin(RoleManager<IdentityRole> roleManager)
        {
            var adminRole = await roleManager.FindByNameAsync("Admin");
            await AddPermissionClaim(roleManager, adminRole, "User");

            var allClaims = await roleManager.GetClaimsAsync(adminRole);
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == Permissions.News.Approve))
            {
                await roleManager.AddClaimAsync(adminRole, new Claim("Permission", Permissions.News.Approve));
            }
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == Permissions.News.View))
            {
                await roleManager.AddClaimAsync(adminRole, new Claim("Permission", Permissions.News.View));
            }
        }

        public async static Task SeedClaimsForStaff(RoleManager<IdentityRole> roleManager)
        {
            var staffRole = await roleManager.FindByNameAsync("Staff");
            await AddPermissionClaim(roleManager, staffRole, "Category");
            await AddPermissionClaim(roleManager, staffRole, "News");
            await AddPermissionClaim(roleManager, staffRole, "Advertisements");
        }

        public async static Task SeedClaimsForUser(RoleManager<IdentityRole> roleManager)
        {
            var userRole = await roleManager.FindByNameAsync("User");

            var allClaims = await roleManager.GetClaimsAsync(userRole);
            if (!allClaims.Any(a => a.Type == "Permission" && a.Value == Permissions.ReadNews.Read))
            {
                await roleManager.AddClaimAsync(userRole, new Claim("Permission", Permissions.ReadNews.Read));
            }
        }

        public static async Task AddPermissionClaim(RoleManager<IdentityRole> roleManager, IdentityRole role, string module)
        {
            var allClaims = await roleManager.GetClaimsAsync(role);
            var allPermissions = Permissions.GeneratePermissionsForModule(module);
            foreach (var permission in allPermissions)
            {
                if (!allClaims.Any(a => a.Type == "Permission" && a.Value == permission))
                {
                    await roleManager.AddClaimAsync(role, new Claim("Permission", permission));
                }
            }
        }



    }
}


