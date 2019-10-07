using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class discount_coupons
    {
        public int id { get; set; }
        public Nullable<int> company_id { get; set; }
        public string mlm_user_id { get; set; }
        public string smart_card_no { get; set; }
        public string coupon_number { get; set; }
        public string coupon_type { get; set; }
        public Nullable<double> discount_percent { get; set; }
        public Nullable<double> max_discount { get; set; }
        public Nullable<double> min_purchase_amt { get; set; }
        public Nullable<System.DateTime> start_date { get; set; }
        public Nullable<System.DateTime> end_date { get; set; }
        public Nullable<byte> status { get; set; }
        public string order_id { get; set; }
    }
}
