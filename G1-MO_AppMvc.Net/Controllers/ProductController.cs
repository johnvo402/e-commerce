using App.Model;
using App.Model;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.FlowAnalysis;
using Microsoft.EntityFrameworkCore;
using PagedList.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Controllers
{
    [RedirectAdmin]

    public class ProductController : Controller
    {

        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;

        public ProductController(E_CommerceContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index(int? page, string searchString, string IdCate = "0")
        {
          

            //Danh sách tất cả sp chưa join
            List<Product> lsProduct = new List<Product>();
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 9;
            var IdAccLoggedIn = _userManager.GetUserId(User);

            if (IdCate != "0")
            {
               
                lsProduct = _context.Products
                                    .AsNoTracking()
                                    .Where(x => x.IdCate.Equals(IdCate) && x.StatusPro == 1   && x.IdAcc != IdAccLoggedIn).Include(e => e.IdAccNavigation)

                                    .Include(p => p.ProductItems).Include(i => i.ImgPros).Include(r => r.Reviews)
                                    .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate).ToList();

            }
            else
            {

                
                //lsIdSeller = _context.UserRoles
                //        .Where(s => s.RoleId == "ba3c30a7-a672-4c4b-bd2c-f67b108e52e9")
                //        .Select(s => s.UserId)
                //        .ToList();
                

                lsProduct = _context.Products
                    .AsNoTracking()
                    .Where(x => x.StatusPro == 1 && x.IdAcc != IdAccLoggedIn) // Chỉ lấy sản phẩm có Id người bán nằm trong danh sách lsIdSeller
                    .Include(e => e.IdAccNavigation)
                    .Include(p => p.ProductItems)
                    .Include(i => i.ImgPros)
                    .Include(r => r.Reviews)
                    .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate)
                    .ToList();

            }

            PagedList<Product> models = new PagedList<Product>(lsProduct.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;

            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                lsProduct = GetProductSearch(searchString, IdCate);
                models = new PagedList<Product>(lsProduct.AsQueryable(), pageNumber, pageSize);
            }
            TempData["searchString"] = searchString;




            //DANH SÁCH CÁC SẢN PHẨM SALE
            var query1 = from p in _context.Products
                         join c in _context.Categories on p.IdCate equals c.IdCate
                         join pI in _context.ProductItems on p.IdPro equals pI.IdPro
                         select new
                         {
                             Product = p,
                             ProductItem = pI,
                             Category = c
                         };
            var saleProduct = query1
                .Where(x => x.ProductItem.Discount != null && x.ProductItem.Discount != 0 && x.Product.StatusPro == 1)
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



            ViewBag.CurrentIdCate = IdCate;

            // ViewBag.Category = new SelectList(_context.Categories, "IdCate", "NameCate", IdCate);

            var lsCategory = _context.Categories
                .AsNoTracking()
                .OrderByDescending(x => x.NameCate)
                .ToList();
            ViewBag.lsCategory = lsCategory;



            return View(models);


        }

        //[Route("/{Name}.html", Name = "ListProduct")]
        public ActionResult List(string Name, int Page = 1)
        {
            try
            {
                var pageSize = 10;
                var danhmuc = _context.Categories.AsNoTracking().SingleOrDefault(x => x.NameCate == Name);
                var lsProduct = _context.Products.AsNoTracking()
                    .Where(x => x.IdCate == danhmuc.IdCate)
                    .OrderByDescending(x => x.Name);
                PagedList<Product> models = new PagedList<Product>(lsProduct, Page, pageSize);
                ViewBag.CurrentPage = Page;
                ViewBag.CurrentCate = danhmuc;

                return View(models);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }

        }


        // [Route("/{Alias}-{id}.html", Name = "ProductDetails")]
        [Route("/{Name}-{id}", Name = "ProductDetails")]

        public IActionResult Detail(string id)
        {
            try
            {
                var query1 = from p in _context.Products.Include(i => i.ImgPros).Include(p => p.ProductItems).Include(p => p.IdCateNavigation).Include(r => r.Reviews)
                             join c in _context.Categories on p.IdCate equals c.IdCate
                             join pI in _context.ProductItems on p.IdPro equals pI.IdPro
                             where p.IdPro == id // Filter by product Id
                             select new
                             {

                                 Product = p,
                                 ProductItem = pI,
                                 Category = c,
                                 Review = p.Reviews.Select(r => new
                                 {
                                     Review = r,
                                     User = r.IdAccNavigation
                                 })
                             };

                //Lấy ra một object
                var product = query1.FirstOrDefault(); // Get the first or default item
                if (product == null)
                {
                    return RedirectToAction("Index");
                }
                var username = _context.Users.FirstOrDefault(u => u.Id == product.Product.IdAcc);

                var viewModel = new ProCateProItemViewModel
                {
                    products = product.Product,
                    productItems = product.ProductItem,
                    categories = product.Category,

                };

                ViewBag.username = username;

                //Lấy ds product cùng loại category 
                /*var query2 = from p in _context.Products
                             join c in _context.Categories on p.IdCate equals c.IdCate
                             join pI in _context.ProductItems on p.IdPro equals pI.IdPro
                             where(p.IdCate == product.Category.IdCate && p.IdPro != id && p.StatusPro == 1)
                             select new
                             {
                                 Product = p,
                                 ProductItem = pI,
                                 Category = c
                             };*/

                var IdAccLoggedIn = _userManager.GetUserId(User);
               

                //Lấy ra một list
                var lsproduct = _context.Products
                    .AsNoTracking()
                    .Where(x => x.StatusPro == 1 &&  x.IdAcc != IdAccLoggedIn && x.IdCate == product.Category.IdCate && x.IdPro != id) // Chỉ lấy sản phẩm có Id người bán nằm trong danh sách lsIdSeller
                    .Include(e => e.IdAccNavigation)
                    .Include(p => p.ProductItems)
                    .Include(i => i.ImgPros)
                    .Include(r => r.Reviews)
                    .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate)
                    .Take(4)
                    .ToList();
                //_context.Products
                //.Where(x => x.Category.IdCate == product.Category.IdCate && x.Product.IdPro != id && x.Product.StatusPro == 1)
                //.OrderByDescending(p => p.Product.UpdateDate != null ? p.Product.UpdateDate : p.Product.CreateDate)
                //.Take(4)
                //.ToList();
                


                ViewBag.lsProduct = lsproduct;




                return View(viewModel);
            }
            catch
            {
                return RedirectToAction("Index", "Home");
            }
        }

        public IActionResult FiltterCate(string IdCate)
        {
            string url = $"/Product/?IdCate={IdCate}";
            if (IdCate == "0")
            {
                url = $"/Product";
            }
            // return Json(new { status = "success", RedirectUrl = url });
            return Redirect(url);
        }

        public IActionResult FiltterPrice(float maxPrice, float minPrice)
        {
            if (maxPrice != null && minPrice != null)
            {
                TempData["maxPrice"] = maxPrice;
                TempData["minPrice"] = minPrice;
            }
            // Trả về JSON với status "success" và URL của action index
            return RedirectToAction("Index", "Product");
        }

        public List<Product> GetProductSearch(string searchString, string IdCate)
        {
            List<Product> lsProduct = new List<Product>();
            if (IdCate != "0")
            {
                lsProduct = _context.Products
                                    .AsNoTracking()
                                    .Where(x => x.IdCate.Equals(IdCate) && x.StatusPro == 1)
                                    .Include(i => i.ImgPros).Include(p => p.ProductItems).Include(p => p.IdCateNavigation).Include(r => r.Reviews)
                                    .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate).ToList();

            }
            else
            {
                lsProduct = _context.Products
                                    .AsNoTracking()
                                    .Where(x => x.StatusPro == 1)
                                    .Include(i => i.ImgPros).Include(p => p.ProductItems).Include(p => p.IdCateNavigation).Include(r => r.Reviews)
                                    .OrderByDescending(p => p.UpdateDate != null ? p.UpdateDate : p.CreateDate).ToList();
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



    }
}

