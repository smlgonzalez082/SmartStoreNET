using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Customers;
using SmartStore.Core.Domain.Orders;
using SmartStore.Core.Security;
using SmartStore.DevTools.Blocks;
using SmartStore.DevTools.Models;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Services.Localization;
using SmartStore.Services.Orders;
using SmartStore.Services.Security;
using SmartStore.Services.Topics;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.Security;
using SmartStore.Web.Framework.Settings;
using SmartStore.Web.Framework.Theming;
using SmartStore.Web.Models.Checkout;
using SmartStore.Web.Models.Common;
using System;
using System.Web.Mvc;

namespace SmartStore.DevTools.Controllers
{
    public class DevToolsController : SmartController
    {
        private readonly ICommonServices _services;
        private readonly CatalogSettings _catalogSettings;
        private readonly CustomerSettings _customerSettings;
        private readonly Lazy<ITopicService> _topicService;
        private readonly Lazy<PrivacySettings> _privacySettings;
        private readonly Lazy<CaptchaSettings> _captchaSettings;

        public DevToolsController(
            ICommonServices services,
            CustomerSettings customerSettings,
            CatalogSettings catalogSettings,
            Lazy<ITopicService> topicService,
            Lazy<PrivacySettings> privacySettings,
            Lazy<CaptchaSettings> captchaSettings)
        {
            _services = services;
            _customerSettings = customerSettings;
            _catalogSettings = catalogSettings;
            _topicService = topicService;
            _privacySettings = privacySettings;
            _captchaSettings = captchaSettings;
        }

        [LoadSetting, ChildActionOnly]
        public ActionResult Configure(ProfilerSettings settings)
        {
            return View(settings);
        }

        [SaveSetting(false), HttpPost, ChildActionOnly, ActionName("Configure")]
        public ActionResult ConfigurePost(ProfilerSettings settings)
        {
            return RedirectToConfiguration("SmartStore.DevTools");
        }

        public ActionResult MiniProfiler()
        {
            return View();
        }

        public ActionResult MachineName()
        {
            ViewBag.EnvironmentIdentifier = _services.ApplicationEnvironment.EnvironmentIdentifier;

            return View();
        }

        public ActionResult QualityStep(int checkoutProgressStep)
        {       
            var step = TempData["CheckoutProgressStep"];

            var model = new CheckoutProgressModel
            {
                CheckoutProgressStep = (CheckoutProgressStep)step
            };
            return View(model);
        }

        public ActionResult QuantityStep(int checkoutProgressStep)
        {      
            var step = TempData["CheckoutProgressStep"];

            var model = new CheckoutProgressModel
            {
                CheckoutProgressStep = (CheckoutProgressStep)step
            };
            return View(model);
        }

        public ActionResult SizeStep(int checkoutProgressStep)
        {       
            var step = TempData["CheckoutProgressStep"];

            var model = new CheckoutProgressModel
            {
                CheckoutProgressStep = (CheckoutProgressStep)step
            };
            return View(model);
        }

        public ActionResult ScheduleStep(int checkoutProgressStep)
        {         
            var step = TempData["CheckoutProgressStep"];

            var model = new CheckoutProgressModel
            {
                CheckoutProgressStep = (CheckoutProgressStep)step
            };
            return View(model);
        }

        public ActionResult Slider()
        {
            return View();
        }

        public ActionResult HomePage()
        {
            var topic = _topicService.Value.GetTopicBySystemName("ContactUs", 0, false);

            var model = new ContactUsModel
            {
                Email = Services.WorkContext.CurrentCustomer.Email,
                FullName = Services.WorkContext.CurrentCustomer.GetFullName(),
                FullNameRequired = _privacySettings.Value.FullNameOnContactUsRequired,
                DisplayCaptcha = _captchaSettings.Value.Enabled && _captchaSettings.Value.ShowOnContactUsPage,
                MetaKeywords = topic?.GetLocalized(x => x.MetaKeywords),
                MetaDescription = topic?.GetLocalized(x => x.MetaDescription),
                MetaTitle = topic?.GetLocalized(x => x.MetaTitle),
            };

            return View(model);
        }

        public ActionResult Footer()
        {
            return View();
        }

        public ActionResult Header()
        {
            var customer = _services.WorkContext.CurrentCustomer;

            var cartItems = Services.WorkContext.CurrentCustomer.GetCartItems(ShoppingCartType.ShoppingCart, Services.StoreContext.CurrentStore.Id);
            ViewBag.cartItemsCount = cartItems.GetTotalProducts();

            var model = new MenuBarModel
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.CustomerLoginType != CustomerLoginType.Email ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _services.WorkContext.OriginalCustomerIfImpersonated != null,
                IsAuthenticated = customer.IsRegistered(),
                DisplayAdminLink = _services.Permissions.Authorize(Permissions.System.AccessBackend),
                HasContactUsPage = Url.Topic("ContactUs").ToString().HasValue(),
                DisplayLoginLink = _customerSettings.UserRegistrationType != UserRegistrationType.Disabled
            };

            return View(model);
        }

        [ChildActionOnly]
        public ActionResult TopBar()
        {
            var customer = _services.WorkContext.CurrentCustomer;

            var model = new MenuBarModel
            {
                RecentlyAddedProductsEnabled = _catalogSettings.RecentlyAddedProductsEnabled,
                CustomerEmailUsername = customer.IsRegistered() ? (_customerSettings.CustomerLoginType != CustomerLoginType.Email ? customer.Username : customer.Email) : "",
                IsCustomerImpersonated = _services.WorkContext.OriginalCustomerIfImpersonated != null,
                IsAuthenticated = customer.IsRegistered(),
                DisplayAdminLink = _services.Permissions.Authorize(Permissions.System.AccessBackend),
                HasContactUsPage = Url.Topic("ContactUs").ToString().HasValue(),
                DisplayLoginLink = _customerSettings.UserRegistrationType != UserRegistrationType.Disabled
            };

            return PartialView("TopBar", model);
        }

        public ActionResult WidgetZone(string widgetZone)
        {
            var storeScope = this.GetActiveStoreScopeConfiguration(_services.StoreService, _services.WorkContext);
            var settings = _services.Settings.LoadSetting<ProfilerSettings>(storeScope);

            if (settings.DisplayWidgetZones)
            {
                ViewData["widgetZone"] = widgetZone;

                return View();
            }

            return new EmptyResult();
        }

        [ChildActionOnly]
        public ActionResult SampleBlock(SampleBlock block)
        {
            // Do something here with your block instance and return a result that should be rendered by the Page Builder.
            return View(block);
        }

        [AdminAuthorize, AdminThemed]
        public ActionResult BackendExtension()
        {
            var model = new BackendExtensionModel
            {
                Welcome = "Hello world!"
            };

            return View(model);
        }

        [AdminAuthorize]
        public ActionResult ProductEditTab(int productId, FormCollection form)
        {
            var model = new BackendExtensionModel
            {
                Welcome = "Hello world!"
            };

            var result = PartialView(model);
            result.ViewData.TemplateInfo = new TemplateInfo { HtmlFieldPrefix = "CustomProperties[DevTools]" };
            return result;
        }

        public ActionResult MyDemoWidget()
        {
            return Content("Hello world! This is a sample widget created for demonstration purposes by Dev-Tools plugin.");
        }
    }
}