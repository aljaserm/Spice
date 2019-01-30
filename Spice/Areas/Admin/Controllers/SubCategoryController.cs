using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using Spice.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Manager)]
    public class SubCategoryController : Controller
    {
        private ApplicationDbContext _db;
        [TempData]
        public string msg { get; set; }
        public SubCategoryController(ApplicationDbContext db)
        {
            _db = db;
        }

        // get Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }

        //get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var subCategory = await _db.SubCategory.Include(s => s.Category).SingleOrDefaultAsync(m => m.Id == id);
            if (subCategory == null)
            {
                return NotFound();
            }

            return View(subCategory);
        }


        //post Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(SubCategory value)
        {
            var subCategory = await _db.SubCategory.SingleOrDefaultAsync(m => m.Id == value.Id);
            _db.SubCategory.Remove(subCategory);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        // get Edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var subs = await _db.SubCategory.SingleOrDefaultAsync(x => x.Id == id);

            if(subs== null)
            {
                return NotFound();
            }
                
            CategoryAndSubsVM categoryAndSubsVM = new CategoryAndSubsVM()
            {
                lstCategory = await _db.Category.ToListAsync(),
                SubCategory = subs,
                lstSubCategory = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).Distinct().ToListAsync()
            };

            return View(categoryAndSubsVM);
        }

        //post edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(CategoryAndSubsVM vM)
        {
            if (ModelState.IsValid)
            {
                var sub = _db.SubCategory.Include(s => s.Category).Where(s => s.Name == vM.SubCategory.Name && s.Category.Id == vM.SubCategory.CategoryId);
                if (sub.Count() > 0)
                {
                    //error
                    msg = "Error: The Sub Category" + vM.SubCategory.Name + " already exists in " + sub.First().Category.Name + " Category. Please try another name";
                }
                else
                {
                    var subcategory= await _db.SubCategory.FindAsync(vM.SubCategory.Id);
                    subcategory.Name = vM.SubCategory.Name;
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            CategoryAndSubsVM subsVM = new CategoryAndSubsVM()
            {
                lstCategory = await _db.Category.ToListAsync(),
                SubCategory = vM.SubCategory,
                lstSubCategory = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).Distinct().ToListAsync(),
                StatusMsg = msg
            };
            return View(subsVM);
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
                    msg = "Error: The Sub Category" + vM.SubCategory.Name +" already exists in " + sub.First().Category.Name + " Category. Please try another name"; 
                }
                else
                {
                    _db.SubCategory.Add(vM.SubCategory);
                    await _db.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
            }
            CategoryAndSubsVM subsVM = new CategoryAndSubsVM()
            {
                lstCategory = await _db.Category.ToListAsync(),
                SubCategory = vM.SubCategory,
                lstSubCategory = await _db.SubCategory.OrderBy(s => s.Name).Select(s => s.Name).Distinct().ToListAsync(),
                StatusMsg=msg

            };
            return View(subsVM);
        }

        [ActionName("GetSubsCategory")]
        public async Task<IActionResult> GetSubsCategory(int id)
        {
            List<SubCategory> subCategories = new List<SubCategory>();
            subCategories = await (from sc in _db.SubCategory
                             where sc.CategoryId == id
                             select sc).ToListAsync();
            return Json(new SelectList(subCategories,"Id","Name"));
        }


        //get Index
        public async Task<IActionResult> Index()
        {
            var subCategory = await _db.SubCategory.Include(s=>s.Category).ToListAsync();
            return View(subCategory);
        }
    }
}