using System;
using System.Data.Entity;
using System.Linq;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Storage.Data.EF.Context;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.EF.Repository
{
    [TestClass]
    public class AppRepositoryTest : CrudRepositoryTest<StorageContext, AccountRepository, Account, Guid>
    {
        private static readonly ILog _logger = LogManager.GetLogger<AppRepositoryTest>();

        private const string TEST = "Test";

        [TestInitialize]
        public void Initialize()
        {
            Setup(true);
        }

        [TestCleanup]
        public void Cleanup()
        {
            TearDown();
        }

        [TestMethod]
        [TestCategory("Database")]
        public void TestAdd()
        {
            _logger.Trace("TestAdd()");

            var app = CreateAndSaveApp();
            _logger.Trace(m => m("saved={0}", app));

            var app2 = Context.Apps.First(a => a.Id == app.Id);
            _logger.Trace(m => m("retrieved={0}", app2));
            Assert.IsNotNull(app2);
            Assert.AreEqual(app.Id, app2.Id);
            Assert.AreEqual(app.Name, app2.Name);
            Assert.AreEqual(app.ProviderAccount, app2.ProviderAccount);
            Assert.AreEqual(app.AppKey, app2.AppKey);
            Assert.AreEqual(app.Provider, app2.Provider);
        }

        [TestMethod]
        [TestCategory("Database")]
        public void TestAddContainer()
        {
            _logger.Trace("TestAddContainer()");

            var app = new Account(TEST, TEST, TEST, "Azure");
            var first = app.CreateContainer(TEST);
            app = CreateAndSaveApp(app);

            var container = Context.Containers
                .Include(c => c.Account)
                .First(c => c.Id == first.Id);
            _logger.Trace(m => m("{0}", container));
            Assert.IsNotNull(container);
            Assert.AreEqual(TEST.ToLower(), container.Name);
            Assert.AreEqual(app.Id, container.AccountId);
        }

        [TestMethod]
        [TestCategory("Database")]
        public void TestFindAll()
        {
            _logger.Trace("TestFindAll()");

            const int APP_COUNT = 3;
            const int CONTAINER_COUNT = 5;

            for (var i = 0; i < APP_COUNT; i++)
            {
                var name = string.Format("{0}.{1}", "Account", i+1);
                var newApp = new Account(name, name, name, "Azure");
                newApp = CreateAppWithContainers(newApp, CONTAINER_COUNT);
                _logger.Trace(m => m("{0}", newApp));
            }

            var apps = Repo.FindAll();
            Assert.IsNotNull(apps);
            //Assert.IsTrue(apps.Count == APP_COUNT); // only works if new database

            var app = apps.First();
            _logger.Trace(m => m("{0}", app));
            Assert.IsNotNull(app.Containers);
            //Assert.IsTrue(app.Containers.Count == CONTAINER_COUNT); // only works if new database
        }

        [TestMethod]
        [TestCategory("Database")]
        public void TestDelete()
        {
            _logger.Trace("TestDelete()");

            var app = CreateAndSaveApp();
            Repo.Delete(app);
            UoW.SaveChanges();

            Assert.IsNull(Context.Apps.FirstOrDefault(a => a.Id == app.Id));
        }

        private Account CreateAndSaveApp(Account account)
        {
            return SaveEntity(e => e.Id, account);
        }

        private Account CreateAndSaveApp()
        {
            return CreateAndSaveApp(new Account(TEST, TEST, TEST, "Azure"));
        }

        private Account CreateAppWithContainers(Account account, int containerCount)
        {
            for (var i = 0; i < containerCount; i++)
            {
                account.CreateContainer(string.Format("{0}-{1}.{2}", account.Name, TEST, i + 1));
            }
            return CreateAndSaveApp(account);
        }

    }
}
