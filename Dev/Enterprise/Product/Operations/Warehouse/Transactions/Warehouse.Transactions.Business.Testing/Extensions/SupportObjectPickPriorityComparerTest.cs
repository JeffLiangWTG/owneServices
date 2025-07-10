using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Warehouse.Integration.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business.Testing
{
	public class SupportObjectPickPriorityComparerTest : WhsTestCaseWithFactory
	{
		public void TestSupportObjectPickPriorityComparer()
		{
			var o1 = new PickPriorityComparerSupportableObjectForTest(0);
			var o2 = new PickPriorityComparerSupportableObjectForTest(1);
			var o3 = new PickPriorityComparerSupportableObjectForTest(2);
			var o4 = new PickPriorityComparerSupportableObjectForTest(5);

			var unSortedList = new List<ISupportPickPriority>
			{
				o3,
				o2,
				o1,
				o4
			};

			var sortedList = new List<ISupportPickPriority>
			{
				o2,
				o3,
				o4,
				o1
			};

			unSortedList.Sort(new SupportObjectPickPriorityComparer());
			AssertEquals(sortedList.Count, unSortedList.Count);
			for (int i = 0; i < unSortedList.Count; i++)
			{
				AssertEquals(sortedList[i], unSortedList[i]);
			}
		}
	}

	public class PickPriorityComparerSupportableObjectForTest : ISupportPickPriority
	{
		readonly int priority;

		public PickPriorityComparerSupportableObjectForTest(int priority)
		{
			this.priority = priority;
		}
		ZInt ISupportPickPriority.PickPriority => priority;
	}
}
