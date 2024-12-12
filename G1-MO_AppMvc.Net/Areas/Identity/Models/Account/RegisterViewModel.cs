// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace App.Areas.Identity.Models.AccountViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Please enter {0}")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter {0}")]
        [StringLength(100, ErrorMessage = "{0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Username")]
        [Required(ErrorMessage = "Please enter {0}")]
        [StringLength(100, ErrorMessage = "{0} must be between {2} and {1} characters long.", MinimumLength = 3)]
        public string UserName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Full Name")]
        [Required(ErrorMessage = "Please enter {0}")]
        [StringLength(100, ErrorMessage = "{0} must be between {2} and {1} characters long.", MinimumLength = 3)]
        public string FullName { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Address")]
        [StringLength(100, ErrorMessage = "{0} must be between {2} and {1} characters long.", MinimumLength = 3)]
        public string HomeAddess { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Phone")]
        [StringLength(100, ErrorMessage = "{0} must be between {2} and {1} characters long.", MinimumLength = 3)]
        public string Phone { get; set; }

        [Display(Name = "Gender")]
        public int Gender { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "BirthDay")]
        [Required(ErrorMessage = "Please enter {0}")]
        [AgeValidation(ErrorMessage = "Just greater than 18 year olds!")]
        public DateTime BirthDay { get; set; }



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
