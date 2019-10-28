using SmartStore.Core.Domain.Catalog;
using SmartStore.Core.Domain.Media;
using SmartStore.Core.Domain.Seo;
using SmartStore.Core.Domain.Tax;
using SmartStore.DevTools.Models;
using SmartStore.Services;
using SmartStore.Services.Catalog;
using SmartStore.Services.Catalog.Modelling;
using SmartStore.Services.Localization;
using SmartStore.Services.Media;
using SmartStore.Services.Orders;
using SmartStore.Services.Search;
using SmartStore.Services.Security;
using SmartStore.Services.Seo;
using SmartStore.Services.Stores;
using SmartStore.Services.Tax;
using SmartStore.Web;
using SmartStore.Web.Controllers;
using SmartStore.Web.Framework.Controllers;
using SmartStore.Web.Framework.UI;
using SmartStore.Web.Infrastructure.Cache;
using SmartStore.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI.WebControls;
using MenuItem = SmartStore.Web.Framework.UI.MenuItem;

namespace SmartStore.DevTools.Controllers
{
    public class MyCheckoutController : PublicControllerBase
    {
        private readonly ICommonServices _services;
        private readonly CatalogHelper _helper;
        private readonly ICatalogSearchService _catalogSearchService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IProductService _productService;
        private readonly IAclService _aclService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly MediaSettings _mediaSettings;
        private readonly SeoSettings _seoSettings;
        private readonly IBreadcrumb _breadcrumb;
        private readonly IRecentlyViewedProductsService _recentlyViewedProductsService;
        private readonly IProductTagService _productTagService;
        private readonly IOrderReportService _orderReportService;
        private readonly IPictureService _pictureService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly ITaxService _taxService;
        private readonly Lazy<TaxSettings> _taxSettings;
        private readonly HttpContextBase _httpContext;

        public MyCheckoutController(
             ICommonServices services,
            CatalogHelper helper,
            ICatalogSearchService catalogSearchService,
            CatalogSettings catalogSettings,
            IProductService productService,
            IStoreMappingService storeMappingService,
            IAclService aclService,
            MediaSettings mediaSettings,
            SeoSettings seoSettings,
            IBreadcrumb breadcrumb,
            IRecentlyViewedProductsService recentlyViewedProductsService,
            IProductTagService productTagService,
            IOrderReportService orderReportService,
            IPictureService pictureService,
            IProductAttributeService productAttributeService,
            ITaxService taxService,
             HttpContextBase httpContext,
            Lazy<TaxSettings> taxSettings)
        {
            _services = services;
            _helper = helper;
            _catalogSearchService = catalogSearchService;
            _catalogSettings = catalogSettings;
            _productService = productService;
            _aclService = aclService;
            _storeMappingService = storeMappingService;
            _mediaSettings = mediaSettings;
            _seoSettings = seoSettings;
            _breadcrumb = breadcrumb;
            _recentlyViewedProductsService = recentlyViewedProductsService;
            _productTagService = productTagService;
            _orderReportService = orderReportService;
            _pictureService = pictureService;
            _productAttributeService = productAttributeService;
            _taxService = taxService;
            _taxSettings = taxSettings;
            _httpContext = httpContext;
        }

        public ActionResult MyBillingAddress()
        {
            return View();
        }

        //public ActionResult CheckoutStep(int categoryId, CatalogSearchQuery query)
        //public ActionResult QualityStep(int categoryId = 18)
        public ActionResult QualityStep(int categoryId = 1)
        {

            //int _categoryId = categoryId;
            var catIds = new int[] { categoryId };
            if (_catalogSettings.ShowProductsFromSubcategories)
            {
                // Include subcategories.
                catIds = catIds.Concat(_helper.GetChildCategoryIds(categoryId)).ToArray();
            }

            CatalogSearchQuery query = new CatalogSearchQuery();

            query.WithCategoryIds(_catalogSettings.IncludeFeaturedProductsInNormalLists ? (bool?)null : false, catIds);

            var searchResult = _catalogSearchService.Search(query);
            var mappingSettings = _helper.GetBestFitProductSummaryMappingSettings(query.GetViewMode());
            mappingSettings.MapSpecificationAttributes = true;
            mappingSettings.MapFullDescription = true;
            ProductSummaryModel products = _helper.MapProductSummaryModel(searchResult.Hits, mappingSettings);

            return View(products);
        }

