using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using Spice.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Manager)]
    public class MenuController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _he;
        [BindProperty]
        public MenuVM vM { get; set; }
        public MenuController(ApplicationDbContext db, IHostingEnvironment he)
        {
            _db = db;
            _he = he;
            vM = new MenuVM()
            {
                lstCategory = _db.Category,
                Menu = new Models.Menu()
            };
        }

        // get Details
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            vM.Menu = await _db.Menu.Include(s => s.Category).Include(s=>s.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            if (vM.Menu == null)
            {
                return NotFound();
            }

            return View(vM);
        }

        //get Delete
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            vM.Menu = await _db.Menu.Include(s => s.Category).Include(s => s.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            if (vM.Menu == null)
            {
                return NotFound();
            }

            return View(vM);
        }


        //post Delete
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id)
        {
            string webRootPath = _he.WebRootPath;
            Menu menu = await _db.Menu.FindAsync(id);

            if (menu != null)
            {
                var imagePath = Path.Combine(webRootPath, menu.Img.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.Menu.Remove(menu);
                await _db.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }

        //get edit
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();
            vM.Menu = await _db.Menu.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m=>m.Id==id);
            
            if (vM.Menu == null)
                return NotFound();
            vM.lstSubCategory = await _db.SubCategory.Where(s => s.CategoryId == vM.Menu.CategoryId).ToListAsync();
            return View(vM);
        }

        //Post Create
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            vM.Menu.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                vM.lstSubCategory = await _db.SubCategory.Where(s => s.CategoryId == vM.Menu.CategoryId).ToListAsync();
                return View(vM);
            }
            //_db.Menu.Add(vM.Menu);
           // await _db.SaveChangesAsync();

            //Working on Image saving
            string WebRootPath = _he.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menu = await _db.Menu.FindAsync(vM.Menu.Id);
            if (files.Count > 0)
            {
                //New Image uploaded
                var upload = Path.Combine(WebRootPath, "images");
                var fileExtensionEdit = Path.GetExtension(files[0].FileName);

                //delete original file
                //if (menu.Img != null)
                //{
                    var imaagepath = Path.Combine(WebRootPath, menu.Img.TrimStart('\\'));
                    if (System.IO.File.Exists(imaagepath))
                    {
                        System.IO.File.Delete(imaagepath);
                    }
               // }
                
                //upload the new image
                using (var fs = new FileStream(Path.Combine(upload, vM.Menu.Id + fileExtensionEdit), FileMode.Create))
                {
                    files[0].CopyTo(fs);
                }
                menu.Img = @"\images\" + vM.Menu.Id + fileExtensionEdit;
            }
            menu.Name = vM.Menu.Name;
            menu.Descrption = vM.Menu.Descrption;
            menu.Price = vM.Menu.Price;
            menu.SpicyLevel = vM.Menu.SpicyLevel;
            menu.CategoryId = vM.Menu.CategoryId;
            menu.SubCategoryId = vM.Menu.SubCategoryId;

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


        //get Create
        public IActionResult Create()
        {
            return View(vM);
        }

        //Post Create
        [HttpPost, ActionName("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePost()
        {
            vM.Menu.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(vM);
            }
            _db.Menu.Add(vM.Menu);
            await _db.SaveChangesAsync();

            //Working on Image saving
            string WebRootPath = _he.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var menu = await _db.Menu.FindAsync(vM.Menu.Id);
            if (files.Count > 0)
            {
                //file uploaded
                var upload = Path.Combine(WebRootPath, "images");
                var fileExtension = Path.GetExtension(files[0].FileName);
                using (var fs = new FileStream(Path.Combine(upload, vM.Menu.Id + fileExtension), FileMode.Create))
                {
                    files[0].CopyTo(fs);
                }
                menu.Img = @"\images\" + vM.Menu.Id + fileExtension;
            }
            else
            {
                //no file uploaded
                var upload = Path.Combine(WebRootPath, @"images\"+SD.DEFAULT_IMAGE);
                System.IO.File.Copy(upload, WebRootPath+@"\images\" +vM.Menu.Id+".png");
                menu.Img = @"\images\" + vM.Menu.Id + ".png";
            }

            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Index()
        {
            var menu = await _db.Menu.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync();
            return View(menu);
        }
    }
}