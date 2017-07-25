using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.EF.Mapping
{
    public class ContainerMap : EntityTypeConfiguration<Container>
    {
        public ContainerMap()
        {
            ToTable("Container");

            Ignore(t => t.StorageAccount);
            Ignore(t => t.StorageProvider);

            // primary key
            HasKey(t => t.Id);

            Property(t => t.Id).HasColumnName("ContainerId").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.Name).HasMaxLength(100).IsRequired();
        }
    }
}
