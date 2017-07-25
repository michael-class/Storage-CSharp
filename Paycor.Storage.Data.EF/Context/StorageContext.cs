using System.Data.Entity;
using Common.Logging;
using Paycor.Neo.Data.Yarn.EF.Repositories;
using Paycor.Neo.Logging.Format;
using Paycor.Storage.Data.EF.Mapping;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.EF.Context
{
    public class StorageContext : DbContext
    {
        private static readonly ILog _logger = LogManager.GetLogger<StorageContext>();

        public const string DB_CONNECTION = "name=DocStorage";

        public StorageContext()
            : this(DB_CONNECTION)
        {
            // for migrations
        }

        public StorageContext(string nameOrConnectionString)
            : base(nameOrConnectionString)
        {
            _logger.Trace(m => m("{0}", LogEventFormatter.Format("StorageContext")
                .AddEvent("create").Add("nameOrConnection", nameOrConnectionString)));
        }

        public StorageContext(EFConfig config) : this(config.NameOrConnectionString) { }

        public DbSet<Account> Apps { get; set; }
        public DbSet<Container> Containers { get; set; }
        public DbSet<Document> Documents { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            _logger.Trace("OnModelCreating()");

            base.OnModelCreating(modelBuilder);

            modelBuilder.Configurations
                .Add(new AccountMap())
                .Add(new ContainerMap())
                .Add(new DocumentMap())
                .Add(new BlobInfoMap());
        }

        static StorageContext()
        {
            Database.SetInitializer<StorageContext>(null);
        }
    }
}
