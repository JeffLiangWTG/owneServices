using System;
using System.Collections.Generic;
using System.Linq;

namespace Enterprise.Warehouse.Cartonisation.Business
{
	class LocalSearchSolutionComparer : IComparer<LocalSearchSolution>
	{
		public int Compare(LocalSearchSolution x, LocalSearchSolution y)
		{
			var result = 0;

			foreach (var comparerDelegate in ComparerDelegates)
			{
				result = comparerDelegate(x).CompareTo(comparerDelegate(y));

				if (result != 0)
				{
					break;
				}
			}

			return result;
		}

		IEnumerable<Func<LocalSearchSolution, IComparable>> ComparerDelegates => comparerDelegates ??= GetNewComparerDelegates();
		IEnumerable<Func<LocalSearchSolution, IComparable>> comparerDelegates;

		IEnumerable<Func<LocalSearchSolution, IComparable>> GetNewComparerDelegates()
		{
			return new Func<LocalSearchSolution, IComparable>[]
			{
				s => -s.TotalPackedItems, // Prefer a solution that packs more items
				s => s.TotalCost,
				s => s.Cartons.Count,
				s => s.TotalVolume,
			}.Concat(GetNewComparerDelegatesCore());
		}

		protected virtual IEnumerable<Func<LocalSearchSolution, IComparable>> GetNewComparerDelegatesCore()
			=> Enumerable.Empty<Func<LocalSearchSolution, IComparable>>();
	}
}
