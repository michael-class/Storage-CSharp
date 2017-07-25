using System;
using Paycor.Neo.Common.Base;
using Paycor.Neo.Common.Extensions;

namespace Paycor.Storage.Domain.Entity
{
    public class BlobInfo
    {
        public string Url { get; private set; }
        public string ContentType { get; private set; }
        public string ETag { get; private set; }
        public string MD5 { get; private set; }
        public DateTime? LastModified { get; private set; }
        public long BlobSize { get; private set; }
        public bool IsEncrypted { get; private set; }
        public bool IsCompressed { get; private set; }

        internal BlobInfo()
        {
            // for EF
            BlobSize = -1;
        }

        public BlobInfo(string url, string contentType, long blobSize = -1, DateTime? lastModified = null, 
            string eTag = null, string md5 = null, bool isCompressed = false, bool isEncrypted = false) 
            : this()
        {
            Url = url;
            ContentType = contentType;
            BlobSize = blobSize;
            LastModified = lastModified;
            ETag = eTag;
            MD5 = md5;
            IsEncrypted = isEncrypted;
            IsCompressed = isCompressed;
        }

        protected bool Equals(BlobInfo other)
        {
            return string.Equals(Url, other.Url) && string.Equals(ContentType, other.ContentType) 
                && string.Equals(ETag, other.ETag) && string.Equals(MD5, other.MD5) 
                && LastModified.Equals(other.LastModified) && BlobSize == other.BlobSize 
                && IsEncrypted.Equals(other.IsEncrypted) && IsCompressed.Equals(other.IsCompressed);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((BlobInfo) obj);
        }

        public override int GetHashCode()
        {
            return Objects.GetHashCode(Url, ContentType, LastModified, ETag, BlobSize, MD5, IsCompressed, IsEncrypted);
        }

        public override string ToString()
        {
            return this.ToStringHelper()
                .Add("Url", Url)
                .Add("ContentType", ContentType)
                .Add("ETag", ETag)
                .Add("MD5", MD5)
                .Add("LastModified", LastModified)
                .Add("BlobSize", BlobSize)
                .Add("IsEncrypted", IsEncrypted)
                .Add("IsCompressed", IsCompressed)
                .OmitNullValues()
                .ToString();
        }
    }
}
