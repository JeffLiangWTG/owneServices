using System.Collections.Generic;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public interface IExpandClauseWrapper
	{
		IEnumerable<IExpandedItemWrapper> GetExpandClauseWrapper();
	}
}
