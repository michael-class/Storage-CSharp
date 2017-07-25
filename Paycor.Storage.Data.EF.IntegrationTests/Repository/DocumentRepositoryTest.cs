using System;
using System.Data.Entity;
using System.Linq;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Neo.Common.Extensions;
using Paycor.Storage.Data.EF.Context;
using Paycor.Storage.Data.Repository;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.EF.Repository
{
    [TestClass]
    public class DocumentRepositoryTest : CrudRepositoryTest<StorageContext, IDocumentRepository, Document, Guid>
    {
        private static readonly ILog _logger = LogManager.GetLogger<DocumentRepositoryTest>();

        private const string TEST = "Test";

        private IAccountRepository _accountRepo;
        private Guid _appId;

        [TestInitialize]
        public void Initialize()
        {
            Setup(true);
            _accountRepo = UoW.Repository<IAccountRepository>();
        }

        [TestCleanup]
        public void Cleanup()
        {
            _accountRepo.DeleteBy(_appId);
            TearDown();
        }


        [TestMethod]
        public void TestAdd()
        {
            _logger.Trace("TestAdd");

            var container = CreateContainer();
            var doc = container.CreateDocument("doc.txt", TEST, "ownerId");
            SaveEntity(e => e.Id, doc);
            _logger.Trace(m => m("savedDoc={0}", doc));

            var doc2 = Context.Documents
                .Include(d => d.Container)
                .First(d => d.Id == doc.Id);
            _logger.Trace(m => m("retrievedDoc={0}", doc2));
            Assert.IsNotNull(doc2);
            Assert.AreEqual(doc.Name, doc2.Name);
            Assert.AreEqual(doc.ContainerId, doc2.ContainerId);
            Assert.AreEqual(doc.Container, doc2.Container);
            Assert.AreEqual(doc.DocumentDate.ToDateString(), doc2.DocumentDate.ToDateString());
            Assert.AreEqual(doc.DocumentType, doc2.DocumentType);
            Assert.AreEqual(doc.DownloadCount, doc2.DownloadCount);
            Assert.AreEqual(doc.OwnerId, doc2.OwnerId);
            Assert.AreEqual(doc.Size, doc2.Size);
            Assert.AreEqual(doc.Status, doc2.Status);
            Assert.AreEqual(doc.StatusDateTime.ToString("G"), doc2.StatusDateTime.ToString("G"));
        }

        [TestMethod]
        public void TestFindBy()
        {
            _logger.Trace("TestFindBy");

            var container = CreateContainer();
            var doc = container.CreateDocument("doc.txt", TEST, "ownerId");
            SaveEntity(e => e.Id, doc);

            var doc2 = Repo.FindBy(doc.Id);
            Assert.IsNotNull(doc2);
            Assert.AreEqual(doc.Name, doc2.Name);
            Assert.AreEqual(doc.ContainerId, doc2.ContainerId);
            Assert.AreEqual(doc.Container, doc2.Container);
        }

        private Container CreateContainer()
        {
            var app = new Account(TEST, TEST, TEST, "Azure");
            var container = app.CreateContainer(TEST);
            _accountRepo.Add(app);
            UoW.SaveChanges(true);
            _logger.Trace(m => m("savedContainer={0}", container));
            _appId = app.Id;
            return container;
        }
    }
}
