using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using Common.Logging;
using LightInject;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Paycor.Neo.Data.Repositories;
using Paycor.Neo.Domain.Entity;
using Paycor.Storage.Data.EF.IoC;

namespace Paycor.Storage.Data.EF.Repository
{
   
    public abstract class CrudRepositoryTest<TContext, TRepository, TEntity, TId> 
        where TEntity : class, IEntity<TId> 
        where TRepository : ICrudRepository<TEntity, TId>
        where TContext : DbContext
    {
        private static readonly ILog _logger = LogManager.GetLogger<CrudRepositoryTest<TContext, TRepository, TEntity, TId>>();

        protected readonly ICollection<TId> _idsToDelete = new Collection<TId>();

        protected ServiceContainer IoC { get; private set; }
        protected IUnitOfWork UoW { get; private set; }
        protected TRepository Repo { get; private set; }
        protected TContext Context { get; private set; }

        protected bool DetectChanges { get; private set; }

        protected virtual void Setup(bool detectChanges)
        {
            _logger.Trace("Setup()");

            IoC = LightInjectTestConfig.Register().Container;

            DetectChanges = detectChanges;

            Context = IoC.GetInstance<TContext>();
            Assert.IsNotNull(Context);

            UoW = IoC.GetInstance<IUnitOfWork>();
            Assert.IsNotNull(UoW);

            Repo = UoW.Repository<TRepository>();
            Assert.IsNotNull(Repo);
        }

        protected virtual void TearDown()
        {
            _logger.Trace("TearDown()");

            foreach (var id in _idsToDelete)
            {
                Repo.DeleteBy(id);
            }
            UoW.SaveChanges();
            IoC.Dispose();
            Context.Dispose();
        }

        protected TEntity SaveEntity(Func<TEntity, TId> getId, TEntity entity)
        {
            var newApp = Repo.Add(entity);
            UoW.SaveChanges(DetectChanges);
            _idsToDelete.Add(getId(entity));
            return newApp;
        }
    }
}
