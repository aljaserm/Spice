using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using Spice.ViewModels;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class OrdersController : Controller
    {
        private readonly ApplicationDbContext _db;
        private int PageSize = 2;
        public OrdersController(ApplicationDbContext db)
        {
            _db = db;
        }

        [Authorize]
        public async Task<IActionResult> Confirm(int id)
        {
            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            OrderDeatilsVM orderDeatilsVM = new OrderDeatilsVM()
            {
                orders = await _db.Orders.Include(o => o.ApplicationUser).FirstOrDefaultAsync(o => o.Id == id && o.ApplicationUserId == claim.Value),
                lstOrderDetails = await _db.OrderDetails.Where(o => o.OrderId == id).ToListAsync()
            };
            return View(orderDeatilsVM);
        }


        [Authorize]
        public async Task<IActionResult> OrderHistory(int productPage=1)
        {
            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            OrderListVM orderListVM = new OrderListVM()
            {
                Orders = new List<OrderDeatilsVM>()
            };



            List<Orders> orders = await _db.Orders.Include(o => o.ApplicationUser).Where(o => o.ApplicationUserId == claim.Value).ToListAsync();
            foreach (Orders order in orders)
            {
                OrderDeatilsVM deatilsVM = new OrderDeatilsVM
                {
                    orders = order,
                    lstOrderDetails = await _db.OrderDetails.Where(o => o.OrderId == order.Id).ToListAsync()
                };
                orderListVM.Orders.Add(deatilsVM);
            }

            var count = orderListVM.Orders.Count;
            orderListVM.Orders = orderListVM.Orders.OrderByDescending(p => p.orders.Id).Skip((productPage - 1) * PageSize).Take(PageSize).ToList();

            orderListVM.pagingInfo = new PagingInfo
            {
                CurrentPage=productPage,
                ItemsPerPage=PageSize,
                TotalItem=count,
                urlParam= "/Customer/Orders/OrderHistory?productPage=:"
            };

            return View(orderListVM);
        }


        [Authorize(Roles = SD.Kitchen + "," + SD.Manager)]
        public async Task<IActionResult> ManageOrder()
        {
            List<OrderDeatilsVM> orderDetailsVM = new List<OrderDeatilsVM>();
            List<Orders> OrderHeaderList = await _db.Orders.Where(o => o.Status == SD.OrderSubmitted || o.Status == SD.OrderProcess).OrderByDescending(u => u.PicupTime).ToListAsync();
            foreach (Orders order in OrderHeaderList)
            {
                OrderDeatilsVM orderDeatils = new OrderDeatilsVM
                {
                    orders = order,
                    lstOrderDetails = await _db.OrderDetails.Where(o => o.OrderId == order.Id).ToListAsync()
                };
                orderDetailsVM.Add(orderDeatils);
            }
            return View(orderDetailsVM.OrderBy(o => o.orders.PicupTime).ToList());
        }

        [Authorize(Roles = SD.Kitchen + "," + SD.Manager)]
        public async Task<IActionResult> OrderPrepare(int OrderId)
        {
            Orders order = await _db.Orders.FindAsync(OrderId);
            order.Status = SD.OrderProcess;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageOrder), order);
        }

        [Authorize(Roles = SD.Kitchen + "," + SD.Manager)]
        public async Task<IActionResult> OrderReady(int OrderId)
        {
            Orders order = await _db.Orders.FindAsync(OrderId);
            order.Status = SD.OrderReady;
            await _db.SaveChangesAsync();

            //TODO Email to customer
            return RedirectToAction(nameof(ManageOrder), order);
        }

        [Authorize(Roles = SD.Kitchen + "," + SD.Manager)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            Orders order = await _db.Orders.FindAsync(OrderId);
            order.Status = SD.OrderCancel;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(ManageOrder), order);
        }


        public async Task<IActionResult> GetOrderDetails(int id)
        {
            OrderDeatilsVM orderDeatilsVM = new OrderDeatilsVM()
            {
                orders= await _db.Orders.FirstOrDefaultAsync(m=>m.Id==id),
                lstOrderDetails = await _db.OrderDetails.Where(m => m.OrderId == id).ToListAsync()
            };
            orderDeatilsVM.orders.ApplicationUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u=>u.Id== orderDeatilsVM.orders.ApplicationUserId);
            return PartialView("_IndividualOrderDetails",orderDeatilsVM);
        }

        public IActionResult GetOrderStatus(int Id)
        {
            return PartialView("_OrderStatus", _db.Orders.Where(m => m.Id == Id).FirstOrDefault().Status);

        }
        public IActionResult Index()
        {
            return View();
        }
    }
}