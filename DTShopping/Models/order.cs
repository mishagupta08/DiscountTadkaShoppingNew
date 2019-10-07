using PagedList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class order
    {
        public int id { get; set; }
        public Nullable<int> payment_type { get; set; }
        public Nullable<int> payment_method { get; set; }
        public string discount_point { get; set; }
        public byte status { get; set; }
        public string shipping_charge { get; set; }
        public double amount { get; set; }
        public int user_id { get; set; }
        public string billing_country { get; set; }
        public string billing_first_name { get; set; }
        public string billing_last_name { get; set; }
        public string billing_address_1 { get; set; }
        public string billing_address_2 { get; set; }
        public string billing_city { get; set; }
        public string billing_state { get; set; }
        public string billing_postcode { get; set; }
        public string billing_email { get; set; }
        public string billing_phone { get; set; }
        public string payment_mode { get; set; }
        public string payment_ref_no { get; set; }
        public System.DateTime dt_payment_ref_date { get; set; }
        public string payment_ref_bank { get; set; }
        public string payment_ref_branch { get; set; }
        public string payment_ref_amount { get; set; }
        public string smartCardNo { get; set; }
        public string mlmUser { get; set; }
        public Nullable<System.DateTime> dispatch_date { get; set; }
        public string dockit_number { get; set; }
        public string return_dockit { get; set; }
        public Nullable<System.DateTime> return_dispatch_date { get; set; }
        public string courier { get; set; }
        public string encrypted_data { get; set; }
        public string RBV { get; set; }
        public string tid { get; set; }
        public string ip_address { get; set; }
        public System.DateTime created { get; set; }
        public System.DateTime modified { get; set; }
        public Nullable<int> company_id { get; set; }
        public int quantity { get; set; }
        public string orderStatus { get; set; }
        public string CompanyName2 { get; set; }
        public string companyName { get; set; }
        public string username { get; set; }
        public string ProductCode { get; set; }
        public string ProductName { get; set; }
        public string market_price { get; set; }
        public string OfferPrice { get; set; }
        public int point_adjusted { get; set; }
    }

    public class PagedOrderList 
    {
        public List<order> OrderList { get; set; }
        public IPagedList<int> pagerCount { get; set; }
    }
}
