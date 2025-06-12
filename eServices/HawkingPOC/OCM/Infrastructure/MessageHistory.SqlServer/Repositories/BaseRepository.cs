using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Repositories;

namespace OcmPoc.Infrastructure.MessageHistory.SqlServer.Repositories
{
	public abstract class BaseRepository<TEntity> : IRepository<TEntity>
		where TEntity : BaseEntity
	{
		readonly IContextFactory<MessageHistoryContext> contextFactory;

		public BaseRepository(IContextFactory<MessageHistoryContext> contextFactory)
		{
			this.contextFactory = contextFactory;
		}

		protected MessageHistoryContext CreateContext() => contextFactory.Create();

		protected abstract DbSet<TEntity> GetDbSet(MessageHistoryContext context);

		public async Task<TEntity> AddAsync(TEntity entity)
		{
			using (var context = CreateContext())
			{
				GetDbSet(context).Add(entity);
				await context.SaveChangesAsync().ConfigureAwait(false);
				return entity;
			}
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync()
		{
			using (var context = CreateContext())
			{
				var dbSet = GetDbSet(context);
				await dbSet.LoadAsync().ConfigureAwait(false);
				return dbSet.ToList();
			}
		}

		public virtual async Task<TEntity> GetByIdAsync(int id)
		{
			using (var context = CreateContext())
			{
				return await GetDbSet(context).FindAsync(id).ConfigureAwait(false);
			}
		}

		public async Task UpdateAsync(IEnumerable<TEntity> entities)
		{
			using (var context = CreateContext())
			{
				foreach (var entity in entities)
				{
					context.Update(entity);
				}
				await context.SaveChangesAsync().ConfigureAwait(false);
			}
		}
	}
}
