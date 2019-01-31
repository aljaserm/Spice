using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using Spice.ViewModels;

namespace Spice.Controllers
{
    [Area("Customer")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _db;
        public HomeController(ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {
            IndexVM indexVM = new IndexVM()
            {
                menus = await _db.Menu.Include(m => m.Category).Include(m => m.SubCategory).ToListAsync(),
                categories = await _db.Category.ToListAsync(),
                coupons= await _db.Coupons.Where(c=>c.Active==true).ToListAsync()

            };
            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            if (claim != null)
            {
                var count = _db.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToList().Count();
                HttpContext.Session.SetInt32(SD.CartCountSession, count);
            }
            
            return View(indexVM);
        }

        [Authorize]
        public async Task<IActionResult> Details(int id)
        {
            var menu = await _db.Menu.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == id).FirstOrDefaultAsync();
            ShoppingCart shopping = new ShoppingCart()
            {
                Menu = menu,
                MenuId = menu.Id
            };
            return View(shopping);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(ShoppingCart value)
        {
            value.Id = 0;
            if(ModelState.IsValid)
            {
                var CIdentity = (ClaimsIdentity)this.User.Identity;
                var claim =CIdentity.FindFirst(ClaimTypes.NameIdentifier);
                value.ApplicationUserId = claim.Value;
                ShoppingCart shopping = await _db.ShoppingCart.Where(s => s.MenuId == value.MenuId && s.ApplicationUserId == value.ApplicationUserId).FirstOrDefaultAsync();
                if (shopping == null)
                {
                    await _db.ShoppingCart.AddAsync(value);
                }
                else
                {
                    shopping.Count = shopping.Count + value.Count;
                }
                await _db.SaveChangesAsync();
                var count = _db.ShoppingCart.Where(c => c.ApplicationUserId == value.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32(SD.CartCountSession, count);
                return RedirectToAction(nameof(Index));
            }
            else
            {

                var menu = await _db.Menu.Include(m => m.Category).Include(m => m.SubCategory).Where(m => m.Id == value.MenuId).FirstOrDefaultAsync();

                ShoppingCart shopping = new ShoppingCart()
                {
                    Menu = menu,
                    MenuId = menu.Id
                };
                return View(shopping);

            }
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
    }
}
