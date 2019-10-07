using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace DTShopping.Models
{
    public class Dashboard
    {
        public List<Category> MenuItems { get; set; }
        public List<Banners> Banners { get; set; }
        public List<R_StateMaster> States { get; set; }
        public List<R_CityMaster> Cities { get; set; }
        public List<Product> Products { get; set; }
        public UserDetails User { get; set; }
        public decimal NetPayment { get; set; }
        public double UsersPoints { get; set; }
        public double TotalProductPoints { get; set; }
        public Product ProductDetail { get; set; }
        public order OrderDetail { get; set; }
        public List<Containers> PaymentModeList { get; set; }
        public ShoppingPortalFrontPageProdList FontpageSections{get;set;}

        public void AssignPaymentModes()
        {
            this.PaymentModeList = new List<Containers>();
            this.PaymentModeList.Add(new Containers { value = "Cash Deposit"});
            this.PaymentModeList.Add(new Containers { value = "Demand Draft" });
            this.PaymentModeList.Add(new Containers { value = "Cheque" });
            this.PaymentModeList.Add(new Containers { value = "NEFT" });
            this.PaymentModeList.Add(new Containers { value = "RTGS" });
        }
    }

    public partial class brand
    {
        public int id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string logo { get; set; }
        public byte status { get; set; }
        public System.DateTime created { get; set; }
        public System.DateTime modified { get; set; }
    }
}