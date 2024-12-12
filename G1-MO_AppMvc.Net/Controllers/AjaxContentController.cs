using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using App.Extension;
using App.Model;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace App.Controllers
{
    public class AjaxContentController : Controller
    {
        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;

        public AjaxContentController(E_CommerceContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public List<ShoppingCartItem> ListCart(string cartId)
        {
            // Lấy danh sách sản phẩm từ bảng ShopCartItem dựa trên CartId
            List<ShoppingCartItem> shopCartItems = _context.ShoppingCartItems
                .Where(item => item.IdCart == cartId).ToList();


            // Trả về view và truyền danh sách sản phẩm đến view
            return shopCartItems;
        }
        public List<Wishlist> ListWish(string userId)
        {
            // Lấy danh sách sản phẩm từ bảng ShopCartItem dựa trên CartId
            List<Wishlist> wishlist = _context.Wishlists
                .Where(item => item.IdAcc == userId).ToList();


            // Trả về view và truyền danh sách sản phẩm đến view
            return wishlist;
        }

        public List<ShoppingCartItem> Cart
        {
            get
            {
                // Sử dụng SessionExtensions để lấy danh sách sản phẩm từ session
                var cart = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart");

                // Kiểm tra nếu giỏ hàng trong session không tồn tại
                // hoặc là một giỏ hàng mới được tạo, nếu không, trả về giỏ hàng từ session
                if (cart == null)
                {
                    // Tạo một danh sách mới nếu giỏ hàng không tồn tại trong session
                    cart = new List<ShoppingCartItem>();
                }

                // Trả về danh sách sản phẩm trong giỏ hàng
                return cart;
            }
        }

        public List<Wishlist> Wishlist
        {
            get
            {
                // Sử dụng SessionExtensions để lấy danh sách sản phẩm từ session
                var wish = HttpContext.Session.Get<List<Wishlist>>("Wishlist");

                // Kiểm tra nếu giỏ hàng trong session không tồn tại
                // hoặc là một giỏ hàng mới được tạo, nếu không, trả về giỏ hàng từ session
                if (wish == null)
                {
                    // Tạo một danh sách mới nếu giỏ hàng không tồn tại trong session
                    wish = new List<Wishlist>();
                }

                // Trả về danh sách sản phẩm trong giỏ hàng
                return wish;
            }
        }
        public IActionResult HeaderCart()
        {
            List<Wishlist> wishlist = Wishlist;
            string userId = _userManager.GetUserId(User);
            if (userId != null)
            {

                var list = ListWish(userId);
                int numDb = list.Count();
                int numSession = wishlist.Count();
                if (numDb > numSession || numDb < numSession)
                {
                    wishlist.Clear();
                    foreach (var item in list)
                    {
                        Wishlist itemnew = new Wishlist
                        {
                            IdAcc = item.IdAcc, // Tạo một id_cart_item duy nhất
                            IdPro = item.IdPro,
                            IdWishlist = item.IdWishlist,

                        };
                        wishlist.Add(itemnew);

                    }
                    HttpContext.Session.Set<List<Wishlist>>("Wishlist", wishlist);
                }

            }
            return ViewComponent("HeaderCart");
        }
        public IActionResult NumberCart()
        {

            List<ShoppingCartItem> cartItem = Cart;
            cartItem.Clear();
            string userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                ShoppingCart cart = _context.ShoppingCarts.SingleOrDefault(c => c.IdAcc == userId);
                var list = ListCart(cart.IdCart);
                int numDb = list.Count();
                int numSession = cartItem.Count();

                foreach (var item in list)
                {
                    ShoppingCartItem itemnew = new ShoppingCartItem
                    {
                        IdCartItem = item.IdCartItem, // Tạo một id_cart_item duy nhất
                        IdCart = item.IdCart,
                        IdProItem = item.IdProItem,
                        Quantity = item.Quantity,
                        Price = item.Price
                    };
                    cartItem.Add(itemnew);

                }
                HttpContext.Session.Set<List<ShoppingCartItem>>("Cart", cartItem);


            }

            return ViewComponent("NumberCart");
        }

    }
}