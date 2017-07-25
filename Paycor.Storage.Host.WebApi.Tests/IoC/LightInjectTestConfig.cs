using System.Configuration;
using Common.Logging;
using Paycor.Neo.Data.Yarn.LightInject;
using Paycor.Storage.Host.WebApi.Controllers.V1;
using Paycor.Storage.Host.WebApi.Models.V1;
using Paycor.Storage.Host.WebApi.Settings;
using Paycor.Storage.Service;

namespace Paycor.Storage.Host.WebApi.IoC
{
    public static class LightInjectTestConfig
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LightInjectTestConfig));

        public static YarnContainerAdapter Register()
        {
            _logger.Debug("Register()");

            var factory = LightInjectConfig.Register();
            var container = factory.Container;
            factory.Register(() =>
                    new DocumentStorageV1Controller(container.GetInstance<IDocumentStorage>(),
                        new DocumentRequestValidator(), new DocumentStorageSettings()));
            return factory;
        }

    }
}