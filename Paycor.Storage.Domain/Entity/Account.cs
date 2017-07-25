using System;
using System.Collections.Generic;
using Common.Logging;
using Paycor.Neo.Common.Base;
using Paycor.Neo.Common.Extensions;
using Paycor.Neo.Logging.Format;
using Paycor.Neo.Domain.Entity;

namespace Paycor.Storage.Domain.Entity
{
    public class Account : Entity<Guid>
    {
        private static readonly ILog _logger = LogManager.GetLogger<Account>();

        public string AppKey { get; private set; }
        public string Name { get; private set; }
        public string Provider { get; private set; }
        public string ProviderAccount { get; private set; }
        public ICollection<Container> Containers { get; private set; }

        private Account() : base(Guid.NewGuid())
        {
            Containers = new List<Container>();
        }

        public Account(string appKey, string name, string providerAccount, string provider) : this()
        {
            Check.NotNullOrEmpty(appKey, "appKey");
            Check.NotNullOrEmpty(name, "name");
            Check.NotNullOrEmpty(providerAccount, "ProviderAccount");
            Check.NotNullOrEmpty(provider, "provider");

            AppKey = appKey;
            Name = name;
            ProviderAccount = providerAccount;
            Provider = provider;
        }

        public Container CreateContainer(string name)
        {
            var container = new Container(this, name);
            Containers.Add(container);
            _logger.Debug(m => m("{0}", 
                LogEventFormatter.Format("Account").AddEvent("CreateContainer").Add("container", container)));
            return container;
        }

        public override string ToString()
        {
            return this.ToStringHelper()
                .Add("Id", Id)
                .Add("AppKey", AppKey)
                .Add("Name", Name)
                .Add("ProviderAccount", ProviderAccount)
                .Add("Provider", Provider)
                .Add("Containers", Containers)
                .OmitNullValues()
                .ToString();
        }
    }
}