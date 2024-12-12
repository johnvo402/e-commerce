using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using AspNetCoreHero.ToastNotification.Abstractions;
using App.Extension;
using App.Model;
using App.ModelViews;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Text.Json.Serialization;
using System.Text.Json;
using IdentityServer4.Extensions;

namespace App.Controllers
{
    [RedirectAdmin]

    public class ShoppingCartController : Controller
    {
        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        public INotyfService _Inotiyfyservice { get; }

        public ShoppingCartController(E_CommerceContext context, INotyfService notyfService, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _Inotiyfyservice = notyfService;
        }

        //Tạo giỏ hàng
        //Nếu có thì lấy giỏ hàng củ ra
        //Nếu chưa có thì tạo mới một giỏ hàng rỗng


        public string GenerateNextCartId()
        {
            // Retrieve the maximum existing Id_pro
            string maxIdAcc = _context.ShoppingCarts
                .Select(p => p.IdCart)
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

            string nextIdAcc = $"CA{nextNumber:D3}"; // Format the new Id_acc

            return nextIdAcc;
        }
        public string GenerateNextCartItemId()
        {
            // Retrieve the maximum existing Id_pro
            string maxIdAcc = _context.ShoppingCartItems
                .Select(p => p.IdCartItem)
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

            string nextIdAcc = $"CI{nextNumber:D3}"; // Format the new Id_acc

            return nextIdAcc;
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





        [HttpPost]
        [Route("api/cart/add")]

        public ActionResult AddToCart(string idProduct, string idProductItem, int quantity, float price)
        {
            try
            {
                if (idProductItem == null)
                {
                    _Inotiyfyservice.Error("Please select a category!", 3);
                    return Json(new { success = true });
                }
                //List<ShoppingCartItem> cartItem = Cart;
                string userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    // Lấy ID của người dùng đang đăng nhập
                    string idCartNew = null;
                    // Kiểm tra xem giỏ hàng đã tồn tại trong cơ sở dữ liệu chưa, nếu chưa tạo mới
                    ShoppingCart cart = _context.ShoppingCarts.SingleOrDefault(c => c.IdAcc == userId); // Thay "default" bằng id_acc của người dùng đăng nhập nếu có
                    if (cart == null)
                    {
                        idCartNew = GenerateNextCartId();
                        cart = new ShoppingCart { IdCart = idCartNew, IdAcc = userId }; // Thay "default" bằng id_acc của người dùng đăng nhập nếu có
                        _context.ShoppingCarts.Add(cart);
                        _context.SaveChanges();
                    }
                    else
                    {
                        idCartNew = cart.IdCart;
                    }

                    // Kiểm tra xem sản phẩm đã tồn tại trong giỏ hàng chưa
                    ShoppingCartItem existingItem = _context.ShoppingCartItems.FirstOrDefault(item => item.IdProItem == idProductItem && item.IdCart == idCartNew);
                    ShoppingCartItem cartExit = cart.ShoppingCartItems.FirstOrDefault(item => item.IdProItem == idProductItem && item.IdCart == idCartNew);
                    var proitem = _context.ProductItems.FirstOrDefault(e => e.IdProItem == idProductItem);
                    if (existingItem != null)
                    {
                        var totalquan = existingItem.Quantity + quantity;
                        if (totalquan > proitem.Quantity)
                        {
                            _Inotiyfyservice.Warning(" Exceeding the available quantity of the product", 5);
                            return Json(new { success = true });
                        }

                        existingItem.Quantity += quantity; // Nếu đã tồn tại, tăng số lượng lên
                                                           //cartExit.Quantity += quantity;
                                                           //HttpContext.Session.Set<List<ShoppingCartItem>>("Cart", cartItem);
                    }
                    else
                    {
                        if (quantity > proitem.Quantity)
                        {
                            _Inotiyfyservice.Warning(" Exceeding the available quantity of the product", 5);
                            return Json(new { success = true });
                        }
                        // Nếu chưa tồn tại, thêm mới vào giỏ hàng
                        ShoppingCartItem newItem = new ShoppingCartItem
                        {
                            IdCartItem = GenerateNextCartItemId(), // Tạo một id_cart_item duy nhất
                            IdCart = idCartNew,
                            IdProItem = idProductItem,
                            Quantity = quantity,
                            Price = price,
                            IdPro = idProduct,
                        };

                        cart.ShoppingCartItems.Add(newItem);
                        //cartItem.Add(newItem);
                        //HttpContext.Session.Set<List<ShoppingCartItem>>("Cart", cartItem);

                    }

                    _context.SaveChanges(); // Lưu thay đổi vào cơ sở dữ liệu

                    _Inotiyfyservice.Success("Add product successfully", 2);
                    return Json(new { success = true });
                    // Trả về kết quả thành công
                }

                return Json(new { success = false });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message }); // Trả về thông báo lỗi
            }
        }



        [HttpGet]
        public IActionResult Remove(string id)
        {

            try
            {

                //Thêm sp vào giỏ hàng
                var item = _context.ShoppingCartItems.SingleOrDefault(p => p.IdCartItem == id);
                //Đã có cập nhật số lượng
                if (item != null)
                {
                    _context.ShoppingCartItems.Remove(item);
                    _context.SaveChanges();
                }
                else
                {
                    // Nếu không tìm thấy mục trong giỏ hàng, bạn có thể thực hiện xử lý tùy ý, ví dụ như throw exception hoặc hiển thị thông báo lỗi
                    throw new Exception("Không tìm thấy mục trong giỏ hàng để xóa.");
                }


                //Lưu lại session
                _Inotiyfyservice.Success("Remove product successfully!", 3);
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }


        [Route("cart", Name = "Cart")]
        public IActionResult Index()
        {
            List<ShoppingCartItem> shopCartItems = null;

            string userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                ShoppingCart cart = _context.ShoppingCarts.SingleOrDefault(c => c.IdAcc == userId);
                if (cart != null)
                {
                    shopCartItems = _context.ShoppingCartItems.Include(p => p.IdProItemNavigation).Include(e => e.IdProNavigation.ImgPros).Include(pr => pr.IdProNavigation.ProductItems)
               .Where(item => item.IdCart == cart.IdCart).ToList();


                    return View(shopCartItems);
                }
                _Inotiyfyservice.Warning("Cart is empty, please add products!", 5);
                return RedirectToAction("Index", "Home");

            }
            else
            {

                return RedirectToAction("Index", "Home");
            }


        }
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(int quantity, string iditem)
        {

            var userid = _userManager.GetUserId(User);
            var cartitem = new ShoppingCartItem();
            var cart = await _context.ShoppingCarts.FirstOrDefaultAsync(u => u.IdAcc == userid);
            if (cart != null)
            {
                var proitem = _context.ProductItems.FirstOrDefault(e => e.IdProItem == iditem);
                cartitem = await _context.ShoppingCartItems.FirstOrDefaultAsync(u => u.IdCart == cart.IdCart && u.IdProItem == iditem);
                if (cartitem != null)
                {

                    if (quantity <= proitem.Quantity)
                    {

                        if (quantity <= 0 || quantity == null)
                        {
                            cartitem.Quantity = 1;
                        }
                        else
                        {
                            cartitem.Quantity = quantity;
                        }
                        _context.ShoppingCartItems.Update(cartitem);
                        await _context.SaveChangesAsync();
                    }




                }
            }
            List<ShoppingCartItem> shopCartItems = null;

            string userId = _userManager.GetUserId(User);
            if (userId != null)
            {
                ShoppingCart carts = _context.ShoppingCarts.SingleOrDefault(c => c.IdAcc == userId);
                if (cart != null)
                {
                    shopCartItems = _context.ShoppingCartItems.Include(p => p.IdProItemNavigation).Include(e => e.IdProNavigation.ImgPros).Include(pr => pr.IdProNavigation.ProductItems)
               .Where(item => item.IdCart == cart.IdCart).ToList();




                }



            }
            return PartialView("_ShoppingCartPartial", shopCartItems);
        }
        [HttpGet]
        public async Task<IActionResult> UpdateKind(string cartItemId, string proitemId)
        {
            var update = new UpdateCart();
            if (cartItemId == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(c => c.ProductItems.Where(e => e.StatusProItem == 1))
                .FirstOrDefaultAsync(p => p.IdPro == proitemId);

            var cartitem = await _context.ShoppingCartItems
                .FirstOrDefaultAsync(c => c.IdCartItem == cartItemId);

            if (product != null)
            {
                update = new UpdateCart
                {
                    proitem = product.ProductItems,
                    ShoppingCartItem = cartitem,
                };
            }

            return PartialView("_UpdateKind", update);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateKind(string idcartitem, string newitem, int newquantity)
        {
            if (idcartitem == null)
            {
                return NotFound();
            }
            var cartItem = _context.ShoppingCartItems.FirstOrDefault(c => c.IdCartItem == idcartitem);
            if (cartItem != null)
            {
                ShoppingCartItem existingItem = _context.ShoppingCartItems.FirstOrDefault(item => item.IdProItem == newitem && item.IdCart == cartItem.IdCart);
                if (existingItem != null)
                {
                    existingItem.Quantity += newquantity;
                    _context.ShoppingCartItems.Remove(cartItem);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var productItem = _context.ProductItems.FirstOrDefault(p => p.IdProItem == newitem);
                    if (productItem != null)
                    {
                        if (productItem.Quantity >= newquantity)
                        {
                            if (newquantity <= 0)
                            {
                                cartItem.Quantity = 1;
                            }
                            else
                            {
                                cartItem.Quantity = newquantity;
                            }
                            cartItem.IdProItem = newitem;
                            cartItem.Price = productItem.ProPrice;
                            _context.ShoppingCartItems.Update(cartItem);
                            await _context.SaveChangesAsync();
                            _Inotiyfyservice.Success("Change Product Successfully!", 3);
                        }
                        else
                        {
                            _Inotiyfyservice.Warning("Exceeding the available quantity of the product!", 5);
                        }
                    }





                }
            }

            return RedirectToAction(nameof(Index));
        }
        [HttpPost]
        public async Task<IActionResult> ApplyVoucher(string code)
        {
            if (code != null)
            {
                double value = 0; // Khởi tạo giá trị ban đầu của value
                List<ShoppingCartItem> shopCartItems = null;

                string userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    ShoppingCart cart = _context.ShoppingCarts.SingleOrDefault(c => c.IdAcc == userId);
                    if (cart != null)
                    {
                        shopCartItems = _context.ShoppingCartItems.Include(p => p.IdProItemNavigation).Include(e => e.IdProNavigation.ImgPros).Include(pr => pr.IdProNavigation.ProductItems)
                   .Where(item => item.IdCart == cart.IdCart).ToList();



                    }
                }
                float total = 0;
                if (shopCartItems != null)
                {
                    foreach (var item in shopCartItems)
                    {
                        total += item.Quantity * (float)item.Price;
                    }
                }
                var voucher = await _context.Vouchers.FirstOrDefaultAsync(v => v.PromotionCode == code && v.UsageStatus == 1 && v.DeleteStatus == false); // Sử dụng FirstOrDefaultAsync trực tiếp
                if (voucher != null)
                {
                    if (voucher.MinValue != null)
                    {
                        if (total < voucher.MinValue)
                        {
                            double minValue = (double)voucher.MinValue;
                            string formattedValue = minValue.ToString("#,##0");
                            HttpContext.Session.Remove("VoucherCode");
                            return Json(new { success = true, value = -1, mess = "Your order must be over " + formattedValue.ToString() + "VND" });
                        }
                    }

                    if (voucher.DiscountType == 1)
                    {
                        if (voucher.MaxiValue != null)
                        {
                            if (total > voucher.MaxiValue)
                            {
                                value = (double)voucher.MaxiValue * (voucher.DiscountAmount / 100);

                            }
                            else
                            {

                                value = (double)total * (voucher.DiscountAmount / 100);
                            }
                        }
                        else
                        {

                            value = (double)total * (voucher.DiscountAmount / 100);
                        }
                    }
                    else
                    {
                        HttpContext.Session.SetString("VoucherCode", $"{code}");
                        value = voucher.DiscountAmount;
                    }
                }
                else
                {
                    HttpContext.Session.Remove("VoucherCode");
                    return Json(new { success = false }); // Trả về JSON báo lỗi nếu voucher không tồn tại
                }
                HttpContext.Session.SetString("VoucherCode", $"{code}");
                return Json(new { success = true, value = value }); // Trả về JSON với giá trị discount
            }
            else
            {
                HttpContext.Session.Remove("VoucherCode");
                return Json(new { success = true, value = -3 });
            }
        }

    }
}
