using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public interface IReferenceDataRepository : IDisposable
	{
		IQueryable<T> Get<T>() where T : class;
		void Add<T>(T data) where T : class;
		void Delete<T>(T data) where T : class;
		void Update<T>(T data) where T : class;
		Task<int> SaveChangesAsync(string userId, bool bulkInsert = false);
		IQueryable<T> GetWithExpand<T>(IDictionary<Type, List<Type>> dict) where T : class;
		Task RecursiveDeleteAsync<TE>(IQueryable<TE> data) where TE : class;
	}
}
