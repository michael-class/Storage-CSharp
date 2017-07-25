using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Common.Logging;
using FluentValidation;
using FluentValidation.Results;
using Paycor.Neo.Common.Base;
using Paycor.Neo.Data.Repositories;
using Paycor.Neo.Logging.Format;
using Paycor.Storage.Data.Repository;
using Paycor.Storage.Data.Storage;
using Paycor.Storage.Domain.Entity;

namespace Paycor.Storage.Service
{
    public class DocumentStorage : IDocumentStorage
    {
        private static readonly ILog _logger = LogManager.GetLogger<DocumentStorage>();

        private const string SOURCE = "DocumentStorage";
        private const string DEFAULT_APP = "Default";
        private const string DEFAULT_ACCOUNT = "DefaultStorage";
        private const string DEFAULT_PROVIDER = "Azure";

        private readonly IUnitOfWork _uow;
        private readonly IAccountRepository _accountRepo;
        private readonly IDocumentRepository _docRepo;
        private readonly IBlockStorage _storage;

        private static readonly object _lock = new object();
        private static ConcurrentDictionary<string, Account> _accountCache;

        private ConcurrentDictionary<string, Account> AccountCache
        {
            get
            {
                lock (_lock)
                {
                    return _accountCache ?? (_accountCache = LoadAccountCache());
                }
            }
        }


        public DocumentStorage(IUnitOfWork uow, IBlockStorage storage)
        {
            _uow = Check.NotNull(uow, "uow");
            _accountRepo = Check.NotNull(uow.Repository<IAccountRepository>(), "IAccountRepository");
            _docRepo = Check.NotNull(uow.Repository<IDocumentRepository>(), "IDocumentRepository");
            _storage = Check.NotNull(storage, "storage");
            _logger.Debug(m => m("{0}", LogEventFormatter.Format(SOURCE).AddEvent("ctor")));
        }

        private T Execute<T>(string @event, Func<T> func, Func<LogEventFormatter, LogEventFormatter> logEvent)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                return func();
            }
            finally
            {
                _logger.Info(m => m("{0}",
                    logEvent(LogEventFormatter.Format(SOURCE).AddEvent(@event)).AddElapsedTime(timer)));
            }
        }

        public Account CreateAccount(Account account)
        {
            Check.NotNull(account, "Account");
            _logger.Trace(m => m("CreateAccount(): Account={0}", account));

            return Execute("CreateAccount", () =>
            {
                var newApp = _accountRepo.Add(account);
                _uow.SaveChanges(true);
                AccountCache.TryAdd(newApp.AppKey, newApp);
                return newApp;
            }, l => l.Add("Account", account));

        }

        public Container CreateContainer(Account account, string docType)
        {
            Check.NotNull(account, "Account");
            Check.NotNullOrEmpty(docType, "docType");
            _logger.Trace(m => m("CreateContainer(): Account={0}, docType={0}", account.AppKey, docType));

            Container container = null;
            return Execute("CreateContainer", () =>
            {
                container = account.CreateContainer(docType);
                _uow.SaveChanges(true);
                return container;
            }, l => l.Add("docType", docType).Add("container", container));

        }

        public Document Find(Guid docId)
        {
            Check.NotDefault(docId, "docId");
            _logger.Trace(m => m("Find(): docId={0}", docId));

            return Execute("Find", () => _docRepo.FindBy(docId), l => l.Add("docId", docId));
        }

        public Document Create(string appKey, string name, string documentType, string ownerId, DateTime? documentDate)
        {
            _logger.Trace(m => m("Create(): appKey={0}, name={1}, type={2}, ownerId={3}, date={4}", 
                appKey, name, documentType, ownerId, documentDate));
            Check.NotNullOrEmpty(appKey, "appKey");

            Document doc = null;
            return Execute("Create", () =>
            {
                var container = LookupContainer(appKey, documentType);

                doc = container.CreateDocument(name, documentType, ownerId, documentDate);
                _docRepo.Add(doc);
                _uow.SaveChanges(true);
                return doc;
            }, l => l.Add("appKey", appKey).Add("doc", doc));

        }

        public Document Upload(Guid docId, Stream inputStream)
        {
            Check.NotNull(inputStream, "inputStream");
            _logger.Trace(m => m("Upload(): docId={0}", docId));

            Document doc = null;
            return Execute("Upload", () =>
            {
                doc = RetrieveDocumentForTransfer(docId);
                var task = _storage.UploadAsync(doc, inputStream);
                doc = task.Result;
                _uow.SaveChanges(true);

                return doc;
            }, l => l.Add("docId", docId).Add("doc", doc));
        }

        public Document Download(Guid docId)
        {
            _logger.Trace(m => m("Download(): docId={0}", docId));

            Document doc = null;
            return Execute("Download", () =>
            {
                doc = RetrieveDocumentForTransfer(docId);
                var task = _storage.DownloadAsync(doc);
                doc = task.Result;
                _uow.SaveChanges(true);

                return doc;
            }, l => l.Add("docId", docId).Add("doc", doc));
        }

        private Document RetrieveDocumentForTransfer(Guid docId)
        {
            Check.NotDefault(docId, "docId");

            var doc = _docRepo.FindBy(docId);
            
            //TODO: check with Rick on error pattern here.
            if (doc == null)
            {
                throw new ValidationException(new[] { new ValidationFailure("docId", "Invalid document ID, document not found.") });
            }
            return doc;
        }

        internal Container LookupContainer(string appKey, string docType)
        {
            _logger.Trace(m => m("LookupContainer(): appKey={0}, docType={1}", appKey, docType));

            Check.NotNullOrEmpty(appKey, "appKey");
            Check.NotNullOrEmpty(docType, "docType");

            var app = LookupAccount(appKey);
            foreach (var container in app.Containers.Where(container => container.Name.Equals(docType, StringComparison.OrdinalIgnoreCase)))
            {
                return container;
            }
            return CreateContainer(app, docType);
        }

        internal Account LookupAccount(string appKey)
        {
            _logger.Trace(m => m("LookupAccount(): appKey={0}", appKey));

            Account account;
            if (AccountCache.TryGetValue(appKey, out account))
            {
                return account;
            }

            // TODO: throw exception for no Account
            // for now use default
            return AccountCache.TryGetValue(DEFAULT_APP, out account) ? account : CreateDefaultApp();
        }

        private Account CreateDefaultApp()
        {
            _logger.Trace("CreateDefaultApp()");
            var account = new Account(DEFAULT_APP, DEFAULT_APP, DEFAULT_ACCOUNT, DEFAULT_PROVIDER);
            return CreateAccount(account);
        }

        internal ConcurrentDictionary<string, Account> LoadAccountCache()
        {
            _logger.Trace("LoadAccountCache()");

            var accounts = _accountRepo.FindAll();
            var cache = new ConcurrentDictionary<string, Account>();
            foreach (var account in accounts)
            {
                cache.TryAdd(account.AppKey, account);
            }
            return cache;
        }

    }
}
