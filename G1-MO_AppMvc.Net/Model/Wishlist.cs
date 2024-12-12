using System;
using System.Collections.Generic;

#nullable disable

namespace App.Model
{
    public partial class Wishlist
    {
        public string IdWishlist { get; set; }
        public string IdPro { get; set; }
        public string IdAcc { get; set; }

        public virtual User IdAccNavgation { get; set; }
        public virtual Product IdProNavigation { get; set; }
    }
}
