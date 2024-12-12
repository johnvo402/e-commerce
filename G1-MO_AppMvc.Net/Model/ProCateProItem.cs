using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Model
{
    public class ProCateProItem
    {
        
        public Product products {get; set;}
        public IEnumerable<ProductItem> productItems {get; set;}
        public Category categories {get; set;}
        public IEnumerable<Review> reviews {get; set;}
    }
}