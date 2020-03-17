using System.Web.Mvc;
using DTShopping.Repository;
using DTShopping.Properties;
using System;
using System.Collections.Generic;
using DTShopping.Models;
using System.Threading.Tasks;
using System.Linq;
using PagedList;
using System.Threading;
using Newtonsoft.Json;

namespace DTShopping.Controllers
{

    public class HomeController : Controller
    {
        APIRepository objRepository = new APIRepository();
        Dashboard model = new Dashboard();
        private const int SUNVISCOMPANYID = 29;
        string Theme = System.Configuration.ConfigurationManager.AppSettings["Theme"] == null ? string.Empty : System.Configuration.ConfigurationManager.AppSettings["Theme"].ToString();
        string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];

        public async Task<ActionResult> Index()
        {
            Dashboard objDashboardDetails = new Dashboard();

            try
            {
                var res = await this.SetCompanyDetailInSession();
                if (res != null)
                {
                    return res;
                }
                //var dt = new List<company>();
                //dt.Add(new company
                //{
                //    id = Convert.ToInt32(companyId)
                //});

                //var companyDetail = await objRepository.GetCompanyById(dt);
                //if (companyDetail == null || companyDetail.default_flag == 0)
                //{
                //    return View("Error");
                //}
                //else
                {
                    //Session["Company"] = companyDetail;
                    objDashboardDetails.Banners = new List<Banners>();
                    objDashboardDetails.Banners = await objRepository.GetBannerImageList(companyId);

                    objDashboardDetails.FontpageSections = new ShoppingPortalFrontPageProdList();
                    objDashboardDetails.FontpageSections = await objRepository.GetShoppingPortalFrontPageProdList(companyId);

                    Session["LatestProduct"] = objDashboardDetails.FontpageSections.SpeacialSegment;
                    Session["Brands"] = objDashboardDetails.FontpageSections.brandlist;

                }
            }
            catch (Exception ex)
            {

            }

            if (Theme == Resources.Orange)
            {
                return View("IndexOrange", objDashboardDetails);
            }
            else
            {
                return View("Index", objDashboardDetails);
            }
        }

        public async Task<ActionResult> TermsAndConditions()
        {
            var res = await this.SetCompanyDetailInSession();
            if (res != null)
            {
                return res;
            }

            return View();
        }

        public async Task<ActionResult> PrivacyPolicy()
        {
            var res = await this.SetCompanyDetailInSession();
            if (res != null)
            {
                return res;
            }

            return View();
        }

        public async Task<ActionResult> About()
        {
            ViewBag.Message = "Your application description page.";
            var res = await this.SetCompanyDetailInSession();
            if (res != null)
            {
                return res;
            }
            return View();
        }

        public async Task<ActionResult> Contact()
        {
            var dt = new List<company>();
            string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];
            dt.Add(new company
            {
                id = Convert.ToInt32(companyId)
            });

