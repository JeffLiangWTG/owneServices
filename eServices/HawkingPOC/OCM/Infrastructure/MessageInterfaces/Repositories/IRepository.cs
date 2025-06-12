using System.Collections.Generic;
using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageInterfaces.Repositories
{
	public interface IRepository<TEntity>
		where TEntity : BaseEntity
	{
		Task<IEnumerable<TEntity>> GetAllAsync();
		Task<TEntity> GetByIdAsync(int id);
		Task<TEntity> AddAsync(TEntity entity);
		Task UpdateAsync(IEnumerable<TEntity> entities);
	}
}
