using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Hawking.Elk.Common
{
    public interface IRepository
    {
    }

    [ContractClass(typeof(RepositoryContract<>))]
    public interface IRepository<TEntity> : IRepository
        where TEntity : class
    {
        void Add(TEntity entity);

        void Update(TEntity entity);

        Task DeleteAsync(params object[] keyValues);

        void Delete(TEntity entity);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);
        Task<IEnumerable<TEntity>> FindAsync(bool detachEntity = true);
        Task<TEntity> FindByKeyAsync(params object[] keyValues);

        Task<int> SaveAsync();
    }

    [ContractClassFor(typeof(IRepository<>))]
    abstract class RepositoryContract<TEntity> : IRepository<TEntity>
        where TEntity : class
    {
        public void Add(TEntity entity)
        {
            Contract.Requires(entity != null);
        }

        public void Update(TEntity entity)
        {
            Contract.Requires(entity != null);
        }

        public Task DeleteAsync(params object[] keyValues)
        {
            Contract.Requires(keyValues != null);
            Contract.Requires(keyValues.Length > 0);
            return default(Task);
        }

        public void Delete(TEntity entity)
        {
            Contract.Requires(entity != null);
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return default(Task<int>);
        }

        public Task<IEnumerable<TEntity>> FindAsync(bool detachEntity = true)
        {
            return default(Task<IEnumerable<TEntity>>);
        }

        public Task<TEntity> FindByKeyAsync(params object[] keyValues)
        {
            Contract.Requires(keyValues != null);
            Contract.Requires(keyValues.Length > 0);
            return default(Task<TEntity>);
        }

        public Task<int> SaveAsync()
        {
            return default(Task<int>);
        }
    }
}