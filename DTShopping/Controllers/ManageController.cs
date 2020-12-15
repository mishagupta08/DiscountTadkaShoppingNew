using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using DTShopping.Models;
using DTShopping.Repository;
using System.Collections.Generic;
using Newtonsoft.Json;
using DTShopping.Properties;

namespace DTShopping.Controllers
{
    //[Authorize]
    public class ManageController : Controller
    {
        private const string AddAction = "Add";
        private const int HOMEDELIVERYCODE = 3;
        private const int PAGESIZE = 7;
        private const string CartProductListAction = "CartProductList";
        private const int SUNVISCOMPANYID = 29;
        private const int SHOPCOMPANYID = 28;
        private const int MYDWORLDCOMPANYID = 44;
        private const int GOHAPPYCARTCOMPANYID = 30;
        private const int URSHOPECOMPANYID = 33;
        int SHOPENTERTAINMENTCOMPANYID = 37;
        int GVENTERTAINMENTCOMPANYID = 38;
        int SJLABSCOMPANYID = 40;
        int ETRADECOMPANYID = 42;
        int DUDHAMRITCOMPANYID = 43;
        int MYAIMTRADESERVICESCOMPANYID = 45;
        string Theme = System.Configuration.ConfigurationManager.AppSettings["Theme"] == null ? string.Empty : System.Configuration.ConfigurationManager.AppSettings["Theme"].ToString();
        int companyId = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CompanyId"]);

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        APIRepository objRepository = new APIRepository();
        Dashboard model = new Dashboard();

        public ManageController()
        {
        }

