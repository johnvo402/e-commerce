using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using App.Data;
using App.Model;
using AspNetCoreHero.ToastNotification.Abstractions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using X.PagedList;

namespace Ecommerce_Project.Controllers
{
    [Authorize(Roles = RoleName.Administrator)]
    public class VoucherController : Controller
    {
        private readonly E_CommerceContext _context;
        public INotyfService _Inotiyfyservice { get; }

        public VoucherController(E_CommerceContext context, INotyfService notyfService)
        {
            _context = context;
            _Inotiyfyservice = notyfService;
        }

        [HttpGet]
        public IActionResult Index(int? page, string searchString, string sortBy)
        { 
            List<SelectListItem> lsTrangThai = new List<SelectListItem>();
            lsTrangThai.Add(new SelectListItem() { Text = "Enabled", Value = "1" });
            lsTrangThai.Add(new SelectListItem() { Text = "Disable", Value = "0" });
            ViewData["lsTrangThai"] = lsTrangThai; 

            // IEnumerable<Voucher> listVC = _context.Vouchers.ToList();
            // return View(listVC);
            var pageNumber = page == null || page <= 0 ? 1 : page.Value;
            var pageSize = 9;
            var lsVoucher = GetVoucherList(sortBy);
            PagedList<Voucher> models = new PagedList<Voucher>(lsVoucher.AsQueryable(), pageNumber, pageSize);
            ViewBag.CurrentPage = pageNumber;
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                lsVoucher = GetVoucherSearch(searchString, sortBy);
                models = new PagedList<Voucher>(lsVoucher.AsQueryable(), pageNumber, pageSize);

            }
            TempData["searchString"] = searchString;
            Statistic();
            return View(models);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(Voucher obj)
        {
            if (obj.DiscountType == 1)
            {
                if (obj.DiscountAmount > 99 || obj.DiscountAmount <= 0)
                {
                    ModelState.AddModelError("DiscountAmount", "The discount percentage cannot be less than 0 or greater than 99.");
                   

                }
            }
            else if (obj.DiscountType == 0)
            {
                if ( obj.DiscountAmount <= 0)
                {
                    ModelState.AddModelError("DiscountAmount", "The discount percentage cannot be less than 0.");


                }
               
               
            }
            if (ModelState.IsValid)
            {
                //VoucherId tự tăng
                obj.VoucherId = GenerateVoucherId();
                //Mã voucher
                if (obj.PromotionCode == null)
                {
                    obj.PromotionCode = randomVoucherCode(obj.PromotionName);
                }
                //set gia tri mặc định của trạng thái sử dụng là đã kích hoạt
                if (obj.UsageStatus == 0)
                {
                    obj.UsageStatus = 1;
                }
                //set gia tri mặc định của trạng thái xóa là đã chưa xóa
                if (obj.DeleteStatus == null)
                {
                    obj.DeleteStatus = false;
                }
               
                _context.Add(obj);
                _context.SaveChanges();
                _Inotiyfyservice.Success("Add Voucher Successfully", 2);
                return RedirectToAction("Index");
            }
            return View();
        }


        // public IActionResult Edit(string id)
        // {
        //     var obj = _context.Vouchers.Find(id);
        //     if (obj == null)
        //         return NotFound();
        //     return View(obj);
        // }

        // [HttpPost]
        // public IActionResult Edit(Voucher obj)
        // {
        //     if (ModelState.IsValid)
        //     {
        //         _context.Vouchers.Update(obj);
        //         _context.SaveChanges();
        //         return RedirectToAction("Index");

