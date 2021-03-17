using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class APIOtherPinCode
    {
        public string reqtype { get; set; }
        public string userid { get; set; }
        public string passwd { get; set; }
        public string pincode { get; set; }
         
    }
     public class ApiPinCoderesponse
    {
        public string request { get; set; }
        public string response { get; set; }
        public string url { get; set; }
      

    }
}