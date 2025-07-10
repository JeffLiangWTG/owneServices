using System;
using System.Collections.Generic;

namespace Enterprise.Customs.ServiceTasks
{
	public static class EnumerableExtensions
	{
		public static T[] ShuffleListInRandomOrder<T>(this List<T> list)
		{
			var n = list.Count;
			var rng = new Random();
			while (n > 1)
			{
				n--;
				var k = rng.Next(n + 1);
				(list[k], list[n]) = (list[n], list[k]);
			}
			return list.ToArray();
		}
	}
}
