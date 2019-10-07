using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class product_images
    {
        public int id { get; set; }
        public int product_id { get; set; }
        public string images { get; set; }
        public System.DateTime created { get; set; }
        public System.DateTime modified { get; set; }
    }
}