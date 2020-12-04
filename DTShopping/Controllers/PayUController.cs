using DTShopping.Models;
using System;
using System.Web.Mvc;
using System.Configuration;
using System.Security.Cryptography;
using System.Text;

namespace DTShopping.Controllers
{
    public class PayUController : Controller
    {
        public string SUrl = "";

        public string FUrl = "";

        private System.Collections.Specialized.NameValueCollection Inputs = new System.Collections.Specialized.NameValueCollection();

        public string Url = ConfigurationManager.AppSettings["PAYU_BASE_URL"];

        public string Method = "post";

        public string FormName = "form1";

        [HttpPost]
        public void Payment(PayuRequest request)
        {

            string firstName = request.FirstName;
            string amount = Convert.ToString(request.TransactionAmount);
            string productInfo = request.ProductInfo;
            string email = request.Email;
            string phone = request.Phone;

            PayUController myremotepost = new PayUController();

            string key = ConfigurationManager.AppSettings["MERCHANT_KEY"];
            string salt = ConfigurationManager.AppSettings["SALT"];

            //posting all the parameters required for integration.

            myremotepost.Url = ConfigurationManager.AppSettings["PAYU_BASE_URL"];

            myremotepost.SUrl = request.surl;
            myremotepost.FUrl = request.furl;

            myremotepost.Add("key", key);
            string txnid = Generatetxnid();
            myremotepost.Add("txnid", txnid);
            myremotepost.Add("amount", amount);
            myremotepost.Add("productinfo", productInfo);
            myremotepost.Add("firstname", firstName);
            myremotepost.Add("phone", phone);
            myremotepost.Add("email", email);
            myremotepost.Add("udf1", request.udf1);
            myremotepost.Add("udf2", request.udf2);
            myremotepost.Add("surl", myremotepost.SUrl);//Change the success url here depending upon the port number of your local system.
            myremotepost.Add("furl", myremotepost.FUrl);//Change the failure url here depending upon the port number of your local system.            
            myremotepost.Add("service_provider", "payu_paisa");
            string hashString = key + "|" + txnid + "|" + amount + "|" + productInfo + "|" + firstName + "|" + email + "|" + request.udf1 + "|" + request.udf2 + "|||||||||" + salt;
            //string hashString = "3Q5c3q|2590640|3053.00|OnlineBooking|vimallad|ladvimal@gmail.com|||||||||||mE2RxRwx";
            string hash = Generatehash512(hashString);
            myremotepost.Add("hash", hash);

            myremotepost.Post();

        }

        public void Add(string name, string value)
        {
            Inputs.Add(name, value);
        }

        public void Post()
        {
            System.Web.HttpContext.Current.Response.Clear();

            System.Web.HttpContext.Current.Response.Write("<html><head>");

            System.Web.HttpContext.Current.Response.Write(string.Format("</head><body onload=\"document.{0}.submit()\">", FormName));
            System.Web.HttpContext.Current.Response.Write(string.Format("<form name=\"{0}\" method=\"{1}\" action=\"{2}\" >", FormName, Method, Url));
            for (int i = 0; i < Inputs.Keys.Count; i++)
            {
                System.Web.HttpContext.Current.Response.Write(string.Format("<input name=\"{0}\" type=\"hidden\" value=\"{1}\">", Inputs.Keys[i], Inputs[Inputs.Keys[i]]));
            }
            System.Web.HttpContext.Current.Response.Write("</form>");
            System.Web.HttpContext.Current.Response.Write("</body></html>");

            System.Web.HttpContext.Current.Response.End();
        }

        public string Generatetxnid()
        {

            Random rnd = new Random();
            string strHash = Generatehash512(rnd.ToString() + DateTime.Now);
            string txnid1 = strHash.ToString().Substring(0, 20);

            return txnid1;
        }



        public string Generatehash512(string text)
        {
            byte[] message = Encoding.UTF8.GetBytes(text);

            UnicodeEncoding UE = new UnicodeEncoding();
            byte[] hashValue;
            SHA512Managed hashString = new SHA512Managed();
            string hex = "";
            hashValue = hashString.ComputeHash(message);
            foreach (byte x in hashValue)
            {
                hex += String.Format("{0:x2}", x);
            }

            return hex;
        }
    }
}