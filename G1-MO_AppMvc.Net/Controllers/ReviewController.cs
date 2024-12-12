using App.Model;
using App.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using System;
using System.Linq;

namespace AppMvc.Net.Controllers
{
    public class ReviewController : Controller
    {
        private readonly E_CommerceContext _context;
        private readonly UserManager<AppUser> _userManager;
        public ReviewController(E_CommerceContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            return View();
        }


        public bool InsertReview(Review rv)
        {
            _context.Reviews.Add(rv);
            _context.SaveChanges();
            return true;

        }

        [HttpPost]
        public IActionResult SubmitReview(Review rv, string IdOrderDetail)
        {
            var orderd = _context.OrderDetails.Where(x => x.IdOrderDetail == IdOrderDetail).SingleOrDefault();
            rv.IdReview = GenerateNextReviewId();
            rv.IdAcc = _userManager.GetUserId(User);
            rv.ReviewDate = System.DateTime.Now;
            orderd.Review = 1;
            _context.Reviews.Add(rv);
            _context.SaveChanges();
            
            return RedirectToAction("Index", "UserOrder",new {type = "5"});
        }



        //public string GenerateNextReviewId()
        //{
        //    // Retrieve the maximum existing Id_pro
        //    string maxIdReview = _context.Reviews
        //        .Select(p => p.IdPro)
        //        .OrderByDescending(id => id)
        //        .FirstOrDefault();

        //    // Generate the next Id_pro
        //    int nextNumber = 1;
        //    if (!string.IsNullOrEmpty(maxIdReview))
        //    {
        //        string numericPart = maxIdReview.Substring(2); // Extract numeric part
        //        if (int.TryParse(numericPart, out int numericValue))
        //        {
        //            nextNumber = numericValue + 1;
        //        }
        //    }

        //    string nextIdReview = $"RV{nextNumber:D3}"; // Format the new Id_pro

        //    return nextIdReview;
        //}

        public string GenerateNextReviewId()
        {
            // Generate a new unique ID using GUID
            string nextIdReview = $"RV{Guid.NewGuid()}";

            return nextIdReview;
        }

    }
}
