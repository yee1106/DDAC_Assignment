using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using DDAC_Assignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc.Rendering;
using DDAC_Assignment.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using DDAC_Assignment.Models.APIs;
using Newtonsoft.Json.Linq;

namespace DDAC_Assignment.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel
    {
        private readonly SignInManager<DDAC_AssignmentUser> _signInManager;
        private readonly UserManager<DDAC_AssignmentUser> _userManager;
        private readonly ILogger<RegisterModel> _logger;
        private readonly IEmailSender _emailSender;
        private readonly RoleManager<IdentityRole> roleManager;

        public RegisterModel(
            UserManager<DDAC_AssignmentUser> userManager,
            RoleManager<IdentityRole> roleMgr,
            SignInManager<DDAC_AssignmentUser> signInManager,
            ILogger<RegisterModel> logger,
            IEmailSender emailSender)
        {
            _userManager = userManager;
            roleManager = roleMgr;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
        }

        public SelectList RoleSelectList = new SelectList(
                new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
                {
                    new SelectListItem { Selected = true, Text = "Select Role", Value = ""},
                    new SelectListItem { Selected = true, Text = role.Roles.User.ToString(), Value = role.Roles.User.ToString()},
                    new SelectListItem { Selected = true, Text = role.Roles.Staff.ToString(), Value = role.Roles.Staff.ToString()},
                }, "Value", "Text", 1
            );

        [BindProperty]
        public InputModel Input { get; set; }

        public string ReturnUrl { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        public class InputModel
        {
            [Required]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Password")]
            public string Password { get; set; }

            [Required]
            [DataType(DataType.Password)]
            [Display(Name = "Confirm password")]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string ConfirmPassword { get; set; }

            [Required]
            [Display(Name = "Full Name")]
            public string FullName { get; set; }

            [Required]
            [Display(Name = "User Role")]
            public string userrole { get; set; }
        }

        public async Task OnGetAsync(string returnUrl = null)
        {
            ReturnUrl = returnUrl;
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        }

        public async Task<IActionResult> OnPostAsync(string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");
            ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            if (ModelState.IsValid)
            {
                var user = new DDAC_AssignmentUser { 
                    FullName = Input.FullName,
                    UserName = Input.Email, 
                    Email = Input.Email,
                    ProfilePicture = false,
                };
                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    string role = Input.userrole;
                    _logger.LogInformation("User created a new account with password.");         

                    await _userManager.AddToRoleAsync(user, role); // assign user role

                    var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                    var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl, role=role },
                        protocol: Request.Scheme);

                    // Call api to send confirmation email by using aws lambda function
                    if (role == "Staff")
                    {
                        // send registration application to default admin to approve the user
                        string response = await Email.approve_staff_registration_api(user.Email, user.FullName, callbackUrl); 
                    }
                    else
                    {
                        // send email confirmation link to user
                        await Email.send_email_confirmation_api(user.Email, user.FullName, callbackUrl);
                    }

                    if (_userManager.Options.SignIn.RequireConfirmedAccount)
                    {
                        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, role = role, returnUrl = returnUrl });
                    }
                    else
                    {
                        await _signInManager.SignInAsync(user, isPersistent: false);
                        return LocalRedirect(returnUrl);
                    }
                }
                
                foreach (var error in result.Errors)
                {
                    StatusMessage = error.Description;
                }
            }

            // If we got this far, something failed, redisplay form
            return Page();
        }
    }
}
