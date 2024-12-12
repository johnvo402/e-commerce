using App.Data;
using App.Model;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Seller.Controllers
{
	[Area("Seller")]
	[Authorize(Roles = RoleName.Seller)]
	public class OrderDetailsController : Controller
	{
		private readonly E_CommerceContext _context;

		//public OrdersController(ECommerceContext context)
		//{
		//	_context = context;
		//}

		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;

		public OrderDetailsController(E_CommerceContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
		}
		[HttpGet("/orderDetail/")]
		public async Task<IActionResult> Index(string id)
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

			HttpContext.Session.SetString("OrderId", id);
			foreach (var item in result)
			{
				viewModelList.Add(new ItemDetailViewModel
				{
					orderDetail = item.OrderDetail,
					productItem = item.ProductItem
				});
			}

			ViewBag.IsProcessed = (_context.Orders.Find(id).OrderInProgress == 1) ? false : true;
			ViewBag.IdOrder = id;

			return View(viewModelList);




			//var data = await _context.OrderDetails.Where(o => o.IdOrder == id).ToListAsync();

			//return View(data);
		}


	}
}
