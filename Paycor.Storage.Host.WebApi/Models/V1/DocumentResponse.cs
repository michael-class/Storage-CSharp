using System.Web.Http.Routing;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Paycor.Neo.Common.Extensions;
using Paycor.Neo.Rest.WebApi;
using Paycor.Storage.Domain.Entity;
using Paycor.Storage.Host.WebApi.Controllers.V1;

namespace Paycor.Storage.Host.WebApi.Models.V1
{
    public class DocumentResponse : DocumentRequest
    {
        [JsonProperty(Order = 1)] 
        public string Url { get; set; }
        [JsonProperty(Order = 2)]
        public string UploadUrl { get; set; }
        [JsonProperty(Order = 3)]
        public string DownloadUrl { get; set; }
        [JsonProperty(Order = 4)]
        public string Id { get; set; }


        [JsonProperty(Order = 21)]
        public string StorageAccount { get; set; }
        [JsonProperty(Order = 22)]
        public string StorageProvider { get; set; }
        [JsonProperty(Order = 23)]
        [JsonConverter(typeof(StringEnumConverter))]
        public DocumentStatus Status { get; set; }
        [JsonProperty(Order = 24)]
        public string ContentType { get; set; }
        [JsonProperty(Order = 25)]
        public long Size { get; set; }
        [JsonProperty(Order = 26)]
        public long BlobSize { get; set; }
        [JsonProperty(Order = 27)]
        public int UploadCount { get; set; }
        [JsonProperty(Order = 28)]
        public int DownloadCount { get; set; }

        public static DocumentResponse From(Document doc, UrlHelper urlHelper)
        {
            if (doc == null) return null;

            var docId = JsonUtils.FormatGuid(doc.Id);
            return new DocumentResponse
            {
                Id = docId,
                Name = doc.Name,
                DocumentType = doc.DocumentType,
                DocumentDate = doc.DocumentDate,
                OwnerId = doc.OwnerId,
                StorageAccount = doc.StorageAccount,
                StorageProvider = doc.StorageProvider,
                Status = doc.Status,
                ContentType = doc.BlobInfo.ContentType,
                Size = doc.Size,
                BlobSize = doc.BlobInfo.BlobSize,
                DownloadCount = doc.DownloadCount,
                UploadCount = doc.UploadCount,
                Url = urlHelper.Link(RoutesV1.DOCUMENT_GET_ROUTE, new { docId } ),
                UploadUrl = urlHelper.Link(RoutesV1.DOCUMENT_UPLOAD_ROUTE, new { docId }),
                DownloadUrl = urlHelper.Link(RoutesV1.DOCUMENT_DOWNLOAD_ROUTE, new { docId }),
            };
        }

        public override string ToString()
        {
            return this.ToStringHelper()
                .Add("Id", Id)
                .Add("Name", Name)
                .Add("DocumentType", DocumentType)
                .Add("DocumentDate", DocumentDate)
                .Add("OwnerId", OwnerId)
                .Add("StorageAccount", StorageAccount)
                .Add("StorageProvider", StorageProvider)
                .Add("Status", Status)
                .Add("ContentType", ContentType)
                .Add("Size", Size)
                .Add("BlobSize", BlobSize)
                .Add("DownloadCount", DownloadCount)
                .Add("UploadCount", UploadCount)
                .Add("Url", Url)
                .Add("UploadUrl", UploadUrl)
                .Add("DownloadUrl", DownloadUrl)
                .ToString();
        }

    }
}