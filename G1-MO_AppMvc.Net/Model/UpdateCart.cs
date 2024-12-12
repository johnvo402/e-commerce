using App.Model;
using System.Collections;
using System.Collections.Generic;

namespace App.Model
{
    public class UpdateCart
    {
        public IEnumerable<ProductItem> proitem {  get; set; }
        public ShoppingCartItem ShoppingCartItem { get; set; }
    }
}
