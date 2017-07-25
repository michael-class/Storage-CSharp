using System.Data.Entity.ModelConfiguration;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.EF.Mapping
{
    public class BlobInfoMap : ComplexTypeConfiguration<BlobInfo>
    {
        public BlobInfoMap()
        {
            Ignore(t => t.MD5);
            Ignore(t => t.ETag);
            Ignore(t => t.LastModified);
            Ignore(t => t.IsCompressed);
            Ignore(t => t.IsEncrypted);

            // uri consists of: https://host/account/container/path/filename
            // host, account & container, up to 255 chars each
            // path can contain 4 segments, up to 255 chars each
            // filename up to 255 chars
            // 8*255 + 8 char protocol = 2048
            Property(t => t.Url).HasColumnName("Url").HasMaxLength(2048);
            Property(t => t.ContentType).HasColumnName("ContentType").HasMaxLength(100);
            Property(t => t.BlobSize).HasColumnName("BlobSize");
        }
    }
}
