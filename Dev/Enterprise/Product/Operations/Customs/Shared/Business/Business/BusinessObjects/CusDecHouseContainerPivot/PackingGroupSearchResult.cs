using System.Collections.Generic;

namespace Enterprise.Customs.Business
{
	internal interface IPackingSearch
	{
	}

	internal struct PackingSearchResult
	{
		public PackingSearchResult(int rank, IPackingSearch packing)
		{
			this.Rank = rank;
			this.Packing = packing;
		}

		public readonly int Rank;
		public readonly IPackingSearch Packing;
	}

	internal class PackingSearchResultSorter : IComparer<PackingSearchResult>
	{
		public int Compare(PackingSearchResult x, PackingSearchResult y)
		{
			return x.Rank.CompareTo(y.Rank);
		}
	}
}
