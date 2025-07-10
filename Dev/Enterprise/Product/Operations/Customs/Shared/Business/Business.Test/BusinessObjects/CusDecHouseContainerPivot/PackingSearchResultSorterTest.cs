using System.Collections.Generic;

namespace Enterprise.Customs.Business.Testing
{
	sealed class PackingSearchResultSorterTest : NUnit.Framework.TestCase
	{
		public void TestSort()
		{
			List<PackingSearchResult> result = new List<PackingSearchResult>();
			result.Add(new PackingSearchResult(2, new Dummy("A")));
			result.Add(new PackingSearchResult(1, new Dummy("B")));

			result.Sort(new PackingSearchResultSorter());
			AssertEquals("After sorted", "B", ((Dummy)result[0].Packing).ID);
			AssertEquals("After sorted", "A", ((Dummy)result[1].Packing).ID);
		}

		class Dummy : IPackingSearch
		{
			public Dummy(string iD)
			{
				this.ID = iD;
			}

			public readonly string ID;
		}
	}
}
