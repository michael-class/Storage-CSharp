using System.IO;
using Common.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Neo.Crypto.Provider;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Azure.Blob
{
    [TestClass]
    public class BlobStorageIntegrationTest
    {
        private static readonly ILog _logger = LogManager.GetLogger<BlobStorageIntegrationTest>();

        private const string DEFAULT = "default";
        //private const string TEST_FILE = ".\\samples\\test.txt";
        private const string TEST_FILE = ".\\samples\\DallasFloorPlan.jpg";

        private BlobStorage _storage;
        private Document _document;

        [TestInitialize]
        public void Setup()
        {
            _storage = new BlobStorage("Default", ProviderFactory.CreateProviderFromKeyFile("crypto.key"));
        }

        [TestCleanup]
        public void TearDown()
        {
            _logger.Trace(m => m("PostTest: {0}", _document));
            if (_document == null || _document.Status == DocumentStatus.Deleted) return;
            _document = _storage.DeleteAsync(_document).Result;
            _logger.Trace(m => m("Post Delete: {0}", _document));
        }

        [TestMethod]
        public void TestUploadDownload()
        {
            _logger.Trace("TestUploadDownload()");

            var app = new Account(DEFAULT, DEFAULT, DEFAULT, "Azure");
            var container = app.CreateContainer(DEFAULT);

            int downloadCount;
            var file = GetUploadFile(TEST_FILE);
            using (var fileStream = new FileStream(file.FullName, FileMode.Open, FileAccess.Read))
            {
                var doc = container.CreateDocument(file.Name, file.Extension, "rgeyer", file.LastWriteTime);
                var task = _storage.UploadAsync(doc, fileStream);
                _document = task.Result;
                downloadCount = _document.DownloadCount;
                _logger.Trace(m => m("{0}", _document));
            }

            using (var fileStream = new FileStream(file.Name, FileMode.Create))
            {
                var task = _storage.DownloadAsync(_document);

                var downloadedDoc = task.Result;

                downloadedDoc.ContentStream.CopyTo(fileStream);

                var downFile = new FileInfo(file.Name);
                Assert.AreEqual(file.Length, downFile.Length);
                Assert.AreEqual(downloadCount+1, downloadedDoc.DownloadCount);
            }
            
        }

        private FileInfo GetUploadFile(string path)
        {
            var file = new FileInfo(path);
            if (!file.Exists)
            {
                Assert.Fail("Upload file does not exist");
            }
            return file;
        }
    }
}
