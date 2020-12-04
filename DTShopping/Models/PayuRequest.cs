using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class PayuRequest
    {
        public double TransactionAmount { get; set; }
        public string ProductInfo { get; set; }
        public string FirstName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string curl { get; set; }
        public string surl { get; set; }
        public string furl { get; set; }
        public string udf1 { get; set; }
        public string udf3 { get; set; }
        public string udf2 { get; set; }
        public string memberId { get; set; }
    }
}