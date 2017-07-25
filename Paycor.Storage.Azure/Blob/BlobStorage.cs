using System;
using System.IO;
using System.Threading.Tasks;
using Common.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Paycor.Neo.Common.Base;
using Paycor.Neo.Common.Extensions;
using Paycor.Neo.Crypto.Provider;
using Paycor.Neo.Logging.Format;
using Paycor.Storage.Azure.Extensions;
using Paycor.Storage.Azure.Utils;
using Paycor.Storage.Data.Storage;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Azure.Blob
{
    public class BlobStorage : IBlockStorage
    {
        private static readonly ILog _logger = LogManager.GetLogger<BlobStorage>();

        private readonly CloudStorageAccount _cloudAccount;
        private readonly IStreamCryptoProvider _crypto;

        public string Account { get; private set; }

        //TODO: remove account from interface and ctor
        public BlobStorage(string account, IStreamCryptoProvider crypto)
        {
            Check.NotNullOrEmpty(account, "account");
            Check.NotNull(crypto, "crypto");

            Account = account;
            _crypto = crypto;
            //TODO: retrieve from a Account cache
            _cloudAccount = StorageAccountUtils.CreateStorageAccount(account);
            _logger.Info(m=>m("{0}", LogEventFormatter.Format("BlobStorage").AddEvent("ctor").Add("account", account)));
        }

        public Task<Document> UploadAsync(Document document, Stream inputStream)
        {
            _logger.Trace(m => m("UploadAsync() : {0}", document));

            Check.NotNull(document, "document").ValidateUpload();
            Check.NotNull(inputStream, "inputStream");

            var container = GetContainer(document);
            container.CreateIfNotExists();

            //TODO: add OperationContext for timer logging
            //TODO: add Request options
            var blob = container.GetBlockBlobReference(document.DocumentPath);
            var length = blob.UploadFromStreamEncrypted(_crypto, inputStream);
            // refresh blob properties after upload
            blob.FetchAttributes();

            var props = blob.Properties;
            var lastModified = props.LastModified.HasValue ? props.LastModified.Value.LocalDateTime : (DateTime?)null;
            var info = new BlobInfo(blob.Uri.ToString(), props.ContentType, props.Length, lastModified, props.ETag,
                props.ContentMD5, isEncrypted: true);
            document.OnUpload(inputStream, info, length);

            return Task.FromResult(document);
        }

        public Task<Document> DownloadAsync(Document document)
        {
            _logger.Trace(m => m("DownloadAsync() : {0}", document));

            Check.NotNull(document, "document").ValidateDownload();

            var blob = CloudBlockBlob(document, true);

            var stream = blob.OpenReadEncrypted(_crypto);

            var props = blob.Properties;
            var lastModified = props.LastModified.HasValue ? props.LastModified.Value.LocalDateTime : (DateTime?)null;
            var info = new BlobInfo(blob.Uri.ToString(), props.ContentType, props.Length, lastModified, props.ETag,
                props.ContentMD5, isEncrypted: true);
            document.OnDownload(stream, info);

            return Task.FromResult(document);
        }

        public Task<Document> DeleteAsync(Document document)
        {
            _logger.Trace("DeleteAsync()");

            var blob = CloudBlockBlob(document, true);
            blob.DeleteIfExists();
            document.OnDelete();

            return Task.FromResult(document);
        }

        private CloudBlockBlob CloudBlockBlob(Document document, bool fetchAttributes = false)
        {
            var container = GetContainer(document);

            //TODO: add OperationContext for timer logging
            //TODO: add Request options
            var blob = container.GetBlockBlobReference(document.DocumentPath);
            if (fetchAttributes) blob.FetchAttributes();
            return blob;
        }

        private CloudBlobContainer GetContainer(Document document)
        {
            var client = _cloudAccount.CreateCloudBlobClient();
            var container = client.GetContainerReference(document.Container.Name.ToLower());
            return container;
        }

        public override string ToString()
        {
            return this.ToStringHelper()
                .Add("ProviderAccount", Account)
                .ToString();
        }
    }
}
