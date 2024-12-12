using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Model;
using Microsoft.CodeAnalysis;
using App.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using App.Models;
using System.IO;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting.Internal;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.RazorPages;
using X.PagedList;
using Microsoft.Extensions.Configuration;
using AspNetCoreHero.ToastNotification.Abstractions;
namespace App.Areas.Seller.Controllers
{
    [Area("Seller")]
    [Authorize(Roles = RoleName.Seller)]
    public class ProductsController : Controller
    {
        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IWebHostEnvironment _environment;//lấy đến folder wwwroot
        private readonly int FileSizeLimit;
        private readonly string[] AllowedExtensions;
        public INotyfService _Inotiyfyservice { get; }
        public ProductsController(E_CommerceContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IWebHostEnvironment environment, IConfiguration configruation, INotyfService inotiyfyservice)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _environment = environment;
            _Inotiyfyservice = inotiyfyservice;
            FileSizeLimit = configruation.GetSection("FileUpload").GetValue<int>("FileSizeLimit");
            AllowedExtensions = configruation.GetSection("FileUpload").GetValue<string>("AllowedExtensions").Split(",");
        }

        

        private bool Validate(IFormFile file)
        {
            if (file.Length > FileSizeLimit)
                return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension) || !AllowedExtensions.Any(s => s.Contains(extension)))
                return false;

            return true;
        }

        [HttpGet("/seller/product/")]
        // GET: Seller/Products
        public async Task<IActionResult> Index(int? page)
        {
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 5;
            var valueIdAcc = _userManager.GetUserId(User);
            var query = from product in _context.Products
                        join category in _context.Categories on product.IdCate equals category.IdCate
                        into catJoin
                        from category in catJoin.DefaultIfEmpty()
                            //join img_pro in _context.ImgPros on product.IdPro equals img_pro.IdPro into imgJoin
                            //from img_pro in imgJoin.DefaultIfEmpty()
                        where product.IdAcc == valueIdAcc
                        select new
                        {
                            Product = product,
                            Category = category,
                            Images = _context.ImgPros.Where(img => img.IdPro == product.IdPro).ToList()
                        };


            var result = await query.ToListAsync();
            var viewModelList = new List<ProJoinCat>();
            foreach (var item in result)
            {
                viewModelList.Add(new ProJoinCat
                {
                    products = item.Product,
                    categories = item.Category,
                    imgPros = item.Images
                });
            }
            IPagedList<ProJoinCat> models = new PagedList<ProJoinCat>((IQueryable<ProJoinCat>)viewModelList.AsQueryable(), pageNumber, pageSize);
            return View(models);
        }

        [HttpGet("/seller/product/enable/")]
        public async Task<IActionResult> Enable(string id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                product.StatusPro = 1; // Assuming 1 indicates enabled status
                _context.Products.Update(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                return NotFound(); // Return HTTP 404 Not Found status if product not found
            }
        }

        // GET: Seller/Products/Details
        [HttpGet("/seller/product/detail/")]
        public async Task<IActionResult> Details(string id)
        {
            var valueIdAcc = _userManager.GetUserId(User);
            var query = from product in _context.Products
                        join category in _context.Categories on product.IdCate equals category.IdCate into catJoin
                        from category in catJoin.DefaultIfEmpty()
                        join productItems in _context.ProductItems on product.IdPro equals productItems.IdPro into proJoin
                        from productItems in proJoin.DefaultIfEmpty()
                        where product.IdAcc == valueIdAcc && product.StatusPro != 0 && product.IdPro == id
                        select new
                        {
                            Product = product,
                            ProductItem = productItems,
                            Category = category
                        };

            var result = await query.ToListAsync();
            var viewModelList = new List<ProCateProItemViewModel>();
            foreach (var item in result)
            {
                viewModelList.Add(new ProCateProItemViewModel
                {
                    products = item.Product,
                    productItems = item.ProductItem,
                    categories = item.Category,
                });
            }
            ViewBag.IDPro = id;
            HttpContext.Session.SetString("IDPro", id);
            return View(viewModelList);
        }

        // GET: Seller/Products/Details
        [HttpGet("/seller/product/ProImage/{id}")]
        public async Task<IActionResult> ProImage(string id)
        {
            var imgs = _context.ImgPros.Where(i => i.IdPro == id);
            var result = await imgs.ToListAsync();
            ViewBag.IDPro = id;
            return View(result);
        }
        // GET: Seller/Products/DeleteIMG
        [HttpGet("/seller/product/DeleteIMG/")]
        public async Task<IActionResult> DeleteIMG(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var img = await _context.ImgPros.FirstOrDefaultAsync(i => i.IdImg == id);

            if (img == null)
            {
                return NotFound();
            }
            string productId = img.IdPro;

            _context.ImgPros.Remove(img);
            await _context.SaveChangesAsync();

            return RedirectToAction("ProImage", "Products", new { id = productId });
        }

        //GET:when create new products then system will auto create IdPro
        public string GenerateNextProductId()
        {
            // Retrieve the maximum existing Id_pro
            string maxIdPro = _context.Products
                .Select(p => p.IdPro)
                .OrderByDescending(id => id)
                .FirstOrDefault();

            // Generate the next Id_pro
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(maxIdPro))
            {
                string numericPart = maxIdPro.Substring(2); // Extract numeric part
                if (int.TryParse(numericPart, out int numericValue))
                {
                    nextNumber = numericValue + 1;
                }
            }

            string nextIdPro = $"PR{nextNumber:D3}"; // Format the new Id_pro

            return nextIdPro;
        }


        public string GenerateNextProductItemsId()
        {
            // Retrieve the maximum existing Id_pro
            string maxIdPro = _context.ProductItems
                .Select(p => p.IdProItem)
                .OrderByDescending(id => id)
                .FirstOrDefault();

            // Generate the next Id_pro
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(maxIdPro))
            {
                string numericPart = maxIdPro.Substring(2); // Extract numeric part
                if (int.TryParse(numericPart, out int numericValue))
                {
                    nextNumber = numericValue + 1;
                }
            }

            string nextIdPro = $"PI{nextNumber:D3}"; // Format the new Id_pro

            return nextIdPro;
        }

        [HttpGet("/seller/productcreate/")]
        // GET: Seller/Products/Create
        public IActionResult Create()
        {
            var idCateList = new SelectList(_context.Categories, "IdCate", "NameCate");
            var idCateListJson = JsonSerializer.Serialize(idCateList);
            HttpContext.Session.SetString("IdCateList", idCateListJson);
            return View();
        }
        [HttpPost("/seller/productcreate/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProProItems p)
        {
            if (!Validate(p.imgFiles))
            {
                ModelState.AddModelError("p.imgFiles", "Invalid image file format");
            }
            if (ModelState.IsValid)
            {
                var idPro = GenerateNextProductId();
                p.products.IdPro = idPro;
                p.products.StatusPro = 1;
                p.products.IdAcc = _userManager.GetUserId(User);
                _context.Add(p.products);
                await _context.SaveChangesAsync();
                p.productItems.IdProItem = GenerateNextProductItemsId();
                p.productItems.IdPro = idPro;
                p.productItems.StatusProItem = 1;
                _context.Add(p.productItems);
                await _context.SaveChangesAsync();

                // Tạo tên file mới
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(p.imgFiles.FileName);

                // Đường dẫn đến thư mục chứa ảnh sản phẩm
                var folderPath = Path.Combine(_environment.WebRootPath, "assests", "img", "product");

                // Đường dẫn lưu trữ trong database
                var pathSaveDB = fileName;

                // Đường dẫn tuyệt đối đến file ảnh
                var filePath = Path.Combine(folderPath, fileName);

                // Tạo mới đối tượng ImgPro và lưu vào database
                p.imgPros = new ImgPro();
                p.imgPros.IdImg = GenerateNextIMGId();
                p.imgPros.IdPro = idPro;
                p.imgPros.LinkImg = pathSaveDB;
                _context.Add(p.imgPros);
                await _context.SaveChangesAsync();

                // Kiểm tra và tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Lưu ảnh vào thư mục
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await p.imgFiles.CopyToAsync(fileStream);
                }

                return RedirectToAction(nameof(Index));
            }

            return View(p);
        }


        [HttpGet("/seller/product/MoreIMG/")]
        public async Task<IActionResult> AddMoreImg(string id)
        {

            var product = await _context.Products.FindAsync(id);


            return View(product);
        }




        //[HttpPost("/seller/product/MoreIMG/")]
        //public async Task<IActionResult> AddMoreImg(string id, IFormFile file)
        //{
        //	//var fileName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + Path.GetExtension(p.imgFiles.FileName);
        //	var fileName = DateTime.Now.ToString("yyyymmddMMss") + "_" + Path.GetFileName(file.FileName);
        //	//đưa vào folder uploads
        //	var folderPath = Path.Combine(_environment.WebRootPath, "uploads");
        //	//gộp tên folder vs file thành đường dẫn
        //	var filePath = Path.Combine(folderPath, fileName);

        //	ImgPro imgPros = new ImgPro();

        //	imgPros.IdImg = GenerateNextIMGId();
        //	imgPros.IdPro = id;

        //	string pathSaveDB = "";
        //	string searchSubstring = "\\wwwroot\\uploads\\";
        //	int startIndex = filePath.IndexOf(searchSubstring);

        //	if (startIndex != -1)
        //	{
        //		// Lấy vị trí bắt đầu của phần "uploads"
        //		startIndex += searchSubstring.Length;

        //		// Cắt lấy phần sau của đường dẫn từ vị trí startIndex
        //		pathSaveDB = "/uploads/" + filePath.Substring(startIndex);
        //	}


        //	imgPros.LinkImg = pathSaveDB;


        //	_context.Add(imgPros);
        //	await _context.SaveChangesAsync();

        //	if (!Directory.Exists(folderPath))//kiểm tra folder có tồn tại hay không, nếu không thì tạo ra
        //		Directory.CreateDirectory(folderPath);

        //	//copy file vừa tạo vào trong folder
        //	using (var fileStream = new FileStream(filePath, FileMode.Create))
        //	{
        //		await file.CopyToAsync(fileStream);
        //	}


        //	return RedirectToAction(nameof(Index));

        //}

        [HttpPost("/seller/product/MoreIMG/")]
        public async Task<IActionResult> AddMoreImg(string id, IFormFile file)
        {
            if (!Validate(file))
            {
                ModelState.AddModelError("file", "Invalid image file format");
            }
            if (ModelState.IsValid)
            {
                var fileName = DateTime.Now.ToString("yyyyMMddHHmmss") + "_" + Path.GetFileName(file.FileName);

                // Đường dẫn đến thư mục chứa ảnh sản phẩm
                var folderPath = Path.Combine(_environment.WebRootPath, "assests", "img", "product");

                // Đường dẫn lưu trữ trong database
                var pathSaveDB = fileName;

                // Đường dẫn tuyệt đối đến file ảnh
                var filePath = Path.Combine(folderPath, fileName);

                // Tạo mới đối tượng ImgPro và lưu vào database
                ImgPro imgPros = new ImgPro();
                imgPros.IdImg = GenerateNextIMGId();
                imgPros.IdPro = id;
                imgPros.LinkImg = pathSaveDB;
                _context.Add(imgPros);
                await _context.SaveChangesAsync();

                // Kiểm tra và tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                // Lưu ảnh vào thư mục
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                return RedirectToAction(nameof(Index));
            }
            _Inotiyfyservice.Error("Invalid image file format(.jpg,.jpeg,.png)", 5);
            return RedirectToAction(nameof(ProImage), new {id =id});
            // Tạo tên file mới
            
        }


        public string GenerateNextIMGId()
        {
            // Retrieve the maximum existing Id_pro
            string maxIdIMG = _context.ImgPros
                .Select(p => p.IdImg)
                .OrderByDescending(id => id)
                .FirstOrDefault();

            // Generate the next Id_pro
            int nextNumber = 1;
            if (!string.IsNullOrEmpty(maxIdIMG))
            {
                string numericPart = maxIdIMG.Substring(2); // Extract numeric part
                if (int.TryParse(numericPart, out int numericValue))
                {
                    nextNumber = numericValue + 1;
                }
            }

            string nextIdImg = $"IM{nextNumber:D3}"; // Format the new Id_pro

            return nextIdImg;
        }

        // GET: Seller/Products/Edit/5
        [HttpGet("/seller/product/edit/")]
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.listCate = new SelectList(_context.Categories, "IdCate", "NameCate", product.IdCate);
            return View(product);
        }

        // POST: Seller/Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/seller/product/edit/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Product product)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    product.StatusPro = 1;
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.IdPro))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(product);
        }

        // GET: Seller/Products/Delete/5
        [HttpGet("/seller/product/delete/")]
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products
                .Include(p => p.IdAccNavigation)
                .Include(p => p.IdCateNavigation)
                .FirstOrDefaultAsync(m => m.IdPro == id);
            if (product == null)
            {
                return NotFound();
            }
            ViewBag.listCate = new SelectList(_context.Categories, "IdCate", "NameCate", product.IdCate);
            return View(product);
        }

        // POST: Seller/Products/Delete/5
        [HttpPost("/seller/product/delete/"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Product p)
        {
            var product = await _context.Products.FindAsync(p.IdPro);
            product.StatusPro = 2;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            _Inotiyfyservice.Success("Delete successfully!", 3);
            return RedirectToAction(nameof(Index));
        }
        [HttpGet("/seller/product/hidden/"), ActionName("Hidden")]
        public async Task<IActionResult> Hidden(string id)
        {
            var product = await _context.Products
                .Include(p => p.IdAccNavigation)
                .Include(p => p.IdCateNavigation)
                .FirstOrDefaultAsync(m => m.IdPro == id);
            product.StatusPro = 0;
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
            _Inotiyfyservice.Success("Hidden successfully!", 3);
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(string id)
        {
            return _context.Products.Any(e => e.IdPro == id);
        }

        [HttpGet("/seller/product/create/items/")]
        public IActionResult CreateItems(String id)
        {
            ViewBag.IdPro = id;
            return View();
        }

        // POST: Seller/Products/CreateItems
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/seller/product/create/items/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateItems(ProProItems p)
        {
            if (ModelState.IsValid)
            {
                p.productItems.IdProItem = GenerateNextProductItemsId();
                p.productItems.StatusProItem = 1;
                _context.Add(p.productItems);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(p);
        }
        [HttpGet("/seller/product/edit/items/")]
        public async Task<IActionResult> EditItems(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productItems = await _context.ProductItems.FirstOrDefaultAsync(p => p.IdProItem == id);
            if (productItems == null)
            {
                return NotFound();
            }
            return View(productItems);
        }

        // POST: Seller/Products/EditItems/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost("/seller/product/edit/items/")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditItems(string id, ProductItem p)
        {
            if (id != p.IdProItem)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    p.StatusProItem = 1;
                    _context.Update(p);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(p.IdProItem))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "Products", new { id = p.IdPro });
            }
            return View(p);
        }


        [HttpGet("/seller/product/deleteItem/")]
        public async Task<IActionResult> DeleteItems(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var productItems = await _context.ProductItems.FirstOrDefaultAsync(m => m.IdProItem == id);
            productItems.StatusProItem = 2;
            _context.ProductItems.Update(productItems);
            await _context.SaveChangesAsync();
            //if (productItems == null)
            //{
            //	return NotFound();
            //}
            return RedirectToAction("Details", "Products", new { id = productItems.IdPro });
        }

        // POST: Seller/Products/Delete/5
        //      [HttpPost("/seller/product/delete/items/"), ActionName("DeleteItems")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteItemsConfirmed(string id)
        //{
        //	var p = await _context.ProductItems.FindAsync(id);
        //	p.StatusProItem = 0;
        //	_context.ProductItems.Update(p);
        //	await _context.SaveChangesAsync();
        //	return RedirectToAction(nameof(Index));
        //}

        [HttpGet("/seller/product/removed/details/")]
        public async Task<IActionResult> RemovedDetail(string id)
        {
            var valueIdAcc = _userManager.GetUserId(User);
            var query = from product in _context.Products
                        join category in _context.Categories on product.IdCate equals category.IdCate into catJoin
                        from category in catJoin.DefaultIfEmpty()
                        join productItems in _context.ProductItems on product.IdPro equals productItems.IdPro into proJoin
                        from productItems in proJoin.DefaultIfEmpty()
                        where product.IdAcc == valueIdAcc && product.StatusPro == 0 && product.IdPro == id
                        select new
                        {
                            Product = product,
                            ProductItem = productItems,
                            Category = category
                        };

            var result = await query.ToListAsync();
            var viewModelList = new List<ProCateProItemViewModel>();
            foreach (var item in result)
            {
                viewModelList.Add(new ProCateProItemViewModel
                {
                    products = item.Product,
                    productItems = item.ProductItem,
                    categories = item.Category,
                });
            }
            return View(viewModelList);
        }

        [HttpGet("/seller/product/removed/")]
        public async Task<IActionResult> Removed()
        {
            var valueIdAcc = _userManager.GetUserId(User);
            var query = from product in _context.Products
                        join category in _context.Categories on product.IdCate equals category.IdCate into catJoin
                        from category in catJoin.DefaultIfEmpty()
                        where product.IdAcc == valueIdAcc && product.StatusPro == 0
                        select new
                        {
                            Product = product,
                            Category = category
                        };

            var result = await query.ToListAsync();
            if (result == null)
            {
                return RedirectToAction("Index", "Home");
            }
            var viewModelList = new List<ProJoinCat>();
            foreach (var item in result)
            {
                viewModelList.Add(new ProJoinCat
                {
                    products = item.Product,
                    categories = item.Category,
                });
            }
            return View(viewModelList);
        }
    }
}
