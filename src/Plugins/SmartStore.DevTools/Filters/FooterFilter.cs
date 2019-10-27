using System;
using System.Web.Mvc;
using SmartStore.Services;
using SmartStore.Services.Customers;
using SmartStore.Web.Framework.UI;

namespace SmartStore.DevTools.Filters
{
    public class FooterFilter : IResultFilter
    {
        private readonly ICommonServices _services;
        private readonly Lazy<IWidgetProvider> _widgetProvider;
        private readonly ProfilerSettings _profilerSettings;

        public FooterFilter(
            ICommonServices services,
            Lazy<IWidgetProvider> widgetProvider,
            ProfilerSettings profilerSettings)
        {
            _services = services;
            _widgetProvider = widgetProvider;
            _profilerSettings = profilerSettings;
        }

        public void OnResultExecuting(ResultExecutingContext filterContext)
        {


            if (filterContext.IsChildAction)
                return;

            var result = filterContext.Result;

            // should only run on a full view rendering result or HTML ContentResult
            if (!result.IsHtmlViewResult())
                return;


            _widgetProvider.Value.RegisterAction(
                //new[] { "body_end_html_tag_before", "admin_content_after", "checkout_steps_before" },
                new[] { "footer_before" },
                "Footer",
                "DevTools",
                new { area = "SmartStore.DevTools" });

        }

        public void OnResultExecuted(ResultExecutedContext filterContext)
        {
        }

    }
}
