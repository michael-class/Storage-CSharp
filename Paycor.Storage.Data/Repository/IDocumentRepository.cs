using System;
using Paycor.Neo.Data.Repositories;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Data.Repository
{
    public interface IDocumentRepository : ICrudRepository<Document, Guid>
    {
    }
}
