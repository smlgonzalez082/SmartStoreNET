using SmartStore.Core.Domain.Catalog;
using SmartStore.Services.Localization;
using SmartStore.Web.Framework;
using SmartStore.Web.Framework.Modelling;
using SmartStore.Web.Models.Catalog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace SmartStore.DevTools.Models
{
    public class QuantityModel : ModelBase, IQuantityInput
    {
        public QuantityModel()
        {
            this.AllowedQuantities = new List<SelectListItem>();
        }
        public int ProductId { get; set; }

        [SmartResourceDisplayName("Products.Qty")]
        public int EnteredQuantity { get; set; }

        [SmartResourceDisplayName("Products.EnterProductPrice")]
        public bool CustomerEntersPrice { get; set; }
        [SmartResourceDisplayName("Products.EnterProductPrice")]
        public decimal CustomerEnteredPrice { get; set; }
        public String CustomerEnteredPriceRange { get; set; }

        public int MinOrderAmount { get; set; }
        public int MaxOrderAmount { get; set; }
        public LocalizedValue<string> QuantityUnitName { get; set; }
        public int QuantityStep { get; set; }
        public bool HideQuantityControl { get; set; }
        public QuantityControlType QuantiyControlType { get; set; }

        public bool DisableBuyButton { get; set; }
        public bool DisableWishlistButton { get; set; }
        public List<SelectListItem> AllowedQuantities { get; set; }
        public bool AvailableForPreOrder { get; set; }
    }
}