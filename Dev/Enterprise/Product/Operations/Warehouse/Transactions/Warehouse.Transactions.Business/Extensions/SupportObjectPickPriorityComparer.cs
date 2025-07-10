using System;
using System.Collections.Generic;
using Enterprise.Warehouse.Integration.Warehouse;

namespace Enterprise.Warehouse.Transactions.Business
{
	public class SupportObjectPickPriorityComparer : IComparer<ISupportPickPriority>, ISupportObjectPickPriorityComparer 
	{
		public int Compare(ISupportPickPriority o1, ISupportPickPriority o2)
		{
			if (o1 == null || o2 == null)
			{
				throw new ArgumentNullException("Object supporting pick priority can not be null");
			}
			var comparer = new PickPriorityComparer();
			return comparer.Compare(o1.PickPriority, o2.PickPriority);
		}
	}
}
