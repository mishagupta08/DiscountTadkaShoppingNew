using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class CompanyProfile
    {
        public int Id { get; set; }
        public string AboutUsText { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<System.DateTime> CreatedDate { get; set; }
        public Nullable<System.DateTime> ModifiedDate { get; set; }
        public string PrivacyPolicy { get; set; }
        public string TermsAndCondition { get; set; }
    }
}