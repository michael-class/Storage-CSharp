using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Paycor.Neo.Rest.WebApi;
using Paycor.Storage.Host.WebApi.IoC;

namespace Paycor.Storage.Host.WebApi
{
    public class WebApiApplication : BaseHttpApplication
    {
        protected void Application_Start()
        {
            LightInjectConfig.Register(GlobalConfiguration.Configuration);

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
