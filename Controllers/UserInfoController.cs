using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using DDAC_Assignment.Areas.Identity.Data;

namespace DDAC_Assignment.Controllers
{
    public class UserInfoController : Controller
    {
        private UserManager<DDAC_AssignmentUser> userManager;

        public UserInfoController(UserManager<DDAC_AssignmentUser> usrMgr)
        {
            userManager = usrMgr;
        }
        public IActionResult Index()
        {
            return View(userManager.Users);
        }
    }
}
