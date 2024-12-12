using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace App.Model
{
    public class ProCateProItemViewModel
    {
        
        public Product products {get; set;}
        public ProductItem productItems {get; set;}
        public Category categories {get; set;}
        public Review reviews {get; set;}
    }
}