            var companyDetail = await objRepository.GetCompanyById(dt);
            if (companyDetail == null || companyDetail.default_flag == 0)
            {
                return View("Error");
            }
            this.model = new Dashboard();
            this.model.CompanyDetail = companyDetail;
            return View(this.model);
        }

        public async Task<ActionResult> GetProductDetail(int prodId)
        {
            if (CheckLoginUserStatus())
            {
                var dashboard = new Dashboard();
                this.objRepository = new APIRepository();
                try
                {
                    var prodList = new List<Product>();

                    prodList.Add(new Product { id = prodId });
                    if (Theme == Resources.Orange)
                    {
                        var container = await objRepository.GetProductDetailByIdForOrangeTheme(prodList);
                        if (container != null)
                        {
                            dashboard.ProductDetail = container.ProductDetail;
                            dashboard.RelatedProductList = container.RelatedProductList;
                            dashboard.SameBrandProductList = container.SameBrandProductList;
                        }
                    }
                    else
                    {
                        dashboard.ProductDetail = await objRepository.GetProductDetailById(prodList);
                    }

                    if (dashboard.ProductDetail != null)
                    {
                        if (!string.IsNullOrEmpty(dashboard.ProductDetail.description_detail))
                        {
                            dashboard.ProductDetail.description_detail = dashboard.ProductDetail.description_detail.Replace("\r\n\r\n", "");
                        }
                        if (!string.IsNullOrEmpty(dashboard.ProductDetail.product_size))
                        {
                            dashboard.ProductDetail.sizeList = dashboard.ProductDetail.product_size.Split(',').ToList();
                        }
                        if (!string.IsNullOrEmpty(dashboard.ProductDetail.Color))
                        {
                            dashboard.ProductDetail.colorList = dashboard.ProductDetail.Color.Split(',').ToList();
                        }
                    }
                }
                catch (Exception ex)
                {

                }

                if (Theme == Resources.Orange)
                {
                    return View("productDetailPageOrange", dashboard);
                }
                else
                {
                    return View("productDetailPage", dashboard);
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }

        }

        public async Task<ActionResult> ProductList(string cat, string BrandId, string root, int? page, string SortBy, string Order, string FilterFromPoint, string FilterToPoint, string searchString, string pointsFilterList)
        {
            this.model = new Dashboard();
            Filters c = new Filters();
            if (!string.IsNullOrEmpty(cat))
            {
                c.CategoryId = Convert.ToInt16(cat);
            }
            if (!string.IsNullOrEmpty(BrandId))
            {
                c.BrandId = Convert.ToInt16(BrandId);
            }
            c.pageNo = page ?? 1;
            c.NoOfRecord = 10;
            c.SelectedFilterName = "Title";
            c.FilterValue = searchString;
            if (!string.IsNullOrEmpty(SortBy))
            {
                if (SortBy.ToLower() == "price")
                {
                    c.SortByPrice = true;
                    if (Order.ToLower() == "asc")
                    {
                        c.IsPriceLowToHigh = true;
                    }
                    else
                    {
                        c.IsPriceLowToHigh = false;
                    }
                }
                else if (SortBy.ToLower() == "points")
                {
                    c.SortByPoints = true;
                    if (Order.ToLower() == "asc")
                    {
                        c.IsPointLowToHigh = true;
                    }
                    else
                    {
                        c.IsPointLowToHigh = false;
                    }
                }
            }
            if (!string.IsNullOrEmpty(FilterFromPoint) && !string.IsNullOrEmpty(FilterToPoint))
            {
                c.FilterFromPoint = Convert.ToInt32(FilterFromPoint);
                c.FilterToPoint = Convert.ToInt32(FilterToPoint);
            }

            if(!string.IsNullOrEmpty(pointsFilterList))
            {
                var pintList = pointsFilterList.Split(',').ToList().ConvertAll(int.Parse);
                if (pintList.Count() >= 2)
                {
                    c.FilterFromPoint = Convert.ToInt32(pintList[0]);
                    c.FilterToPoint = Convert.ToInt32(pintList[pintList.Count()-1]);
                }
            }

            var result = await objRepository.GetCategoryProducts(c);
            List<Product> listProducts = new List<Product>();
            double totalcount = 0;
            if (result.Status == true && result.ResponseValue != null)
            {
                totalcount = result.TotalRecords;
                listProducts = JsonConvert.DeserializeObject<List<Product>>(result.ResponseValue);
            }

            string catName = string.Empty;
            if (!string.IsNullOrEmpty(cat))
            {
                var catDetail = await objRepository.GetCategoryDetail(c);
                catName = catDetail.title;
            }
            //if (!string.IsNullOrEmpty(BrandId))
            //{
            //    var brandDetail = await objRepository.GetBrandDetail(c);
            //    brandDetail = catDetail.title;
            //}

            PagewiseProducts finalprodlist = new PagewiseProducts();
            finalprodlist.ProductList = listProducts;

            var list = new List<int>();
            for (var i = 1; i <= totalcount; i++)
            {
                list.Add(i);
            }
            finalprodlist.pagerCount = list.ToPagedList(Convert.ToInt32(c.pageNo), 10);


            ViewBag.category = cat;
            ViewBag.CategoryName = catName;
            ViewBag.Page = page;
            ViewBag.ParentId = root;
            ViewBag.brand = BrandId;
            this.model.finalProductList = finalprodlist;
            await SetCompanyDetailInSession();
            if (Theme == Resources.Orange)
            {
                //this.model.FontpageSections = new ShoppingPortalFrontPageProdList();
                //this.model.FontpageSections = await objRepository.GetShoppingPortalFrontPageProdList(companyId);

                //if (finalprodlist != null && finalprodlist.ProductList != null && finalprodlist.ProductList.Count > 0)
                //{
                //    var pId = finalprodlist.ProductList.FirstOrDefault().id;
                //    var prodList = new List<Product>();
                //    prodList.Add(new Product { id = pId });


                //    var container = await objRepository.GetProductDetailByIdForOrangeTheme(prodList);
                //    if (container != null)
                //    {
                //        this.model.RelatedProductList = container.RelatedProductList;
                //    }

                //}

                return View("ProductListOrange", this.model);
            }
            else
            {
                return View(this.model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> GetCategories()
        {
            List<Category> list = new List<Category>();
            try
            {
                //if (Session["MenuList"] != null)
                //{

                //}
                //else
                //{
                var MenuItems = await objRepository.GetMenuList();
                if (MenuItems != null)
                {
                    MenuItems = MenuItems.Where(r => r.status == true).OrderBy(r => r.ordar).ToList();
                    list = getNestedChildren(MenuItems.Where(r => r.status == true && r.parent_id == 1 && r.parent_id != r.id).ToList(), MenuItems.Where(r => r.status == true).ToList());
                    Session["MenuList"] = list;
                }
                //}                                                      
            }
            catch (Exception ex)
            {

            }

            if (Theme == Resources.Orange)
            {
                return PartialView("CategoryOrange", list);
            }
            else
            {
                return PartialView("Category", list);
            }
        }

        public ActionResult getCatHeirarchy(string Cat, string subCat, string brandId)
        {
            SideBar objsidebar = new SideBar();
            var CategoryList = new List<Category>();
            int categor = 0;
            if (!string.IsNullOrEmpty(Cat))
            {
                categor = Convert.ToInt16(Cat);
            }
            if (Session["MenuList"] != null && categor != 0)
            {
                var MenuItems = Session["MenuList"] as List<Category>;
                CategoryList = getNestedChildren(MenuItems.Where(r => r.status == true && r.id == categor).ToList(), MenuItems);
            }
            ViewBag.ParentId = Cat;
            ViewBag.category = subCat;
            ViewBag.brand = brandId;
            ViewBag.Page = 1;
            objsidebar.categoryList = CategoryList;
            if (Session["LatestProduct"] != null)
            {
                var product = Session["LatestProduct"] as List<Product>;
                objsidebar.latestProduct = product.FirstOrDefault();
            }

            if (Theme == Resources.Orange)
            {
                return PartialView("_filterSideBarOrange", objsidebar);
            }
            else
            {
                return PartialView("_filterSideBar", objsidebar);
            }
        }

        public List<Category> getNestedChildren(List<Category> ParentList, List<Category> MenuList)
        {
            var orderedList = new List<Category>();
            if (ParentList.Count > 0)
            {
                foreach (var parent in ParentList)
                {
                    var submenu = MenuList.Where(r => r.parent_id == parent.id).ToList();
                    if (submenu.Count > 0 && !(parent.id == parent.parent_id))
                    {
                        parent.Childern = new List<Category>();
                        parent.Childern = (getNestedChildren(submenu, MenuList));
                    }
                    orderedList.Add(parent);
                }

            }
            return orderedList;
        }

        public ActionResult Claims()
        {
            return View();
        }

        public async Task<ActionResult> SearchProductList(int? page, string SortBy, string Order, string searchString)
        {
            this.model = new Dashboard();
            Filters c = new Filters();

            c.pageNo = page;
            c.NoOfRecord = 10;
            c.SelectedFilterName = "Title";
            c.FilterValue = searchString;
            if (!string.IsNullOrEmpty(SortBy))
            {
                if (SortBy.ToLower() == "price")
                {
                    c.SortByPrice = true;
                    if (Order.ToLower() == "asc")
                    {
                        c.IsPriceLowToHigh = true;
                    }
                    else
                    {
                        c.IsPriceLowToHigh = false;
                    }
                }
                else if (SortBy.ToLower() == "points")
                {
                    c.SortByPoints = true;
                    if (Order.ToLower() == "asc")
                    {
                        c.IsPointLowToHigh = true;
                    }
                    else
                    {
                        c.IsPointLowToHigh = false;
                    }
                }
            }

            var result = await objRepository.GetCategoryProducts(c);
            List<Product> listProducts = new List<Product>();
            double totalcount = 0;
            if (result.Status == true && result.ResponseValue != null)
            {
                totalcount = result.TotalRecords;
                listProducts = JsonConvert.DeserializeObject<List<Product>>(result.ResponseValue);
            }

            PagewiseProducts finalprodlist = new PagewiseProducts();
            finalprodlist.SearchString = searchString;
            finalprodlist.ProductList = listProducts;

            var list = new List<int>();
            for (var i = 1; i <= totalcount; i++)
            {
                list.Add(i);
            }
            finalprodlist.pagerCount = list.ToPagedList(Convert.ToInt32(page), 10);
            ViewBag.Page = page;
            this.model.finalProductList = finalprodlist;
            if (Theme == Resources.Orange)
            {
                this.model.FontpageSections = new ShoppingPortalFrontPageProdList();
                this.model.FontpageSections = await objRepository.GetShoppingPortalFrontPageProdList(companyId);

                if (finalprodlist != null && finalprodlist.ProductList != null && finalprodlist.ProductList.Count > 0)
                {
                    var pId = finalprodlist.ProductList.FirstOrDefault().id;
                    var prodList = new List<Product>();
                    prodList.Add(new Product { id = pId });


                    var container = await objRepository.GetProductDetailByIdForOrangeTheme(prodList);
                    if (container != null)
                    {
                        this.model.RelatedProductList = container.RelatedProductList;
                    }

                }

                return View("SearchProductListOrange", this.model);
            }
            else
            {
                return View(this.model);
            }
        }

        public async Task<ActionResult> GetAllDealProducts(string Deal, int? page, string SortBy, string Order, int? catId)
        {
            PagewiseProducts productlist = new PagewiseProducts();
            try
            {
                Filters c = new Filters();
                string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];

                c.CompanyId = Convert.ToInt16(companyId);
                c.pageNo = page;
                c.NoOfRecord = 10;
                productlist.SearchString = Deal;
                productlist.sortby = SortBy;
                productlist.order = Order;
                c.CategoryId = catId;

                if (!string.IsNullOrEmpty(SortBy))
                {
                    if (SortBy.ToLower() == "price")
                    {
                        c.SortByPrice = true;
                        if (Order.ToLower() == "asc")
                        {
                            c.IsPriceLowToHigh = true;
                        }
                        else
                        {
                            c.IsPriceLowToHigh = false;
                        }
                    }
                    else if (SortBy.ToLower() == "points")
                    {
                        c.SortByPoints = true;
                        if (Order.ToLower() == "asc")
                        {
                            c.IsPointLowToHigh = true;
                        }
                        else
                        {
                            c.IsPointLowToHigh = false;
                        }
                    }
                }

                var result = await objRepository.GetDealProductsFullList(c, Deal);
                List<Product> listProducts = new List<Product>();
                double totalcount = 0;
                if (result.Status == true && result.ResponseValue != null)
                {
                    totalcount = result.TotalRecords;
                    listProducts = JsonConvert.DeserializeObject<List<Product>>(result.ResponseValue);
                }

                productlist.ProductList = listProducts;

                var list = new List<int>();
                for (var i = 1; i <= totalcount; i++)
                {
                    list.Add(i);
                }

                productlist.pagerCount = list.ToPagedList(Convert.ToInt32(page), 10);
                ViewBag.Page = page;
            }
            catch (Exception ex)
            {
            }
            return View("DealProducts", productlist);

        }

        public ActionResult MyAccount()
        {
            return View();
        }

        public ActionResult UpdateAccount()
        {
            var userDetail = Session["UserDetail"] as UserDetails;
            return View(userDetail);
        }

        public ActionResult DiscountCoupons()
        {
            return View();
        }

        public async Task<ActionResult> Orders(int? page)
        {
            if (Session["UserDetail"] == null)
            {
                return RedirectToAction("Login", "Account");
            }
            else
            {
                var userID = 0;
                var companyID = 0;
                Filters objFilter = new Filters();
                PagedOrderList UserOrderList = new PagedOrderList();
                if (Session["UserDetail"] != null)
                {
                    userID = (Session["UserDetail"] as UserDetails).id;
                    companyID = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["CompanyId"]);

                    objFilter.CompanyId = companyID;
                    objFilter.VendorId = userID;
                    objFilter.pageNo = page;
                    objFilter.pageName = "VendorOrderProductList";
                    objFilter.SelectedFilterName = "userOrderList";
                    var result = await objRepository.GetUserOrderList(objFilter);
                    double totalcount = 0;
                    if (result.Status == true && result.ResponseValue != null)
                    {
                        totalcount = result.TotalRecords;
                        UserOrderList.OrderProductList = JsonConvert.DeserializeObject<List<order_products>>(result.ResponseValue);
                    }

                    var list = new List<int>();
                    for (var i = 1; i <= totalcount; i++)
                    {
                        list.Add(i);
                    }
                    UserOrderList.pagerCount = list.ToPagedList(Convert.ToInt32(page ?? 1), 10);

                }
                return View(UserOrderList);
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

        public async Task<ActionResult> Checkout(int deliveryType)
        {
            order objUserOrder = new order();
            try
            {
                if (Session["UserDetail"] == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    var userDetail = Session["UserDetail"] as UserDetails;
                    objUserOrder.user_id = userDetail.id;
                    var result = await objRepository.GetUserOpenOrder(objUserOrder);
                    if (result != null)
                    {
                        objUserOrder = result;
                    }
                    else
                    {
                        objUserOrder.billing_city = userDetail.CityName;
                        objUserOrder.billing_state = userDetail.StateName;
                        objUserOrder.billing_phone = userDetail.phone;
                        objUserOrder.user_id = userDetail.id;
                        objUserOrder.billing_first_name = userDetail.first_name;
                        objUserOrder.billing_last_name = userDetail.last_name;
                    }

                    this.model = new Dashboard();
                    this.model.OrderDetail = objUserOrder;
                    this.model.OrderDetail.delievryType = (deliveryType);
                    //this.model.deliveryTypeList = await objRepository.DeliveryTypeList();
                    await AssignStateCityList(objUserOrder.billing_state);
                }
            }
            catch (Exception ex)
            {

            }

            if (Theme == Resources.Orange)
            {
                var manage = new ManageController();
                await manage.SetShoppingCartProductInModel(this.model);
                return View("CheckoutOrange", this.model);
            }
            else
            {
                return View(this.model);
            }
        }

        [HttpGet]
        public async Task<ActionResult> getCartCount()
        {
            UserDetails userDetail = new UserDetails();
            Response userResponse = new Response();
            try
            {
                if (Session["UserDetail"] == null)
                {
                    userResponse.Points = 0;
                    userResponse.CartProductCount = 0;
                }
                else
                {
                    userDetail = Session["UserDetail"] as UserDetails;
                    var result = await objRepository.getCartCount(userDetail);

                    if (result.Status == true)
                    {
                        userResponse = result;
                        Session["Points"] = result.Points;
                        Session["CartNumber"] = result.CartProductCount;
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return Json(userResponse, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> CreateOrder(Dashboard obj)
        {
            var orderstatus = string.Empty;
            order objorder = obj.OrderDetail;
            try
            {
                objorder.created = DateTime.Now;
                //objorder.status = 2;
                objorder.company_id = Convert.ToInt16(System.Configuration.ConfigurationManager.AppSettings["CompanyId"]);
                if (objorder.company_id == SUNVISCOMPANYID)
                {
                    UserDetails current_user = (Session["UserDetail"] != null) ? Session["UserDetail"] as UserDetails : null;
                    if (current_user != null)
                    {
                        objorder.billing_first_name = current_user.first_name;
                        objorder.billing_last_name = current_user.last_name;
                    }
                }

                Response response = new Response();
                if (objorder.id == 0)
                {
                    response = await objRepository.CreateOrder(objorder, "Add");
                }
                else
                {
                    response = await objRepository.CreateOrder(objorder, "EditAddress");
                }

                if (response.Status == true)
                {
                    Session["OrderId"] = response.ResponseValue;
                    orderstatus = "Success";
                    //return RedirectToAction("GetCartProductList", "Manage", new { isWithPayment = true });
                }
                else
                {
                    orderstatus = "Fail";
                }
            }
            catch (Exception ex)
            {
                orderstatus = "Fail";
            }

            return Json(orderstatus, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<ActionResult> UpdateAccount(UserDetails objorder)
        {
            var orderstatus = string.Empty;
            try
            {
                objorder.modified = DateTime.Now;
                var response = await objRepository.UpdateAccount(objorder);
                if (response.Status == true)
                {
                    orderstatus = response.ResponseValue;
                }
                else
                {
                    orderstatus = "Fail";
                }
            }
            catch (Exception ex)
            {
                orderstatus = "Fail";
            }
            return Json(orderstatus, JsonRequestBehavior.AllowGet);
        }

        private async Task AssignStateCityList(string biilingState)
        {

            this.model.States = await this.objRepository.GetStateList();
            if (this.model.States == null)
            {
                this.model.States = new List<R_StateMaster>();
                this.model.States.Add(new R_StateMaster
                {
                    Id = 0,
                    Name = "-Not Available-"
                });
            }

            if (!string.IsNullOrEmpty(biilingState))
            {
                this.model.Cities = await this.objRepository.GetCityListById(biilingState);
            }

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

        public async Task<ActionResult> GetCityListByState(string Id)
        {
            var cityList = new List<R_CityMaster>();
            this.objRepository = new APIRepository();
            this.model = new Dashboard();
            if (string.IsNullOrEmpty(Id))
            {
                return null;
            }
            else
            {
                cityList = await this.objRepository.GetCityListById(Id);
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

        public async Task<ActionResult> SetCompanyDetailInSession()
        {
            if (Session["Company"] == null)
            {
                string companyId = System.Configuration.ConfigurationManager.AppSettings["CompanyId"];

                try
                {
                    var dt = new List<company>();
                    dt.Add(new company
                    {
                        id = Convert.ToInt32(companyId)
                    });

                    var companyDetail = await objRepository.GetCompanyById(dt);
                    if (companyDetail == null || companyDetail.default_flag == 0)
                    {
                        return View("Error");
                    }
                    else
                    {
                        Session["Company"] = companyDetail;
                    }
                }
                catch (Exception e)
                {
                    // return null;
                }
            }

            return null;
        }
    }
}