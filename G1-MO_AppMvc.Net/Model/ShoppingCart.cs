using System;
using System.Collections.Generic;

#nullable disable

namespace App.Model
{
    public partial class ShoppingCart
    {
        public ShoppingCart()
        {
            ShoppingCartItems = new HashSet<ShoppingCartItem>();
        }

        public string IdCart { get; set; }
        public string IdAcc { get; set; }

        public virtual User IdAccNavigation { get; set; }
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
    }
}
