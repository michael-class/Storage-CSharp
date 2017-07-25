using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.EF.Mapping
{
    public class AccountMap : EntityTypeConfiguration<Account>
    {
        public AccountMap()
        {
            ToTable("Account");

            // primary key
            HasKey(t => t.Id);

            Property(t => t.Id).HasColumnName("AccountId").HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(t => t.AppKey).HasMaxLength(100).IsRequired();
            Property(t => t.Name).HasMaxLength(100).IsRequired();
            Property(t => t.ProviderAccount).HasMaxLength(255).IsRequired();
            Property(t => t.Provider).HasMaxLength(100).IsRequired();
        }
    }
}
