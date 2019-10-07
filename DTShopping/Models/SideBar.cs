using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class SideBar
    {
        public List<Category> categoryList { get; set; }
        public Product latestProduct { get; set; }
    }
}