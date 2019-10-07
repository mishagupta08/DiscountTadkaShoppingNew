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
        private const string CartProductListAction = "CartProductList";

        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        APIRepository objRepository = new APIRepository();
        Dashboard model = new Dashboard();

        public ManageController()
        {
        }

        public async Task<ActionResult> SaveDetailFormOtp(Dashboard detailModel)
        {
            this.model = new Dashboard();
            objRepository = new APIRepository();
            var result = new Response();

            var detail = (UserDetails)(Session["UserDetail"]);
            detail.password_str = detailModel.User.password_str;
            detail.OtpCode = detailModel.User.OtpCode;
            result = await this.objRepository.MangeOtpFunctions(detail, "ValidateOtp");
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
                detailModel.OrderDetail = new order();
                detailModel.OrderDetail.id = Session["OrderId"] != null ? Convert.ToInt32(Session["OrderId"]) : 0;
                result = await objRepository.CreateOrder(detailModel.OrderDetail, "EditWithOtp");
            }

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

            result = await objRepository.CreateOrder(detailModel.OrderDetail, "Edit");

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

        [HttpGet]
        public async Task<ActionResult> thankYouPage()
        {
            return View();
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
        public async Task<ActionResult> UpdateProductQuantityDetail(int prodId, int quantity)
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
        public async Task<ActionResult> GetCartProductList(bool isWithPayment)
        {
            this.model = new Dashboard();
            this.objRepository = new APIRepository();
            if (CheckLoginUserStatus())
            {
                try
                {

                    var cart = new CartFilter();
                    var detail = (UserDetails)(Session["UserDetail"]);
                    cart.username = detail.username;
                    cart.password = detail.password_str;
                    cart.userId = detail.id;
                    var response = await objRepository.ManageCart(cart, CartProductListAction);
                    if (response != null && response.Status)
                    {
                        this.model.Products = JsonConvert.DeserializeObject<List<Product>>(response.ResponseValue);
                        this.model.UsersPoints = response.Points;
                        this.model.User = new UserDetails();
                        this.model.User.username = detail.username;
                        this.model.AssignPaymentModes();
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

            if (isWithPayment)
            {
                if (this.model == null)
                {
                    this.model = new Dashboard();
                }

                return PartialView("cartPaymentDetailView", this.model);
            }
            else
            {
                return PartialView("cartDetailView", this.model);
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