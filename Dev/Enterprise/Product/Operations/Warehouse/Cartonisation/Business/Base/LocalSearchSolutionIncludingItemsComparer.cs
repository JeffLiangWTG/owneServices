using System;
using System.Collections.Generic;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	class LocalSearchSolutionIncludingItemsComparer : LocalSearchSolutionComparer
	{
		protected override IEnumerable<Func<LocalSearchSolution, IComparable>> GetNewComparerDelegatesCore()
		{
			return new Func<LocalSearchSolution, IComparable>[]
			{
				s => s.TotalDistinctProductsPerPackage,
				s => s.TotalDistinctLocationsPerPackage,
			};
		}
	}
}
