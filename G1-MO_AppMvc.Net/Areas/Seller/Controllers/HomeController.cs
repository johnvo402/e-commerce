using App.Data;
using App.Model;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace App.Areas.Seller.Controllers
{
	[Area("Seller")]
	[Authorize(Roles = RoleName.Seller)]

	public class HomeController : Controller
	{
		private readonly E_CommerceContext _context;
		private readonly UserManager<AppUser> _userManager;
		private readonly SignInManager<AppUser> _signInManager;

		public HomeController(E_CommerceContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
		{
			_context = context;
			_userManager = userManager;
			_signInManager = signInManager;
		}


		[HttpGet("/seller/")]
		public IActionResult Index()
		{

			string userId = _userManager.GetUserId(User);
			return View();
		}


	}
}
