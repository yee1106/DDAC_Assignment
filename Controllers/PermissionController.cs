using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using DDAC_Assignment.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace DDAC_Assignment.Controllers
{
    [Authorize(Roles = "Admin")]
    public class PermissionController : Controller
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        public PermissionController(RoleManager<IdentityRole> roleManager)
        {
            _roleManager = roleManager;
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

        public async Task<bool> Publish(string Message)
        {
            try
            {
                // create SNS client 
                List<string> accesskeylist = getAWSCredential();
                var SNSClient = new AmazonSimpleNotificationServiceClient(accesskeylist[0],
                    accesskeylist[1], accesskeylist[2], Amazon.RegionEndpoint.USEast1);

                // publish message to AWS SNS
                PublishRequest pubRequest = new PublishRequest(Configuration.topicArn, Message);
                PublishResponse pubResponse = await SNSClient.PublishAsync(pubRequest);

                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        // Permission Management for the selected role
        public async Task<ActionResult> Index(string roleId, string msg=null)
        {
            ViewBag.msg = msg;
            var model = new PermissionViewModel();
            var allPermissions = new List<RoleClaimsViewModel>();
            List<Type> policies = new List<Type>();
            policies.Add(typeof(Permissions.User));
            policies.Add(typeof(Permissions.News));
            policies.Add(typeof(Permissions.Category));
            policies.Add(typeof(Permissions.Advertisements));
            policies.Add(typeof(Permissions.ReadNews));
            allPermissions.GetPermissions(policies, roleId);

            var role = await _roleManager.FindByIdAsync(roleId);
            model.RoleId = roleId;
            ViewBag.role_name = role.Name;

            var claims = await _roleManager.GetClaimsAsync(role);
            var allClaimValues = allPermissions.Select(a => a.Value).ToList();
            var roleClaimValues = claims.Select(a => a.Value).ToList();
            var authorizedClaims = allClaimValues.Intersect(roleClaimValues).ToList();
            foreach (var permission in allPermissions)
            {
                if (authorizedClaims.Any(a => a == permission.Value))
                {
                    permission.Selected = true;
                }
            }
            model.RoleClaims = allPermissions;
            return View(model);
        }

        // Update permission on the current role
        public async Task<IActionResult> Update(PermissionViewModel model)
        {
            var role_to_update = await _roleManager.FindByIdAsync(model.RoleId);
            var claims = await _roleManager.GetClaimsAsync(role_to_update);
            foreach (var claim in claims)
            {
                await _roleManager.RemoveClaimAsync(role_to_update, claim);
            }
            var selectedClaims = model.RoleClaims.Where(a => a.Selected).ToList();
            foreach (var claim in selectedClaims)
            {
                await _roleManager.AddPermissionClaim(role_to_update, claim.Value);
            }
            string Message = "Dear Subscriber, \n\n"
                        + "Please be informed the permissions for the role '" + role_to_update.Name + "' has been updated.'"
                        + "'.\n\n" + "Thank You.";

            if (await Publish(Message))
                return RedirectToAction("Index", new { roleId = model.RoleId, msg = "Permissions have been updated." });
            else
                return RedirectToAction("Index", new { roleId = model.RoleId, msg = "Failed to send notification to SNS!" });
        }
    }
}
