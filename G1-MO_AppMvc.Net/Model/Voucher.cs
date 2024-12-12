using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

#nullable disable

namespace App.Model
{

    [Index(nameof(Voucher.PromotionCode), IsUnique = true)]
    public partial class Voucher : IValidatableObject
    {

        [Display(Name = "ID")]
        public string VoucherId { get; set; }

        [Display(Name = "PromotionCode")]
        public string PromotionCode { get; set; }

        [Display(Name = "PromotionName")]
        [Required(ErrorMessage = "PromotionName is required")]
        public string PromotionName { get; set; }

        public int DiscountType { get; set; }

        [Display(Name = "DiscountAmount")]
        [Required(ErrorMessage = "DiscountAmount is required")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public double DiscountAmount { get; set; }

        [Display(Name = "MaxiValue")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public double? MaxiValue { get; set; }

        [Display(Name = "MinValue")]
        [DataType(DataType.Currency)]
        [DisplayFormat(DataFormatString = "{0:C0}")]
        public double? MinValue { get; set; }

        [Display(Name = "Quantity")]
        [Required(ErrorMessage = "Quantity is required")]
        [RegularExpression(@"^\d+$", ErrorMessage = "Quantity must be a positive integer")]
        public int Quantity { get; set; }

        [Display(Name = "StartDate")]
        public DateTime StartDate { get; set; }

        [Display(Name = "EndDate")]
        public DateTime EndDate { get; set; }
        [Display(Name = "Usage status")]
        public int UsageStatus { get; set; }
        public bool DeleteStatus { get; set; }


        // kế thừa giao diện IValidatableObject và thực thi phương thức Validate() để dử dụng Index
        // có x.VoucherId != VoucherId để update có thể truyền giá trị vào mà ko bắt unique
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Tạo một danh sách để lưu trữ các lỗi
            List<ValidationResult> validationResult = new List<ValidationResult>();
            // Kiểm tra nếu EndDate nhỏ hơn StartDate
            if (EndDate < StartDate)
            {
                ValidationResult errorMessage = new ValidationResult("End date must be greater than or equal to start date.", new[] { "EndDate" });
                validationResult.Add(errorMessage);
            }

            // Kiểm tra sự tồn tại của mã khuyến mãi
            E_CommerceContext db = new E_CommerceContext();
            var validateCode = db.Vouchers.FirstOrDefault(x => x.PromotionCode == PromotionCode && x.VoucherId != VoucherId);
            if (validateCode != null)
            {
                ValidationResult errorMessage = new ValidationResult("Promotion code already exists.", new[] { "PromotionCode" });
                validationResult.Add(errorMessage);
            }

            return validationResult;
        }
        //có thể viết đơn giản như sau:
        //        IEnumerable<ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        // {
        //     ProductDbContext db = new ProductDbContext();
        //     var validateName = db.Products.FirstOrDefault(x => x.ProductName == ProductName && x.Id != Id);
        //     if (validateName != null)
        //     {
        //         ValidationResult errorMessage = new ValidationResult("Product name already exists.", new[] { "ProductName" });
        //         yield return errorMessage;
        //     }
        //     else
        //     {
        //         yield return ValidationResult.Success;
        //     }

        // }

    }
}