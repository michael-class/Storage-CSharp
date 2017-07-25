using System;
using System.Collections.Generic;
using Paycor.Neo.Data.Repositories;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.Repository
{
    public interface IAccountRepository : ICrudRepository<Account, Guid>
    {
        ICollection<Account> FindAll();
    }
}