        public async Task<ActionResult> GetShoppingCartProductList()
        {
            this.model = new Dashboard();
            if (CheckLoginUserStatus())
            {
                try
                {
                    var userDetail = Session["UserDetail"] as UserDetails;
                    await SetShoppingCartProductInModel(this.model, userDetail);
                }
                catch (Exception ex)
                {
                    return null;
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }


            return View("cartProdListOnHover", this.model);
        }

        public async Task SetShoppingCartProductInModel(Dashboard dModel, UserDetails detail)
        {
            var cart = new CartFilter();
            //var detail = (UserDetails)(Session["UserDetail"]);
            cart.userId = detail.id;
            cart.username = detail.username;
            cart.password = detail.password_str;
            cart.companyId = Convert.ToInt16(companyId);
            var response = await objRepository.ManageCart(cart, CartProductListAction);
            if (response != null && response.Status)
            {
                dModel.Products = JsonConvert.DeserializeObject<List<Product>>(response.ResponseValue);
                if (dModel.Products != null)
                {
                    dModel.NetPayment = 0;
                    var prodPrice = 0.0;
                    dModel.ShippingCharge = 0;
                    dModel.TotalProductPoints = 0;
                    foreach (var prod in dModel.Products)
                    {
                        if (prod.offer_price == null || prod.offer_price == "0")
                        {
                            prodPrice = Convert.ToDouble(prod.market_price);
                        }
                        else
                        {
                            prodPrice = Convert.ToDouble(prod.offer_price);
                        }

                        prod.shippng_charge = (prod.vendor_qty ?? 1) * (prod.shippng_charge ?? 0);
                        //prod.TotalPayment = prodPrice * (prod.vendor_qty ?? 1) + (prod.shippng_charge ?? 0);
                        //this.model.TotalProductPoints += (prod.RBV ?? 0) * (prod.vendor_qty ?? 1);
                        //this.model.NetPayment += prod.TotalPayment;
                        dModel.ShippingCharge += (prod.shippng_charge ?? 0);
                        dModel.TotalProductPoints += (prod.shippng_charge ?? 0);
                        dModel.NetPayment += prod.amount;
                    }
                }
            }
        }

        private bool CheckLoginUserStatus()
        {
            if (Session["UserDetail"] == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public async Task<ActionResult> GetUserPointsListView()
        {
            this.model = new Dashboard();
            this.objRepository = new APIRepository();
            var result = new Response();
            if (CheckLoginUserStatus())
            {
                var point = new PointsLedger();
                var detail = (UserDetails)(Session["UserDetail"]);
                if (detail != null)
                {
                    point.UserId = detail.id;
                }

                result = await objRepository.ManagePoints(point, "ListByUserId");
                if (result != null && result.Status)
                {
                    this.model.ledgerList = new List<PointsLedger>();
                    this.model.ledgerList = JsonConvert.DeserializeObject<List<PointsLedger>>(result.ResponseValue);
                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }

            return View("_retailerPointsPartialView", model);
        }

        public async Task<ActionResult> GetOrderDetailPage(int id)
        {
            if (CheckLoginUserStatus())
            {
                this.model = new Dashboard();
                this.objRepository = new APIRepository();
                var data = new order();
                data.id = id;
                this.model.orderDetailContainer = await this.objRepository.ManageOrderWithProducts(data);
                return View("orderDetailPrintPage", this.model);
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }
        }

        public async Task<ActionResult> SaveDetailFormOtp(Dashboard detailModel)
        {
            this.model = new Dashboard();
            objRepository = new APIRepository();
            var result = new Response();
            var CompanyId = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CompanyId"]);
            var detail = (UserDetails)(Session["UserDetail"]);
            //detail.username = detailModel.User.username;
            //detail.password_str = detailModel.User.password_str;
            detail.Amount = detailModel.Amount;
            detail.OtpCode = detailModel.User.OtpCode;
            detailModel.User.username = detail.username;
            detail.WalletType = detailModel.User.WalletType;
            if (companyId != ETRADECOMPANYID)
            {
                result = await this.objRepository.MangeOtpFunctions(detail, "ValidateOtp");
            }

            detailModel.OrderDetail = new order();
            detailModel.OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;
            if (result == null && companyId != ETRADECOMPANYID)
            {
                return Json(Resources.ErrorMessage);
            }
            else if (!result.Status && companyId != ETRADECOMPANYID)
            {
                return Json("Invald OTP.");
            }
            else
            {
                if (CompanyId == 17)
                {
                    result = await objRepository.CreateOrder(detailModel.OrderDetail, "EditWithOtp");
                    if (result == null)
                    {
                        return Json(Resources.ErrorMessage);
                    }
                    else if (!result.Status)
                    {
                        return Json(result.ResponseValue);
                    }
                    else
                    {
                        return Json("ok");
                    }
                }
                else if (CompanyId == SHOPCOMPANYID || companyId == SJLABSCOMPANYID || CompanyId == SUNVISCOMPANYID || CompanyId == GOHAPPYCARTCOMPANYID || CompanyId == URSHOPECOMPANYID || CompanyId == GVENTERTAINMENTCOMPANYID || CompanyId == SHOPENTERTAINMENTCOMPANYID || companyId == ETRADECOMPANYID || CompanyId == DUDHAMRITCOMPANYID || CompanyId == MYDWORLDCOMPANYID || CompanyId == MYAIMTRADESERVICESCOMPANYID)
                {
                    var usr = new UserDetails();
                    usr = detail;
                    //usr.password_str = usr.passwordDetail;
                    usr.orderId = detailModel.OrderDetail.id;
                    var deductWallet = await objRepository.DeductWalletBalnce(usr);
                    if (deductWallet.Status)
                    {
                        var Wallet = JsonConvert.DeserializeObject<WalletDeduction>(deductWallet.ResponseValue);
                        if (Wallet.response.ToLower() == "ok")
                        {
                            detail.Voucher = Wallet.voucherno;

                            var Walletconfirm = new Response();
                            if (CompanyId == URSHOPECOMPANYID)
                            {
                                Walletconfirm.Status = true;
                            }
                            else
                            {
                                Walletconfirm = await objRepository.WalletConfirmationAPI(usr);
                            }

                            if (Walletconfirm.Status)
                            {
                                var ConfirmWallet = new WalletDetails();
                                if (CompanyId == URSHOPECOMPANYID)
                                {
                                    ConfirmWallet.response = Wallet.response;
                                }
                                else
                                {
                                    ConfirmWallet = JsonConvert.DeserializeObject<WalletDetails>(Walletconfirm.ResponseValue);
                                }

                                if (ConfirmWallet == null || ConfirmWallet.response == null)
                                {
                                    return Json("Empty reponse received while wallet confirmation.");
                                }
                                if (ConfirmWallet.response.ToLower() == "ok")
                                {
                                    detailModel.OrderDetail = new order();
                                    detailModel.OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;
                                    detailModel.OrderDetail.payment_ref_no = Wallet.voucherno;
                                    detailModel.OrderDetail.payment_ref_amount = detailModel.Amount.ToString();
                                    detailModel.OrderDetail.user_id = usr.id;
                                    result = await objRepository.CreateOrder(detailModel.OrderDetail, "EditWithOtp");
                                    if (result == null)
                                    {
                                        return Json(Resources.ErrorMessage);
                                    }
                                    else if (!result.Status)
                                    {
                                        return Json(result.ResponseValue);
                                    }
                                    else
                                    {
                                        return Json("ok");
                                    }
                                }
                                else
                                {
                                    return Json("Something went wrong");
                                }
                            }
                            else
                            {
                                return Json(Walletconfirm.ResponseValue);
                            }
                        }
                        else
                        {
                            return Json(Wallet.msg);
                        }

                    }
                    else
                    {
                        return Json(deductWallet.ResponseValue);
                    }
                }
            }
            return Json("Something went wrong");
        }

        public async Task<ActionResult> SaveOrderDetailWithPoints(Dashboard detailModel)
        {
            this.model = new Dashboard();
            objRepository = new APIRepository();
            var result = new Response();
            detailModel.OrderDetail.id = Convert.ToInt16(Session["OrderId"]);

            detailModel.OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;
            detailModel.OrderDetail.payment_ref_amount = Convert.ToString(detailModel.NetPayment);
            result = await objRepository.CreateOrder(detailModel.OrderDetail, "editwithpoints");

            if (result == null)
            {
                return Json(Resources.ErrorMessage);
            }
            else if (!result.Status)
            {
                return Json(result.ResponseValue);
            }
            else
            {
                //redirct to thank you page
                //return Json(result.ResponseValue);

                return RedirectToAction("thankYouPage", "Manage");
            }
        }

        public async Task<ActionResult> UpdateOrderDetail(Dashboard detailModel)
        {
            if (detailModel == null || detailModel.OrderDetail == null)
            {
                return Json(Resources.ErrorMessage, JsonRequestBehavior.AllowGet);
            }
            else if (string.IsNullOrEmpty(detailModel.OrderDetail.OtpCode))
            {
                return Json("Please enter valid OTP", JsonRequestBehavior.AllowGet);
            }
            else
            {
                this.model = new Dashboard();
                objRepository = new APIRepository();
                var result = new Response();

                var detail = (UserDetails)(Session["UserDetail"]);
                detail.OtpCode = detailModel.OrderDetail.OtpCode;
                result = await this.objRepository.MangeOtpFunctions(detail, "ValidateOtp");
                if (result == null)
                {
                    return Json(Resources.ErrorMessage);
                }
                else if (!result.Status)
                {
                    return Json("Invald OTP.");
                }
                else
                {
                    detailModel.OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;
                    detailModel.OrderDetail.user_id = Session["UserDetail"] != null ? (Session["UserDetail"] as UserDetails).id : 0;
                    result = await objRepository.CreateOrder(detailModel.OrderDetail, "Edit");

                    if (result == null)
                    {
                        return Json(Resources.ErrorMessage, JsonRequestBehavior.AllowGet);
                    }
                    else if (!result.Status)
                    {
                        return Json(result.ResponseValue, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("ok", JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult> GetProductDetail(int prodId)
        {
            this.model = new Dashboard();
            this.objRepository = new APIRepository();
            try
            {
                var prodList = new List<Product>();
                prodList.Add(new Product { id = prodId });
                this.model.ProductDetail = await objRepository.GetProductDetailById(prodList);
                if (this.model.ProductDetail != null)
                {
                    this.model.ProductDetail.description_detail = this.model.ProductDetail.description_detail.Replace("\r\n\r\n", "");
                    if (!string.IsNullOrEmpty(this.model.ProductDetail.product_size) && this.model.ProductDetail.product_size != "0")
                    {
                        this.model.ProductDetail.sizeList = this.model.ProductDetail.product_size.Split(',').ToList();
                    }
                    if (!string.IsNullOrEmpty(this.model.ProductDetail.Color))
                    {
                        this.model.ProductDetail.colorList = this.model.ProductDetail.Color.Split(',').ToList();
                    }
                }
            }
            catch (Exception ex)
            {

            }

            return View("productDetailPage", this.model);
        }

        public async Task<ActionResult> ConfirmPassword(Dashboard detailModel)
        {
            this.model = new Dashboard();
            objRepository = new APIRepository();
            var result = new Response();

            var detail = (UserDetails)(Session["UserDetail"]);
            if (companyId == URSHOPECOMPANYID)
            {
                if (detailModel == null || detailModel.User == null)
                {
                    detailModel.User = new UserDetails();
                }
                detailModel.User = detail;
            }
            else
            {
                detail.passwordDetail = detailModel.User.password_str;
            }

            detail.Amount = detailModel.Amount;

            result = await this.objRepository.CheckUserExistance(detail);
            if (result == null)
            {
                return Json(Resources.ErrorMessage);
            }
            else if (!result.Status)
            {
                return Json("Not Found");
            }
            else if ((detail.company_id == GVENTERTAINMENTCOMPANYID || detail.company_id == SHOPENTERTAINMENTCOMPANYID) && string.IsNullOrEmpty(detail.WalletType))
            {
                return Json("Please select Wallet type");
            }
            else
            {
                detailModel.User.username = detail.username;
                detailModel.User.WalletType = detail.WalletType;
                result = await this.objRepository.CheckWalletBalance(detailModel.User);
                if (result.Status)
                {
                    var Wallet = JsonConvert.DeserializeObject<WalletDetails>(result.ResponseValue);

                    if (detail.Amount <= Wallet.wallet)
                    {
                        //send otp by default if have wallet balance.
                        if (companyId == ETRADECOMPANYID)
                        {
                           var res = await SaveDetailFormOtp(detailModel);
                            
                            return Json(res);
                        }
                        else
                        {
                            await GenerateOtpDetail();
                            return Json("Sufficient:" + Wallet.wallet);
                        }
                        
                    }
                    else
                    {
                        return Json("InSufficient:" + Wallet.wallet);
                    }
                }
                else
                {
                    return Json("No Wallet Balance Found");
                }
            }
        }

        public async Task<ActionResult> DeductWallet(Dashboard detailModel)
        {
            this.model = new Dashboard();
            objRepository = new APIRepository();
            var result = new Response();

            var detail = (UserDetails)(Session["UserDetail"]);
            detail.password_str = detailModel.User.password_str;

            var usr = new UserDetails();
            usr = detail;
            usr.password_str = usr.passwordDetail;
            usr.WalletType = detailModel.User.WalletType;

            result = await this.objRepository.CheckUserExistance(usr);
            if (result == null)
            {
                return Json(Resources.ErrorMessage);
            }
            else if (!result.Status)
            {
                return Json(result.ResponseValue);
            }
            else
            {
                result = await this.objRepository.CheckWalletBalance(usr);
                if (result.Status)
                {
                    var Wallet = JsonConvert.DeserializeObject<WalletDetails>(result.ResponseValue);

                    if (this.model.Amount <= Wallet.wallet)
                    {
                        return Json("Sufficient");
                    }
                    else
                    {
                        return Json("InSufficient");
                    }
                }
                else
                {
                    return Json("No Wallet Balance Found");
                }
            }
        }

        [HttpGet]
        public async Task<ActionResult> thankYouPage()
        {

            Dashboard data = new Dashboard();
            data.OrderDetail = new order();
            data.OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;

            if (data.OrderDetail.id != 0)
            {
                var result = await this.objRepository.GetUserOrder(data.OrderDetail);
                if (result != null)
                {
                    data.OrderDetail = result.OrderDetail;
                    data.OrderProducts = result.OrderProducts;
                }
            }
            return View(data);
        }

        [HttpPost]
        public async Task<ActionResult> GenerateOtpDetail()
        {
            this.model = new Dashboard();
            var result = new Response();
            var message = string.Empty;
            this.objRepository = new APIRepository();
            try
            {
                if (CheckLoginUserStatus())
                {
                    var detail = (UserDetails)(Session["UserDetail"]);
                    result = await this.objRepository.MangeOtpFunctions(detail, "GenerateOtp");
                    if (result == null)
                    {
                        message = "Something went wrong.Please try again later.";
                    }
                    else
                    {
                        message = result.ResponseValue;
                    }
                }
            }
            catch (Exception e)
            {
                message = e.Message;
            }

            return Json(message);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateShippingChargeDetail(int deliveryType)
        {
            this.model = new Dashboard();
            var message = string.Empty;
            this.objRepository = new APIRepository();
            string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
            try
            {
                if (CheckLoginUserStatus())
                {
                    var detail = (UserDetails)(Session["UserDetail"]);
                    var filter = new CartFilter();
                    filter.username = detail.username;
                    filter.password = detail.password_str;
                    filter.companyId = Convert.ToInt16(companyId);
                    filter.deliveryType = deliveryType;
                    filter.userId = detail.id;
                    var res = await objRepository.ManageCart(filter, "UpdateShippingCharge");

                    if (res == null)
                    {
                        message = "Something went wrong. Please try again later.";
                    }
                    else
                    {
                        message = res.ResponseValue;
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(message);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateProductQuantityDetail(int prodId, int quantity, int ?deliveryType)
        {
            this.model = new Dashboard();
            var message = string.Empty;
            this.objRepository = new APIRepository();
            string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
            try
            {
                if (CheckLoginUserStatus())
                {
                    var detail = (UserDetails)(Session["UserDetail"]);
                    var filter = new CartFilter();
                    filter.productId = prodId;
                    filter.username = detail.username;
                    filter.password = detail.password_str;
                    filter.userId = detail.id;
                    filter.companyId = Convert.ToInt16(companyId);
                    filter.quantity = quantity;
                    filter.deliveryType = deliveryType??0;
                    var res = await objRepository.ManageCart(filter, "UpdateQuantity");
                    if (res == null)
                    {
                        message = "Something went wrong. Please try again later.";
                    }
                    else
                    {
                        message = res.ResponseValue;
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(message);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteCartProduct(int prodId)
        {
            this.model = new Dashboard();
            var message = string.Empty;
            string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
            this.objRepository = new APIRepository();
            try
            {
                if (CheckLoginUserStatus())
                {
                    var detail = (UserDetails)(Session["UserDetail"]);
                    var filter = new CartFilter();
                    filter.productId = prodId;
                    filter.userId = detail.id;
                    filter.username = detail.username;
                    filter.password = detail.password_str;
                    filter.companyId = Convert.ToInt16(companyId);
                    var res = await objRepository.ManageCart(filter, "Remove");
                    if (res == null)
                    {
                        message = "Something ent wrong. Please try again later.";
                    }
                    else
                    {
                        message = res.ResponseValue;
                    }
                }
                else
                {
                    return RedirectToAction("Login", "Account");
                }
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            return Json(message);
        }

        [HttpGet]
        public async Task<ActionResult> GetCartProductList(bool isWithPayment, int deliveryType)

        {
            this.model = new Dashboard();
            this.objRepository = new APIRepository();

            if (CheckLoginUserStatus())
            {
                try
                {
                    string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
                    this.model.deliveryTypeList = await objRepository.DeliveryTypeList();
                    var cart = new CartFilter();
                    var detail = (UserDetails)(Session["UserDetail"]);
                    cart.username = detail.username;
                    cart.password = detail.password_str;
                    cart.userId = detail.id;
                    cart.companyId = Convert.ToInt16(companyId);
                    cart.deliveryType = deliveryType;

                    var res = await objRepository.ManageCart(cart, "UpdateShippingCharge");
                    var response = await objRepository.ManageCart(cart, CartProductListAction);
                    if (response != null && response.Status)
                    {
                        this.model.Products = JsonConvert.DeserializeObject<List<Product>>(response.ResponseValue);
                        this.model.UsersPoints = response.Points;
                        this.model.User = new UserDetails();
                        //this.model.User.username = detail.username;
                        this.model.User = detail;
                        //this.model.AssignPaymentModes();
                        //call here 
                        var resp = await objRepository.CreateOrder(new order(), "ListPaymentModes");
                        if (resp.Status == true)
                        {
                            this.model.PaymentModeList = JsonConvert.DeserializeObject<List<Containers>>(resp.ResponseValue);
                            var waletMode = this.model.PaymentModeList.FirstOrDefault(p => p.value.Contains("Wallet"));
                            if (waletMode != null)
                            {
                                this.model.PaymentModeList.Remove(waletMode);
                            }
                            this.model.WalletPaymentModeList = this.model.PaymentModeList.Where(p => p.value.Contains("PhonePe") || p.value.Contains("PayTm")).ToList();
                            this.model.PaymentModeList.RemoveAll(p => this.model.WalletPaymentModeList.Contains(p));
                        }
                        if (this.model.Products != null)
                        {
                            this.model.NetPayment = 0;
                            var prodPrice = 0.0;
                            foreach (var prod in this.model.Products)
                            {
                                if (prod.offer_price == null || prod.offer_price == "0")
                                {
                                    prodPrice = Convert.ToDouble(prod.market_price);
                                }
                                else
                                {
                                    prodPrice = Convert.ToDouble(prod.offer_price);
                                }

                                prod.shippng_charge = (prod.vendor_qty ?? 1) * (prod.shippng_charge ?? 0);
                                //prod.TotalPayment = prodPrice * (prod.vendor_qty ?? 1) + (prod.shippng_charge ?? 0);
                                //this.model.TotalProductPoints += (prod.RBV ?? 0) * (prod.vendor_qty ?? 1);
                                //this.model.NetPayment += prod.TotalPayment;

                                this.model.NetPayment += prod.amount;
                            }
                            this.model.OrderDetail.payment_ref_amount = Convert.ToString(this.model.NetPayment);
                        }
                    }
                }
                catch (Exception ex)
                {

                }
            }
            else
            {
                return RedirectToAction("Login", "Account");
            }


            if (this.model == null)
            {
                this.model = new Dashboard();
            }

            if (this.model.OrderDetail == null)
            {
                this.model.OrderDetail = new order();
                this.model.OrderDetail.payment_ref_amount = Convert.ToString(this.model.NetPayment);
            }

            if (deliveryType == 0)
            {
                this.model.OrderDetail.delievryType = HOMEDELIVERYCODE;
            }
            else
            {
                this.model.OrderDetail.delievryType = deliveryType;
            }

            if (isWithPayment)
            {
                var CompanyId = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["CompanyId"]);
                var profile = new CompanyProfile { CompanyId = CompanyId };
                //get companyProfileDetail
                var result = await objRepository.ManageCompanyProfile(profile, "ByCompanyId");
                if (!(result == null || result.Status == false))
                {
                    this.model.CompanyProfileDetail = JsonConvert.DeserializeObject<CompanyProfile>(result.ResponseValue);
                }

                if (this.model.User == null || this.model.User.phone == null)
                {
                    this.model.User = new UserDetails();
                    this.model.User.otpPhone = "Please enter mobile no.";
                }
                else if (model.User.phone.Length == 10)
                {
                    this.model.User.otpPhone = this.model.User.phone;
                    var sub = this.model.User.otpPhone.Substring(3, 3);
                    this.model.User.otpPhone = this.model.User.otpPhone.Replace(sub, "XXX");
                }

                this.model.couponList = await this.objRepository.GetCouponList(this.model.User.id);
                this.model.User.WalletType = "M";
                if (Theme == Resources.Orange)
                {
                    return PartialView("cartPaymentDetailViewOrange", this.model);
                }
                else
                {
                    return PartialView("cartPaymentDetailView", this.model);
                }
            }
            else
            {
                if (Theme == Resources.Orange)
                {
                    return PartialView("cartDetailViewOrange", this.model);
                }
                else
                {
                    return PartialView("cartDetailView", this.model);
                }
            }
        }

        public async Task<ActionResult> AddProductInToCart(int ProductId, int Quantity, string size, string color)
        {
            this.model = new Dashboard();
            this.objRepository = new APIRepository();
            string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
            try
            {
                if (CheckLoginUserStatus())
                {
                    var cart = new CartFilter();
                    cart.productId = ProductId;
                    cart.quantity = Quantity;
                    cart.companyId = Convert.ToInt16(companyId);

                    var detail = (UserDetails)(Session["UserDetail"]);
                    cart.username = detail.username;
                    cart.password = detail.password_str;
                    cart.userId = Convert.ToInt16(detail.id);
                    cart.Size = size;
                    cart.Color = color;

                    var response = await objRepository.ManageCart(cart, AddAction);
                    return Json(response);
                }
                else
                {
                    Response response = new Models.Response();
                    response.Status = false;
                    response.ResponseValue = "login";
                    return Json(response);
                }
            }
            catch (Exception ex)
            {

            }

            return PartialView("productDetailPage", this.model);
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var model = new IndexViewModel
            {
                HasPassword = HasPassword(),
                PhoneNumber = await UserManager.GetPhoneNumberAsync(userId),
                TwoFactor = await UserManager.GetTwoFactorEnabledAsync(userId),
                Logins = await UserManager.GetLoginsAsync(userId),
                BrowserRemembered = await AuthenticationManager.TwoFactorBrowserRememberedAsync(userId)
            };
            return View(model);
        }

        //
        // POST: /Manage/RemoveLogin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await UserManager.RemoveLoginAsync(User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }
            return RedirectToAction("ManageLogins", new { Message = message });
        }

        //
        // GET: /Manage/AddPhoneNumber
        public ActionResult AddPhoneNumber()
        {
            return View();
        }

        //
        // POST: /Manage/AddPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AddPhoneNumber(AddPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            // Generate the token and send it
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), model.Number);
            if (UserManager.SmsService != null)
            {
                var message = new IdentityMessage
                {
                    Destination = model.Number,
                    Body = "Your security code is: " + code
                };
                await UserManager.SmsService.SendAsync(message);
            }
            return RedirectToAction("VerifyPhoneNumber", new { PhoneNumber = model.Number });
        }

        //
        // POST: /Manage/EnableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EnableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), true);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // POST: /Manage/DisableTwoFactorAuthentication
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DisableTwoFactorAuthentication()
        {
            await UserManager.SetTwoFactorEnabledAsync(User.Identity.GetUserId(), false);
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", "Manage");
        }

        //
        // GET: /Manage/VerifyPhoneNumber
        public async Task<ActionResult> VerifyPhoneNumber(string phoneNumber)
        {
            var code = await UserManager.GenerateChangePhoneNumberTokenAsync(User.Identity.GetUserId(), phoneNumber);
            // Send an SMS through the SMS provider to verify the phone number
            return phoneNumber == null ? View("Error") : View(new VerifyPhoneNumberViewModel { PhoneNumber = phoneNumber });
        }

        //
        // POST: /Manage/VerifyPhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> VerifyPhoneNumber(VerifyPhoneNumberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePhoneNumberAsync(User.Identity.GetUserId(), model.PhoneNumber, model.Code);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.AddPhoneSuccess });
            }
            // If we got this far, something failed, redisplay form
            ModelState.AddModelError("", "Failed to verify phone");
            return View(model);
        }

        //
        // POST: /Manage/RemovePhoneNumber
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemovePhoneNumber()
        {
            var result = await UserManager.SetPhoneNumberAsync(User.Identity.GetUserId(), null);
            if (!result.Succeeded)
            {
                return RedirectToAction("Index", new { Message = ManageMessageId.Error });
            }
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user != null)
            {
                await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
            }
            return RedirectToAction("Index", new { Message = ManageMessageId.RemovePhoneSuccess });
        }

        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Manage/SetPassword
        public ActionResult SetPassword()
        {
            return View();
        }

        //
        // POST: /Manage/SetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Manage/ManageLogins
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : "";
            var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
            if (user == null)
            {
                return View("Error");
            }
            var userLogins = await UserManager.GetLoginsAsync(User.Identity.GetUserId());
            var otherLogins = AuthenticationManager.GetExternalAuthenticationTypes().Where(auth => userLogins.All(ul => auth.AuthenticationType != ul.LoginProvider)).ToList();
            ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }



        //
        // GET: /Manage/LinkLoginCallback
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }
            var result = await UserManager.AddLoginAsync(User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? RedirectToAction("ManageLogins") : RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

        public async Task<ActionResult> GetPaymentWindow(double netPayment)
        {
            try
            {
                var userDetail = Session["UserDetail"] as UserDetails;
                if (userDetail == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var cntrl = new PayUController();
                    var payrequest = new PayuRequest
                    {
                        FirstName = userDetail.first_name,
                        TransactionAmount = netPayment,
                        Email = userDetail.email,
                        Phone = userDetail.phone,
                        udf1 = string.Empty,
                        udf2 = Session["OrderId"] != null ? Convert.ToString((Session["OrderId"])) : "0",
                        memberId = userDetail.username,
                        ProductInfo = "order products",
                        surl = "http://" + Request.Url.Authority + "/Manage/Return",
                        furl = "http://" + Request.Url.Authority + "/Manage/Return"
                    };
                    cntrl.Payment(payrequest);
                }
            }
            catch (Exception e)
            {

            }

            return null;
        }

        [HttpPost]
        public async Task<ActionResult> Return(FormCollection form)
        {
            var OrderDetail = new order();
            OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;
            OrderDetail.user_id = Session["UserDetail"] != null ? (Session["UserDetail"] as UserDetails).id : 0;

            var resp = await objRepository.CreateOrder(new order(), "ListPaymentModes");
            if (resp.Status == true)
            {
                this.model.PaymentModeList = JsonConvert.DeserializeObject<List<Containers>>(resp.ResponseValue);
                var waletMode = this.model.PaymentModeList.FirstOrDefault(p => p.value.Contains("PayUMoney"));
                if (waletMode != null)
                {
                    OrderDetail.payment_mode = Convert.ToString(waletMode.Id);
                }
            }
            OrderDetail.dt_payment_ref_date = DateTime.Now;
            if (form["status"] == "success")
            {
                OrderDetail.payment_ref_no = Convert.ToString(form["txnid"]);

                OrderDetail.payment_ref_bank = form["bank_ref_num"];
                OrderDetail.narration = "PG_REQUEST," + form["udf2"] + "," + Convert.ToString(form["mode"]) + "," + Convert.ToString(form["txnid"]) + "," + Convert.ToString(form["bank_ref_num"]);
                //                payment_ref_amount = amo
                //TotalAmount
            }
            else
            {
                OrderDetail.narration = "PG_REQUEST, " + form["udf2"] + "  " + form["status"];
            }

            var result = await objRepository.CreateOrder(OrderDetail, "Edit");
            if (form["status"] == "success")
            {
                return RedirectToAction("thankYouPage", "Manage");
            }
            else
            {

            }

            return null;
        }

        #region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }

        public enum ManageMessageId
        {
            AddPhoneSuccess,
            ChangePasswordSuccess,
            SetTwoFactorSuccess,
            SetPasswordSuccess,
            RemoveLoginSuccess,
            RemovePhoneSuccess,
            Error
        }

        #endregion
    }
}