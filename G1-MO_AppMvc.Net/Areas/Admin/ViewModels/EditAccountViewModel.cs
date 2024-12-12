using System;

namespace App.Areas.Admin.ViewModels
{
    public class EditAccountViewModel
    {
        public string IdAcc { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Fullname { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int Gender { get; set; }
        public DateTime? Birthday { get; set; }
        public int IsSeller { get; set; }
    }
}
