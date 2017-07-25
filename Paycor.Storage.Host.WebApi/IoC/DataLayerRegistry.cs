using System;
using System.Configuration;
using System.Web.Hosting;
using Common.Logging;
using LightInject;
using Paycor.Neo.Crypto.Provider;
using Paycor.Neo.Crypto.Storage;
using Paycor.Neo.Data.Repositories;
using Paycor.Neo.Data.Yarn.EF.Logging;
using Paycor.Neo.Data.Yarn.EF.Repositories;
using Paycor.Storage.Azure.Blob;
using Paycor.Storage.Data.EF.Context;
using Paycor.Storage.Data.EF.Repository;
using Paycor.Storage.Data.Repository;
using Paycor.Storage.Data.Storage;
using Yarn.Extensions;
using IRepository = Paycor.Neo.Data.Repositories.IRepository;

namespace Paycor.Storage.Host.WebApi.IoC
{
    public class DataLayerRegistry : BaseCompositeRoot
    {
        private static readonly ILog _logger = LogManager.GetLogger<DataLayerRegistry>();

        public const string DB_CONNECTION = "name=DocStorage";

        public override void Compose(IServiceRegistry registry)
        {
            _logger.Trace("Compose()");

            // EF Configuration
            var efConfig = new EFConfig(DB_CONNECTION) { EnableLogging = true, AutoDetectChangesEnabled = false };
            registry.Register(factory => efConfig);

            const string ACCOUNT = "AccountRepository";
            const string DOCUMENT = "DocumentRepository";

            // Delegate Yarn Repositories
            registry.Register(factory => CreateYarnRepository(factory, ACCOUNT), ACCOUNT, PerRequestLifetimeIfHosted());
            registry.Register(factory => CreateYarnRepository(factory, DOCUMENT), DOCUMENT, PerRequestLifetimeIfHosted());

            // Data Contract Repositories
            registry.Register<IAccountRepository>(factory => new AccountRepository(factory.GetInstance<Yarn.IRepository>(ACCOUNT)), PerRequestLifetimeIfHosted());
            registry.Register<IDocumentRepository>(factory => new DocumentRepository(factory.GetInstance<Yarn.IRepository>(DOCUMENT)), PerRequestLifetimeIfHosted());

            // EF DbContext
            registry.Register(factory => new StorageContext(factory.GetInstance<EFConfig>()), PerRequestLifetimeIfHosted());

            // Unit of Work
            registry.Register<IUnitOfWork>(factory => new EFUnitOfWork(
                factory.GetInstance<StorageContext>(),
                factory.GetInstance<EFConfig>(),
                new IRepository[]
                {
                    factory.GetInstance<IAccountRepository>(),
                    factory.GetInstance<IDocumentRepository>()
                }), PerRequestLifetimeIfHosted());

            // Blob Storage objects
            var keyFile = GetCryptoKeyFilePath();
            registry.Register<IBlockStorage>(
                factory =>
                    new BlobStorage("default", ProviderFactory.CreateProviderFromKeyFile(keyFile)),
                        new PerContainerLifetime());
        }

        private static Yarn.IRepository CreateYarnRepository(IServiceFactory factory, string name)
        {
            return EFRepositoryFactory.Create(factory.GetInstance<EFConfig>())
                .WithInterceptor(ctx => new LogInterceptor(name, ctx));
        }

        private static string GetCryptoKeyFilePath()
        {
            var appSetting = Neo.Crypto.Storage.Settings.KeyFileAppSetting;
            var keyFile = ConfigurationManager.AppSettings.Get(appSetting);
            if (!string.IsNullOrEmpty(keyFile)) return MapPath(keyFile);

            var msg =
                string.Format(
                    "Crypto key file app setting not found.  Please add the following app setting in App.config or Web.config. {0}",
                    appSetting);
            throw new ApplicationException(msg);
        }

        private static string MapPath(string keyFile)
        {
            // will return null if running in unit test, therefore, just return the file path
            var path = HostingEnvironment.MapPath(keyFile);
            return path ?? keyFile;
        }
    }
}