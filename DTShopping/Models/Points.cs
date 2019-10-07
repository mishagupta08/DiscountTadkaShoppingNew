using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public partial class PointsLedger
    {
        public string Id { get; set; }
        public Nullable<int> UserId { get; set; }
        public Nullable<int> CompanyId { get; set; }
        public Nullable<System.DateTime> UpdatonDate { get; set; }
        public Nullable<decimal> Credit { get; set; }
        public Nullable<System.DateTime> CreditValidUpTo { get; set; }
        public Nullable<decimal> Dabit { get; set; }
        public Nullable<decimal> AvailableBalance { get; set; }
        public string ReferenceNo { get; set; }
        public string Remark { get; set; }

    }
}