        //     }
        //     return View();
        // }

        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return NotFound();
            }
            return View(voucher);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("VoucherId,UsageStatus,PromotionName,DiscountType,PromotionCode,DiscountAmount,Quantity,MaxiValue,MinValue,StartDate,EndDate,DeleteStatus")] Voucher voucher)
        {
            if (id != voucher.VoucherId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(voucher);
                    await _context.SaveChangesAsync();
                    _Inotiyfyservice.Success("Update Voucher Successfully", 2);
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VoucherExists(voucher.VoucherId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

            }
            return View(voucher);
        }

        private bool VoucherExists(string id)
        {
            return _context.Vouchers.Any(e => e.VoucherId == id);
        }

        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var voucher = await _context.Vouchers.FirstOrDefaultAsync(m => m.VoucherId == id);
            if (voucher == null)
            {
                return NotFound();
            }

            return View(voucher);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            voucher.DeleteStatus = true;
            await _context.SaveChangesAsync();
            _Inotiyfyservice.Success("Delete Voucher Successfully", 2);
            return RedirectToAction(nameof(Index));
        }


        // Hàm để tạo VoucherId
        private string GenerateVoucherId()
        {
            // Generate a new unique VoucherId
            string id = "VC";
            var lastVoucher = _context.Vouchers.OrderByDescending(v => v.VoucherId).FirstOrDefault();

            if (lastVoucher == null)
            {
                return id + "000001";
            }
            else
            {
                int k = Convert.ToInt32(lastVoucher.VoucherId.Substring(2)) + 1;
                //Nếu giá trị số nguyên có ít hơn 6 chữ số, nó sẽ được đặt thêm các số 0 ở đầu
                return id + k.ToString("D6");
            }
        }


        //Tạo random mã voucher
        private string randomVoucherCode(string promotionName)
        {
            var chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var stringChars = new char[6];
            var random = new Random();

            for (int i = 0; i < stringChars.Length; i++)
            {
                stringChars[i] = chars[random.Next(chars.Length)];
            }

            var finalString = promotionName + "_" + new String(stringChars);
            return finalString;
        }

        public IEnumerable<Voucher> GetVoucherList(string sortBy)
        {
            var lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).ToList();
            try
            {
                if (sortBy != null)
                {
                    switch (sortBy)
                    {
                        case "ID":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.VoucherId).ToList();
                            break;
                        case "ID_Descending":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.VoucherId).ToList();
                            break;
                        case "PromotionCode":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.PromotionCode).ToList();
                            break;
                        case "PromotionCode_Descending":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.PromotionCode).ToList();
                            break;
                        case "PromotionName":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.PromotionName).ToList();
                            break;
                        case "PromotionName_Descending":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.PromotionName).ToList();
                            break;
                        case "DiscountAmount":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.DiscountAmount).ToList();
                            break;
                        case "DiscountAmount_Descending":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.DiscountAmount).ToList();
                            break;
                        case "MaxValue":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.MaxiValue).ToList();
                            break;
                        case "MaxValue_Descending":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.MaxiValue).ToList();
                            break;
                        case "MinValue":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.MinValue).ToList();
                            break;
                        case "MinValue_Descending":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.MinValue).ToList();
                            break;
                        case "Quantity":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.Quantity).ToList();
                            break;
                        case "Quantity_Descending":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.Quantity).ToList();
                            break;
                        case "Used Time":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.StartDate).ToList();
                            break;
                        case "Used Time_Descending":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.StartDate).ToList();
                            break;
                        case "UsageStatus":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.UsageStatus).ToList();
                            break;
                        case "UsageStatus_Enable":
                            lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false && v.UsageStatus == 1).ToList();
                            break;
                        default:
                            break;
                    }
                }

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return lsVoucher;
        }

        public IEnumerable<Voucher> GetVoucherSearch(string searchString, string sortBy)
        {
            var lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).ToList();
            try
            {
                if (!String.IsNullOrEmpty(searchString))
                {
                    lsVoucher = lsVoucher.Where(v => v.PromotionName.ToLower()
                                .Contains(searchString)
                                || v.PromotionCode.ToLower().Contains(searchString))
                                .ToList();
                    if (sortBy != null)
                    {
                        switch (sortBy)
                        {
                            case "ID":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderBy(v => v.VoucherId).ToList();
                                break;
                            case "ID_Descending":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.VoucherId).ToList();
                                break;
                            case "PromotionCode":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderBy(v => v.PromotionCode).ToList();
                                break;
                            case "PromotionCode_Descending":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.PromotionCode).ToList();
                                break;
                            case "PromotionName":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderBy(v => v.PromotionName).ToList();
                                break;
                            case "PromotionName_Descending":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.PromotionName).ToList();
                                break;
                            case "DiscountAmount":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderBy(v => v.DiscountAmount).ToList();
                                break;
                            case "DiscountAmount_Descending":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.DiscountAmount).ToList();
                                break;
                            case "MaxValue":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderBy(v => v.MaxiValue).ToList();
                                break;
                            case "MaxValue_Descending":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.MaxiValue).ToList();
                                break;
                            case "MinValue":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderBy(v => v.MinValue).ToList();
                                break;
                            case "MinValue_Descending":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.MinValue).ToList();
                                break;
                            case "Quantity":
                                lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderBy(v => v.Quantity).ToList();
                                break;
                            case "Quantity_Descending":
                                lsVoucher = _context.Vouchers.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.Quantity).ToList();
                                break;
                            case "Used Time":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderBy(v => v.StartDate).ToList();
                                break;
                            case "Used Time_Descending":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderByDescending(v => v.StartDate).ToList();
                                break;
                            case "UsageStatus":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false).OrderBy(v => v.UsageStatus).ToList();
                                break;
                            case "UsageStatus_Disabel":
                                lsVoucher = lsVoucher.Where(v => v.DeleteStatus == false && v.UsageStatus == 0).ToList();
                                break;
                            default:
                                break;
                        }
                    }
                }
                else
                {
                    return lsVoucher;
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return lsVoucher;
        }

        public Voucher GetVoucherById(string id)
        {
            Voucher voucher = null;
            try
            {
                voucher = _context.Vouchers.SingleOrDefault(v => v.VoucherId == id);
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return voucher;
        }

        public void Statistic()
        {
            var voucher = _context.Vouchers.Count();
            var voucheruse = _context.Vouchers.Where(r => r.UsageStatus == 0).Count();
            var voucherdelete = _context.Vouchers.Where(r => r.DeleteStatus == true).Count();
            ViewBag.Voucher = voucher;
            ViewBag.Voucheruse = voucheruse;
            ViewBag.Voucherdelete = voucherdelete;
            
        }







    }
}