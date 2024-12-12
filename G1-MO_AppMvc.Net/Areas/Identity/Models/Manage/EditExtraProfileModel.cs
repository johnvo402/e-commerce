using System;
using System.ComponentModel.DataAnnotations;

namespace App.Areas.Identity.Models.ManageViewModels
{
    public class EditExtraProfileModel
    {
        [Display(Name = "Username")]
        public string UserName { get; set; }

        [Display(Name = "Email Address")]
        public string UserEmail { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Display(Name = "Address")]
        [StringLength(400)]
        public string HomeAdress { get; set; }

        [Display(Name = "Date of Birth")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:yyyy-MM-dd}")]
        [AgeValidation(ErrorMessage = "Just greater than 18 year olds!")]
        public DateTime? BirthDate { get; set; }

        [Display(Name = "Gender")]
        public int Gender { get; set; }

        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        public string Avt { get; set; }




    }
    public class AgeValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            DateTime birthDate = (DateTime)value;
            int age = DateTime.Now.Year - birthDate.Year;
            return age >= 18;
        }
    }
}