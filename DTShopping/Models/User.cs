using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class UserDetails
    {
        public int id { get; set; }
        public Nullable<int> role_id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string passwordDetail { get; set; }
        public string password_str { get; set; }
        public string smart_card_no { get; set; }
        public Nullable<int> company_id { get; set; }
        public string mlm_user { get; set; }
        public string phone { get; set; }
        public Nullable<double> balance_point { get; set; }
        public Nullable<double> wallet_amount { get; set; }
        public Nullable<byte> status { get; set; }
        public Nullable<System.DateTime> last_login { get; set; }
        public Nullable<System.DateTime> created { get; set; }
        public Nullable<System.DateTime> modified { get; set; }
        public Nullable<System.DateTime> point_last_updated { get; set; }
        public string companyName { get; set; }
        public string GstNo { get; set; }
        public string Address { get; set; }
        public Nullable<int> StateId { get; set; }
        public string StateName { get; set; }
        public Nullable<int> CityId { get; set; }
        public string CityName { get; set; }
        public Nullable<int> DistrictId { get; set; }
        public string DistrictName { get; set; }
        public string AccountNumber { get; set; }
        public string BankName { get; set; }
        public string OtpCode { get; set; }
        public string IfscCode { get; set; }
    }
}