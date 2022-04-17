using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DDAC_Assignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using DDAC_Assignment.Models;

namespace DDAC_Assignment.Controllers
{
    public class RoleController : Controller
    {
        
        private RoleManager<IdentityRole> roleManager;
        private UserManager<DDAC_AssignmentUser> userManager;
        public RoleController(RoleManager<IdentityRole> roleMgr, UserManager<DDAC_AssignmentUser> usrMgr)
        {
            roleManager = roleMgr;
            userManager = usrMgr;
        }

        public IActionResult Index(string msg = null)
        {
            ViewBag.msg = msg;
            return View(roleManager.Roles);
        }

        private void Errors(IdentityResult result)
        {
            foreach (IdentityError error in result.Errors)
                ModelState.AddModelError("", error.Description);
        }

        // show the page for create new role
        public IActionResult Create()
        {
            return View();
        }

        // create new role
        [HttpPost]
        public async Task<IActionResult> Create(role role)
        {
            if (ModelState.IsValid)
            {
                // create new role
                IdentityResult result = await roleManager.CreateAsync(new IdentityRole(role.Name));
                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Role", new { msg = "Role created!" });
                }
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }
            }
            return View(role);
        }

        // show the page to update selected role
        public async Task<IActionResult> Update(string id)
        {
            // get selected role
            IdentityRole role = await roleManager.FindByIdAsync(id);

            if (role != null)
                return View(role);
            else
                return RedirectToAction("Index");
        }
        
        // update selected role
        [HttpPost]
        public async Task<IActionResult> Update(string id, string Name)
        {
            // get selected role
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                // update selected with new role name
                role.Name = Name;
                IdentityResult result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("Index", new { msg = "Role updated!" });
                else
                    return View(role);
            }
            else
                ModelState.AddModelError("", "Role Not Found");
            return View(role);
        }

        // delete selected role 
        public async Task<IActionResult> Delete(string id)
        {
            // get selected role
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                // delete selected role
                await roleManager.DeleteAsync(role);
            }
            else
            {
                return RedirectToAction("Index", "Role", new { msg = "Unable to delete role" });
            }
            return RedirectToAction("Index", "Role", new { msg = "Role deleted!" });
        }

        // View all users with selected role
        public async Task<IActionResult> ViewUser(string id)
        {
            // get selected role
            IdentityRole role = await roleManager.FindByIdAsync(id);

            // retrieve all user with the selected role
            List<DDAC_AssignmentUser> user_list = new List<DDAC_AssignmentUser>();
            foreach (DDAC_AssignmentUser user in userManager.Users)
            {
                if( await userManager.IsInRoleAsync(user, role.Name)) 
                    user_list.Add(user); 
            }
            ViewBag.users = user_list;

            if (role != null)
                return View(role);
            else
                return RedirectToAction("Index");
        }
    }
}
