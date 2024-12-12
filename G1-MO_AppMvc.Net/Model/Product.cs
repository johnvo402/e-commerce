using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace App.Model
{
    public partial class Product
    {
        public Product()
        {
            ImgPros = new HashSet<ImgPro>();
            ProductItems = new HashSet<ProductItem>();
            Reviews = new HashSet<Review>();
            ShoppingCartItems = new HashSet<ShoppingCartItem>();
            Wishlists = new HashSet<Wishlist>();
        }


        public string IdPro { get; set; }
        [Required(ErrorMessage = "Product name not null!!")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Description not null!!")]
        public string Description { get; set; }
        public string IdCate { get; set; }
        public string IdAcc { get; set; }
        public int StatusPro { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? BestSaler { get; set; }

        public virtual User IdAccNavigation { get; set; }
        public virtual Category IdCateNavigation { get; set; }
        public virtual ICollection<ImgPro> ImgPros { get; set; }
        public virtual ICollection<ProductItem> ProductItems { get; set; }
        public virtual ICollection<Review> Reviews { get; set; }
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
        public virtual ICollection<Wishlist> Wishlists { get; set; }
    }

}
