using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Hosting;
using System.Web.Http;
using Common.Logging;
using FluentValidation;
using Paycor.Neo.Common.Base;
using Paycor.Neo.Logging.Format;
using Paycor.Neo.Rest.Model.V1;
using Paycor.Neo.Rest.WebApi;
using Paycor.Storage.Host.WebApi.Models.V1;
using Paycor.Storage.Host.WebApi.Settings;
using Paycor.Storage.Service;

namespace Paycor.Storage.Host.WebApi.Controllers.V1
{
    //[WebApiAuthenticationFilter]
    //[Authorize]
    [RoutePrefix("api/storage/v1")]
    public class DocumentStorageV1Controller : BaseApiController
    {
        private static readonly ILog _logger = LogManager.GetLogger<DocumentStorageV1Controller>();

        private const string SOURCE = "DocStorageApi";

        private readonly IDocumentStorage _documentStorage;
        private readonly DocumentRequestValidator _validator;
        private readonly string _uploadFolder;

        public DocumentStorageV1Controller(IDocumentStorage documentStorage, DocumentRequestValidator validator, IDocumentStorageSettings settings)
        {
            Check.NotNull(documentStorage, "documentStorage");
            Check.NotNull(validator, "validator");
            Check.NotNull(settings, "settings");

            _documentStorage = documentStorage;
            _validator = validator;
            _uploadFolder = settings.UploadFolder;

            _logger.Debug(m => m("{0}", 
                LogEventFormatter.Format("DocumentStorageV1Controller").AddEvent(".ctor").Add("uploadFolder", _uploadFolder)));
        }

        [HttpGet]
        [Route("ping")]
        public IHttpActionResult Ping()
        {
            _logger.Trace("Ping()");

            return Ok();
        }

#if DEBUG
        [HttpGet]
        [Route("error")]
        public IHttpActionResult Error()
        {
            _logger.Trace("Error()");

            throw new InvalidOperationException("Test error endpoint");
        }
#endif

        [Route(@"documents/{docId}", Name = RoutesV1.DOCUMENT_GET_ROUTE)]
        public IHttpActionResult Get(string docId)
        {
            _logger.Trace(m => m("Get(): docId={0}", docId));

            var docGuid = ValidateDocId(docId);

            var result = DocumentResponse.From(_documentStorage.Find(docGuid), Url);
            if (result != null)
            {
                return Ok(result);
            }
            return NotFound(RestError.IdNotFoundError("Document", docId));
        }

        /// <summary>
        /// Creates a new document record using the supplied appkey
        /// </summary>
        /// <param name="appKey">Application key used to lookup storage account</param>
        /// <param name="doc">document information</param>
        /// <returns>the new created document information, <see cref="DocumentResponse"/> wrapped in a <see cref="RestResponse"/></returns>
        [Route(@"app/{appKey}", Name = RoutesV1.DOCUMENT_POST_ROUTE)]
        public IHttpActionResult Post(string appKey, [FromBody]DocumentRequest doc)
        {
            _logger.Trace(m => m("Put(): appKey={0}; doc={1}", appKey, doc));

            ValidateDocumentRequest(doc);

            var newDoc = _documentStorage.Create(appKey, doc.Name, doc.DocumentType, doc.OwnerId, doc.DocumentDate);
            return Created(DocumentResponse.From(newDoc, Url));

        }

