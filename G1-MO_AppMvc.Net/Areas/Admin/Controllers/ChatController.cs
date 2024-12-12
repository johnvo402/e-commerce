using App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace App.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Admin")]
    public class ChatController : Controller
	{
		[HttpGet("/chatadmin/")]
		public IActionResult Index()
		{
			return View();
		}
	}
}
