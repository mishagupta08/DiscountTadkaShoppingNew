using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class R_CityMaster
    {
        public int cityID { get; set; }
        public string cityName { get; set; }
        public string stateName { get; set; }
        public Nullable<int> StateId { get; set; }
    }
}