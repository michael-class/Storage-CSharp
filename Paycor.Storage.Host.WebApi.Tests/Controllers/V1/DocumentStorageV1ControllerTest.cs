using System;
using System.IO;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.Http.Routing;
using Common.Logging;
using LightInject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Paycor.Neo.Logging.Context;
using Paycor.Neo.Rest.Model.V1;
using Paycor.Storage.Domain.Entity;
using Paycor.Storage.Host.WebApi.IoC;
using Paycor.Storage.Host.WebApi.Models.V1;

namespace Paycor.Storage.Host.WebApi.Controllers.V1
{
    [TestClass]
    public class DocumentStorageV1ControllerTest
    {
        private static readonly ILog _logger = LogManager.GetLogger<DocumentStorageV1ControllerTest>();

        private DocumentStorageV1Controller _controller;
        private ServiceContainer _container;

        [TestInitialize]
        public void Setup()
        {
            //rjg: work around for bizarre .net initialization issue.  test will abort without this.
            //  see: http://stackoverflow.com/questions/15693262/serialization-exception-in-net-4-5
            System.Configuration.ConfigurationManager.GetSection("dummy");

            var urlHelper = new Mock<UrlHelper>();
            urlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<object>()))
                .Returns("http://localhost/storage/api/storage/v1/documents/123");
//            urlHelper.Setup(x => x.Link(It.IsAny<string>(), It.IsAny<IDictionary<string, object>>()))
//                .Returns("http://localhost/storage/api/storage/v1/documents/123");

            var factory = LightInjectTestConfig.Register();
            _container = factory.Container;
            _controller = _container.GetInstance<DocumentStorageV1Controller>();
            Assert.IsNotNull(_controller);

            _controller.Request = new HttpRequestMessage();
            _controller.Configuration = new HttpConfiguration();
            _controller.Url = urlHelper.Object;
            CallContextManager.Register(new CallContext(Guid.NewGuid().ToString()));
        }

        [TestCleanup]
        public void Cleanup()
        {
            _container.Dispose();
        }

        [TestMethod]
        public void TestUpload()
        {
            _logger.Trace("TestUpload");

            var actionResult = _controller.Post("default", new DocumentRequest
            {
                Name = "test.jpg",
                DocumentType = "test",
                OwnerId = "rgeyer",
            });
            var contentResult = actionResult as NegotiatedContentResult<RestDataResponse<DocumentResponse>>;
            Assert.IsNotNull(contentResult);
            var doc = contentResult.Content.Data;
            _logger.Debug(m => m("{0}", contentResult.Content));

            using (var fileStream = new FileStream("Samples\\DallasFloorPlan.jpg", FileMode.Open))
            {
                var result = _controller.Upload(doc.Id, fileStream);
                var cResult = result as OkNegotiatedContentResult<RestDataResponse<DocumentResponse>>;
                Assert.IsNotNull(cResult);
                var response = cResult.Content;
                _logger.Debug(m => m("{0}", response));
                Assert.AreEqual(DocumentStatus.Committed, response.Data.Status);
            }

            var download = _controller.Download(doc.Id);
            var messageResult = download as ResponseMessageResult;
            Assert.IsNotNull(messageResult);
            var content = messageResult.Response.Content;
            using (var fileStream = new FileStream(doc.Name, FileMode.Create))
            {
                content.CopyToAsync(fileStream).GetAwaiter().GetResult();
            }
            Assert.IsTrue(File.Exists(doc.Name));
        }

    }
}
