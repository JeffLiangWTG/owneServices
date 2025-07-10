using System;
using System.Linq;
using System.Threading.Tasks;

namespace CargoWise.RefDbRepo.NewService
{
	public interface IReferenceDataRepository : IDisposable
	{
		IQueryable<T> Get<T>() where T : class;
		void Add<T>(T data) where T : class;
		void Update<T>(T data) where T : class;
		// Data modification might be moved to another web site once staging data receives data from users
		Task<int> SaveChangesAsync();
	}
}
