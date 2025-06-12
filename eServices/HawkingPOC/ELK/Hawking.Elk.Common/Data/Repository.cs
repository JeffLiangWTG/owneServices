using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Hawking.Elk.Common.Model;
using Microsoft.EntityFrameworkCore;

namespace Hawking.Elk.Common
{
    public class Repository<TEntity> : IRepository<TEntity>, IDisposable
            where TEntity : class, IEntity
    {
        #region Member variables

        readonly DbContext dbContext;
        readonly DbSet<TEntity> dbSet;

        #endregion

        #region Constructor

        public Repository(DbContext dbContext)
        {
            Contract.Requires(dbContext != null);
            this.dbContext = dbContext;
            dbSet = dbContext.Set<TEntity>();
        }

        #endregion

        protected DbContext DbContext => dbContext;

        public async Task<TEntity> FindByKeyAsync(params object[] keyValues)
        {
            return await dbSet.FindAsync(keyValues);
        }

        public async Task<IEnumerable<TEntity>> FindAsync(bool detachEntity = true)
        {
            IQueryable<TEntity> query = dbSet;

            if (detachEntity)
            {
                query = query.AsNoTracking();
            }

            return await query.ToArrayAsync();
        }

        public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return await dbSet.CountAsync();
            }
            else
            {
                return await dbSet.CountAsync(predicate);
            }
        }

        public IQueryable<TEntity> FindWhere(
            Expression<Func<TEntity, bool>> filter = null,
            Func<IQueryable<TEntity>, IOrderedQueryable<TEntity>> orderBy = null,
            bool detachEntity = true,
            params Expression<Func<TEntity, object>>[] includeProperties)
        {
            Contract.Requires(dbSet != null);
            Contract.Ensures(Contract.Result<IQueryable<TEntity>>() != null);

            IQueryable<TEntity> query = dbSet;

            if (detachEntity)
            {
                query = query.AsNoTracking();
            }

            if (filter != null)
            {
                query = query.Where(filter);
            }

            foreach (var includeProperty in includeProperties)
            {
                query = query.Include(includeProperty);
            }

            return orderBy != null ? orderBy(query) : query;
        }

        #region Not Supported Methods

        public void Add(TEntity entity)
        {
            throw new NotSupportedException();
        }

        public void Update(TEntity entity)
        {
            throw new NotSupportedException();
        }

        public async Task DeleteAsync(params object[] keyValues)
        {
            throw new NotSupportedException();
        }

        public void Delete(TEntity entity)
        {
            throw new NotSupportedException();
        }

        public async Task<int> SaveAsync()
        {
            throw new NotSupportedException();
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool isDisposing)
        {
            if (!isDisposed)
            {
                if (isDisposing)
                {
                    dbContext.Dispose();
                }
            }
            isDisposed = true;
        }

        bool isDisposed;

        #endregion
    }
}

