using System.IO;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Paycor.Neo.Crypto.Provider;
using Paycor.Storage.Azure.Utils;

namespace Paycor.Storage.Azure.Extensions
{
    /// <summary>
    /// Adapted from https://github.com/stefangordon/azure-encryption-extensions
    /// </summary>
    /// TODO: add async methods
    public static class BlobExtensions
    {
        public static long UploadFromFileEncrypted(this ICloudBlob blob, IStreamCryptoProvider provider, string path, FileMode mode,
            AccessCondition accessCondition = null, BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            long length;

            using (var fileStream = new FileStream(path, mode))
            using (var encryptedStream = provider.EncryptedStream(fileStream))
            {
                length = fileStream.Length;
                blob.UploadFromStream(encryptedStream, accessCondition, options, operationContext);
            }
            return length;
        }

        public static long UploadFromStreamEncrypted(this ICloudBlob blob, IStreamCryptoProvider provider, Stream stream,
            AccessCondition accessCondition = null, BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            var countingStream = new DelegatingStream(stream);
            using (var encryptedStream = provider.EncryptedStream(countingStream))
            {
                blob.UploadFromStream(encryptedStream, accessCondition, options, operationContext);
            }
            return countingStream.BytesRead;
        }

        public static long DownloadToFileEncrypted(this ICloudBlob blob, IStreamCryptoProvider provider, string path, FileMode mode,
            AccessCondition accessCondition = null, BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            long length;
            using (var fileStream = new FileStream(path, mode))
            {
                blob.DownloadToStreamEncrypted(provider, fileStream, accessCondition, options, operationContext);
                length = fileStream.Length;
            }
            return length;
        }

        public static long DownloadToStreamEncrypted(this ICloudBlob blob, IStreamCryptoProvider provider, Stream stream,
            AccessCondition accessCondition = null, BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            long length;
            using (var blobStream = blob.OpenRead(accessCondition, options, operationContext))
            using (var decryptedStream = provider.DecryptedStream(blobStream))
            {
                decryptedStream.CopyTo(stream);
                length = decryptedStream.Length;
            }
            return length;
        }

        public static Stream OpenReadEncrypted(this ICloudBlob blob, IStreamCryptoProvider provider, 
            AccessCondition accessCondition = null, BlobRequestOptions options = null,
            OperationContext operationContext = null)
        {
            var blobStream = blob.OpenRead(accessCondition, options, operationContext);
            return provider.DecryptedStream(blobStream);
        }
    }
}
