using System.Collections.Generic;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class PickPriorityComparer : IComparer<int>
	{
		public int Compare(int priority1, int priority2)
		{
			if (priority1 == priority2)
			{
				return 0;
			}
			else if (priority2 == 0)
			{
				return -1;
			}
			else if (priority1 == 0)
			{
				return 1;
			}
			else
			{
				return priority1.CompareTo(priority2);
			}
		}
	}
}
