using App.Model;
using App.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace App.Controllers
{
    [Authorize]
    public class WishController : Controller
    {
        private readonly E_CommerceContext _context;
        public INotyfService _Inotiyfyservice { get; }
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public WishController(E_CommerceContext context, INotyfService inotiyfyservice, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _Inotiyfyservice = inotiyfyservice;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string GenerateNextWishListId()
        {
            // Retrieve the maximum existing Id_pro
            string maxIdAcc = _context.Wishlists
                .Select(p => p.IdWishlist)
                .OrderByDescending(id => id)
                .FirstOrDefault();

            // Generate the next Id_pro
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(maxIdAcc))
            {
                string numericPart = maxIdAcc.Substring(2); // Extract numeric part
                if (int.TryParse(numericPart, out int numericValue))
                {
                    nextNumber = numericValue + 1;
                }
            }

            string nextIdAcc = $"WL{nextNumber:D3}"; // Format the new Id_acc

            return nextIdAcc;
        }

        public IActionResult Index()
        {
            var userId = _userManager.GetUserId(User);
            var wishlist = _context.Wishlists.Include(p => p.IdProNavigation).ThenInclude(i => i.ImgPros).Include(pr => pr.IdProNavigation.ProductItems).Where(u => u.IdAcc == userId).ToList();
            return View(wishlist);
        }
        [HttpGet]
        public async Task<IActionResult> Create(string id, string url)
        {
            var userId = _userManager.GetUserId(User);
            var existingWishlistItem = await _context.Wishlists
     .FirstOrDefaultAsync(w => w.IdAcc == userId && w.IdPro == id);

            if (existingWishlistItem == null)
            {
                // Nếu sản phẩm chưa tồn tại trong Wishlist, thêm mới
                var wishlist = new Wishlist
                {
                    IdWishlist = GenerateNextWishListId(),
                    IdPro = id,
                    IdAcc = userId
                };

                await _context.Wishlists.AddAsync(wishlist);
                await _context.SaveChangesAsync();
                _Inotiyfyservice.Success("Add to wish list successfully!", 3);
            } else
            {
                _Inotiyfyservice.Warning("Product exist in your wish list!", 3);
            }
            return RedirectToAction("Index", url);
        }
        [HttpGet]
        public async Task<IActionResult> Remove(string id)
        {
            var wish = await _context.Wishlists.FindAsync(id);
            if (wish != null)
            {
                 _context.Wishlists.Remove(wish);
                await _context.SaveChangesAsync();
                _Inotiyfyservice.Success("Remove successfully", 2);
            }
            return RedirectToAction("Index");
        }
    }
}
