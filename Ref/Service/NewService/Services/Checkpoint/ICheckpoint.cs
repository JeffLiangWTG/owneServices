using System.Linq;
using CargoWise.RefDbRepo.Service.Schema_0_9_New;

namespace CargoWise.RefDbRepo.NewService
{
	public interface ICheckpoint
	{
		IQueryable<RefDbVersionControl> Filter(IQueryable<RefDbVersionControl> source);
		IQueryable<T> FilterView<T>(IQueryable<T> source) where T : class, IDataSetView;
	}
}
