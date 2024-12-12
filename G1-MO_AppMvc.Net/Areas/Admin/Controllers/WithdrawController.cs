using App.Data;
using App.Model;
using App.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace App.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = RoleName.Administrator)]
    [Route("/RequestWithdraw/[action]")]
    public class WithdrawController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public readonly AppDbContext _dbContext;
        private readonly E_CommerceContext _context;

        public WithdrawController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext dbContext, E_CommerceContext context, INotyfService inotiyfyservice)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _context = context;
            _Inotiyfyservice = inotiyfyservice;
        }

        public INotyfService _Inotiyfyservice { get; }



        public IActionResult IndexWithdraw()
        {

            var wallet = _context.Wallets.Include(e => e.IdAccNavigation).Where(e => e.Status == 2).ToList();

            return View(wallet);
        }
        [HttpPost]
        public IActionResult TransferMoney(string id)
        {
            var wallet = _context.Wallets.FirstOrDefault(e => e.IdWallet == id);
            wallet.Balance = wallet.Balance - (wallet.Request-(wallet.Request * 5/100));
            wallet.Status = 1;
            wallet.Request = 0;
            _context.SaveChanges();
            _Inotiyfyservice.Success("Thank u!");
            return RedirectToAction("IndexWithdraw", "RequestWithdraw");
        }
    }
}