        /// <summary>
        /// Uploads the specified document using MultiPart request
        /// </summary>
        /// <param name="docId">Id for the uploaded document</param>
        /// <returns>the updated document information, <see cref="DocumentResponse"/> wrapped in a <see cref="RestResponse"/></returns>
        [HttpPost]
        [Route(@"documents/{docId}/content", Name = RoutesV1.DOCUMENT_UPLOAD_ROUTE)]
        public async Task<IHttpActionResult> Upload(string docId)
        {
            _logger.Trace(m => m("Upload(): docId={0}", docId));

            var docGuid = ValidateDocId(docId);
            var doc = _documentStorage.Find(docGuid);
            if (doc == null)
            {
                return NotFound(RestError.IdNotFoundError("Document", docId));
            }

            var content = Request.Content;
            if (!content.IsMimeMultipartContent())
            {
                //TODO: validate request type
                return Upload(docId, content.ReadAsStreamAsync().Result);
            }

            // process multipart request
            var path = HostingEnvironment.MapPath(_uploadFolder);
            var streamProvider = new MultipartFormDataStreamProvider(path);
            await Request.Content.ReadAsMultipartAsync(streamProvider).ContinueWith(t =>
            {   
                if (t.IsFaulted || t.IsCanceled)
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);

                // accept only first file
                var fileInfo = streamProvider.FileData.First(i =>
                {
                    var info = new FileInfo(i.LocalFileName);
                    try
                    {
                        var fileStream = new FileStream(info.FullName, FileMode.Open);
                        doc = _documentStorage.Upload(docGuid, fileStream);
                        return true;
                    }
                    finally
                    {
                        _logger.Trace(m => m("{0}", LogEventFormatter.Format(SOURCE).AddEvent("deleteMultiPartFile").Add("file", info.FullName)));
                        try
                        {
                            info.Refresh();
                            info.Delete();
                        }
                        catch (Exception e)
                        {
                            // log and swallow
                            _logger.Warn(m => m("{0}", LogEventFormatter.FormatError(SOURCE, e).AddEvent("deleteMultiPartFile").Add("file", info)));
                        }
                    }
                });
                _logger.Trace(m => m("{0}", LogEventFormatter.Format(SOURCE).AddEvent("MultiPartFile").Add("fileInfo", fileInfo)));
            });
            return Ok(DocumentResponse.From(doc, Url));

        }

        /// <summary>
        /// Uploads the specified document as a simple stream
        /// </summary>
        /// <param name="docId">Id for the uploaded document</param>
        /// <param name="inputStream">content stream</param>
        /// <returns>the updated document information, <see cref="DocumentResponse"/> wrapped in a <see cref="RestResponse"/></returns>
        public IHttpActionResult Upload(string docId, Stream inputStream)
        {
            var docGuid = ValidateDocId(docId);
            var doc = _documentStorage.Find(docGuid);
            if (doc == null)
            {
                return NotFound(RestError.IdNotFoundError("Document", docId));
            }

            doc = _documentStorage.Upload(docGuid, inputStream);
            return Ok(DocumentResponse.From(doc, Url));
        }

        /// <summary>
        /// Download the specified document
        /// </summary>
        /// <param name="docId">Id of the document to download</param>
        /// <returns>the document content stream</returns>
        [HttpGet]
        [Route(@"documents/{docId}/content", Name = RoutesV1.DOCUMENT_DOWNLOAD_ROUTE)]
        public IHttpActionResult Download(string docId)
        {
            _logger.Trace(m => m("Download(): docId={0}", docId));

            var docGuid = ValidateDocId(docId);
            var doc = _documentStorage.Find(docGuid);
            if (doc == null)
            {
                return NotFound(RestError.IdNotFoundError("Document", docId));
            }

            doc = _documentStorage.Download(docGuid);
            var message = Request.CreateResponse(HttpStatusCode.OK);
            var size = doc.Size <= 0 ? new long?() : doc.Size;
            message.Content = new StreamContent(doc.ContentStream);
            message.Content.Headers.ContentLength = size;
            message.Content.Headers.ContentType = new MediaTypeHeaderValue(doc.BlobInfo.ContentType);
            message.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            {
                FileName = doc.Name,
                Size = size
            };
            return ResponseMessage(message);
        }

        /// <summary>
        /// Marks the specified document for deletion
        /// </summary>
        /// <param name="docId">Id of the document to download</param>
        /// <returns>the updated document information, <see cref="DocumentResponse"/> wrapped in a <see cref="RestResponse"/></returns>
        public IHttpActionResult Delete(string docId)
        {
            return null;
        }

        private Guid ValidateDocId(string id)
        {
            Guid guid;
            if (Guid.TryParse(id, out guid)) return guid;

            var result = _validator.HandleInvalidDocId(id);
            throw new ValidationException(result.Errors);
        }

        private void ValidateDocumentRequest(DocumentRequest doc)
        {
            _validator.ValidateAndThrow(doc);
        }

    }
}