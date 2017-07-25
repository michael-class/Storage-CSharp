using System.Web.Http;
using Paycor.Neo.Rest.WebApi;
using Paycor.Storage.Host.WebApi.Utils;

namespace Paycor.Storage.Host.WebApi
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.Formatters.Add(new BinaryMediaTypeFormatter());

            // Web API configuration and services
            config.ConfigureJsonSerialization()
                .RegisterGlobalExceptionLogger()
                .RegisterRestExceptionHandler();

            // Web API routes
            config.MapHttpAttributeRoutes();
        }
    }
}