        //public ActionResult CheckoutStep(int categoryId, CatalogSearchQuery query)
        public ActionResult QuantityStep(int productId, int quantity = 0)
        {

            // var productId = 16;

            QuantityModel addToCart = new QuantityModel();

            var product = _productService.GetProductById(productId);

            addToCart.ProductId = product.Id;
            addToCart.EnteredQuantity = quantity != 0 ? quantity : product.OrderMinimumQuantity;
            addToCart.MinOrderAmount = product.OrderMinimumQuantity;
            addToCart.MaxOrderAmount = product.OrderMaximumQuantity;
            //addToCart.QuantityUnitName = model.QuantityUnitName; // TODO: (mc) remove 'QuantityUnitName' from parent model later
            addToCart.QuantityStep = product.QuantityStep > 0 ? product.QuantityStep : 1;
            addToCart.HideQuantityControl = product.HideQuantityControl;
            addToCart.QuantiyControlType = product.QuantiyControlType;
            addToCart.AvailableForPreOrder = product.AvailableForPreOrder;

            addToCart.CustomerEntersPrice = product.CustomerEntersPrice;

            var allowedQuantities = product.ParseAllowedQuatities();
            foreach (var qty in allowedQuantities)
            {
                addToCart.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString()
                });
            }


            return View(addToCart);
        }

        // public ActionResult QuantitySizeStep(int productId, string attributes, ProductVariantQuery query)
        //public ActionResult SizeStep(int productId, int quantity)      QuantityModel
        //[HttpPost]
        public ActionResult SizeStep(int productId, int quantity)
        {
            //int productId = 60;
            ViewBag.countfilters = quantity;
            string attributes = "";
            ProductVariantQuery query = new ProductVariantQuery();
            var product = _productService.GetProductById(productId);
            if (product == null || product.Deleted || product.IsSystemProduct)
                return HttpNotFound();

            // Is published? Check whether the current user has a "Manage catalog" permission.
            // It allows him to preview a product before publishing.
           if (!product.Published)
                return HttpNotFound();

            // ACL (access control list)
            if (!_aclService.Authorize(product))
                return HttpNotFound();

            // Store mapping
            if (!_storeMappingService.Authorize(product))
                return HttpNotFound();

            // Is product individually visible?
            if (!product.VisibleIndividually)
            {
                // Find parent grouped product.
                var parentGroupedProduct = _productService.GetProductById(product.ParentGroupedProductId);
                if (parentGroupedProduct == null)
                    return HttpNotFound();

                var seName = parentGroupedProduct.GetSeName();
                if (seName.IsEmpty())
                    return HttpNotFound();

                var routeValues = new RouteValueDictionary();
                routeValues.Add("SeName", seName);

                // Add query string parameters.
                Request.QueryString.AllKeys.Each(x => routeValues.Add(x, Request.QueryString[x]));

                return RedirectToRoute("Product", routeValues);
            }

            // Prepare the view model
            var model = _helper.PrepareProductDetailsPageModel(product, query);

            // Some cargo data
            model.PictureSize = _mediaSettings.ProductDetailsPictureSize;
            model.CanonicalUrlsEnabled = _seoSettings.CanonicalUrlsEnabled;

            // Save as recently viewed
            _recentlyViewedProductsService.AddProductToRecentlyViewedList(product.Id);

            // Activity log
            _services.CustomerActivity.InsertActivity("PublicStore.ViewProduct", T("ActivityLog.PublicStore.ViewProduct"), product.Name);

            // Breadcrumb
            if (_catalogSettings.CategoryBreadcrumbEnabled)
            {
                _helper.GetCategoryBreadcrumb(_breadcrumb, ControllerContext, product);

                _breadcrumb.Track(new MenuItem
                {
                    Text = model.Name,
                    Rtl = model.Name.CurrentLanguage.Rtl,
                    EntityId = product.Id,
                    Url = Url.RouteUrl("Product", new { model.SeName })
                });
            }

            //return View(model.ProductTemplateViewPath, model);

            return View("SizeStep", model);
        }

        public ActionResult ScheduleStep(ProductDetailsModel model, int quantity)
        {
            ViewBag.frecuency = _httpContext.Session["ShipmentFrecuency"];
            ViewBag.quantity = quantity;

            ViewBag.frecuencies = new List<FrecuencyModel> {
                                    new   FrecuencyModel{
                                    Text = "Every Month",
                                    Selected =  (ViewBag.frecuency != null && ViewBag.frecuency == 1) || (ViewBag.frecuency == null) ? true:false,
                                    Value = 1

                                    } ,
                                    new FrecuencyModel {
                                    Text = "Every 2 Months",
                                    Selected =  (ViewBag.frecuency != null && ViewBag.frecuency == 2) ? true:false,
                                      Value = 2

                                    } ,
                                    new FrecuencyModel{
                                    Text = "Every 3 Months",
                                    Selected = (ViewBag.frecuency != null && ViewBag.frecuency == 3) ? true:false,
                                      Value = 3

                                    } ,
                                    new FrecuencyModel{
                                    Text = "Every 4 Months",
                                    Selected =  (ViewBag.frecuency != null && ViewBag.frecuency == 4) ? true:false,
                                     Value = 4

                                    }
                                    };


            return View("ScheduleStep", model);
        }

        [HttpPost]
        public ActionResult ScheduleStep(int shipmentFrecuency)
        {
            _httpContext.Session["ShipmentFrecuency"] = shipmentFrecuency;

            return Json("OK");
        }

        [ChildActionOnly]
        public ActionResult ProductTierPrices(int productId)
        {
           /* if (!_services.Permissions.Authorize(StandardPermissionProvider.DisplayPrice))
            {
                return Content("");
            }     */

            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", productId));
            }

            if (!product.HasTierPrices)
            {
                // No tier prices
                return Content("");
            }

            var model = _helper.CreateTierPriceModel(product);

            return PartialView("Product.TierPrices", model);
        }

        [ChildActionOnly]
        public ActionResult ProductSpecifications(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", productId));
            }

            var model = _helper.PrepareProductSpecificationModel(product);

            if (model.Count == 0)
            {
                return Content("");
            }

            return PartialView("Product.Specs", model);
        }


        [ChildActionOnly]
        public ActionResult ProductTags(int productId)
        {
            var product = _productService.GetProductById(productId);
            if (product == null)
            {
                throw new ArgumentException(T("Products.NotFound", productId));
            }

            var cacheKey = string.Format(ModelCacheEventConsumer.PRODUCTTAG_BY_PRODUCT_MODEL_KEY, product.Id, _services.WorkContext.WorkingLanguage.Id, _services.StoreContext.CurrentStore.Id);
            var cacheModel = _services.Cache.Get(cacheKey, () =>
            {
                var model = product.ProductTags
                    // Filter by store
                    .Where(x => _productTagService.GetProductCount(x.Id, _services.StoreContext.CurrentStore.Id) > 0)
                    .Select(x =>
                    {
                        var ptModel = new ProductTagModel
                        {
                            Id = x.Id,
                            Name = x.GetLocalized(y => y.Name),
                            SeName = x.GetSeName(),
                            ProductCount = _productTagService.GetProductCount(x.Id, _services.StoreContext.CurrentStore.Id)
                        };
                        return ptModel;
                    })
                    .ToList();
                return model;
            });

            return PartialView("Product.Tags", cacheModel);
        }

        [ChildActionOnly]
        public ActionResult RelatedProducts(int productId, int? productThumbPictureSize)
        {
            var products = new List<Product>();
            var relatedProducts = _productService.GetRelatedProductsByProductId1(productId);

            foreach (var product in _productService.GetProductsByIds(relatedProducts.Select(x => x.ProductId2).ToArray()))
            {
                // Ensure has ACL permission and appropriate store mapping
                if (_aclService.Authorize(product) && _storeMappingService.Authorize(product))
                    products.Add(product);
            }

            if (products.Count == 0)
            {
                return Content("");
            }

            var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Grid, x =>
            {
                x.ThumbnailSize = productThumbPictureSize;
                x.MapDeliveryTimes = false;
            });

            var model = _helper.MapProductSummaryModel(products, settings);
            model.ShowBasePrice = false;

            return PartialView("Product.RelatedProducts", model);
        }

        [ChildActionOnly]
        public ActionResult ProductsAlsoPurchased(int productId, int? productThumbPictureSize)
        {
            if (!_catalogSettings.ProductsAlsoPurchasedEnabled)
            {
                return Content("");
            }

            // load and cache report
            var productIds = _services.Cache.Get(string.Format(ModelCacheEventConsumer.PRODUCTS_ALSO_PURCHASED_IDS_KEY, productId, _services.StoreContext.CurrentStore.Id), () =>
            {
                return _orderReportService.GetAlsoPurchasedProductsIds(_services.StoreContext.CurrentStore.Id, productId, _catalogSettings.ProductsAlsoPurchasedNumber);
            });

            // Load products
            var products = _productService.GetProductsByIds(productIds);

            // ACL and store mapping
            products = products.Where(p => _aclService.Authorize(p) && _storeMappingService.Authorize(p)).ToList();

            if (products.Count == 0)
            {
                return Content("");
            }

            // Prepare model
            var settings = _helper.GetBestFitProductSummaryMappingSettings(ProductSummaryViewMode.Mini, x =>
            {
                x.ThumbnailSize = productThumbPictureSize;
            });

            var model = _helper.MapProductSummaryModel(products, settings);

            return PartialView("Product.AlsoPurchased", model);
        }


        [HttpPost]
        public ActionResult UpdateProductDetails(int productId, string itemType, int bundleItemId, ProductVariantQuery query, FormCollection form)
        {
            int quantity = 1;
            int galleryStartIndex = -1;
            string galleryHtml = null;
            string dynamicThumbUrl = null;
            var isAssociated = itemType.IsCaseInsensitiveEqual("associateditem");
            var pictureModel = new ProductDetailsPictureModel();
            var m = new ProductDetailsModel();
            var product = _productService.GetProductById(productId);
            var bItem = _productService.GetBundleItemById(bundleItemId);
            IList<ProductBundleItemData> bundleItems = null;
            ProductBundleItemData bundleItem = (bItem == null ? null : new ProductBundleItemData(bItem));

            // Quantity required for tier prices.
            string quantityKey = form.AllKeys.FirstOrDefault(k => k.EndsWith("EnteredQuantity"));
            if (quantityKey.HasValue())
            {
                int.TryParse(form[quantityKey], out quantity);
            }

            if (product.ProductType == ProductType.BundledProduct && product.BundlePerItemPricing)
            {
                bundleItems = _productService.GetBundleItems(product.Id);
                if (query.Variants.Count > 0)
                {
                    // May add elements to query object if they are preselected by bundle item filter.
                    foreach (var itemData in bundleItems)
                    {
                        _helper.PrepareProductDetailsPageModel(itemData.Item.Product, query, false, itemData, null);
                    }
                }
            }

            // Get merged model data.
            _helper.PrepareProductDetailModel(m, product, query, isAssociated, bundleItem, bundleItems, quantity);

            if (bundleItem != null)
            {
                // Update bundle item thumbnail.
                if (!bundleItem.Item.HideThumbnail)
                {
                    var picture = m.GetAssignedPicture(_pictureService, null, bundleItem.Item.ProductId);
                    dynamicThumbUrl = _pictureService.GetUrl(picture, _mediaSettings.BundledProductPictureSize, false);
                }
            }
            else if (isAssociated)
            {
                // Update associated product thumbnail.
                var picture = m.GetAssignedPicture(_pictureService, null, productId);
                dynamicThumbUrl = _pictureService.GetUrl(picture, _mediaSettings.AssociatedProductPictureSize, false);
            }
            else if (product.ProductType != ProductType.BundledProduct)
            {
                // Update image gallery.
                var pictures = _pictureService.GetPicturesByProductId(productId);

                if (product.HasPreviewPicture && pictures.Count > 1)
                {
                    pictures.RemoveAt(0);
                }

                if (pictures.Count <= _catalogSettings.DisplayAllImagesNumber)
                {
                    // All pictures rendered... only index is required.
                    var picture = m.GetAssignedPicture(_pictureService, pictures);
                    galleryStartIndex = (picture == null ? 0 : pictures.IndexOf(picture));
                }
                else
                {
                    var allCombinationPictureIds = _productAttributeService.GetAllProductVariantAttributeCombinationPictureIds(product.Id);

                    _helper.PrepareProductDetailsPictureModel(
                        pictureModel,
                        pictures,
                        product.GetLocalized(x => x.Name),
                        allCombinationPictureIds,
                        false,
                        bundleItem,
                        m.SelectedCombination);

                    galleryStartIndex = pictureModel.GalleryStartIndex;
                    galleryHtml = this.RenderPartialViewToString("Product.Picture", pictureModel);
                }

                m.PriceDisplayStyle = _catalogSettings.PriceDisplayStyle;
                m.DisplayTextForZeroPrices = _catalogSettings.DisplayTextForZeroPrices;
            }

            object partials = null;

            if (m.IsBundlePart)
            {
                partials = new
                {
                    BundleItemPrice = this.RenderPartialViewToString("Product.Offer.Price", m),
                    BundleItemStock = this.RenderPartialViewToString("Product.StockInfo", m)
                };
            }
            else
            {
                var dataDictAddToCart = new ViewDataDictionary();
                dataDictAddToCart.TemplateInfo.HtmlFieldPrefix = string.Format("addtocart_{0}", m.Id);

                decimal adjustment = decimal.Zero;
                decimal taxRate = decimal.Zero;
                var finalPriceWithDiscountBase = _taxService.GetProductPrice(product, product.Price, _services.WorkContext.CurrentCustomer, out taxRate);

                if (!_taxSettings.Value.PricesIncludeTax && _services.WorkContext.TaxDisplayType == TaxDisplayType.IncludingTax)
                {
                    adjustment = (m.ProductPrice.PriceValue - finalPriceWithDiscountBase) / (taxRate / 100 + 1);
                }
                else if (_taxSettings.Value.PricesIncludeTax && _services.WorkContext.TaxDisplayType == TaxDisplayType.ExcludingTax)
                {
                    adjustment = (m.ProductPrice.PriceValue - finalPriceWithDiscountBase) * (taxRate / 100 + 1);
                }
                else
                {
                    adjustment = m.ProductPrice.PriceValue - finalPriceWithDiscountBase;
                }

                partials = new
                {
                    Attrs = this.RenderPartialViewToString("Product.Attrs", m),
                    Price = this.RenderPartialViewToString("Product.Offer.Price", m),
                    Stock = this.RenderPartialViewToString("Product.StockInfo", m),
                    OfferActions = this.RenderPartialViewToString("Product.Offer.Actions", m, dataDictAddToCart),
                    TierPrices = this.RenderPartialViewToString("Product.TierPrices", _helper.CreateTierPriceModel(product, adjustment)),
                    BundlePrice = product.ProductType == ProductType.BundledProduct ? this.RenderPartialViewToString("Product.Bundle.Price", m) : (string)null
                };
            }

            object data = new
            {
                Partials = partials,
                DynamicThumblUrl = dynamicThumbUrl,
                GalleryStartIndex = galleryStartIndex,
                GalleryHtml = galleryHtml
            };

            return new JsonResult { Data = data };
        }
    }
}