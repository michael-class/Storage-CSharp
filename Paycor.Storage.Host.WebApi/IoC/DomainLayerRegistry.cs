using Common.Logging;
using LightInject;
using Paycor.Neo.Data.Repositories;
using Paycor.Storage.Data.Storage;
using Paycor.Storage.Service;

namespace Paycor.Storage.Host.WebApi.IoC
{
    public class DomainLayerRegistry : BaseCompositeRoot
    {
        private static readonly ILog _logger = LogManager.GetLogger<DomainLayerRegistry>();

        public override void Compose(IServiceRegistry registry)
        {
            _logger.Trace("Compose()");

            registry.Register<IDocumentStorage>(
                factory => new DocumentStorage(
                    factory.GetInstance<IUnitOfWork>(),
                    factory.GetInstance<IBlockStorage>()),
                    PerRequestLifetimeIfHosted());
        }
    }
}