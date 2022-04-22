using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DDAC_Assignment.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Page(string page)
        {
            if (!string.IsNullOrEmpty(page))
            {
                Program.selectedPage = page;
                ViewBag.pageName = page;
            }
            return View();
        }
    }
}
