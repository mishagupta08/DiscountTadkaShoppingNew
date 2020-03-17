using DTShopping.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class ProductDetailOrangeThemeContainer
    {
        public Product ProductDetail { get; set; }

        public List<Product> RelatedProductList { get; set; }

        public List<Product> SameBrandProductList { get; set; }
    }
}