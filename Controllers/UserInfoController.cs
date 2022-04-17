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
            ViewBag.Role = new SelectList(roleManager.Roles.Where(u => !u.Name.Contains("Admin"))
                                    .ToList(), "Name", "Name");
            return View();
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
                    userrole = user.userroles,
                    EmailConfirmed = true,
                };
                IdentityResult result = await userManager.CreateAsync(webUser, user.Password);
                if (result.Succeeded)
                    return RedirectToAction("Index", "UserInfo");
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }

            }
            return View(user);
        }



        public async Task<IActionResult> Update(string id)
        {
            DDAC_AssignmentUser user = await userManager.FindByIdAsync(id);
            Boolean a = false; Boolean b = false; Boolean c = false; Boolean d = false;
            if (user.userrole == "Admin")
            {
                a = true;
            }
            else if (user.userrole == "User")
            {
                b = true;
            }
            else if (user.userrole == "Staff")
            {
                c = true;
            }
            ViewBag.users = new List<SelectListItem>
            {
                new SelectListItem {Selected = c, Text = "Select Option", Value = ""},
                new SelectListItem {Selected = a, Text = "Admin", Value = "Admin"},
                new SelectListItem {Selected = b, Text = "User", Value = "User"},
                new SelectListItem {Selected = b, Text = "Staff", Value = "Staff"}
            };
            if (user != null)
                return View(user);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, string FullName, string email, string userrole)
        {
            DDAC_AssignmentUser user = await userManager.FindByIdAsync(id);
            if (user != null)
            {
                user.FullName = FullName;
                user.Email = email;
                user.UserName = email;
                user.userrole = userrole;

                IdentityResult result = await userManager.UpdateAsync(user);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    return View(user);
            }
            else
                ModelState.AddModelError("", "User Not Found");
            return View(user);
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
