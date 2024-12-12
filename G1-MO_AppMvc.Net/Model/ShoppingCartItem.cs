using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace App.Model
{
    public partial class ShoppingCartItem
    {
        public string IdCartItem { get; set; }
        public string IdCart { get; set; }
        public string IdProItem { get; set; }
        public int Quantity { get; set; }
        public double? Price { get; set; }
        public string IdPro { get; set; }

        public virtual ShoppingCart IdCartNavigation { get; set; }
        public virtual ProductItem IdProItemNavigation { get; set; }
        public virtual Product IdProNavigation { get; set; }
    }
}
