using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class OrderDetailContainer
    {
        public order OrderDetail { get; set; }

        public List<order_products> OrderProducts { get; set; }
    }
}