using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	class PickPriorityComparerTest : WhsTestCaseWithFactory
	{
		public void TestPickPriorityComparer()
		{
			var expectedList = new List<int>()
			{
				1,
				1,
				2,
				3,
				4,
				5,
				0,
				0
			};
			var unSortedList = new List<int>()
			{
				0,
				2,
				1,
				0,
				3,
				5,
				1,
				4
			};
			unSortedList.Sort(new PickPriorityComparer());
			AssertEquals(expectedList.Count, unSortedList.Count);
			for (int i = 0; i < unSortedList.Count; i++)
			{
				AssertEquals(expectedList[i], unSortedList[i]);
			}
		}
	}
}
