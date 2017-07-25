using System;
using System.IO;
using System.Threading;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Neo.Common.Extensions;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Domain.Tests.Entity
{
    [TestClass]
    public class DocumentTest
    {
        private static readonly ILog _logger = LogManager.GetLogger<DocumentTest>();

        private const string NAME = "name";
        private const string TYPE = "type";
        private const string OWNER_ID = "ownerid";
        private readonly DateTime _date = DateTime.Parse("01/01/2014");
        private Container _container;

        [TestInitialize]
        public void Setup()
        {
            _container = new Container();
        }

        [TestMethod]
        public void TestDocument()
        {
            var doc = CreateDocument();
            _logger.Trace(m=>m("{0}", doc));

            Assert.AreEqual(NAME, doc.Name);
            Assert.AreEqual(TYPE, doc.DocumentType);
            Assert.AreEqual(OWNER_ID, doc.OwnerId);
            Assert.AreEqual(_container.Id, doc.Container.Id);
            Assert.AreEqual(_date.Date, doc.DocumentDate);
            Assert.AreEqual(0, doc.DownloadCount);
            Assert.AreEqual(0, doc.UploadCount);
            Assert.AreEqual(-1, doc.Size);
            //Assert.IsNull(doc.BlobInfo);  //EF requires complex type to be non null
        }

        [TestMethod]
        public void TestOnUpload()
        {
            var doc = CreateDocument();
            var count = doc.UploadCount;
            var statusTime = doc.StatusDateTime;
            Thread.Sleep(10); // to simulate latency

            var blob = new BlobInfo();
            var bytes = "Now is the time for all good men...".ToBytes();
            var stream = new MemoryStream(bytes);

            _logger.Trace(m => m("Before={0}", doc));
            doc.OnUpload(stream, blob, stream.Length);

            Assert.AreEqual(count + 1, doc.UploadCount);
            Assert.AreEqual(bytes.Length, doc.Size);
            Assert.AreEqual(blob, doc.BlobInfo);
            Assert.AreEqual(DocumentStatus.Committed, doc.Status);
            Assert.IsTrue(doc.StatusDateTime > statusTime);
        }

        [TestMethod]
        public void TestOnDownload()
        {
            var doc = CreateDocument();
            var count = doc.DownloadCount;
            Thread.Sleep(10); // to simulate latency

            var blob = UploadDocument(doc);
            doc.OnDownload(new MemoryStream(), blob);

            Assert.AreEqual(count + 1, doc.DownloadCount);
            Assert.AreEqual(DocumentStatus.Committed, doc.Status);
        }

        [TestMethod]
        public void TestOnPendingDelete()
        {
            var doc = CreateDocument();
            var statusTime = doc.StatusDateTime;
            Thread.Sleep(10); // to simulate latency

            UploadDocument(doc);
            _logger.Trace(m => m("Before={0}", doc));
            doc.OnPendingDelete();

            Assert.AreEqual(DocumentStatus.PendingDelete, doc.Status);
            Assert.IsTrue(doc.StatusDateTime > statusTime);
        }

        [TestMethod]
        public void TestBuildDocumentPath()
        {
            var doc = CreateDocument();
            Assert.AreEqual(string.Format("2014/01/01/ownerid/{0}", doc.Id), doc.DocumentPath);
        }

        private BlobInfo UploadDocument(Document doc)
        {
            var blob = new BlobInfo();
            var bytes = "Now is the time for all good men...".ToBytes();
            var stream = new MemoryStream(bytes);
            doc.OnUpload(stream, blob, stream.Length);
            return blob;
        }

        private Document CreateDocument()
        {
            return new Document(_container, NAME, TYPE, OWNER_ID, _date);
        }
    }
}
