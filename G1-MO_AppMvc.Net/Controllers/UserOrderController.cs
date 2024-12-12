using App.Model;
using App.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Authorize]
    public class UserOrderController : Controller
    {
        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public UserOrderController(E_CommerceContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, INotyfService inotiyfyservice)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _Inotiyfyservice = inotiyfyservice;
        }

        public INotyfService _Inotiyfyservice { get; }
       

        //[HttpGet("/user/order/")]
        public async Task<IActionResult> Index(int? page,string type ="1")
        {
            List<Order> orders = new List<Order>();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 6;
            if (type =="1")
            {
                string userId = _userManager.GetUserId(User);
                orders = _context.Orders.Where(x => x.IdAcc == userId).Include(e => e.OrderDetails).ThenInclude(e => e.IdProItemNavigation)
                    .ThenInclude(e => e.IdProNavigation).ThenInclude(e => e.IdAccNavigation).OrderByDescending(e => e.IdOrder).ToList();
            } else if (type =="2")
            {
                string userId = _userManager.GetUserId(User);
                orders = _context.Orders.Where(x => x.IdAcc == userId && x.OrderInProgress == 1 && x.OrderStatus != 0).Include(e => e.OrderDetails).ThenInclude(e => e.IdProItemNavigation)
                    .ThenInclude(e => e.IdProNavigation).ThenInclude(e => e.IdAccNavigation).OrderByDescending(e => e.IdOrder).ToList();
                ViewBag.Active = 1;
            } else if (type =="3") {
                string userId = _userManager.GetUserId(User);
                orders = _context.Orders.Where(x => x.IdAcc == userId && x.OrderInProgress == 2 && x.OrderStatus != 0).Include(e => e.OrderDetails).ThenInclude(e => e.IdProItemNavigation)
                    .ThenInclude(e => e.IdProNavigation).ThenInclude(e => e.IdAccNavigation).OrderByDescending(e => e.IdOrder).ToList();
                ViewBag.Active = 2;
            }
            else if (type == "4")
            {
                string userId = _userManager.GetUserId(User);
                orders = _context.Orders.Where(x => x.IdAcc == userId && x.OrderInProgress == 3 && x.OrderStatus != 0).Include(e => e.OrderDetails).ThenInclude(e => e.IdProItemNavigation)
                    .ThenInclude(e => e.IdProNavigation).ThenInclude(e => e.IdAccNavigation).OrderByDescending(e => e.IdOrder).ToList();
                ViewBag.Active = 3;
            }
            else if (type == "5")
            {
                string userId = _userManager.GetUserId(User);
                orders = _context.Orders.Where(x => x.IdAcc == userId && x.OrderInProgress == 4 && x.OrderStatus != 0).Include(e => e.OrderDetails).ThenInclude(e => e.IdProItemNavigation)
                    .ThenInclude(e => e.IdProNavigation).ThenInclude(e => e.IdAccNavigation).OrderByDescending(e => e.IdOrder).ToList();
                ViewBag.Active = 4;
            }
            else if (type == "6")
            {
                string userId = _userManager.GetUserId(User);
                orders = _context.Orders.Where(x => x.IdAcc == userId && x.OrderStatus == 0).Include(e => e.OrderDetails).ThenInclude(e => e.IdProItemNavigation)
                    .ThenInclude(e => e.IdProNavigation).ThenInclude(e => e.IdAccNavigation).OrderByDescending(e => e.IdOrder).ToList();
                ViewBag.Active = 5;
            }
            ViewBag.Type = type;
            PagedList<Order> models = new PagedList<Order>(orders.AsQueryable(), pageNumber, pageSize);
            return View(models);
        }
        //public ActionResult CheckoutOrder()
        //{

        //	string userId = _userManager.GetUserId(User);
        //	List<Order> orders = _context.Orders.Where(x => x.IdAcc == userId).ToList();
        //	return View(orders);
        //}

        public ActionResult Received(string Id)
        {
            Order order = _context.Orders.Find(Id);
            order.OrderInProgress = 4;
            _context.SaveChanges();

            // Cập nhật sản phẩm
            List<OrderDetail> orderDetails = _context.OrderDetails.Where(x => x.IdOrder == Id).ToList();
            foreach (var item in orderDetails)
            {
                ProductItem productItem = _context.ProductItems
    .Include(pi => pi.IdProNavigation) // Include thông tin về sản phẩm liên kết với mục sản phẩm
    .FirstOrDefault(pi => pi.IdProItem == item.IdProItem);

                productItem.Quantity -= item.Quantity;
                var total = item.Quantity * item.Price;
                var user = productItem.IdProNavigation.IdAcc;
                var wallet = _context.Wallets.SingleOrDefault(e => e.IdAcc == user);
                wallet.Balance += total;
                item.Review = 0;
                _context.OrderDetails.Update(item);
                _context.SaveChanges();
                
            }
            _Inotiyfyservice.Success("Thank for !", 3);




            return RedirectToAction("Index", "UserOrder", new {type = "5"});
        }

        //[HttpGet("/userOrderDetail/")]
        public async Task<IActionResult> OrderDetails(string id)
        {
            var query = from order_detail in _context.OrderDetails
                        join product_item in _context.ProductItems
                        on order_detail.IdProItem equals product_item.IdProItem
                        where order_detail.IdOrder == id
                        select new
                        {
                            OrderDetail = order_detail,
                            ProductItem = product_item
                        };

            var result = await query.ToListAsync();
            var viewModelList = new List<ItemDetailViewModel>();

            foreach (var item in result)
            {
                viewModelList.Add(new ItemDetailViewModel
                {
                    orderDetail = item.OrderDetail,
                    productItem = item.ProductItem
                });
            }
            ViewBag.IdOrder = id;
            Order order = _context.Orders.Where(o => o.IdOrder == id).FirstOrDefault();
            ViewBag.OrderInProgress = order.OrderInProgress;

            return View(viewModelList);
        }

        [HttpGet("/User/CancelOrder")]
        public IActionResult CancelOrder( string id,string type = "1")
        {

            var order = _context.Orders.SingleOrDefault(e=>e.IdOrder == id);
            if (order != null)
            {
                order.OrderStatus = 0;
                order.OrderInProgress = 0;
                _context.SaveChanges();
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
                _Inotiyfyservice.Success("Cancel successfully!", 3);
            }


            
            return RedirectToAction("Index", "UserOrder", new {type = type});
        }

        [HttpGet("/User/ListCancelOrder")]
        public IActionResult userListOrderCanceled()
        {
            string userId = _userManager.GetUserId(User);
            var order = _context.Orders.Where(o => o.OrderStatus == 0 && o.IdAcc == userId);
            return View(order);
        }
        [HttpGet]
        public async Task<IActionResult> OrderDetail(string id)
        {
            var orders = await _context.Orders.Where(x => x.IdOrder == id).Include(e => e.OrderDetails).ThenInclude(e => e.IdProItemNavigation).ThenInclude(e => e.IdProNavigation).ThenInclude(e=>e.ImgPros).SingleOrDefaultAsync();
            if (orders.OrderInProgress == 4)
            {
                ViewBag.Active = 4;
            }
            
            return PartialView("_OrderDetailPartial",orders);
        }
    }
}
