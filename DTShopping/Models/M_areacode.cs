using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class M_areacode
    {
        public string areacode { get; set; }
        public string areaname { get; set; }
    }

     public class AreaCoderesponse
    {
         public List<M_areacode> pincodedetail { get; set; }
         public string  statecode { get; set; }
         public string statename { get; set; }
         public string districtname { get; set; }
        public string districtCode { get; set; }
        public string cityname { get; set; }
        public string citycode { get; set; }
        public string response { get; set; }
    }
}