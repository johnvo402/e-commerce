using System;
using System.Collections.Generic;

#nullable disable

namespace App.Model
{
    public partial class Contact
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public DateTime DateSent { get; set; }
        public string Message { get; set; }
        public string Phone { get; set; }
    }
}
