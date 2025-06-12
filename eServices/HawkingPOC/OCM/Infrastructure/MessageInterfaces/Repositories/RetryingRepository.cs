using System.Collections.Generic;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using Polly;

namespace OcmPoc.Infrastructure.MessageInterfaces.Repositories
{
	public class RetryingRepository<TEntity> : IRepository<TEntity>
		where TEntity : BaseEntity
	{
		readonly IRepository<TEntity> repository;

		public RetryingRepository(IAsyncPolicy policy, IRepository<TEntity> repository)
		{
			Policy = policy;
			this.repository = repository;
		}
		
		protected IAsyncPolicy Policy { get; }

		public async Task<TEntity> AddAsync(TEntity entity)
		{
			return await Policy.ExecuteAsync(async () => await repository.AddAsync(entity));
		}

		public async Task<IEnumerable<TEntity>> GetAllAsync()
		{
			return await Policy.ExecuteAsync(async () => await repository.GetAllAsync());
		}

		public async Task<TEntity> GetByIdAsync(int id)
		{
			return await Policy.ExecuteAsync(async () => await repository.GetByIdAsync(id));
		}

		public async Task UpdateAsync(IEnumerable<TEntity> entities)
		{
			await Policy.ExecuteAsync(async () => await repository.UpdateAsync(entities));
		}
	}
}
