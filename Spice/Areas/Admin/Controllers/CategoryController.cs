using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;

namespace Spice.Areas.Admin.Controllers
{
    [Authorize(Roles =SD.Manager)]
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //get Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        //get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        ////post Delete
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirm(int? id)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        _db.Update(value);
        //        await _db.SaveChangesAsync();
        //        return RedirectToAction(nameof(Index));
        //    }
        //    return View(value);
        //}

        //post Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Category value)
        {
            if (ModelState.IsValid)
            {
               var removedCategory= _db.Category.Find(value.Id);
                if (removedCategory == null)
                {
                    return NotFound();
                }
                _db.Remove(removedCategory);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(value);
        }

        //get Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var category = await _db.Category.FindAsync(id);
            if(category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        //post Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Category value)
        {
            if (ModelState.IsValid)
            {
                _db.Update(value);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(value);
        }

        //get Create
        public IActionResult Create()
        {
            return View();
        }

        //post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Category value)
        {
            if (ModelState.IsValid)
            {
                _db.Category.Add(value);
                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(value);
        }

        //get Index
        public async Task<IActionResult> Index()
        {
            return View( await _db.Category.ToListAsync());
        }
    }
}