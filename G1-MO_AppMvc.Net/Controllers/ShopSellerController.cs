using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using App.Model;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PagedList.Core;

namespace App.Controllers
{
    // [Route("[controller]")]
    public class ShopSellerController : Controller
    {

        private readonly E_CommerceContext _context;
        public INotyfService _Inotiyfyservice { get; }

        public ShopSellerController(E_CommerceContext context, INotyfService notyfService)
        {
            _context = context;
            _Inotiyfyservice = notyfService;
        }
        public IActionResult Index(int? page, string id_acc, string searchString, string IdCate = "0")
        {

            ViewBag.IdAcc = id_acc;
            List<Product> lsProduct = new List<Product>();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 9;
            if (IdCate != "0")
            {
                if (id_acc != null)
                {
                    lsProduct = _context.Products
                                        .AsNoTracking()
                                        .Include(p => p.ProductItems)
                                        .Include(p => p.IdAccNavigation).Include(e => e.ImgPros).Include(e=>e.Reviews)
                                        .Where(p => p.IdAcc.Equals(id_acc) && p.IdCate.Equals(IdCate)&& p.StatusPro == 1)
                                        .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate).ToList();
                    TempData["CurrentCate"] = IdCate;
                }
            }
            else
            {
                if (id_acc != null)
                {
                    lsProduct = _context.Products
                                        .AsNoTracking()
                                        .Include(p => p.ProductItems)
                                        .Include(p => p.IdAccNavigation).Include(e=>e.ImgPros).Include(e => e.Reviews)
                                        .Where(p => p.IdAcc.Equals(id_acc) && p.StatusPro == 1)
                                        .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate).ToList();
                }
            }
            



            PagedList<Product> models = new PagedList<Product>(lsProduct.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            ViewBag.IdAcc = id_acc;

            //Search
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                lsProduct = GetProductSearch(id_acc, searchString, IdCate);
                models = new PagedList<Product>(lsProduct.AsQueryable(), pageNumber, pageSize);
            }
            TempData["searchString"] = searchString;

            //LẤY MỘT SẢN PHẨM JION
            var query = from p in _context.Products
                        join c in _context.Categories on p.IdCate equals c.IdCate
                        join pI in _context.ProductItems on p.IdPro equals pI.IdPro
                        select new
                        {
                            Product = p,
                            ProductItem = pI,
                            Category = c
                        };
            //Lấy ra một object
            var result = query.FirstOrDefault(); // Get the first or default item
            if (result == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var product = new ProCateProItemViewModel
            {
                products = result.Product,
                productItems = result.ProductItem,
                categories = result.Category
            };
            ViewBag.Product = product;

            //DANH SÁCH CÁC SẢN PHẨM SALE
            var query1 = from p in _context.Products
                         join c in _context.Categories on p.IdCate equals c.IdCate
                         join pI in _context.ProductItems on p.IdPro equals pI.IdPro
                         where (p.IdAcc == id_acc)
                         select new
                         {
                             Product = p,
                             ProductItem = pI,
                             Category = c
                         };
            var saleProduct = query1
                .Where(x => x.ProductItem.Discount != null && x.ProductItem.Discount != 0)
                .ToList();
            var viewModelList = new List<ProCateProItemViewModel>();

            foreach (var item in saleProduct)
            {
                viewModelList.Add(new ProCateProItemViewModel
                {
                    products = item.Product,
                    productItems = item.ProductItem,
                    categories = item.Category,
                });

            }
            ViewBag.SaleProduct = viewModelList;

            //Sort by Category

            var lsCate = new List<Category>();
            var distinctCategories = query1.Select(item => item.Category).Distinct();
            lsCate.AddRange(distinctCategories);
            ViewBag.lsCate = lsCate;
            // ViewBag.Category = new SelectList(lsCate, "IdCate", "NameCate", IdCate);

            return View(models);
        }

        public IActionResult Filtter(string id_acc, string IdCate)
        {
            string url = $"/ShopSeller?id_acc={id_acc}&IdCate={IdCate}";
            if (IdCate == "0")
            {
                url = $"/ShopSeller?id_acc={id_acc}";
            }
            // return Json(new { status = "success", RedirectUrl = url });
            return Redirect(url);
        }

        public List<Product> GetProductSearch(string id_acc, string searchString, string IdCate)
        {
            List<Product> lsProduct = new List<Product>();
            if (IdCate != "0")
            {
                if (id_acc != null)
                {
                    lsProduct = _context.Products
                                        .AsNoTracking()
                                        .Include(p => p.ProductItems)
                                        .Include(p => p.IdAccNavigation)
                                        .Where(p => p.IdAcc.Equals(id_acc) && p.IdCate.Equals(IdCate) && p.StatusPro == 1)
                                        .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate).ToList();
                }
            }
            else
            {
                if (id_acc != null)
                {
                    lsProduct = _context.Products
                                        .AsNoTracking()
                                        .Include(p => p.ProductItems)
                                        .Include(p => p.IdAccNavigation)
                                        .Where(p => p.IdAcc.Equals(id_acc) && p.StatusPro == 1)
                                        .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate).ToList();
                }
            }
            
            try
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    lsProduct = lsProduct.Where(p => p.Name.ToLower()
                                            .Contains(searchString)).ToList();
                }
                else
                {
                    return lsProduct;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }

            return lsProduct;
        }

        public IActionResult FiltterSearch(string id_acc, string searchString)
        {
            string url = $"/ShopSeller?id_acc={id_acc}&searchString={searchString}";
            if (searchString == "")
            {
                url = $"/ShopSeller?id_acc={id_acc}";
            }
            // return Json(new { status = "success", RedirectUrl = url });
            return Redirect(url);
        }


        public IActionResult FiltterPrice(string id_acc, int indexPrice)
        {
            string url = $"/ShopSeller?id_acc={id_acc}&indexPrice={indexPrice}";
            // return Json(new { status = "success", RedirectUrl = url });
            return Redirect(url);
        }

    }
}