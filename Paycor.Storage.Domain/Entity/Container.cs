using System;
using Common.Logging;
using Paycor.Neo.Common.Base;
using Paycor.Neo.Common.Extensions;
using Paycor.Neo.Logging.Format;
using Paycor.Neo.Domain.Entity;

namespace Paycor.Storage.Domain.Entity
{
    public class Container : Entity<Guid>
    {
        private static readonly ILog _logger = LogManager.GetLogger<Container>();

        public Account Account { get; private set; }
        public Guid AccountId { get; private set; }
        public String Name { get; private set; }

        // non-persistent
        public string StorageAccount
        {
            get { return Account == null ? null : Account.Name; }
        }

        public string StorageProvider
        {
            get { return Account == null ? null : Account.Provider; }
        }

        internal Container() : base(Guid.NewGuid()) { }

        internal Container(Account account, string name) : this()
        {
            Check.NotNull(account, "Account");
            Check.NotNullOrEmpty(name, "name");

            Account = account;
            AccountId = account.Id;
            Name = name.ToLower();
        }

        public Document CreateDocument(string name, string documentType, string ownerId, DateTime? documentDate = null)
        {
            var doc = new Document(this, name, documentType, ownerId, documentDate);
            _logger.Debug(m => m("{0}", LogEventFormatter.Format("Account").AddEvent("CreateDocument").Add("doc", doc)));
            return doc;
        }

        public override string ToString()
        {
            return this.ToStringHelper()
                .Add("Id", Id)
                .Add("AccountId", AccountId)
                .Add("Name", Name)
                .ToString();
        }

    }
}
