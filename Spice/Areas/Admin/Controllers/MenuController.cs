using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.ViewModels;

namespace Spice.Areas.Admin.Controllers
{
    [Area("Admin")]
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

        //get Create
        public IActionResult Create()
        {
            return View(vM);
        }


        public async Task<IActionResult> Index()
        {
            var menu = await _db.Menu.Include(m=>m.Category).Include(m=>m.SubCategory).ToListAsync();
            return View(menu);
        }
    }
}