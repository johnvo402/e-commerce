using App.Areas.Identity.Controllers;
using App.Areas.Identity.Models.UserViewModels;
using App.Data;
using App.Model;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator)]
    [Route("/ManageRequest/[action]")]
    public class RegisterSellerController : Controller
    {
        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _contextapp;
        private readonly ILogger<RoleController> _logger;

        public RegisterSellerController(ILogger<RoleController> logger, AppDbContext contextapp, E_CommerceContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _contextapp = contextapp;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [TempData]
        public string StatusMessage { get; set; }

        [HttpGet]
        public async Task<IActionResult> IndexRequest(int? page)
        {
            var qr = await _userManager.GetUsersInRoleAsync("RequestSeller");
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            IQueryable<AppUser> queryableQr = qr.AsQueryable();
            IPagedList<AppUser> models = new PagedList<AppUser>(queryableQr, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(models);

           
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var account = await _context.Users
                .FirstOrDefaultAsync(m => m.Id == id);
            if (account == null)
            {
                return NotFound();
            }

            return View(account);
        }

        [HttpPost]
        public async Task<IActionResult> AcceptAsync(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            var role = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, role);
            await _userManager.AddToRoleAsync(user, RoleName.Seller);
            StatusMessage = "Accept successfully!";
            return RedirectToAction("IndexRequest");
        }

        [HttpPost]
        public async Task<IActionResult> CancelAsync(string id)
        {

            var user = await _userManager.FindByIdAsync(id);
            var role = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, role);
            await _userManager.AddToRoleAsync(user, RoleName.Buyer);
            StatusMessage = "Refused!";
            return RedirectToAction("IndexRequest");
        }
    }
}
