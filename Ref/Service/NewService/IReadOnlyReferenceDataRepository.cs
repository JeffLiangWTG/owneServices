using System;
using System.Linq;

namespace CargoWise.RefDbRepo.NewService
{
	public interface IReadOnlyReferenceDataRepository : IDisposable
	{
		IQueryable<T> Get<T>() where T : class;
		bool IsDbProvider { get; }
	}
}
