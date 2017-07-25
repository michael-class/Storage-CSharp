using System;
using Common.Logging;
using Paycor.Neo.Data.Yarn.EF.Repositories;
using Paycor.Storage.Data.Repository;
using Paycor.Storage.Domain.Entity;
using Yarn;

namespace Paycor.Storage.Data.EF.Repository
{
    public class DocumentRepository : CrudRepository<Document, Guid>, IDocumentRepository
    {
        private static readonly ILog _logger = LogManager.GetLogger<DocumentRepository>();

        public DocumentRepository(IRepository @delegate) : base(@delegate)
        {
            _logger.Trace("DocumentRepository.ctor()");
        }

        public override Document FindBy(Guid id)
        {
            return base.FindBy(doc => doc.Id == id, service => service.Include(d => d.Container));
        }
    }
}
