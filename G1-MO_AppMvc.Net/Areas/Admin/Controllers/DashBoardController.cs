using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using App.Model;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using App.Data;


namespace App.Areas.Admin.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    [Area("Admin")]

    public class DashBoardController : Controller
    {
        private readonly E_CommerceContext _context;

        public DashBoardController(E_CommerceContext context)
        {
            _context = context;
        }
        [HttpGet("/dashboard/")]
        public IActionResult Index()
        {
            StatisticBy(2);
            int currentYear = DateTime.Now.Year;
            var data = Chart(currentYear);
            ViewBag.JsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            return View();

        }
        [HttpGet("/statistic/")]
        public IActionResult Statictis(int id)
        {
            StatisticBy(id);
            int currentYear = DateTime.Now.Year;
            var data = Chart(currentYear);

            ViewBag.JsonData = Newtonsoft.Json.JsonConvert.SerializeObject(data);

            return View("Index");
        }
        [HttpGet("/datachart/")]
        public ActionResult GetDataChart(int year)
        {

            StatisticBy(2);
            var chart = Chart(year);
            return Json(chart);
        }




        private double CalculatePercentageChange(double oldValue, double newValue)
        {
            if (oldValue == newValue)
            {
                return 0;
            }
            else if (oldValue == 0)
            {
                return 100; // Nếu giá trị cũ là 0, phần trăm tăng là 100%
            }

            return ((newValue - oldValue) / oldValue) * 100;
        }

        private int CalculatePercentageInt(int oldValue, int newValue)
        {


            if (oldValue == newValue)
            {
                return 0;
            }
            else if (oldValue == 0)
            {
                return 100; // Nếu giá trị cũ là 0, phần trăm tăng là 100%
            }

            return ((newValue - oldValue) / oldValue) * 100;
        }
        private void StatisticBy(int id)
        {
            int start = 0 - id - 1;
            int end = 0 - id;
            DateTime currentDate = DateTime.Now;
            DateTime startOfCurrentWeek = currentDate.AddDays(start);
            DateTime startOfPreviousWeek = startOfCurrentWeek.AddDays(end);

            double currentWeekTotal = _context.Orders
                .Where(o => o.OrderDate >= startOfCurrentWeek && o.OrderDate <= currentDate)
                .Sum(o => o.OrderTotal);

            double previousWeekTotal = _context.Orders
                .Where(o => o.OrderDate >= startOfPreviousWeek && o.OrderDate < startOfCurrentWeek)
                .Sum(o => o.OrderTotal);

            double percentageChange = CalculatePercentageChange(previousWeekTotal, currentWeekTotal);

            ViewBag.CurrentWeekTotal = currentWeekTotal.ToString("N2");
            ViewBag.PercentageChange = percentageChange.ToString("N2");
            //số đơn hàng được đặt trong 7 ngày
            int ordersCountCurrent = _context.Orders
                .Count(o => o.OrderDate >= startOfCurrentWeek && o.OrderDate <= currentDate);
            int orderCountPrevious = _context.Orders
                .Count(o => o.OrderDate >= startOfPreviousWeek && o.OrderDate < startOfCurrentWeek);
            int percentOrder = CalculatePercentageInt(orderCountPrevious, ordersCountCurrent);
            ViewBag.OrdersCount = ordersCountCurrent.ToString("#,##0");
            ViewBag.OrderCountPrevious = orderCountPrevious.ToString("#,##0");
            ViewBag.OrderPercnet = percentOrder.ToString("N2");
            //đếm tổng số tài khoản
            int totalUserCurrent = _context.Users
                .Count(u => u.DateCreate >= startOfCurrentWeek && u.DateCreate <= currentDate);
            int totalUserPre = _context.Users
                .Count(u => u.DateCreate >= startOfPreviousWeek && u.DateCreate <= startOfCurrentWeek);
            int percentAcc = CalculatePercentageInt(totalUserPre, totalUserCurrent);
            if (id == 2)
            {
                ViewBag.Staby = "1 day";
            }
            else if (id == 7)
            {
                ViewBag.Staby = "7 days";
            }
            else if (id == 30)
            {
                ViewBag.Staby = "30 days";
            }
            else if (id == 365)
            {
                ViewBag.Staby = "Years";
            }

            ViewBag.TotalUserCount = totalUserCurrent;
            ViewBag.PrecentAcc = percentAcc.ToString("N2");

        }

        public Tuple<List<GetAccountMonthAndYear>, List<GetAccountMonthAndYear>, List<GetAccountMonthAndYear>> Chart(int year)
        {
            var distinctYears = _context.Users
                    .Where(u => u.DateCreate != null) // Ensure DateCreate is not null
                        .Select(u => u.DateCreate.Value.Year) // Extract year from DateCreate
                    .Distinct() // Select distinct years
                    .ToList();
            ViewBag.AccountYear = distinctYears;

            var accountbuyer = _context.CallGetAccountBuyer(year).ToList();
            var accountseller = _context.CallGetAccountSeller(year).ToList();
            var accountdisable = _context.CallGetAccountDisable(year).ToList();
            return Tuple.Create(accountbuyer, accountseller, accountdisable);
        }



    }
}
