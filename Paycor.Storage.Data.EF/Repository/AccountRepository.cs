using System;
using System.Collections.Generic;
using Common.Logging;
using Paycor.Neo.Data.Yarn.EF.Repositories;
using Paycor.Storage.Data.Repository;
using Paycor.Storage.Domain.Entity;
using Yarn;
using Yarn.Specification;

namespace Paycor.Storage.Data.EF.Repository
{
    public class AccountRepository : CrudRepository<Account, Guid>, IAccountRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger<AccountRepository>();

        public AccountRepository(IRepository @delegate)
            : base(@delegate)
        {
            _logger.Trace("AccountRepository.ctor()");
        }

        public override Account FindBy(Guid id)
        {
            return base.FindBy(a => a.Id == id, service => service.Include(app => app.Containers));
        }

        public ICollection<Account> FindAll()
        {
            return base.FindAll(new Specification<Account>(app => true), 
                service => service.Include(app => app.Containers), 
                orderBy: app => app.Name);
        }

    }
}
