using App.Extension;
using App.Model;
using App.Models;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Controllers
{
    [RedirectAdmin]
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AppDbContext _contextapp;

        public CheckoutController(E_CommerceContext context, UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager, AppDbContext contextapp, INotyfService inotiyfyservice)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _contextapp = contextapp;
            _Inotiyfyservice = inotiyfyservice;
        }

        public INotyfService _Inotiyfyservice { get; }



        public async Task<IActionResult> Index(string? status)
        {
            if (status != null)
            {
                if (status == "cancel")
                {
                    _Inotiyfyservice.Warning("Cancel successfully!", 3);
                }
                else if (status == "error")
                {
                    _Inotiyfyservice.Error("There was an error during payment!", 5);
                }
                else if (status == "success")
                {


                    return RedirectToAction("Order", new { methodPayment = 2 });

                }
            }
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
            string voucherCode = HttpContext.Session.GetString("VoucherCode");
            double value = 0;
            var voucher = _context.Vouchers.Where(v => v.PromotionCode == voucherCode && v.UsageStatus == 1 && v.DeleteStatus == false).FirstOrDefault();
            if (voucher != null)
            {
                // Khởi tạo giá trị ban đầu của value

                float total = 0;
                if (shopCartItems != null)
                {
                    foreach (var item in shopCartItems)
                    {
                        total += item.Quantity * (float)item.Price;
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

                    value = voucher.DiscountAmount;
                }
                HttpContext.Session.SetString("Discount", $"{value}");
            }

            else
            {
                value = 0;
                HttpContext.Session.SetString("Discount", $"{value}");
            }



            ViewBag.DiscountVoucher = value;



            var user = await _userManager.GetUserAsync(User);

            if (userId != null)
            {
                ShoppingCart cart = await _context.ShoppingCarts.SingleOrDefaultAsync(c => c.IdAcc == userId);
                shopCartItems = _context.ShoppingCartItems.Include(p => p.IdProItemNavigation).Include(p => p.IdProNavigation)
                .Where(item => item.IdCart == cart.IdCart).ToList();


                ViewData["Cart"] = shopCartItems;

                return View(user);

            }
            return RedirectToAction("Index", "Home");
        }
        public string GenerateNextOrderId()
        {
            // Retrieve the maximum existing Id_pro
            string maxIdAcc = _context.Orders
                .Select(p => p.IdOrder)
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

            string nextIdAcc = $"OR{nextNumber:D3}"; // Format the new Id_acc

            return nextIdAcc;
        }
        public string GenerateNextOrderDetailId()
        {
            // Retrieve the maximum existing Id_pro
            string maxIdAcc = _context.OrderDetails
                .Select(p => p.IdOrderDetail).OrderByDescending(id => id)
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

            string nextIdAcc = $"OD{nextNumber:D3}"; // Format the new Id_acc

            return nextIdAcc;
        }
        [HttpPost]
        public async Task<IActionResult> CheckoutShipCod(AppUser user, int methodPayment)
        {

            if (user.FullName != null && user.PhoneNumber != null && user.HomeAdress != null && user.Email != null)
            {
                HttpContext.Session.Set("Usercheckout", user);
                if (methodPayment == 1)
                {
                    return RedirectToAction("Order", new { user = user, methodPayment });
                }
                else
                {


                    Order order = null;
                    List<ShoppingCartItem> shopCartItems = null;
                    var userId = _userManager.GetUserId(User);
                    var orderId = GenerateNextOrderId();
                    double orderTotal = 0;

                    ShoppingCart cart = await _context.ShoppingCarts.SingleOrDefaultAsync(c => c.IdAcc == userId);
                    shopCartItems = _context.ShoppingCartItems.Include(p => p.IdProItemNavigation).Include(p => p.IdProNavigation)
.Where(item => item.IdCart == cart.IdCart).ToList();
                    string discount = HttpContext.Session.GetString("Discount");
                    double d = 0;

                    if (!string.IsNullOrEmpty(discount))
                    {
                        if (!double.TryParse(discount, out d))
                        {
                        }
                    }

                    foreach (var item in shopCartItems)
                    {
                        orderTotal += item.Quantity * (double)item.Price;
                    }
                    double orderTotalAfterDiscount = 0;


                    orderTotalAfterDiscount = orderTotal - d;




                    order = new Order
                    {
                        OrderStatus = 1,
                        OrderDate = DateTime.Now,
                        OrderInProgress = 1,
                        PaymentMethodId = methodPayment,
                        IdAcc = userId,
                        Address = user.HomeAdress,
                        IdOrder = orderId,
                        OrderTotal = orderTotal,
                        OrderTotalDiscount = orderTotalAfterDiscount,
                        Email = user.Email,
                        Fullname = user.FullName,
                        Phone = user.PhoneNumber,

                    };
                    if (orderTotal < 10000 || orderTotal > 1000000000 || orderTotalAfterDiscount < 10000 || orderTotalAfterDiscount > 1000000000)
                    {
                        _Inotiyfyservice.Error("The payment amount is too large or too small! Please choose another method!", 20);
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return RedirectToAction("CheckOut", "Payment", order);
                    }



                }
            }
            else
            {

                _Inotiyfyservice.Error("Please fill in all information!", 5);
                return RedirectToAction("Index");


            }
        }

        public async Task<IActionResult> Order(int methodPayment)
        {
            AppUser user = HttpContext.Session.Get<AppUser>("Usercheckout");
            if (user.FullName != null && user.PhoneNumber != null && user.HomeAdress != null && user.Email != null)
            {
                List<ShoppingCartItem> shopCartItems = null;
                Order order = null;
                OrderDetail orderDetail = null;
                var userId = _userManager.GetUserId(User);


                ShoppingCart cart = await _context.ShoppingCarts.SingleOrDefaultAsync(c => c.IdAcc == userId);
                shopCartItems = _context.ShoppingCartItems.Include(p => p.IdProItemNavigation).Include(p => p.IdProNavigation)
                .Where(item => item.IdCart == cart.IdCart).OrderBy(e=>e.IdPro).ToList();





                string discount = HttpContext.Session.GetString("Discount");
                double d = 0;

                if (!string.IsNullOrEmpty(discount))
                {
                    if (!double.TryParse(discount, out d))
                    {
                    }
                }


                int numorder = 0;
                var sellerId = "";
                foreach (var item in shopCartItems)
                {
                    if (sellerId != item.IdProNavigation.IdAcc)
                    {
                        sellerId = item.IdProNavigation.IdAcc;
                        numorder++;
                    }

                }

                double orderTotalAfterDiscount = 0;



                sellerId = "";

                foreach (var item in shopCartItems)
                {
                    var orderId = "";
                    if (sellerId != item.IdProNavigation.IdAcc)
                    {
                        orderId = GenerateNextOrderId();
                        sellerId = item.IdProNavigation.IdAcc;
                        order = null;
                        double orderTotal = 0;
                        foreach (var iteme in shopCartItems)
                        {
                            if (iteme.IdProNavigation.IdAcc == sellerId)
                            {
                                orderTotal += iteme.Quantity * (double)iteme.Price;
                            }
                        }
                        orderTotalAfterDiscount = orderTotal - (d / numorder);
                        order = new Order
                        {
                            OrderStatus = 1,
                            OrderDate = DateTime.Now,
                            OrderInProgress = 1,
                            PaymentMethodId = methodPayment,
                            IdAcc = userId,
                            Address = user.HomeAdress,
                            IdOrder = orderId,
                            OrderTotal = orderTotal,
                            OrderTotalDiscount = orderTotalAfterDiscount,
                            Email = user.Email,
                            Fullname = user.FullName,
                            Phone = user.PhoneNumber,

                        };


                        _context.Orders.Add(order);
                        await _context.SaveChangesAsync();
                        foreach (var items in shopCartItems)
                        {
                            if (items.IdProNavigation.IdAcc == sellerId )
                            {
                                var proitems = await _context.ProductItems.Include(e => e.IdProNavigation).SingleOrDefaultAsync(e => e.IdProItem == items.IdProItem);
                                orderDetail = null;
                                orderDetail = new OrderDetail
                                {
                                    IdOrder = orderId,
                                    IdOrderDetail = GenerateNextOrderDetailId(),
                                    IdProItem = items.IdProItem,
                                    Quantity = items.Quantity,
                                    Price = (double)items.Price,
                                    OrderTotal = items.Quantity * (double)items.Price,
                                };
                                proitems.Quantity -= items.Quantity;
                                _context.OrderDetails.Add(orderDetail);
                                
                                await _context.SaveChangesAsync();
                            }

                        }
                       
                    }


                }


               foreach( var item in shopCartItems)
                {
                    _context.ShoppingCartItems.Remove(item);
                }

                _context.ShoppingCarts.Remove(cart);
                await _context.SaveChangesAsync();

                string codeVoucher = HttpContext.Session.GetString("VoucherCode");
                if (codeVoucher != null)
                {
                    var voucher = await _context.Vouchers.SingleOrDefaultAsync(e => e.PromotionCode == codeVoucher);
                    voucher.Quantity--;
                    await _context.SaveChangesAsync();
                }


                HttpContext.Session.Remove("Usercheckout");
                HttpContext.Session.Remove("Discount");
                HttpContext.Session.Remove("VoucherCode");
                _Inotiyfyservice.Success("Checkout successfully!", 3);
                return RedirectToAction("Index", "Home");


            }
            else
            {

                _Inotiyfyservice.Error("Please fill in all information!", 5);
                return RedirectToAction("Index");


            }
        }
        [HttpPost]
        public async Task<IActionResult> CheckoutVnpay(AppUser user)
        {
            if (ModelState.IsValid)
            {
                List<ShoppingCartItem> shopCartItems = null;
                Order order = null;
                OrderDetail orderDetail = null;
                var userId = _userManager.GetUserId(User);


                ShoppingCart cart = await _context.ShoppingCarts.SingleOrDefaultAsync(c => c.IdAcc == userId);
                shopCartItems = _context.ShoppingCartItems.Include(p => p.IdProItemNavigation).Include(p => p.IdProNavigation)
                 .Where(item => item.IdCart == cart.IdCart).ToList();


                var orderId = GenerateNextOrderId();
                double orderTotal = 0;
                foreach (var item in shopCartItems)
                {
                    orderTotal += item.Quantity * (double)item.Price;
                }

                string discount = HttpContext.Session.GetString("Discount");
                double d = 0;

                if (!string.IsNullOrEmpty(discount))
                {
                    if (!double.TryParse(discount, out d))
                    {
                    }
                }

                foreach (var item in shopCartItems)
                {
                    orderTotal += item.Quantity * (double)item.Price;
                }
                double orderTotalAfterDiscount = 0;

                if (d < 50)
                {
                    orderTotalAfterDiscount = orderTotal - (orderTotal * d) / 100;
                }
                else
                {
                    orderTotalAfterDiscount = orderTotal - d;
                }


                order = new Order
                {
                    OrderStatus = 1,
                    OrderDate = DateTime.Now,
                    OrderInProgress = 1,
                    PaymentMethodId = 2,
                    IdAcc = userId,
                    Address = user.HomeAdress,
                    IdOrder = orderId,
                    OrderTotal = orderTotal
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                foreach (var item in shopCartItems)
                {
                    orderDetail = new OrderDetail
                    {

                        IdOrder = orderId,
                        IdOrderDetail = GenerateNextOrderDetailId(),
                        IdProItem = item.IdProItem,
                        Quantity = item.Quantity,
                        Price = (double)item.Price,
                        OrderTotal = item.Quantity * (double)item.Price,
                    };


                    _context.OrderDetails.Add(orderDetail);
                    await _context.SaveChangesAsync();

                }
                foreach (var item in shopCartItems)
                {
                    _context.ShoppingCartItems.Remove(item);
                }
                _context.ShoppingCarts.Remove(cart);


                await _context.SaveChangesAsync();
                return RedirectToAction("CheckOut", "Payment", order);
            }


            return View(user);
        }
    }
}