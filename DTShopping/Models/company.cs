using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class company
    {
        public int id { get; set; }
        public string company_name { get; set; }
        public string website_title { get; set; }
        public string logo { get; set; }
        public string website_theme { get; set; }
        public double credit_point { get; set; }
        public string add1 { get; set; }
        public string add2 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string country { get; set; }
        public string zip { get; set; }
        public string company_landline_no { get; set; }
        public string extension { get; set; }
        public string fax { get; set; }
        public string website { get; set; }
        public string discount_tadka_website { get; set; }
        public string reward_tadka_website { get; set; }
        public string contact_name { get; set; }
        public string contact_no1 { get; set; }
        public string contact_no2 { get; set; }
        public string contact_email { get; set; }
        public string contact_email2 { get; set; }
        public string description { get; set; }
        public string theme_name { get; set; }
        public string domain_name { get; set; }
        public string front_page_url { get; set; }
        public byte default_flag { get; set; }
        public byte default_banner { get; set; }
        public bool globleProductDisplay { get; set; }
        public bool globlePrice { get; set; }
        public Nullable<System.DateTime> created { get; set; }
    }
}