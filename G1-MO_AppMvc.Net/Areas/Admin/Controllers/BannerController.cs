using App.Data;
using App.Models;
using App.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using static System.Net.Mime.MediaTypeNames;
using App.Model;
using System.Linq;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.Extensions.Configuration;



namespace App.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Admin")]
    [Route("/ManageBanner/[action]")]

    public class BannerController : Controller
    {
        private readonly AppDbContext _context;

        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly int FileSizeLimit;
        private readonly string[] AllowedExtensions;
        public INotyfService _Inotiyfyservice { get; }

        public BannerController(AppDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IConfiguration configruation, INotyfService inotiyfyservice)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
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
        [HttpGet]
        public async Task<IActionResult> IndexBanner()
        {
            var firstBanner = await _context.Banners.FirstOrDefaultAsync();
            return View(firstBanner);
        }
        private Task<AppUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(HttpContext.User);
        }

        public class UploadFile
        {

            [DataType(DataType.Upload)]
           
            [Display(Name = "Choose file")]
            public IFormFile FileUp { get; set; }
            [Display(Name = "Input context")]
         
            public string Text { get; set; }
        }

        [HttpGet]
        public IActionResult UploadImg()
        {
            var upload = new UploadFile();
            var firstBanner =  _context.Banners.FirstOrDefault();
            if (firstBanner != null)
            {
                upload.Text = firstBanner.Text;
            }
           
            return View(upload);
        }
        [HttpPost]
        public async Task<IActionResult> UploadImgAsync(UploadFile file)
        {
            if(!Validate(file.FileUp))
            {
                ModelState.AddModelError("FileUp", "Please choose the correct format .jpg,.jpeg,.png");
            }
           
            if (ModelState.IsValid)
            {
                var firstBanner = await _context.Banners.FirstOrDefaultAsync();
                if (firstBanner != null)
                {
                    if (file.FileUp == null)
                    {
                        firstBanner.Text = file.Text;
                        _context.Update(firstBanner);
                        _context.SaveChanges();

                    }
                    else
                    {
                        var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                   + Path.GetExtension(file.FileUp.FileName);

                        var file2 = Path.Combine("wwwroot", "assests", "img", "hero", file1);
                        using (var filename = new FileStream(file2, FileMode.Create))
                        {
                            await file.FileUp.CopyToAsync(filename);
                        }
                        firstBanner.Link = file1;
                        firstBanner.Text = file.Text;
                        _context.Update(firstBanner);
                        _context.SaveChanges();
                    }
                } else
                {
                    var banner = new Models.Banner();
                    if (file.FileUp == null)
                    {
                       
                        banner.Text = file.Text;
                        

                    }
                    else
                    {
                        var file1 = Path.GetFileNameWithoutExtension(Path.GetRandomFileName())
                   + Path.GetExtension(file.FileUp.FileName);

                        var file2 = Path.Combine("wwwroot", "assests", "img", "hero", file1);
                        using (var filename = new FileStream(file2, FileMode.Create))
                        {
                            await file.FileUp.CopyToAsync(filename);
                        }
                        banner.Link = file1;
                        banner.Text = file.Text;
                        
                    }
                    _context.Add(banner);
                    _context.SaveChanges();
                }
                _Inotiyfyservice.Success("Update banner successfully!", 3);
                return RedirectToAction("IndexBanner");

            }

            return View(file);
        }

        private bool CheckImageDimensions(IFormFile file, int width, int height)
        {
            using (var image = SixLabors.ImageSharp.Image.Load(file.OpenReadStream()))
            {
                return image.Width == width && image.Height == height;
            }
        }
    }
}
