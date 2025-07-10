using System.Linq;

namespace CargoWise.RefDbRepo.NewService
{
	public sealed class ReadOnlyReferenceDataRepository : IReadOnlyReferenceDataRepository
	{
		public ReadOnlyReferenceDataRepository(string nameOrConnectionString)
		{
			repository = new ReferenceDataRepository(nameOrConnectionString);
		}

		readonly ReferenceDataRepository repository;

		public bool IsDbProvider
		{
			get { return true; }
		}

		public IQueryable<T> Get<T>() where T : class
		{
			return repository.Get<T>();
		}

		public void Dispose()
		{
			repository.Dispose();
		}
	}
}
