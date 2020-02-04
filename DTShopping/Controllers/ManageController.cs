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

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        APIRepository objRepository = new APIRepository();
        Dashboard model = new Dashboard();

        public ManageController()
        {
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
                return RedirectToAction("Index", "Login");
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
                return RedirectToAction("Index", "Login");
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
                if (CompanyId == 17)
                {
                    detailModel.OrderDetail = new order();
                    detailModel.OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;
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
                else if (CompanyId == 28 || CompanyId == 29)
                {
                    var deductWallet = await objRepository.DeductWalletBalnce(detail);
                    if (deductWallet.Status)
                    {
                        var Wallet = JsonConvert.DeserializeObject<WalletDeduction>(deductWallet.ResponseValue);
                        if (Wallet.response.ToLower() == "ok")
                        {
                            detail.Voucher = Wallet.voucherno;
                            var Walletconfirm = await objRepository.WalletConfirmationAPI(detail);
                            if (Walletconfirm.Status)
                            {
                                var ConfirmWallet = JsonConvert.DeserializeObject<WalletDetails>(Walletconfirm.ResponseValue);
                                if (ConfirmWallet.response.ToLower() == "ok")
                                {
                                    detailModel.OrderDetail = new order();
                                    detailModel.OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;
                                    detailModel.OrderDetail.payment_ref_no = Wallet.voucherno;
                                    detailModel.OrderDetail.payment_ref_amount = detailModel.Amount.ToString();
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
            this.model = new Dashboard();
            objRepository = new APIRepository();
            var result = new Response();

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
            detail.password_str = detailModel.User.password_str;
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
            else
            {
                result = await this.objRepository.CheckWalletBalance(detail);
                if (result.Status)
                {
                    var Wallet = JsonConvert.DeserializeObject<WalletDetails>(result.ResponseValue);

                    if (detail.Amount <= Wallet.wallet)
                    {
                        return Json("Sufficient:" + Wallet.wallet);
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

            result = await this.objRepository.CheckUserExistance(detail);
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
                result = await this.objRepository.CheckWalletBalance(detail);
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
        public async Task<ActionResult> UpdateProductQuantityDetail(int prodId, int quantity, int deliveryType)
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
                    filter.deliveryType = deliveryType;
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
                        this.model.User.username = detail.username;
                        //this.model.AssignPaymentModes();
                        //call here 
                        var resp = await objRepository.CreateOrder(new order(), "ListPaymentModes");
                        if (resp.Status == true)
                        {
                            this.model.PaymentModeList = JsonConvert.DeserializeObject<List<Containers>>(resp.ResponseValue);
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
                return PartialView("cartPaymentDetailView", this.model);
            }
            else
            {
                return PartialView("cartDetailView", this.model);
            }
        }

        public async Task<ActionResult> AddProductInToCart(int ProductId, int Quantity)
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