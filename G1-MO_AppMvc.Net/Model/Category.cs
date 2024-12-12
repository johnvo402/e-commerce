using System;
using System.Collections.Generic;

#nullable disable

namespace App.Model
{
    public partial class Category
    {
        public Category()
        {
            Products = new HashSet<Product>();
        }

        public string IdCate { get; set; }
        public string NameCate { get; set; }

        public virtual ICollection<Product> Products { get; set; }
    }
}
