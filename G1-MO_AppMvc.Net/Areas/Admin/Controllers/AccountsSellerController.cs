using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using App.Areas.Admin.ViewModels;
using App.Data;
using Microsoft.AspNetCore.Authorization;
using App.Model;
using App.Areas.Identity.Controllers;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using App.Areas.Identity.Models.UserViewModels;
using App.ExtendMethods;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;

namespace App.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Admin")]
    [Route("/ManageSeller/[action]")]
    public class AccountsSellerController : Controller
    {
        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _contextapp;
        private readonly ILogger<RoleController> _logger;

        public AccountsSellerController(ILogger<RoleController> logger, AppDbContext contextapp, E_CommerceContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _contextapp = contextapp;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Accounts
        [TempData]
        public string StatusMessage { get; set; }

        //
        // GET: /ManageUser/Index
        [HttpGet]
        public async Task<IActionResult> IndexSeller(int? page)
        {
            var qr = await _userManager.GetUsersInRoleAsync("Seller");
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            IQueryable<AppUser> queryableQr = qr.AsQueryable();
            IPagedList<AppUser> models = new PagedList<AppUser>(queryableQr, pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;

            return View(models);
        }

        // GET: /ManageUser/AddRole/id
        [HttpGet("{id}")]
        public async Task<IActionResult> AddRoleAsync(string id)
        {
            // public SelectList allRoles { get; set; }
            var model = new AddUserRoleModel();

            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            model.BirthDate = model.user.BirthDate;
            model.HomeAdress = model.user.HomeAdress;
            model.UserName = model.user.UserName;
            model.UserEmail = model.user.Email;
            model.PhoneNumber = model.user.PhoneNumber;
            model.Gender = model.user.Gender;
            model.FullName = model.user.FullName;

            model.RoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray<string>();

            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            ViewBag.allRoles = new SelectList(roleNames);

           

            return View(model);
        }

        // GET: /ManageUser/AddRole/id
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleAsync(string id, AddUserRoleModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            model.user = await _userManager.FindByIdAsync(id);

            if (model.user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }
           

            var OldRoleNames = (await _userManager.GetRolesAsync(model.user)).ToArray();

            var deleteRoles = OldRoleNames.Where(r => !model.RoleNames.Contains(r));
            var addRoles = model.RoleNames.Where(r => !OldRoleNames.Contains(r));

            List<string> roleNames = await _roleManager.Roles.Select(r => r.Name).ToListAsync();

            ViewBag.allRoles = new SelectList(roleNames);

            var resultDelete = await _userManager.RemoveFromRolesAsync(model.user, deleteRoles);
            if (!resultDelete.Succeeded)
            {
                ModelState.AddModelError(resultDelete);
                return View(model);
            }

            var resultAdd = await _userManager.AddToRolesAsync(model.user, addRoles);
            model.user.HomeAdress = model.HomeAdress;
            model.user.BirthDate = model.BirthDate;
            model.user.Gender = model.Gender;
            model.user.FullName = model.FullName;
            model.user.PhoneNumber = model.PhoneNumber;

            await _userManager.UpdateAsync(model.user);
            if (!resultAdd.Succeeded)
            {
                ModelState.AddModelError(resultAdd);
                return View(model);
            }


            StatusMessage = "Update successfully!";

            return RedirectToAction("IndexSeller",model);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> SetPasswordAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            var user = await _userManager.FindByIdAsync(id);
            ViewBag.user = ViewBag;

            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            return View();
        }

        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPasswordAsync(string id, SetUserPasswordModel model)
        {
            if (string.IsNullOrEmpty(id))
            {
                return NotFound($"Không có user");
            }

            var user = await _userManager.FindByIdAsync(id);
            ViewBag.user = ViewBag;

            if (user == null)
            {
                return NotFound($"Không thấy user, id = {id}.");
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            await _userManager.RemovePasswordAsync(user);

            var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);
            if (!addPasswordResult.Succeeded)
            {
                foreach (var error in addPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }

            StatusMessage = $"Vừa cập nhật mật khẩu cho user: {user.UserName}";

            return RedirectToAction("IndexSeller");
        }

        // GET: Admin/Accounts/Details/5
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

       

        // GET: Admin/Accounts/Delete/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Disable(string id)
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

        // POST: Admin/Accounts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Disablefirmed(string id)
        {
            DateTime currentTime = DateTime.Now;
            var account = await _context.Users.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            account.Status = 0;
            account.BlockDate = currentTime;
            await _context.SaveChangesAsync();
            StatusMessage = "Disabled successfully!";
            return RedirectToAction(nameof(IndexSeller));

        }
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activation(string id)
        {
            
            var account = await _context.Users.FindAsync(id);
            if (account == null)
            {
                return NotFound();
            }
            account.Status = 1;
            account.BlockDate = null;
            await _context.SaveChangesAsync();
            StatusMessage = "Actived successfully!";
            return RedirectToAction(nameof(IndexSeller));

        }

        private bool AccountExists(string id)
        {
            return _context.Users.Any(e => e.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> Search(string name, int? page)
        {
            if (name != null)
            {
                var qr = await _userManager.GetUsersInRoleAsync("Seller");
                var user = qr.Where(u => u.FullName != null && u.FullName.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
                var pageNumber = page == null || page <= 0 ? 1 : page.Value;
                var pageSize = 5;
                IQueryable<AppUser> queryableQr = user.AsQueryable();
                IPagedList<AppUser> models = new PagedList<AppUser>(queryableQr, pageNumber, pageSize);
                ViewBag.CurrentPage = pageNumber;


                ViewBag.valuesearch = name;
                return View("IndexSeller", models);
               
            }
            else
            {
                return RedirectToAction("IndexSeller");
            }

        }
    }
}
