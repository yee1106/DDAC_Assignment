using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDAC_Assignment.Data;
using DDAC_Assignment.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DDAC_Assignment.Controllers
{
    public class NewsController : Controller
    {
        private readonly ILogger<NewsController> _logger;
        private readonly DDAC_AssignmentNewsDatabase _context;


        public NewsController(ILogger<NewsController> logger, DDAC_AssignmentNewsDatabase context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index(string id)
        {
            ViewBag.id = id;
            News news = _context.News.First(n => n.ID.ToString() == id);
            return View(news);
        }

        public IActionResult Page(string page)
        {
            if (!string.IsNullOrEmpty(page))
            {
                Program.selectedPage = page;
                ViewBag.pageName = page;
            }
            List<News> news = _context.News.Where(n=>n.Category.Equals(page)).OrderByDescending(n => n.PublishedDate).Take(10).ToList();
            return View(news);
        }

        
    }
}
