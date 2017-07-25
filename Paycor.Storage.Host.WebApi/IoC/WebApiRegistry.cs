using Common.Logging;
using LightInject;
using Paycor.Storage.Host.WebApi.Models.V1;
using Paycor.Storage.Host.WebApi.Settings;

namespace Paycor.Storage.Host.WebApi.IoC
{
    public class WebApiRegistry : BaseCompositeRoot
    {
        private static readonly ILog _logger = LogManager.GetLogger<WebApiRegistry>();

        public override void Compose(IServiceRegistry registry)
        {
            _logger.Trace("Compose()");

            registry.Register(factory => new DocumentRequestValidator(), new PerContainerLifetime());
            registry.Register<IDocumentStorageSettings>(factory => new DocumentStorageSettings());

        }
    }
}