using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class Product
    {
        public int id { get; set; }
        public Nullable<int> brand_id { get; set; }
        public Nullable<int> category_id { get; set; }
        public string product_code { get; set; }
        public string slug { get; set; }
        public string title { get; set; }
        public string short_description { get; set; }
        public string description_detail { get; set; }
        public string info_url { get; set; }
        public string small_image { get; set; }
        public string medium_image { get; set; }
        public string large_image { get; set; }
        public string market_price { get; set; }
        public string offer_price { get; set; }
        public Nullable<int> point_adjusted_old { get; set; }
        public Nullable<int> point_adjusted { get; set; }
        public Nullable<int> shippng_charge { get; set; }
        public string cost_price { get; set; }
        public string business_volume { get; set; }
        public Nullable<int> weight_detail { get; set; }
        public Nullable<byte> featured { get; set; }
        public Nullable<byte> display { get; set; }
        public Nullable<byte> instock { get; set; }
        public Nullable<byte> latest { get; set; }
        public Nullable<byte> font_display { get; set; }
        public string product_type { get; set; }
        public string product_size { get; set; }
        public string product_varient { get; set; }
        public Nullable<int> product_segment { get; set; }
        public string other_product_ids { get; set; }
        public Nullable<System.DateTime> created { get; set; }
        public Nullable<System.DateTime> modified { get; set; }
        public Nullable<int> vendor_id { get; set; }
        public int no_units { get; set; }
        public Nullable<int> vendor_qty { get; set; }
        public string vendor_offer { get; set; }
        public string VendorName { get; set; }
        public Nullable<int> RBV { get; set; }
        public double TotalPayment { get; set; }
        public decimal amount { get; set; }
    }

    public class PagewiseProducts {
        public IPagedList<int> pagerCount { get; set; }
        public List<Product> ProductList{get;set;}
        public string order { get; set; }
        public string sortby { get; set; }
        public string SearchString { get; set; }

    }
}
