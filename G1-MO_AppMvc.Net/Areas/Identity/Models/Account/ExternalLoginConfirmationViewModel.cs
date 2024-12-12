// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace App.Areas.Identity.Models.AccountViewModels
{
    public class ExternalLoginConfirmationViewModel
    {
        [Required(ErrorMessage = "Please enter {0}")]
        [EmailAddress(ErrorMessage = "Please enter a valid email format")]

        public string Email { get; set; }
        [Required(ErrorMessage = "Just input {0}")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Just input {0}")]
        public string FullName { get; set; }
    }
}
