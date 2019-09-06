using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MyMuseo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            // TO AVOID ERROR 
            AntiForgeryConfig.SuppressIdentityHeuristicChecks = true;
        }
        protected void Application_BeginRequest()
        {
            // Ensure any request is returned over SSL/TLS in production
            if (!Request.IsLocal && !Context.Request.IsSecureConnection)
            {
                if (!Context.Request.Url.ToString().Contains("tungson"))
                {
                    var redirect = Context.Request.Url.ToString().ToLower(CultureInfo.CurrentCulture).Replace("http:", "https:");
                    Response.Redirect(redirect);
                }
            }
        }

    }
}
