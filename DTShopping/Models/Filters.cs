using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class Filters
    {
        public Nullable<int> pageNo { get; set; }

        public int NoOfRecord { get; set; }

        public Nullable<int> VendorId { get; set; }

        public Nullable<int> CompanyId { get; set; }

        public Nullable<int> CategoryId { get; set; }

        public Nullable<int> BrandId { get; set; }

        public byte Status { get; set; }

        public string pageName { get; set; }

        public Nullable<int> Id { get; set; }

        public bool SortByPrice { get; set; }

        public bool IsPriceLowToHigh { get; set; }

        public bool SortByPoints { get; set; }

        public bool IsPointLowToHigh { get; set; }

        public int? FilterFromPrice { get; set; }

        public int? FilterToPrice { get; set; }

        public int? FilterFromPoint { get; set; }

        public int? FilterToPoint { get; set; }

        public string SelectedFilterName { get; set; }

        public string FilterValue { get; set; }
    }
}