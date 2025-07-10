using System;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transactions.Business.Common.Testing
{
	class AggregatorTest : TestCase
	{
		public void TestMax()
		{
			AssertExceptionThrown(typeof(ArgumentException), delegate()
			{ Aggregator.Max(); });
			AssertEquals(4, Aggregator.Max(4));
			AssertEquals(12, Aggregator.Max(1, -3, 12, 0));
			AssertEquals(-1, Aggregator.Max(-3, -1, -12, -10));
			AssertEquals(int.MaxValue, Aggregator.Max(-5, int.MaxValue, 0, int.MinValue, 5));
		}

		public void TestFilter()
		{
			object[] array = new object[] { "John", 6, 5, -3, null, "Mary" };
			string[] result = Aggregator.Filter<string>(array);
			AssertEquals(2, result.Length);
			AssertCollectionContains("John", result);
			AssertCollectionContains("Mary", result);

			int[] resultInt = Aggregator.Filter<int>(array);
			AssertEquals(3, resultInt.Length);
			AssertCollectionContains(6, resultInt);
			AssertCollectionContains(5, resultInt);
			AssertCollectionContains(-3, resultInt);
		}
	}
}
