using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace DDAC_Assignment.Controllers
{
    public class NewsController : Controller
    {
        public IActionResult Index(string page)
        {
            if (!string.IsNullOrEmpty(page))
            {
                ViewBag.pageName = page;
            }
            return View();
        }
    }
}
