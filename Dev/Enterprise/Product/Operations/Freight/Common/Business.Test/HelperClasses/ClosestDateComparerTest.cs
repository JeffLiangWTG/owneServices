using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class ClosestDateComparerTest : TestCase
	{
		public void TestCompare()
		{
			var date1 = new ZDateTime(2011, 12, 31, 12, 00, 0, 0);
			var date2 = new ZDateTime(2012, 1, 1, 12, 00, 0, 0);

			var comparer = new ClosestDateComparer(new ZDateTime(2012, 1, 1));
			AssertEquals("dates are equally apart", 0, comparer.Compare(date1, date2));

			date1 = new ZDateTime(2011, 12, 31, 20, 00, 0, 0);
			date2 = new ZDateTime(2012, 1, 1, 12, 00, 0, 0);

			AssertEquals("date1 is closer", -1, comparer.Compare(date1, date2));

			date1 = new ZDateTime(2011, 12, 31, 20, 00, 0, 0);
			date2 = new ZDateTime(2012, 1, 1, 1, 00, 0, 0);

			AssertEquals("date2 is closer", 1, comparer.Compare(date1, date2));
		}
	}
}
