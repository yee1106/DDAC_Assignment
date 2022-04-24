using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using DDAC_Assignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using DDAC_Assignment.Models;
using Amazon.SimpleNotificationService.Model;
using Amazon.SimpleNotificationService;
using Microsoft.Extensions.Configuration;
using System.IO;
using static System.Text.Json.JsonSerializer;
using Newtonsoft.Json.Linq;

namespace DDAC_Assignment.Controllers
{
    public class RoleController : Controller
    {
        
        private RoleManager<IdentityRole> roleManager;
        private UserManager<DDAC_AssignmentUser> userManager;
        private static string topicArn = Configuration.topicArn;

        public RoleController(RoleManager<IdentityRole> roleMgr, UserManager<DDAC_AssignmentUser> usrMgr)
        {
            roleManager = roleMgr;
            userManager = usrMgr;
        }

        public List<string> getAWSCredential()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json");
            IConfigurationRoot configure = builder.Build();
            List<string> accesskeylist = new List<string>();
            accesskeylist.Add(configure["AWSCredential:accesskey"]);
            accesskeylist.Add(configure["AWSCredential:secretkey"]);
            accesskeylist.Add(configure["AWSCredential:sectiontoken"]);

            return accesskeylist;
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

        // subscribe to role and permission management using AWS SNS
        public async Task<IActionResult> SubscribeAsync()
        {
            string msg = "";
            try
            {
                // retrieve current logged in user
                DDAC_AssignmentUser user = await userManager.GetUserAsync(User);

                // create SNS client 
                List<string> accesskeylist = getAWSCredential();
                var SNSClient = new AmazonSimpleNotificationServiceClient(accesskeylist[0],
                    accesskeylist[1], accesskeylist[2], Amazon.RegionEndpoint.USEast1);

                // add email as the subscriber
                SubscribeRequest emailRequest = new SubscribeRequest(topicArn, "email", user.Email);
                SubscribeResponse emailSubscribeResponse = await SNSClient.SubscribeAsync(emailRequest);
                var arn = emailSubscribeResponse;
                System.Diagnostics.Debug.WriteLine(arn);

                msg = "Subscription confirmation has been sent! Please check your email to confirm the subscription.";

            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }

            return RedirectToAction("Index", "Role", new { msg = msg });
        }

        public async Task<bool> Publish(string Message)
        {
            try
            {
                // create SNS client 
                List<string> accesskeylist = getAWSCredential();
                var SNSClient = new AmazonSimpleNotificationServiceClient(accesskeylist[0],
                    accesskeylist[1], accesskeylist[2], Amazon.RegionEndpoint.USEast1);

                // publish message to AWS SNS
                PublishRequest pubRequest = new PublishRequest(topicArn, Message);
                PublishResponse pubResponse = await SNSClient.PublishAsync(pubRequest);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
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

                    string Message = "Dear Subscriber, \n\n"
                        + "The new role '"+role.Name + "' has been created.'"
                        + "'.\n\n" + "Thank You.";

                    if (await Publish(Message))
                        return RedirectToAction("Index", "Role", new { msg = "Role created!" });
                    else
                        return RedirectToAction("Update", new { msg = "Failed to send notification to SNS!" });
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
                string old_role_name = role.Name;

                // update selected with new role name
                role.Name = Name;
                IdentityResult result = await roleManager.UpdateAsync(role);
                if (result.Succeeded)
                {
                    string Message = "Dear Subscriber, \n\n"
                        + "'"+old_role_name + "' has been updated to '" + Name 
                        + "'.\n\n" + "Thank You.";

                    if (await Publish(Message))
                        return RedirectToAction("Index", new { msg = "Role updated!" });
                    else {
                        return RedirectToAction("Update", new { msg = "Failed to send notification to SNS!" });
                    }
                }
                    
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

                string Message = "Dear Subscriber, \n\n"
                    + "Please be informed the role '" + role.Name + "' has been deleted.'"
                    + "'.\n\n" + "Thank You.";

                if (await Publish(Message))
                    return RedirectToAction("Index", "Role", new { msg = "Role deleted!" });
                else
                    return RedirectToAction("Index", new { msg = "Failed to send notification to SNS!" });
            }
            else
            {
                return RedirectToAction("Index", "Role", new { msg = "Unable to delete role" });
            }
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
