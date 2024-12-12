using App.Data;
using App.Models;
using App.Model;
using App.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;

namespace App.Controllers
{
	[RedirectAdmin]
	public class HomeController : Controller
	{
		private string admin = GetMD5("0");
		private string seller = GetMD5("1");
		private string buyer = GetMD5("2");
		private readonly UserManager<AppUser> _userManager;
		private readonly RoleManager<IdentityRole> _roleManager;
		public readonly AppDbContext _dbContext;
		private readonly E_CommerceContext _context;



		private readonly ILogger<HomeController> _logger;
		E_CommerceContext objModel = new E_CommerceContext();

		public HomeController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext dbContext, ILogger<HomeController> logger, E_CommerceContext context)
		{
			_userManager = userManager;
			_roleManager = roleManager;
			_dbContext = dbContext;
			_logger = logger;
			_context = context;
		}

		public async Task<IActionResult> SeedDataAsync()
		{
			var rolenames = typeof(RoleName).GetFields().ToList();
			foreach (var r in rolenames)
			{
				var rolename = (string)r.GetRawConstantValue();
				var rfound = await _roleManager.FindByNameAsync(rolename);
				if (rfound == null)
				{
					await _roleManager.CreateAsync(new IdentityRole(rolename));
				}
			}
			//admin , admin123, admin@example.com
			var useradmin = await _userManager.FindByEmailAsync("admin");
			if (useradmin == null)
			{
				useradmin = new AppUser()
				{
					UserName = "admin",
					Email = "admin@example.com",
					EmailConfirmed = true,
				};
				await _userManager.CreateAsync(useradmin, "admin123");
				await _userManager.AddToRoleAsync(useradmin, RoleName.Administrator);
			}
			

			return View("Index");
		}


		public IActionResult Index()
		{
            var IdAccLoggedIn = _userManager.GetUserId(User);
            //check cookies, lấy ra cookies để kiểm tra

            //Danh sách Category
            var lsCategory = _context.Categories
                .AsNoTracking()
                .OrderByDescending(x => x.NameCate)
                .ToList();
            ViewBag.lsCategory = lsCategory;

			//Danh sách tất cả sp bestsaler chưa join====================================================
			List<Product> lsProduct = new List<Product>();
			

			lsProduct = _context.Products
                                .AsNoTracking()
								.Include(p => p.ProductItems).Include(i => i.ImgPros)
								.Where(u => u.StatusPro == 1 && u.IdAcc != IdAccLoggedIn)
								.OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate).Take(8).ToList();

			
			


			var query = from p in _context.Products
						join c in _context.Categories on p.IdCate equals c.IdCate
						join pI in _context.ProductItems on p.IdPro equals pI.IdPro
						select new
						{
							Product = p,
							ProductItem = pI,
							Category = c
						};
			//Lấy ra một object
			var result = query.FirstOrDefault(); // Get the first or default item
			if (result != null)
			{
                var product = new ProCateProItemViewModel
                {
                    products = result.Product,
                    productItems = result.ProductItem,
                    categories = result.Category
                };
                ViewBag.Product = product;

            }
			var voucher = _context.Vouchers.Where(x=>x.UsageStatus == 1 && x.DeleteStatus == false && x.StartDate <= DateTime.Now).ToList();
			ViewBag.Voucher = voucher;




            return View(lsProduct);
		}


		

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
		public IActionResult Error()
		{
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
		}
		

		//create a string MD5
		public static string GetMD5(string str)
		{
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] fromData = Encoding.UTF8.GetBytes(str);
			byte[] targetData = md5.ComputeHash(fromData);
			string byte2String = null;

			for (int i = 0; i < targetData.Length; i++)
			{
				byte2String += targetData[i].ToString("x2");

			}
			return byte2String;
		}

	}
}
