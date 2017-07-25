using System.Runtime.Remoting.Messaging;
using System.Web.Http;
using Common.Logging;
using LightInject;
using Paycor.Neo.Data.Yarn.LightInject;
using Paycor.Storage.Host.WebApi.Controllers.V1;
using Paycor.Storage.Host.WebApi.Models.V1;
using Paycor.Storage.Host.WebApi.Settings;
using Paycor.Storage.Service;
using Yarn;

namespace Paycor.Storage.Host.WebApi.IoC
{
    public static class LightInjectConfig
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LightInjectConfig));

        public static void Register(HttpConfiguration config)
        {
            _logger.Debug("Register()");

            var container = Register();

            container.Container.RegisterApiControllers();
            container.Container.EnablePerWebRequestScope();
            container.Container.EnableWebApi(config);

        }

        public static YarnContainerAdapter Register()
        {
            var containerAdapter = new YarnContainerAdapter();

            var container = containerAdapter.Container;

            //register other services
            container.RegisterFrom<DataLayerRegistry>();
            container.RegisterFrom<DomainLayerRegistry>();
            container.RegisterFrom<WebApiRegistry>();

            ObjectContainer.Initialize(() => containerAdapter); //Tells Yarn to use our container
            return containerAdapter;
        }

    }
}