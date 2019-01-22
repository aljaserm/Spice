using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class CategoryController : Controller
    {
        private ApplicationDbContext _db;
        public CategoryController(ApplicationDbContext db)
        {
            _db = db;
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