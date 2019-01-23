using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SubCategoryController : Controller
    {
        private ApplicationDbContext _db;
        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        //get Create
        public async Task<IActionResult> Create()
        {
            CategoryAndSubsVM categoryAndSubsVM = new CategoryAndSubsVM()
            {
                lstCategory = await _db.Category.ToListAsync(),
                SubCategory = new Models.SubCategory(),
                lstSubCategory = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).Distinct().ToListAsync()
            };

            return View(categoryAndSubsVM);
        }

        //post Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CategoryAndSubsVM vM)
        {
            if (ModelState.IsValid)
            {
                var sub = _db.SubCategory.Include(s => s.Category).Where(s=>s.Name==vM.SubCategory.Name && s.Category.Id==vM.SubCategory.CategoryId);
                if (sub.Count() > 0)
                {
                    //error
                }
                else
                {
                    _db.SubCategory.Add(vM.SubCategory);
                    await _db.SaveChangesAsync();
                    RedirectToAction(nameof(Index));
                }
            }
            CategoryAndSubsVM subsVM = new CategoryAndSubsVM()
            {
                lstCategory = await _db.Category.ToListAsync(),
                SubCategory = vM.SubCategory,
                lstSubCategory = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).Distinct().ToListAsync()
            };
            return View(subsVM);
        }


        //get Index
        public async Task<IActionResult> Index()
        {
            var subCategory = await _db.SubCategory.Include(s=>s.Category).ToListAsync();
            return View(subCategory);
        }
    }
}