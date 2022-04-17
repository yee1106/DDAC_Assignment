using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using DDAC_Assignment.Models;


namespace DDAC_Assignment.Controllers
{
    public class RoleController : Controller
    {
        
        private RoleManager<IdentityRole> roleManager;
        public RoleController(RoleManager<IdentityRole> roleMgr)
        {
            roleManager = roleMgr;
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

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(role role)
        {
            if (ModelState.IsValid)
            {

                IdentityResult result = await roleManager.CreateAsync(new IdentityRole(role.Name));
                if (result.Succeeded)
                    return RedirectToAction("Index", "Role");
                else
                {
                    foreach (IdentityError error in result.Errors)
                        ModelState.AddModelError("", error.Description);
                }

            }
            return View(role);
        }

        public async Task<IActionResult> Update(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);

            if (role != null)
                return View(role);
            else
                return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> Update(string id, string Name)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                role.Name = Name;

                IdentityResult result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                    return RedirectToAction("Index");
                else
                    return View(role);
            }
            else
                ModelState.AddModelError("", "Role Not Found");
            return View(role);
        }

        public async Task<IActionResult> Delete(string id)
        {
            IdentityRole role = await roleManager.FindByIdAsync(id);
            if (role != null)
            {
                await roleManager.DeleteAsync(role);
            }
            else
            {
                return RedirectToAction("Index", "Role", new { msg = "Unable to delete role" });
            }
            return RedirectToAction("Index", "Role", new { msg = "Role deleted!" });
        }
    }
}
