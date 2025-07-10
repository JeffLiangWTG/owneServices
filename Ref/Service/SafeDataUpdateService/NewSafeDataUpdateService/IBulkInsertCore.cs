using System.Collections.Generic;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public interface IBulkInsertCore
	{
		Task<int> SaveChangeWithBulkInsertCore(EntityEntry[] entries, IDictionary<EntityEntry, EntityState> itemStates, SafeDbContext entities);
	}
}
