using System.Data.Entity;
using Common.Logging;
using LightInject;
using Paycor.Neo.Data.Yarn.EF.Logging;
using Paycor.Neo.Data.Yarn.EF.Repositories;
using Paycor.Neo.Data.Yarn.LightInject;
using Paycor.Storage.Data.EF.Context;
using Paycor.Storage.Data.EF.Repository;
using Paycor.Storage.Data.Repository;
using Yarn;
using Yarn.Extensions;

namespace Paycor.Storage.Data.EF.IoC
{
    public static class LightInjectTestConfig
    {
        private static readonly ILog _logger = LogManager.GetLogger(typeof(LightInjectTestConfig));

        public const string DB_CONNECTION = "DocStorage";

        public static YarnContainerAdapter Register()
        {
            _logger.Trace("Register()");

            var containerAdapter = new YarnContainerAdapter();
            ObjectContainer.Initialize(() => containerAdapter); //Tells Yarn to use our container

            var container = containerAdapter.Container;

            // EF Configuration
            var efConfig = new EFConfig(DB_CONNECTION) { EnableLogging = true, AutoDetectChangesEnabled = false };
            container.Register(factory => efConfig);

            const string APP = "AccountRepository";
            const string DOCUMENT = "DocumentRepository";

            // Delegate Yarn Repositories
            container.Register(factory => CreateYarnRepository(factory, APP), APP/*,  new PerContainerLifetime() */);
            container.Register(factory => CreateYarnRepository(factory, DOCUMENT), DOCUMENT/*,  new PerContainerLifetime() */);

            // Data Contract Repositories
            container.Register<IAccountRepository>(factory => new AccountRepository(factory.GetInstance<IRepository>(APP))/*,  new PerContainerLifetime() */);
            container.Register<IDocumentRepository>(factory => new DocumentRepository(factory.GetInstance<IRepository>(DOCUMENT))/*,  new PerContainerLifetime() */);

            // EF DbContext
            container.Register(factory => new StorageContext(factory.GetInstance<EFConfig>())/*,  new PerContainerLifetime() */);

            // Unit of Work
            container.Register<Neo.Data.Repositories.IUnitOfWork>(factory => new EFUnitOfWork(
                factory.GetInstance<StorageContext>(),
                factory.GetInstance<EFConfig>(),
                new Neo.Data.Repositories.IRepository[]
                {
                    factory.GetInstance<IAccountRepository>(),
                    factory.GetInstance<IDocumentRepository>()
                })/* ,  new PerContainerLifetime() */);

            return containerAdapter;
        }

        private static IRepository CreateYarnRepository(IServiceFactory factory, string name)
        {
            return EFRepositoryFactory.Create(factory.GetInstance<EFConfig>())
                .WithInterceptor(ctx => new LogInterceptor(name, ctx));
        }

    }
}
