using App.Model;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using System;
using System.IO;
using System.Linq;

namespace AppMvc.Net.Areas.Seller.Controllers
{
	public class ExcelController : Controller
	{
		private readonly E_CommerceContext _context;
		private readonly UserManager<AppUser> _userManager;
		public ExcelController(E_CommerceContext context, UserManager<AppUser> userManager)
		{
			_context = context;
			_userManager = userManager;
		}


		public ActionResult ExportProduct()
		{


			var valueIdAcc = _userManager.GetUserId(User);
			var listProduct = _context.Products.Where(p => p.IdAcc == valueIdAcc).ToList();


			var stream = new MemoryStream();

			using (var package = new ExcelPackage(stream))
			{
				var sheet = package.Workbook.Worksheets.Add("Product");
				////do du lieu vao sheet
				//sheet.Cells.LoadFromCollection(listProduct, true);

				sheet.Cells[1, 1].Value = "Day export";

				sheet.Cells[1, 2].Value = $"{DateTime.Now.Date.ToString("yyyy/MM/dd")}";

				sheet.Cells[2, 1].Value = "The establishment";

				sheet.Cells[2, 2].Value = _context.Users.Where(u => u.Id == valueIdAcc).FirstOrDefault().FullName;


				sheet.Cells[4, 1].Value = "Product ID";
				sheet.Cells[4, 2].Value = "Product name";
				sheet.Cells[4, 3].Value = "Product Description";

				int rowIndex = 6;



				foreach (var item in listProduct)
				{
					sheet.Cells[rowIndex, 1].Value = item.IdPro;
					sheet.Cells[rowIndex, 2].Value = item.Name;
					sheet.Cells[rowIndex, 3].Value = item.Description;
					rowIndex++;
				}

				//save
				package.Save();

			}

			stream.Position = 0;




			var fileName = $"ProductList_{DateTime.Now.Date.ToString("yyyyMMdd")}.xlsx";

			//return File(stream, "type=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
			return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

		}

		public ActionResult ExportOrder()
		{
			var userId = _userManager.GetUserId(User);

			var orders = _context.Orders
				.Include(o => o.OrderDetails)
				.ThenInclude(od => od.IdProItemNavigation)
				.ThenInclude(pi => pi.IdProNavigation)
				.Where(o => o.IdAcc == userId)
				.ToList();

			var stream = new MemoryStream();

			using (var package = new ExcelPackage(stream))
			{
				var sheet = package.Workbook.Worksheets.Add("Order");

				sheet.Cells[1, 1].Value = "Day export";
				sheet.Cells[1, 2].Value = $"{DateTime.Now.Date.ToString("yyyy/MM/dd")}";
				sheet.Cells[2, 1].Value = "The establishment";
				sheet.Cells[2, 2].Value = _context.Users.Where(u => u.Id == userId).FirstOrDefault().FullName;
				sheet.Cells[4, 1].Value = "Order ID";
				sheet.Cells[4, 2].Value = "Order date";
				sheet.Cells[4, 3].Value = "Order total";
				sheet.Cells[4, 4].Value = "Product Item";
				sheet.Cells[4, 5].Value = "Quantity";
				sheet.Cells[4, 6].Value = "Price Product Item";
				sheet.Cells[4, 7].Value = "Total";
				int rowIndex = 6;

				foreach (var order in orders)
				{
					sheet.Cells[rowIndex, 1].Value = order.IdOrder;
					sheet.Cells[rowIndex, 2].Value = order.OrderDate.ToString("yyyy/MM/dd");
					sheet.Cells[rowIndex, 3].Value = order.OrderTotal;

					rowIndex++;



					// Add Order Details
					foreach (var orderDetail in order.OrderDetails)
					{
						sheet.Cells[rowIndex, 4].Value = _context.ProductItems.Where(o => o.IdProItem == orderDetail.IdProItem).FirstOrDefault().Name; // Replace "Product Name" with the actual property that contains product name.
						sheet.Cells[rowIndex, 5].Value = orderDetail.Quantity;
						sheet.Cells[rowIndex, 6].Value = orderDetail.Price;
						sheet.Cells[rowIndex, 7].Value = orderDetail.OrderTotal;

						rowIndex++;
					}
				}

				package.Save();
			}

			stream.Position = 0;
			var fileName = $"Order_{DateTime.Now.Date.ToString("yyyyMMdd")}.xlsx";
			return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
		}

		//public ActionResult ExportOrder()
		//{


		//	var userId = _userManager.GetUserId(User);
		//	var orders = _context.Orders
		//	.Include(o => o.OrderDetails)
		//	 .ThenInclude(od => od.IdProItemNavigation)
		//		 .ThenInclude(pi => pi.IdProNavigation)
		//	.Where(o => o.IdAcc == userId)
		//	.ToList();

		//	var stream = new MemoryStream();

		//	using (var package = new ExcelPackage(stream))
		//	{
		//		var sheet = package.Workbook.Worksheets.Add("Order");
		//		////do du lieu vao sheet
		//		//sheet.Cells.LoadFromCollection(listProduct, true);

		//		sheet.Cells[1, 1].Value = "Day export";

		//		sheet.Cells[1, 2].Value = $"{DateTime.Now.Date.ToString("yyyy/MM/dd")}";

		//		sheet.Cells[2, 1].Value = "The establishment";

		//		sheet.Cells[2, 2].Value = _context.Users.Where(u => u.Id == userId).FirstOrDefault().FullName;


		//		sheet.Cells[4, 1].Value = "Order ID";
		//		sheet.Cells[4, 2].Value = "Order date";
		//		sheet.Cells[4, 3].Value = "Order total";

		//		int rowIndex = 6;



		//		if (orders.Count > 0)
		//		{
		//			foreach (var item in orders)
		//			{
		//				sheet.Cells[rowIndex, 1].Value = item.IdOrder;
		//				sheet.Cells[rowIndex, 2].Value = item.OrderDate.ToString("yyyy/MM/dd");
		//				sheet.Cells[rowIndex, 3].Value = item.OrderTotal;
		//				rowIndex++;
		//			}
		//		}

		//		//save
		//		package.Save();

		//	}

		//	stream.Position = 0;




		//	var fileName = $"Order_{DateTime.Now.Date.ToString("yyyyMMdd")}.xlsx";

		//	//return File(stream, "type=application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
		//	return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

		//}
		public IActionResult Index()
		{
			return View();
		}
	}
}
