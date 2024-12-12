namespace App.Model
{
    public class CartAndProduct
    {
        public Product products { get; set; }
        public ProductItem productItems { get; set; }

        public ShoppingCartItem shoppingCartItems { get; set;}
    }
}
