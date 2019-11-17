using SmartStore.Web.Framework.Modelling;
using System;
using System.Collections.Generic;

namespace SmartStore.DevTools.Models
{
    public class AttributeProductModel : ModelBase
    {
        public string [] AttributeValues { get; set; }

        public int EnteredQuantity { get; set; }

        public int Id { get; set; }

        public int ProductId { get; set; }

        public string ProductName { get; set; }

        public string [] ControlIds { get; set; }
        
    }
}