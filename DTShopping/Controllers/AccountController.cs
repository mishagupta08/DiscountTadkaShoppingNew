using System.Threading.Tasks;
using System.Web.Mvc;
using DTShopping.Models;
using DTShopping.Repository;
using System.Collections.Generic;
using System;
using System.Web.Security;
using Newtonsoft.Json;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Globalization;
using DTShopping.Properties;
using DTShopping.Controllers;
using System.Net;
using System.Reflection;

namespace DTShopping
{
    //[Authorize]
    public class AccountController : Controller
    {
        private APIRepository _APIManager;
        string Theme = System.Configuration.ConfigurationManager.AppSettings["Theme"] == null ? string.Empty : System.Configuration.ConfigurationManager.AppSettings["Theme"].ToString();
        General clsgen = new General();
        private static byte[] KeyByte = Encoding.ASCII.GetBytes("6b04d38748f94490a636cf1be3d82841");
        private static byte[] IVByte = Encoding.ASCII.GetBytes("f8adbf3c94b7463d");

        public Dashboard model;

        public AccountController()
        {
        }

        public AccountController(APIRepository Manager)
        {
            _APIManager = Manager;
        }


        //
        // GET: /Account/Login
        [AllowAnonymous]
        public async Task<ActionResult> Login(string returnUrl)
        {
            if (Theme == Resources.Orange)
            {
                return await LoginOrangeFunction(returnUrl);
            }
            else
            {
                ViewBag.ReturnUrl = returnUrl;
                return View();
            }

        }

