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
    public class CategoriesController : Controller
    {
        private readonly DDAC_AssignmentNewsDatabase _context;

        public CategoriesController(DDAC_AssignmentNewsDatabase context)
        {
            _context = context;
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            return View(await _context.Category.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public async Task<IActionResult> Create()
        {
            //generate the listing for the drop down box
            IQueryable<string> querydropdownlist = from m in _context.Category
                                                   orderby m.CategoryName
                                                   select m.CategoryName;
            List<string> list = new List<string>(await querydropdownlist.Distinct().ToListAsync());
            ViewBag.Category = list;
            return View();
        }

        // POST: Categories/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,CategoryName,ParentCategory,Description")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            //generate the listing for the drop down box
            IQueryable<string> querydropdownlist = from m in _context.Category
                                                   orderby m.CategoryName
                                                   select m.CategoryName;
            List<string> list = new List<string>(await querydropdownlist.Distinct().ToListAsync());
            list.Remove(category.CategoryName);
            ViewBag.Category = list;
            return View(category);
        }

        // POST: Categories/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,CategoryName,ParentCategory,Description")] Category category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                     UpdateItem(id, category);
                    //_context.Update(category);
                    await _context.SaveChangesAsync();

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.ID))
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
            return View(category);
        }

        public async Task UpdateItem(int id, Category category)
        {
            var categories = _context.Category.ToList();
            var oldCategory = await _context.Category.FindAsync(id);

            if (oldCategory.CategoryName != category.CategoryName)
            {
                foreach (var item in categories)
                {
                    if (item.ParentCategory == oldCategory.CategoryName)
                    {
                        item.ParentCategory = category.CategoryName;
                        _context.Update(item);
                    }
                }

                var news = _context.News.ToList();
                foreach (var newItem in news)
                {
                    if (newItem.ParentCategory == oldCategory.CategoryName)
                    {
                        newItem.ParentCategory = category.CategoryName;
                        _context.Update(newItem);
                    }
                    if (newItem.Category == oldCategory.CategoryName)
                    {
                        newItem.Category = category.CategoryName;
                        _context.Update(newItem);
                    }
                }

                var advertisement = _context.Advertisement.ToList();
                foreach (var advertisementItem in advertisement)
                {
                    if (advertisementItem.Category.Contains(oldCategory.CategoryName))
                    {
                        advertisementItem.Category = advertisementItem.Category.Replace(oldCategory.CategoryName, category.CategoryName);
                    }
                    _context.Update(advertisementItem);
                }
            }
            oldCategory.CategoryName = category.CategoryName;
            //oldCategory.ParentCategory = category.ParentCategory;
            oldCategory.Description = category.Description;
            _context.Update(oldCategory);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category
                .FirstOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var category = await _context.Category.FindAsync(id);
            _context.Category.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.ID == id);
        }
    }
}
