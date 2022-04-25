using DDAC_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DDAC_Assignment.Data;

namespace DDAC_Assignment.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly DDAC_AssignmentNewsDatabase _context;

        public HomeController(ILogger<HomeController> logger, DDAC_AssignmentNewsDatabase context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            var news = _context.News.OrderByDescending(n=>n.PublishedDate).Take(10).ToList();
            return View(news);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Latest()
        {
            return View();
        }

    }
}
