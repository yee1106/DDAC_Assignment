using System.Threading.Tasks;
using DDAC_Assignment.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DDAC_Assignment.Areas.Identity.Pages.Account.Manage
{
    public class PersonalDataModel : PageModel
    {
        private readonly UserManager<DDAC_AssignmentUser> _userManager;
        private readonly ILogger<PersonalDataModel> _logger;
        public DDAC_AssignmentUser current_user;

        public PersonalDataModel(
            UserManager<DDAC_AssignmentUser> userManager,
            ILogger<PersonalDataModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<IActionResult> OnGet()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }
            current_user = user;
            return Page();
        }
    }
}