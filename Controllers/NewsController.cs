using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDAC_Assignment.Data;
using DDAC_Assignment.Models;
using DDAC_Assignment.Models.customer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
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
            var news = _context.News.First(n => n.ID.ToString() == id);

            var headerAdvertisement = _context.Advertisement.FirstOrDefault(a =>
                DateTime.Now >= a.PublishedDate && DateTime.Now <= a.PublishedDate.AddDays(a.Duration) &&
                a.Category == news.Category && a.Position == "Header" && a.Visibility == "Visible");
            //var headerAdvertisement = _context.Advertisement.FirstOrDefault();

            var footerAdvertisement = _context.Advertisement.FirstOrDefault(a =>
                DateTime.Now >= a.PublishedDate && DateTime.Now <= a.PublishedDate.AddDays(a.Duration) &&
                a.Category == news.Category && a.Position == "Footer" && a.Visibility == "Visible");
            //var footerAdvertisement = _context.Advertisement.FirstOrDefault();

            if (headerAdvertisement != null)
            {
                var fileName = headerAdvertisement.ImagePath.Replace(" ", "+");
                ViewBag.headerAd = $"https://advertisementimage.s3.amazonaws.com/advertisementImages/{fileName}";
                _logger.LogInformation($"https://advertisementimage.s3.amazonaws.com/advertisementImages/{fileName}");
            }

            if (footerAdvertisement != null)
            {
                var fileName = footerAdvertisement.ImagePath.Replace(" ", "+");
                ViewBag.footerAd = $"https://advertisementimage.s3.amazonaws.com/advertisementImages/{fileName}";
            }


            return View(news);
        }

        public IActionResult Page(string page)
        {
            if (!string.IsNullOrEmpty(page))
            {
                Program.selectedPage = page;
                ViewBag.pageName = page;
            }

            List<News> news = _context.News.Where(n => n.Category.Equals(page)).OrderByDescending(n => n.PublishedDate)
                .Take(10).ToList();
            return View(news);
        }
    }
}