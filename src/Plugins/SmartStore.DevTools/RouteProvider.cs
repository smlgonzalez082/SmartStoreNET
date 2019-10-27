using System.Web.Mvc;
using System.Web.Mvc.Routing.Constraints;
using System.Web.Routing;
using SmartStore.Web.Framework.Localization;
using SmartStore.Web.Framework.Routing;

namespace SmartStore.DevTools
{

    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            var idConstraint = new MinRouteConstraint(1);


            routes.MapRoute("SmartStore.DevTools",
                 "Plugin/SmartStore.DevTools/{action}/{id}",
                 new { controller = "DevTools", action = "Configure", id = UrlParameter.Optional },
                 new[] { "SmartStore.DevTools.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.DevTools";


            routes.MapLocalizedRoute("TopBar",
             "topbar/",
             new { controller = "DevTools", action = "TopBar" },
             new[] { "SmartStore.DevTools.Controllers" });

            routes.MapLocalizedRoute("QualityFilterStep",
               "qualityfilterstep/",
               new { controller = "DevTools", action = "QualityStep" },
               new[] { "SmartStore.DevTools.Controllers" });


            routes.MapLocalizedRoute("QuantityFilterStep",
                "quantityfilterstep/",
                new { controller = "DevTools", action = "QuantityStep" },
                new[] { "SmartStore.DevTools.Controllers" });

            routes.MapLocalizedRoute("SizeFilterStep",
              "sizefilterstep/",
              new { controller = "DevTools", action = "SizeStep" },
              new[] { "SmartStore.DevTools.Controllers" });

            routes.MapLocalizedRoute("SliderFilter",
              "slider/",
              new { controller = "DevTools", action = "Slider" },
              new[] { "SmartStore.DevTools.Controllers" });

            routes.MapLocalizedRoute("HomePageFilter",
             "home/",
             new { controller = "DevTools", action = "HomePage" },
             new[] { "SmartStore.DevTools.Controllers" });


            routes.MapLocalizedRoute("FooterFilter",
           "footer/",
           new { controller = "DevTools", action = "Footer" },
           new[] { "SmartStore.DevTools.Controllers" });

            routes.MapLocalizedRoute("HeaderFilter",
           "header/",
            new { controller = "DevTools", action = "Header" },
            new[] { "SmartStore.DevTools.Controllers" });


            routes.MapLocalizedRoute("QualityStep",
                "MyCheckout/QualityStep/{categoryId}",
                new { controller = "MyCheckout", action = "QualityStep" },
                new { categoryId = idConstraint },
                new[] { "SmartStore.DevTools.Controllers" })
                .DataTokens["area"] = "SmartStore.DevTools";

            routes.MapLocalizedRoute("QuantityStep",
                "MyCheckout/QuantityStep/{productId}",
                new { controller = "MyCheckout", action = "QuantityStep" },
                new { productId = idConstraint },
                new[] { "SmartStore.DevTools.Controllers" })
                .DataTokens["area"] = "SmartStore.DevTools";


            routes.MapLocalizedRoute("SizeStep",
                "MyCheckout/SizeStep/{productId}/{quantity}",
                new { controller = "MyCheckout", action = "SizeStep" },
                new { productId = idConstraint },
                new[] { "SmartStore.DevTools.Controllers" })
                .DataTokens["area"] = "SmartStore.DevTools";

            routes.MapLocalizedRoute("ScheduleStep",
              "MyCheckout/ScheduleStep",
              new { controller = "MyCheckout", action = "ScheduleStep" },
              new[] { "SmartStore.DevTools.Controllers" })
              .DataTokens["area"] = "SmartStore.DevTools";


            routes.MapRoute("SmartStore.DevTools.MyCheckout",
                 "MyCheckout/{action}",
                 new { controller = "MyCheckout", action = "QualityQuantityStep" },
                 new[] { "SmartStore.DevTools.Controllers" }
            )
            .DataTokens["area"] = "SmartStore.DevTools";
        }
        public int Priority
        {
            get
            {
                return 0;
            }
        }
    }

}
