using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.EF.Mapping
{
    public class DocumentMap : EntityTypeConfiguration<Document>
    {
        public DocumentMap()
        {
            ToTable("Document");
            

            // primary key
            HasKey(t => t.Id);

            Ignore(t => t.StorageAccount);
            Ignore(t => t.StorageProvider);
            Ignore(t => t.ContentStream);
            Ignore(t => t.DocumentPath);
            Ignore(t => t.CanBeDownloaded);
            Ignore(t => t.CanBeUploaded);

            Property(t => t.Id).HasColumnName("DocumentId").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Name).HasMaxLength(255).IsRequired();
            Property(t => t.OwnerId).HasMaxLength(50).IsRequired();
            Property(t => t.DocumentType).HasMaxLength(100).IsRequired();
            Property(t => t.DocumentDate).HasColumnType("Date").IsRequired();
            Property(t => t.StatusDateTime).IsRequired();
            Property(t => t.Status).HasColumnType("tinyint").IsRequired();

            //Property(t => t.BlobSize).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Computed);
        }
    }
}
