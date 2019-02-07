using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Spice.Data;
using Spice.Models;
using Spice.Utility;
using Spice.ViewModels;
using Stripe;

namespace Spice.Areas.Customer.Controllers
{
    [Area("Customer")]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _es;
        [BindProperty]
        public OrdersVM ordersVM { get; set; }
        public CartController(ApplicationDbContext db, IEmailSender es)
        {
            _db = db;
            _es = es;
        }
        public async Task<IActionResult> Index()
        {
            ordersVM = new OrdersVM()
            {
                Orders = new Models.Orders()
            };
            ordersVM.Orders.OrderTotal = 0;

            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            var shoppingCart = _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value);
            if(shoppingCart!= null)
            {
                ordersVM.lstshoppingCarts = shoppingCart.ToList();
            }
            foreach (var i in ordersVM.lstshoppingCarts)
            {
                i.Menu = await _db.Menu.FirstOrDefaultAsync(m=>m.Id== i.MenuId);
                ordersVM.Orders.OrderTotal = ordersVM.Orders.OrderTotal + (i.Menu.Price*i.Count);
                i.Menu.Descrption = SD.ConvertToRawHtml(i.Menu.Descrption);
                if (i.Menu.Descrption.Length > 100)
                {
                    i.Menu.Descrption = i.Menu.Descrption.Substring(0, 99) + " ...";
                }
            }
            ordersVM.Orders.OrderTotalOriginal = ordersVM.Orders.OrderTotal;
            if (HttpContext.Session.GetString(SD.CouponCodeSession) != null)
            {
                ordersVM.Orders.CouponCode = HttpContext.Session.GetString(SD.CouponCodeSession);
                var coupon = await _db.Coupons.Where(c => c.Name.ToLower() == ordersVM.Orders.CouponCode.ToLower()).FirstOrDefaultAsync();
                ordersVM.Orders.OrderTotal = SD.CalculateDiscount(coupon,ordersVM.Orders.OrderTotalOriginal);
            }
            return View(ordersVM);
        }





        public async Task<IActionResult> Summary()
        {
            ordersVM = new OrdersVM()
            {
                Orders = new Models.Orders()
            };
            ordersVM.Orders.OrderTotal = 0;

            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);
            ApplicationUser user = await _db.ApplicationUsers.Where(u => u.Id == claim.Value).FirstOrDefaultAsync();
            var shoppingCart = _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value);
            if (shoppingCart != null)
            {
                ordersVM.lstshoppingCarts = shoppingCart.ToList();
            }
            foreach (var i in ordersVM.lstshoppingCarts)
            {
                i.Menu = await _db.Menu.FirstOrDefaultAsync(m => m.Id == i.MenuId);
                ordersVM.Orders.OrderTotal = ordersVM.Orders.OrderTotal + (i.Menu.Price * i.Count);
                i.Menu.Descrption = SD.ConvertToRawHtml(i.Menu.Descrption);
            }
            ordersVM.Orders.OrderTotalOriginal = ordersVM.Orders.OrderTotal;
            ordersVM.Orders.PickupName = user.Name;
            ordersVM.Orders.Phone = user.PhoneNumber;
            ordersVM.Orders.PicupTime = DateTime.Now;

            if (HttpContext.Session.GetString(SD.CouponCodeSession) != null)
            {
                ordersVM.Orders.CouponCode = HttpContext.Session.GetString(SD.CouponCodeSession);
                var coupon = await _db.Coupons.Where(c => c.Name.ToLower() == ordersVM.Orders.CouponCode.ToLower()).FirstOrDefaultAsync();
                ordersVM.Orders.OrderTotal = SD.CalculateDiscount(coupon, ordersVM.Orders.OrderTotalOriginal);
            }
            return View(ordersVM);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ActionName("Summary")]
        public async Task<IActionResult> SummaryPost(string stripeEmail, string stripeToken)
        {
            var CIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = CIdentity.FindFirst(ClaimTypes.NameIdentifier);

            ordersVM.lstshoppingCarts = await _db.ShoppingCart.Where(s => s.ApplicationUserId == claim.Value).ToListAsync();
            ordersVM.Orders.PaymentStatus =SD.PayementPending;
            ordersVM.Orders.OrderDate =DateTime.Now;
            ordersVM.Orders.ApplicationUserId = claim.Value;
            ordersVM.Orders.Status =SD.PayementPending;
            ordersVM.Orders.PicupTime =Convert .ToDateTime(ordersVM.Orders.PicupTime.ToShortDateString()+" " + ordersVM.Orders.PicupTime.ToShortTimeString());

            List<OrderDetails> lstorderDetails = new List<OrderDetails>();
            _db.Orders.Add(ordersVM.Orders);
            await _db.SaveChangesAsync();
            ordersVM.Orders.OrderTotalOriginal = 0;

            foreach (var i in ordersVM.lstshoppingCarts)
            {
                i.Menu = await _db.Menu.FirstOrDefaultAsync(m => m.Id == i.MenuId);
                OrderDetails orderDetails = new OrderDetails
                {
                    MenuId = i.MenuId,
                    OrderId = ordersVM.Orders.Id,
                    Description = i.Menu.Descrption,
                    Name = i.Menu.Name,
                    Price = i.Menu.Price,
                    Count = i.Count
                };
                ordersVM.Orders.OrderTotalOriginal += orderDetails.Count * orderDetails.Price;
                _db.OrderDetails.Add(orderDetails);                
            }


            if (HttpContext.Session.GetString(SD.CouponCodeSession) != null)
            {
                ordersVM.Orders.CouponCode = HttpContext.Session.GetString(SD.CouponCodeSession);
                var coupon = await _db.Coupons.Where(c => c.Name.ToLower() == ordersVM.Orders.CouponCode.ToLower()).FirstOrDefaultAsync();
                ordersVM.Orders.OrderTotal = SD.CalculateDiscount(coupon, ordersVM.Orders.OrderTotalOriginal);
            }
            else
            {
                ordersVM.Orders.OrderTotal = ordersVM.Orders.OrderTotalOriginal;
            }
            ordersVM.Orders.CouponCodeDiscount = ordersVM.Orders.OrderTotalOriginal - ordersVM.Orders.OrderTotal;
            //await _db.SaveChangesAsync();

            _db.ShoppingCart.RemoveRange(ordersVM.lstshoppingCarts);
            HttpContext.Session.SetInt32(SD.CartCountSession,0);
            await _db.SaveChangesAsync();

            //Stripe
            if (stripeToken != null)
            {
                var customers = new CustomerService();
                var charges = new ChargeService();

                var customer = customers.Create(new CustomerCreateOptions
                {
                    Email = stripeEmail,
                    SourceToken = stripeToken
                });

                var charge = charges.Create(new ChargeCreateOptions
                {
                    Amount = Convert.ToInt32(ordersVM.Orders.OrderTotal*100),
                    Description = "Order #"+ ordersVM.Orders.Id,
                    Currency = "usd",
                    CustomerId = customer.Id
                });
                ordersVM.Orders.TransactionId = charge.BalanceTransactionId;
                if (charge.Status.ToLower() == "succeeded")
                {
                    var subject = "Spice -- Order " + ordersVM.Orders.Id+ " Submitted";
                    var email = _db.Users.Where(u => u.Id == claim.Value).FirstOrDefault();
                    var name = _db.ApplicationUsers.Where(u => u.Id == claim.Value).FirstOrDefault();
                    var body = "Dear <b>"+name.Name+"</b><br>Order <b>"+ ordersVM.Orders.Id+"</b> has been submitted successfully<br> We will notify you when your order is ready for pick up.<b>Order Total:</b> $"+ ordersVM.Orders.OrderTotal+ "<br> Thank you for using us<br> Lokking forward to serve next time.<br>Spice Management<br><b>The Spicer The Better</b>";
                    await _es.SendEmailAsync(name.Email,subject,body);
                    ordersVM.Orders.PaymentStatus = SD.PayementApproved;
                    ordersVM.Orders.Status = SD.OrderSubmitted;
                }
                else
                {
                    ordersVM.Orders.PaymentStatus = SD.PayementRejected;
                }
            }
            else
            {
                ordersVM.Orders.PaymentStatus = SD.PayementRejected;
            }
            await _db.SaveChangesAsync();
            
            //return RedirectToAction(nameof(Index));
            return RedirectToAction("Confirm", "Orders", new { id = ordersVM.Orders.Id });

        }



        public IActionResult AddCoupon()
        {
            if (ordersVM.Orders.CouponCode == null)
            {
                ordersVM.Orders.CouponCode = "";
            }
            HttpContext.Session.SetString(SD.CouponCodeSession, ordersVM.Orders.CouponCode);

            return RedirectToAction(nameof(Index));
        }

        public IActionResult RemoveCoupon()
        {
           
            HttpContext.Session.SetString(SD.CouponCodeSession,string.Empty);

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Plus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c=>c.Id== cartId);
            cart.Count += 1;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Minus(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            if (cart.Count == 1)
            {
                _db.ShoppingCart.Remove(cart);
                await _db.SaveChangesAsync();
                var count = _db.ShoppingCart.Where(c => c.ApplicationUser == cart.ApplicationUser).ToList().Count;
                HttpContext.Session.SetInt32(SD.CartCountSession,count);
            }
            else
            {
                cart.Count -= 1;
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        public async Task<IActionResult> Remove(int cartId)
        {
            var cart = await _db.ShoppingCart.FirstOrDefaultAsync(c => c.Id == cartId);
            _db.ShoppingCart.Remove(cart);
            await _db.SaveChangesAsync();
            var count = _db.ShoppingCart.Where(c => c.ApplicationUser == cart.ApplicationUser).ToList().Count;
            HttpContext.Session.SetInt32(SD.CartCountSession, count);
            return RedirectToAction(nameof(Index));
        }
        
    }
}