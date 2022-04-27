using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using DDAC_Assignment.Areas.Identity.Data;
using DDAC_Assignment.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

namespace DDAC_Assignment.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<DDAC_AssignmentUser> _userManager;
        private readonly SignInManager<DDAC_AssignmentUser> _signInManager;

        private string bucketname = Configuration.bucketName;

        public IndexModel(
            UserManager<DDAC_AssignmentUser> userManager,
            SignInManager<DDAC_AssignmentUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public string profilePictureS3Path = null;
        public IFormFile ProfilePicture { set; get; }

        public class InputModel
        {
            [Required(ErrorMessage = "Email cannot be empty!")]
            [EmailAddress]
            [Display(Name = "Email")]
            public string Email { get; set; }

            [Required(ErrorMessage = "Full Name cannot be empty!")]
            [Display(Name = "User Full Name")]
            [StringLength(256, ErrorMessage = "You must key in full name between 5 to 256 chars!", MinimumLength = 5)]
            [DataType(DataType.Text)]
            public string FullName { get; set; }
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

        private async Task LoadAsync(DDAC_AssignmentUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);
            var email = await _userManager.GetEmailAsync(user);

            Username = userName;

            Input = new InputModel
            {
                Email = email,
                FullName = user.FullName,
            };

            
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            if (user.ProfilePicture)
            {
                await LoadImageFromS3(user.Id);
            } 

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
           
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (Input.Email != user.Email && Input.Email!=null)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, Input.Email);
                user.UserName = user.Email;
                if (!setEmailResult.Succeeded)
                {
                    StatusMessage = "Unexpected error when trying to set email.";
                    return RedirectToPage();
                }
            }

            // upload profile picture to S3
            if (ProfilePicture != null)
            {
                await uploadImageToS3Async(ProfilePicture, user.Id);
                user.ProfilePicture = true;
            }

            if (Input.FullName != user.FullName)
            {
                user.FullName = Input.FullName;
            }
            IdentityResult identity_result = await _userManager.UpdateAsync(user);
            if (!identity_result.Succeeded)
            {
                StatusMessage = "Cannot save current user";
                return RedirectToPage();
            }

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
        public async Task LoadImageFromS3(string user_id)
        {
            //create each presign URL to the objects
            var path = "profilePictures/" + user_id;

            //permanent image access
            string permanent_url = "https://" + bucketname + ".s3.amazonaws.com/" + path;
            profilePictureS3Path = permanent_url;
        }

        //upload image to S3
        public async Task uploadImageToS3Async(IFormFile image, string filename)
        {
            List<string> accesskeylist = getAWSCredential();
            if (image != null)
            {
                using (var S3client = new AmazonS3Client(accesskeylist[0], accesskeylist[1], accesskeylist[2], Amazon.RegionEndpoint.USEast1))
                {
                    using (var uploadStream = new MemoryStream())
                    {
                        await image.CopyToAsync(uploadStream);
                        var uploadRequest = new TransferUtilityUploadRequest
                        {
                            InputStream = uploadStream,
                            Key = filename,
                            BucketName = bucketname + "/profilePictures",
                            CannedACL = S3CannedACL.PublicRead
                        };

                        var transferringtoS3 = new TransferUtility(S3client);
                        await transferringtoS3.UploadAsync(uploadRequest);
                    }
                }
            }
        }
    }
}
