using System;
using Paycor.Storage.Domain.Entity;
using Yarn.Specification;

namespace Paycor.Storage.Data.Query
{
    public class DocumentQuery
    {
        public Specification<Document> MatchesPendingDelete()
        {
            return new Specification<Document>(doc => doc.Status == DocumentStatus.PendingDelete);
        }

        public Specification<Document> MatchesContainer(Guid containerId)
        {
            return new Specification<Document>(doc => doc.ContainerId == containerId);
        }
    }
}
