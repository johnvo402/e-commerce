using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Model;

namespace App.ModelViews
{ 
    public class CartCartItem
    {
        public ShoppingCart cart { get; set; }
        public ShoppingCartItem cartItem { get; set; }
        public ProCateProItemViewModel product { get; set; }
        public int amount { get; set; }
        public double TotalMoney => amount * cartItem.Price.Value;
    }
}