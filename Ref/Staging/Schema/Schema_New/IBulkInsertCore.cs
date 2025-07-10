using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	internal interface IBulkInsertCore
	{
		Task<int> BulkInsertAsync<T>(StagingDbContext stagingDbContext, IEnumerable<T> data, int? timeout, IDbTransaction transaction = null);
	}
}
