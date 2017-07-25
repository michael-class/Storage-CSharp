using System;
using System.IO;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Service
{
    public interface IDocumentStorage
    {
        //TODO: move to IAppService
        Account CreateAccount(Account account);
        Container CreateContainer(Account account, string docType);

        Document Find(Guid docId);
        Document Create(string appKey, string name, string documentType, string ownerId, DateTime? documentDate);
        Document Upload(Guid docId, Stream inputStream);
        Document Download(Guid docId);
    }
}