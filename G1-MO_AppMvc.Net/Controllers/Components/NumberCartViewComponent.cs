using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using App.Extension;
using App.Model;
using App.ModelViews;
using Microsoft.AspNetCore.Mvc;

namespace App.Controllers.Components
{
    public class NumberCartViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {   
            var cart = HttpContext.Session.Get<List<ShoppingCartItem>>("Cart");
            return View(cart);
        }
    }
}