using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.UI.Services;
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
        private readonly IEmailSender _es;
        private int PageSize = 2;
        public OrdersController(ApplicationDbContext db, IEmailSender es)
        {
            _db = db;
            _es = es;
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
        public async Task<IActionResult> OrderHistory(int productPage = 1)
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
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = "/Customer/Orders/OrderHistory?productPage=:"
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
            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var subject = "Spice -- Order " + order.Id + " Ready";
            var email = _db.Users.Where(u => u.Id == claim.Value).FirstOrDefault();
            var name = _db.ApplicationUsers.Where(u => u.Id == claim.Value).FirstOrDefault();
            var body = "Your Order is ready for pick up";
            await _es.SendEmailAsync(name.Email, subject, body);
            return RedirectToAction(nameof(ManageOrder), order);
        }

        [Authorize(Roles = SD.Kitchen + "," + SD.Manager)]
        public async Task<IActionResult> OrderCancel(int OrderId)
        {
            Orders order = await _db.Orders.FindAsync(OrderId);
            order.Status = SD.OrderCancel;
            await _db.SaveChangesAsync();
            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var subject = "Spice -- Order " + order.Id + " Canceled";
            var email = _db.Users.Where(u => u.Id == claim.Value).FirstOrDefault();
            var name = _db.ApplicationUsers.Where(u => u.Id == claim.Value).FirstOrDefault();
            var body = "Your Order is canceled";
            await _es.SendEmailAsync(name.Email, subject, body);
            return RedirectToAction(nameof(ManageOrder), order);
        }


        public async Task<IActionResult> GetOrderDetails(int id)
        {
            OrderDeatilsVM orderDeatilsVM = new OrderDeatilsVM()
            {
                orders = await _db.Orders.FirstOrDefaultAsync(m => m.Id == id),
                lstOrderDetails = await _db.OrderDetails.Where(m => m.OrderId == id).ToListAsync()
            };
            orderDeatilsVM.orders.ApplicationUser = await _db.ApplicationUsers.FirstOrDefaultAsync(u => u.Id == orderDeatilsVM.orders.ApplicationUserId);
            return PartialView("_IndividualOrderDetails", orderDeatilsVM);
        }

        public IActionResult GetOrderStatus(int Id)
        {
            return PartialView("_OrderStatus", _db.Orders.Where(m => m.Id == Id).FirstOrDefault().Status);

        }

        [Authorize]
        public async Task<IActionResult> OrderPickup(int productPage = 1, string searchName = null, string searchEmail = null, string searchPhone = null)
        {
            //var CIdentity = (ClaimsIdentity)this.User.Identity;
            //var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            OrderListVM orderListVM = new OrderListVM()
            {
                Orders = new List<OrderDeatilsVM>()
            };

            StringBuilder sb = new StringBuilder();
            sb.Append("/Customer/Orders/OrderPickup?productPage=:");
            sb.Append("&searchName=");
            if (searchName != null)
            {
                sb.Append(searchName);
            }
            sb.Append("&searchEmail=");
            if (searchEmail != null)
            {
                sb.Append(searchEmail);
            }
            sb.Append("&searchPhone=");
            if (searchPhone != null)
            {
                sb.Append(searchPhone);
            }
            List<Orders> orders = new List<Orders>();
            if (searchName != null || searchEmail != null || searchPhone != null)
            {
                var user = new ApplicationUser();
                if (searchName != null)
                {
                    orders = await _db.Orders.Include(o => o.ApplicationUser).Where(u => u.PickupName.ToLower().Contains(searchName.ToLower())).OrderByDescending(o => o.OrderDate).ToListAsync();
                }
                else
                if (searchEmail != null)
                {
                    user = await _db.ApplicationUsers.Where(u => u.Email.ToLower().Contains(searchEmail.ToLower())).FirstOrDefaultAsync();
                    orders = await _db.Orders.Include(o => o.ApplicationUser).Where(o => o.ApplicationUserId == user.Id).OrderByDescending(o => o.OrderDate).ToListAsync();
                }
                else
                if (searchPhone != null)
                {
                    orders = await _db.Orders.Include(o => o.ApplicationUser).Where(u => u.Phone.Contains(searchPhone)).OrderByDescending(o => o.OrderDate).ToListAsync();
                }
            }
            else
            {
                orders = await _db.Orders.Include(o => o.ApplicationUser).Where(o => o.Status == SD.OrderReady).ToListAsync();
            }
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
                CurrentPage = productPage,
                ItemsPerPage = PageSize,
                TotalItem = count,
                urlParam = sb.ToString()
            };

            return View(orderListVM);
        }

        [Authorize(Roles =SD.FrontDesk+","+SD.Manager)]
        [HttpPost]
        //[ActionName("OrderPickup")]
        public async Task<IActionResult> OrderPickup(OrderDeatilsVM value)
        {
            Orders order = await _db.Orders.FindAsync(value.orders.Id);
            order.Status = SD.OrderDone;
            await _db.SaveChangesAsync();
            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var subject = "Spice -- Order " + order.Id + " Picked";
            var email = _db.Users.Where(u => u.Id == claim.Value).FirstOrDefault();
            var name = _db.ApplicationUsers.Where(u => u.Id == claim.Value).FirstOrDefault();
            var body = "Your Order is picked up";
            await _es.SendEmailAsync(name.Email, subject, body);
            return RedirectToAction(nameof(OrderPickup), order);
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}