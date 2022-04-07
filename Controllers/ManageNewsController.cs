using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DDAC_Assignment.Data;
using DDAC_Assignment.Models;

namespace DDAC_Assignment.Controllers
{
    public class ManageNewsController : Controller
    {
        private readonly DDAC_AssignmentNewsDatabase _context;

        public ManageNewsController(DDAC_AssignmentNewsDatabase context)
        {
            _context = context;
        }

        // GET: News
        public async Task<IActionResult> Index()
        {
            return View(await _context.News.ToListAsync());
        }

        // GET: News/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.ID == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // GET: News/Create
        public async Task<IActionResult> Create()
        {
            //generate the listing for the drop down box
            IQueryable<Category> querydropdownlist = from m in _context.Category
                                                   orderby m.CategoryName
                                                   select m;
            List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = list;

            return View();
        }

        // POST: News/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,Title,Content,Actor,PublishedDate,Category,ParentCategory")] News news)
        {
            if (ModelState.IsValid)
            {
                //generate the listing for the drop down box
                IQueryable<Category> querydropdownlist = from m in _context.Category
                                                         orderby m.CategoryName
                                                         select m;
                List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
                news.ParentCategory = "None";
                foreach (var item in list)
                {
                    if (item.CategoryName == news.Category)
                    {
                        news.ParentCategory = item.ParentCategory;
                    }
                }
                news.Status = "Pending";
                _context.Add(news);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        // GET: News/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News.FindAsync(id);
            if (news == null)
            {
                return NotFound();
            }
            //generate the listing for the drop down box
            IQueryable<Category> querydropdownlist = from m in _context.Category
                                                     orderby m.CategoryName
                                                     select m;
            List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = list;
            return View(news);
        }

        // POST: News/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,Title,Content,Actor,PublishedDate,Category,ParentCategory")] News news)
        {
            if (id != news.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //generate the listing for the drop down box
                    IQueryable<Category> querydropdownlist = from m in _context.Category
                                                             orderby m.CategoryName
                                                             select m;
                    List<Category> list = new List<Category>(await querydropdownlist.Distinct().ToListAsync());
                    news.ParentCategory = "None";
                    foreach (var item in list)
                    {
                        if (item.CategoryName == news.Category)
                        {
                            news.ParentCategory = item.ParentCategory;
                        }
                    }
                    _context.Update(news);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!NewsExists(news.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(news);
        }

        // GET: News/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var news = await _context.News
                .FirstOrDefaultAsync(m => m.ID == id);
            if (news == null)
            {
                return NotFound();
            }

            return View(news);
        }

        // POST: News/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var news = await _context.News.FindAsync(id);
            _context.News.Remove(news);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool NewsExists(int id)
        {
            return _context.News.Any(e => e.ID == id);
        }
    }
}
