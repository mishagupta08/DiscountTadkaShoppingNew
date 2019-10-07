using DTShopping.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class ShoppingPortalFrontPageProdList
    {
        public List<Product> SpeacialSegment { get; set; }
        public List<Product> DealOfDay { get; set; }
        public List<discount_coupons> CouponList { get; set; }
        public List<Product> SevenDaySaver { get; set; }
        public List<Product> FashionProdList { get; set; }
        public List<Product> ElectronicProdList { get; set; }
        public List<brand> brandlist { get; set; }
    }
}