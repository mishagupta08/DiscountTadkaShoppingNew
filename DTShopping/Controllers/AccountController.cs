using System.Threading.Tasks;
using System.Web.Mvc;
using DTShopping.Models;
using DTShopping.Repository;
using System.Collections.Generic;
using System;
using System.Web.Security;
using Newtonsoft.Json;

namespace DTShopping
{
    //[Authorize]
    public class AccountController : Controller
    {
        private APIRepository _APIManager;

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
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        //
        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult> Login(UserDetails User)
        {
            try
            {
                var companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
                if(!string.IsNullOrEmpty(companyId))
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

        // GET: /Account/Register
        [AllowAnonymous]
        public async Task<ActionResult> Register()
        {
            this.model = new Dashboard();
            this._APIManager = new APIRepository();
            await this.AssignStateCityList();
            return View(this.model);
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

        


    }
}