        [AllowAnonymous]
        public async Task<ActionResult> LoginOrangeFunction(string returnUrl)
        {
            this.model = new Dashboard();
            ViewBag.ReturnUrl = returnUrl;
            this._APIManager = new APIRepository();
            await this.AssignStateCityList();
            return View("LoginOrange", this.model);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> LoginApiUser(string data)
        {
            try
            {
                //var dataStr = "GH637158|281614|30|" + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
               // var d = Encrypt(dataStr, KeyByte, IVByte);
               //var dataStr = "AT6665090|123123|45|02-12-2020 16:39:20";
               // string encrypted = Encrypt(dataStr, KeyByte, IVByte);
               //data = encrypted;
               var detail = Decrypt(data, KeyByte, IVByte);



                // var filename = AppDomain.CurrentDomain.BaseDirectory + "App_Data\\" + "log\\" + "logErrors.txt";
                //var sw = new System.IO.StreamWriter(filename, true);
                // sw.WriteLine(DateTime.Now.ToString() + " " + detail );
                //sw.Close();
                //var base64EncodedBytes = System.Convert.FromBase64String(data);
                //var detail = System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
                string path = System.Web.HttpContext.Current.Server.MapPath("~/Logs/ErrorLog");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                clsgen.ErrorLog(path, ("LoginApiUser:" + detail));
                if (detail != null && detail.Contains("|"))
                {
                    var dataArray = detail.Split('|');

                    if (dataArray.Length == 4)
                    {
                        //check if request is within 1 min
                        DateTime queryTime = DateTime.ParseExact(dataArray[3], "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        var currenttimeStr = DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        DateTime currentTime = DateTime.ParseExact(currenttimeStr, "dd-MM-yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                        var span = currentTime.Subtract(queryTime);
                        if (span.TotalMinutes <= 1)
                        {
                            var userDetail = new UserDetails();
                            userDetail.username = dataArray[0];
                            userDetail.passwordDetail = dataArray[1];
                            userDetail.company_id = Convert.ToInt32(dataArray[2]);

                            _APIManager = new APIRepository();
                            var result = await _APIManager.Login(userDetail);
                            Session["UserDetail"] = null;
                            if (result == null)
                            {
                                return null;
                            }
                            else
                            {
                                if (result.Status == true)
                                {
                                    UserDetails user = JsonConvert.DeserializeObject<UserDetails>(result.ResponseValue);
                                    FormsAuthentication.SetAuthCookie(user.username, false);
                                    Session["UserDetail"] = user;

                                    return RedirectToAction("Index", "Home");
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                    }
                    else
                    {
                        return null;
                    }
                }
            }
            catch (Exception e)
            {
                ExceptionLogging.SendErrorToText(e);
            }

            return null;
        }
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(UserDetails User)
        {
            try
            {
                var companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
                if (!string.IsNullOrEmpty(companyId))
                    User.company_id = Convert.ToInt16(companyId);
                User.role_id = 1;
                _APIManager = new APIRepository();
                var result = await _APIManager.Login(User);
                Session["UserDetail"] = null;
                if (result != null)
                {
                    if (result.Status == true)
                    {
                        UserDetails user = JsonConvert.DeserializeObject<UserDetails>(result.ResponseValue);
                        FormsAuthentication.SetAuthCookie(user.username, false);
                        Session["UserDetail"] = user;
                        return Json("Success", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(result.ResponseValue, JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Login Failed", JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }




        [AllowAnonymous]
        public async Task<ActionResult> OtherRegister()
        {
            this.model = new Dashboard();
            this._APIManager = new APIRepository();
            await this.AssignOtherAreaCode("");
            return View(this.model);
        }

        // GET: /Account/Register
        [AllowAnonymous]
        public async Task<ActionResult> Register()
        {
            this.model = new Dashboard();
            this._APIManager = new APIRepository();
            await this.AssignStateCityList();
            return View(this.model);
        }

        public async Task<ActionResult> getPinCodeStateDetail(string pinCode)
        {
            this.model = new Dashboard();
            this._APIManager = new APIRepository();
            AreaCoderesponse Area = new AreaCoderesponse();
            // var arealist= AssignOtherAreaCode(pinCode);
            Response responseDetail = new Response();
            General clsgen = new General();
            string jsonResponse = string.Empty;
            try
            {
                APIOtherPinCode otherPincode = new APIOtherPinCode();
                otherPincode.reqtype = "getpincodedetail";
                otherPincode.pincode = pinCode;
                string output1 = JsonConvert.SerializeObject(otherPincode);
                HttpWebRequest reponse;
                reponse = clsgen.JSON(output1, "https://cpanel.gohappynetwork.com/DTProcess.aspx");
                jsonResponse = clsgen.GetResponse(reponse);

                Area = JsonConvert.DeserializeObject<AreaCoderesponse>(jsonResponse);

                ApiPinCoderesponse Code = new ApiPinCoderesponse();
                {
                    Code.request = output1;
                    Code.response = jsonResponse;
                    Code.url = "https://cpanel.gohappynetwork.com/DTProcess.aspx";
                }
                var statusID = await this._APIManager.SaveAPIRequest(Code);
                //statusID = await Task.Run(() => entity.sp_SaveAPIRequest(output1, jsonResponse, "https://cpanel.gohappynetwork.com/DTProcess.aspx"));
                //responseDetail.ResponseValue = jsonResponse;
            }
            catch (Exception ex)

            {

            }
            return Json(Area);
        }

 
        public async Task<ActionResult> SaveOtherRegister(string mobileNo,string referralid,string name,string address,string statecode, string district, string city, string email, string areacode, string citycode, string districtcode, string panno ,string pinCode)
        {

            General clsgen = new General();
            AreaCoderesponse Area = new AreaCoderesponse();
            this._APIManager = new APIRepository();
            string jsonResponse = string.Empty;
            ApiOtherRegister register = new ApiOtherRegister();
            try
            {
            register.reqtype = "register";
            register.username = string.Empty;
            register.name = name;
            register.mobile = "0";
            register.pincode = pinCode;
            register.referralid = referralid == null ? "" : referralid;
            register.side = "1";
            register.fname = string.Empty;
            register.dob = "08-04-1992";
            register.ismarried = "Y";
            register.marriagedate = "08-05-2018";
            register.address = address;
            register.statecode = statecode;
            register.district =district;
            register.city =city;
            register.mobl = mobileNo;
            register.phoneno = "1482220754";
            register.email = email;
            register.nominee = "testjfds";
            register.relation = "dsdd";
            register.mpasswd = "123456";
            register.areacode = areacode;
            register.citycode = citycode;
            register.districtcode = districtcode;
            register.frelation = "D/O";
            register.actype = "CHOOSE Account Type";
            register.bankcode = "30";
            register.panno = panno;
            register.accountno = "123456868";
            register.ifsc = "PUNB112478";
            register.branch = "sanganerigate";
            register.aadharno = "12323434";
            string output1 = JsonConvert.SerializeObject(register);
            HttpWebRequest reponse;
            reponse = clsgen.JSON(output1, "https://cpanel.gohappynetwork.com/DTProcess.aspx");
            jsonResponse = clsgen.GetResponse(reponse);

            ApiPinCoderesponse Code = new ApiPinCoderesponse();
            {
                Code.request = output1;
                Code.response = jsonResponse;
                Code.url = "https://cpanel.gohappynetwork.com/DTProcess.aspx";
            }
            var statusID = await this._APIManager.SaveAPIRequest(Code);
        }
        catch(Exception ex)
          {

          }
            return Json(jsonResponse);
        }

        //
        // POST: /Account/Register
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            return View(model);
        }

        public ActionResult LogOff()
        {
            //AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            FormsAuthentication.SignOut();
            Session["UserDetail"] = null;
            return RedirectToAction("Index", "Home");
        }

         private async Task AssignOtherAreaCode(string pinCode)
        {
            this.model.AreaCoderesponse = await this._APIManager.GetAreaCode(pinCode);
            if (this.model.AreaCoderesponse == null)
            {
                this.model.areacode = new List<M_areacode>();
                this.model.areacode.Add(new M_areacode
                {
                    areacode = "0",
                    areaname = "-Not Available-"
                });
            }
        }

        private async Task AssignStateCityList()
        {
            this.model.States = await this._APIManager.GetStateList();
            if (this.model.States == null)
            {
                this.model.States = new List<R_StateMaster>();
                this.model.States.Add(new R_StateMaster
                {
                    Id = 0,
                    Name = "-Not Available-"
                });
            }

            //if (this.model.RetailerDetail != null)
            //{
            //    this.model.CityList = await this.repository.GetCityList(this.model.RetailerDetail.StateId);
            //}

            if (this.model.Cities == null)
            {
                this.model.Cities = new List<R_CityMaster>();
                this.model.Cities.Add(new R_CityMaster
                {
                    cityID = 0,
                    cityName = "-Not Available-"
                });
            }
        }

        public async Task<ActionResult> SaveDetail(Dashboard dashboardModel)
        {
            if (dashboardModel == null || dashboardModel.User == null)
            {
                return Json("Please send complete detail.");
            }

            this._APIManager = new APIRepository();
            var res = await this._APIManager.Register(dashboardModel.User);
            if (res.Status == false)
            {
                return Json(res.ResponseValue);
            }
            else
            {
                return Json("Registration done successfully.");
            }
        }

        public async Task<ActionResult> GetCityListByState(string Id)
        {
            var cityList = new List<R_CityMaster>();
            this._APIManager = new APIRepository();
            this.model = new Dashboard();
            if (string.IsNullOrEmpty(Id))
            {
                return null;
            }
            else
            {
                cityList = await this._APIManager.GetCityListById(Id);
            }

            if (cityList == null || cityList.Count == 0)
            {
                cityList = new List<R_CityMaster>();
                if (this.model.Cities == null)
                {
                    this.model.Cities = new List<R_CityMaster>();
                    this.model.Cities.Add(new R_CityMaster
                    {
                        cityID = 0,
                        cityName = "-Not Available-"
                    });
                }
            }
            return Json(cityList);
        }

        static void EncryptAesManaged(string raw)
        {
            try
            {
                // Create Aes that generates a new key and initialization vector (IV).    
                // Same key must be used in encryption and decryption    
                // Encrypt string    

                using (AesManaged aes = new AesManaged())
                {
                    string encrypted = Encrypt(raw, KeyByte, IVByte);
                    // Decrypt. the bytes to a string.    
                    string decrypted = Decrypt(encrypted, KeyByte, IVByte);
                }
            }
            catch (Exception exp)
            {
                Console.WriteLine(exp.Message);
            }
            Console.ReadKey();
        }

        /******Encrypt Functions*****/
        static string Encrypt(string plainText, byte[] Key, byte[] IV)
        {
            byte[] encrypted;
            // Create a new AesManaged.    
            using (AesManaged aes = new AesManaged())
            {
                // Create encryptor    
                ICryptoTransform encryptor = aes.CreateEncryptor(Key, IV);
                // Create MemoryStream    
                using (MemoryStream ms = new MemoryStream())
                {
                    // Create crypto stream using the CryptoStream class. This class is the key to encryption    
                    // and encrypts and decrypts data from any given stream. In this case, we will pass a memory stream    
                    // to encrypt    
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    {
                        // Create StreamWriter and write data to a stream    
                        using (StreamWriter sw = new StreamWriter(cs))
                            sw.Write(plainText);
                        encrypted = ms.ToArray();
                    }
                }
            }
            // Return encrypted data    
            return Convert.ToBase64String(encrypted);
        }

        /*****END****/

        /******Decrypt Functions*****/
        static string Decrypt(string data, byte[] Key, byte[] IV)
        {
            byte[] cipherText = Convert.FromBase64String(data);
            string plaintext = null;
            // Create AesManaged    
            using (AesManaged aes = new AesManaged())
            {
                // Create a decryptor    
                ICryptoTransform decryptor = aes.CreateDecryptor(Key, IV);
                // Create the streams used for decryption.    
                using (MemoryStream ms = new MemoryStream(cipherText))
                {
                    // Create crypto stream    
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    {
                        // Read crypto stream    
                        using (StreamReader reader = new StreamReader(cs))
                            plaintext = reader.ReadToEnd();
                    }
                }
            }
            return plaintext;
        }

        /*****END****/
        public void ErrorLog(string sPathName, string sErrMsg)
        {
            string sLogFormat = (DateTime.Now.ToShortDateString().ToString() + (" "
                        + (DateTime.Now.ToLongTimeString().ToString() + " ==> ")));
            string sYear = DateTime.Now.Year.ToString();
            string sMonth = DateTime.Now.Month.ToString();
            string sDay = DateTime.Now.Day.ToString();
            string sErrorTime = (sYear
                        + (sMonth + sDay));
            StreamWriter sw = new StreamWriter((sPathName + sErrorTime), true);
            sw.WriteLine((sLogFormat + sErrMsg));
            sw.Flush();
            sw.Close();
        }
    }
}