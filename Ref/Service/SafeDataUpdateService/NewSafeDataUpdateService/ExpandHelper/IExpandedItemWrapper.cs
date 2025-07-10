using System;

namespace CargoWise.RefDbRepo.NewSafeDataUpdateService
{
	public interface IExpandedItemWrapper
	{
		Type GetExpandType();
		IExpandClauseWrapper GetExpandClause();
	}
}
