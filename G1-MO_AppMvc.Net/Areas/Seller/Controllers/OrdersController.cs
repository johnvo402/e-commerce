using Microsoft.AspNetCore.Mvc;

using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using App.Model;
using Microsoft.AspNetCore.Authorization;
using App.Data;
using Microsoft.AspNetCore.Identity;
using App.Models;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace App.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = RoleName.Seller)]
    public class OrdersController : Controller
    {
        private readonly E_CommerceContext _context;

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public OrdersController(E_CommerceContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }
        

		
		[HttpGet("/seller/order/")]
		public async Task<IActionResult> Index()
		{
			var userId = _userManager.GetUserId(User);
			var orders = _context.Orders
		 .Include(o => o.OrderDetails)
			 .ThenInclude(od => od.IdProItemNavigation)
				 .ThenInclude(pi => pi.IdProNavigation)
		 .Where(o => o.OrderDetails.Any(e => e.IdProItemNavigation.IdProNavigation.IdAcc == userId)).OrderByDescending(o => o.IdOrder)
		 .ToList();

			return View(orders);
		}

		[HttpGet("/seller/cancel/")]
		public IActionResult Cancel()
		{
			return View();
		}

		[HttpGet("/Processed")]
		public ActionResult Processed()
		{
			string ID = HttpContext.Session.GetString("OrderId");
			Order order = _context.Orders.Find(ID);
			order.OrderInProgress = 2;
			_context.SaveChanges();
			return RedirectToAction("Index", "Orders", new { area = "Seller" });
		}



        [HttpGet("/Delivering")]
        public ActionResult Delivering(string ID)
        {

            Order order = _context.Orders.Find(ID);
            order.OrderInProgress = 3;
            _context.SaveChanges();
            return RedirectToAction("Index", "Orders", new { area = "Seller" });
        }



        [HttpGet("/CancelOrder")]
        public IActionResult CancelOrder(string ID)
        {

            Order order = _context.Orders.Find(ID);
            order.OrderStatus = 0;
            var user = order.IdAcc;
            var wallet = _context.Wallets.SingleOrDefault(e => e.IdAcc == user);
            if (order.OrderTotalDiscount != null && order.OrderTotalDiscount > 0)
            {
                wallet.Balance += (double)order.OrderTotalDiscount;
                _context.SaveChanges();
            }
            else
            {
                wallet.Balance += order.OrderTotal;
                _context.SaveChanges();
            }
            _context.SaveChanges();
            return RedirectToAction("Index", "Orders", new { area = "Seller" });
        }

        [HttpGet("/ListOrderCanceled")]
        public IActionResult ListOrderCanceled()
        {
            string userId = _userManager.GetUserId(User);
            var order = _context.Orders.Where(o => o.OrderStatus == 0 && o.IdAcc == userId);
            return View(order);
        }

        //[HttpGet("/seller/product/OrderCanceled/")]
        //public async Task<IActionResult> OrdersCanceled()
        //{
        //	var valueIdAcc = _userManager.GetUserId(User);
        //	var orders = await _context.Orders.Where(o => o.OrderStatus == 0 && o.IdAcc == valueIdAcc).ToListAsync();


        //	return View(orders);
        //}
    }
}
