using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DDAC_Assignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using DDAC_Assignment.Models;
using Microsoft.EntityFrameworkCore;

namespace DDAC_Assignment.Controllers
{
    public class UserInfoController : Controller
    {
        private UserManager<DDAC_AssignmentUser> userManager;
        private RoleManager<IdentityRole> roleManager;

        public UserInfoController(UserManager<DDAC_AssignmentUser> usrMgr, RoleManager<IdentityRole> roleMgr)
        {
            userManager = usrMgr;
            roleManager = roleMgr;
        }

        private async Task<List<string>> GetUserRoles(DDAC_AssignmentUser user)
        {
            return new List<string>(await userManager.GetRolesAsync(user));
        }
        public async Task<IActionResult> IndexAsync(string msg=null)
        {
            
            ViewBag.msg = msg;
            var users = await userManager.Users.ToListAsync(); 
            var UserModelList = new List<User>();
            foreach (DDAC_AssignmentUser user in users)
            {
                var UserModel = new User();
                UserModel.Id = user.Id;
                UserModel.Email = user.Email;
                UserModel.FullName = user.FullName;
                UserModel.Roles = await GetUserRoles(user);
                UserModelList.Add(UserModel);
            }
            return View(UserModelList);
        }

        public IActionResult registerUser()
        {
            User user = new User();
            // retrieve all identity roles
            var UserRoleList = new List<role.RoleSelector>();
            foreach (var role in roleManager.Roles)
            {
                var Role = new role.RoleSelector
                {
                    Id = role.Id,
                    Name = role.Name,
                    Selected = false
                };
                UserRoleList.Add(Role);
            }
            user.roleSelectors = UserRoleList;
            return View(user);
        }

        [HttpPost]
        public async Task<IActionResult> registerUser(User user)
        {
            if (ModelState.IsValid)
            {
                DDAC_AssignmentUser webUser = new DDAC_AssignmentUser
                {
                    UserName = user.Email,
                    Email = user.Email,
                    FullName = user.FullName,
                    EmailConfirmed = true,
                };
                IdentityResult result = await userManager.CreateAsync(webUser, user.Password);

                result = await userManager.AddToRolesAsync(webUser, user.roleSelectors.Where(x => x.Selected).Select(y => y.Name));
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot add selected roles to user");
                    ViewBag.msg = "Cannot add selected roles to user";
                    return View(user);
                }

                return RedirectToAction("Index", "UserInfo", new { msg="User created!" });

            }
            return View(user);
        }



        public async Task<IActionResult> Update(string id, string msg=null)
        {
            ViewBag.msg = msg;
            DDAC_AssignmentUser user = await userManager.FindByIdAsync(id);
            var UserModel = new User();
            UserModel.Id = user.Id;
            UserModel.Email = user.Email;
            UserModel.FullName = user.FullName;

            // retrieve current user role list selector
            var UserRoleList = new List<role.RoleSelector>();
            foreach (var role in roleManager.Roles)
            {
                var UserRole = new role.RoleSelector
                {
                    Id = role.Id,
                    Name = role.Name
                };

                if (await userManager.IsInRoleAsync(user, role.Name))
                {
                    UserRole.Selected = true;
                }
                else
                {
                    UserRole.Selected = false;
                }
                UserRoleList.Add(UserRole);
            }
            UserModel.roleSelectors = UserRoleList;

            if (user != null)
                return View(UserModel);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(User user)
        {
            string msg = null;
            DDAC_AssignmentUser current_user = await userManager.FindByIdAsync(user.Id);
            if (user != null)
            {
 
                current_user.FullName = user.FullName;
                if (current_user.Email != "admin@ddac.com")
                {
                    current_user.Email = user.Email;
                    current_user.UserName = user.Email;
                }
                else
                {
                    // put back admin details
                    System.Diagnostics.Debug.WriteLine("put back admin details");
                    user.Email = current_user.Email;
                    for (int s = 0; s < user.roleSelectors.Count; s++)
                    {
                        if (user.roleSelectors[s].Name == "Admin")
                        {
                            System.Diagnostics.Debug.WriteLine("set selector as true");
                            user.roleSelectors[s].Selected = true;
                        }
                    }
                    
                }

                // save updated data for current user 
                IdentityResult identity_result = await userManager.UpdateAsync(current_user);
                if (!identity_result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot save current user");
                    ViewBag.msg = "Cannot save current user";
                    return View(user);
                }

                // remove user existing roles 
                var existing_roles = await userManager.GetRolesAsync(current_user);
                System.Diagnostics.Debug.WriteLine(existing_roles.ToString());

                var result = await userManager.RemoveFromRolesAsync(current_user, existing_roles); 
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot remove user existing roles");
                    ViewBag.msg = "Cannot remove user existing roles";
                    return View(user);
                }
                
                // add selected roles
                result = await userManager.AddToRolesAsync(current_user, user.roleSelectors.Where(x => x.Selected).Select(y => y.Name));
                if (!result.Succeeded)
                {
                    ModelState.AddModelError("", "Cannot add selected roles to user");
                    ViewBag.msg = "Cannot add selected roles to user";
                    return View(user);
                }
            }
            else
            {
                ModelState.AddModelError("", "User Not Found");
                msg = "User Not Found";
            }

            msg = "User Updated!";
            return RedirectToAction("Update", "UserInfo", new { msg = msg });
        }

        public async Task<IActionResult> Delete(string id)
        {
            DDAC_AssignmentUser to_delete = await userManager.FindByIdAsync(id);
            if (to_delete != null)
            {
                await userManager.DeleteAsync(to_delete);
            }
            else
            {
                return RedirectToAction("Index", "UserInfo", new { msg = "Unable to delete user" });
            }
            return RedirectToAction("Index", "UserInfo", new { msg = "User deleted!" });
        }

    }
}
