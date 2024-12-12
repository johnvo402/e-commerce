using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace App.Model
{

    public partial class ProductItem
    {
        public ProductItem()
        {
            OrderDetails = new HashSet<OrderDetail>();
            ShoppingCartItems = new HashSet<ShoppingCartItem>();
        }


        public string IdProItem { get; set; }
        public string IdPro { get; set; }
        [Required(ErrorMessage = "Kind of product not null!!")]
        [DisplayName("Kind of product")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Quantity not null!!")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be greater than 1 ")]
        public int Quantity { get; set; }
        [Required(ErrorMessage = "ProPrice not null!!")]
        [Range(1, int.MaxValue, ErrorMessage = "Price must be greater than 1 ")]

        public double ProPrice { get; set; }
        [Required(ErrorMessage = "Discount not null!!")]
        public double? Discount { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Discount != null)
            {
                if (Discount < 0 || Discount > 100)
                {
                    yield return new ValidationResult("Discount must be between 0 and 100", new[] { nameof(Discount) });
                }
            }
        }
        public int? StatusProItem { get; set; }

        public virtual Product IdProNavigation { get; set; }
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        public virtual ICollection<ShoppingCartItem> ShoppingCartItems { get; set; }
    }

}
