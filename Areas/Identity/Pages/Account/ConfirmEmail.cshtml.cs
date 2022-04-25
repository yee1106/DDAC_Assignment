using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using DDAC_Assignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using DDAC_Assignment.Models.APIs;

namespace DDAC_Assignment.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<DDAC_AssignmentUser> _userManager;

        public ConfirmEmailModel(UserManager<DDAC_AssignmentUser> userManager)
        {
            _userManager = userManager;
        }

        public string Role { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string userId, string code, string role)
        {
            if (userId == null || code == null)
            {
                return RedirectToPage("/Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);

            Role = role;
            if (role == "Staff")
            {
                if(result.Succeeded)
                {
                    //send email notification to user
                    await Email.notify_registration_success(user.Email, user.FullName);
                }

                StatusMessage = result.Succeeded ? "You have successfully approved the staff registration." : "Error approving staff registration.";
            }
            else
            {
                StatusMessage = result.Succeeded ? "Thank you for confirming your email." : "Error confirming your email.";
            }
            
            return Page();
        }
    }
}
