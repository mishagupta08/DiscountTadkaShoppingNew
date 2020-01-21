using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class CatProducts
    {
        public string CategoryName { get; set; }
        public string CategoryId { get; set; }
        public List<Product> ProductList { get; set; }
    }
}