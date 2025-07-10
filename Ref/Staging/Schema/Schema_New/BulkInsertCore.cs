using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using CargoWise.RefDbRepo.Common.Utils;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore.Storage;

namespace CargoWise.RefDbRepo.Staging.Schema_New
{
	class BulkInsertCore : IBulkInsertCore
	{
		public async Task<int> BulkInsertAsync<T>(StagingDbContext stagingDbContext, IEnumerable<T> data, int? timeout, IDbTransaction transaction = null)
		{
			await stagingDbContext.BulkInsertAsync(data, transaction, SqlBulkCopyOptions.CheckConstraints, null, timeout);
			return data.Count();
		}
	}
}
