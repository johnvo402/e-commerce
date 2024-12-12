using App.Model;
using App.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using System;
using System.Linq;

namespace App.Controllers
{
    [RedirectAdmin]
    public class WalletController : Controller
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public readonly AppDbContext _dbContext;
        private readonly E_CommerceContext _context;

        public WalletController(UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext dbContext, E_CommerceContext context, INotyfService inotiyfyservice)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _context = context;
            _Inotiyfyservice = inotiyfyservice;
        }

        public INotyfService _Inotiyfyservice { get; }



        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var wallet = _context.Wallets.FirstOrDefault(e => e.IdAcc == userId);
            if (wallet == null)
            {
                wallet = new Wallet()
                {
                    IdWallet = $"WL{Guid.NewGuid()}",
                    Balance = 0,
                    IdAcc = userId,
                    Status = 1
                };
                _context.Wallets.Add(wallet);
                _context.SaveChanges();

            }
            return View(wallet);
        }

        [HttpPost]
        public IActionResult EditAccount(Wallet wallet)
        {
            int number;
            if (int.TryParse(wallet.NumberCard, out number))
            {
                var userId = _userManager.GetUserId(User);
                var wallets = _context.Wallets.FirstOrDefault(e => e.IdAcc == userId);
                wallets.NameBank = wallet.NameBank;
                wallets.NameCard = wallet.NameCard;
                wallets.NumberCard = wallet.NumberCard;
                _context.SaveChanges();
                _Inotiyfyservice.Success("Update successfully!", 3);

                return RedirectToAction("Index");
            }
            _Inotiyfyservice.Error("Wrong Number card !", 3);
            return RedirectToAction("Index");

        }

        [HttpPost]
        public IActionResult Withdraw(Wallet wallet)
        {
            var uId = _userManager.GetUserId(User);
            var wal = _context.Wallets.FirstOrDefault(e => e.IdAcc == uId);
            if (wal.NameBank == null || wal.NameCard == null || wal.NumberCard == null)
            {
                _Inotiyfyservice.Error("Please create your card information!", 5);
            }
            else
            {
                if (wallet.Request < 50000)
                {
                    _Inotiyfyservice.Error("The value must be greater than or equal to 50000.!", 5);
                }
                else
                {
                    var userId = _userManager.GetUserId(User);
                    var wallets = _context.Wallets.FirstOrDefault(e => e.IdAcc == userId);
                    if (wallets.Status == 2)
                    {
                        _Inotiyfyservice.Warning("You have another request!", 3);
                        return RedirectToAction("Index");
                    }
                    if (wallet.Request > wallets.Balance)
                    {
                        _Inotiyfyservice.Warning("Exceed current balance!", 3);
                        return RedirectToAction("Index");
                    }
                    wallets.Request = wallet.Request;
                    wallets.Status = 2;
                    _context.SaveChanges();
                    _Inotiyfyservice.Success("Request withdraw successfully!", 3);
                }



            }


            return RedirectToAction("Index");
        }

        public IActionResult Cancel(string id)
        {
            var wallet = _context.Wallets.SingleOrDefault(e => e.IdWallet == id);
            wallet.Request = 0;
            wallet.Status = 1;
            _context.SaveChanges();
            _Inotiyfyservice.Success("Cancel successfully!", 3);
            return RedirectToAction("Index");
        }
    }
}
