using System;
using System.Collections;

namespace Enterprise.Warehouse.Transactions.Business.Common
{
	public static class Aggregator
	{
		public static int Max(params int[] elements)
		{
			if (elements.Length == 0)
			{
				throw new ArgumentException("Calling Max() with an empty list of arguments");
			}
			int result = elements[0];
			for (int i = 1; i < elements.Length; i++)
			{
				if (result < elements[i])
				{
					result = elements[i];
				}
			}
			return result;
		}

		public static T[] Filter<T>(IEnumerable collection)
		{
			int count = 0;
			foreach (object item in collection)
			{
				if (item != null && item is T)
				{
					count++;
				}
			}
			T[] result = new T[count];
			int i = 0;
			foreach (object item in collection)
			{
				if (item != null && item is T)
				{
					result[i++] = (T)item;
				}
			}
			return result;
		}
	}
}
