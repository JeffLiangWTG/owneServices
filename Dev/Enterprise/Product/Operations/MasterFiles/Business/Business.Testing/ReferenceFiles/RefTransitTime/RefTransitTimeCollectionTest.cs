using System.ComponentModel;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(RefTransitTimeCollection))]
	sealed class RefTransitTimeCollectionTest : ActiveBusinessObjectCollectionTestCase<RefTransitTimeCollection>
	{
		public void TestTransitTimeFormattedSort()
		{
			var collection = new RefTransitTimeCollection(Factory);

			var transitTime1 = collection.AddNew();
			transitTime1.TransitDays = 5;
			transitTime1.TransitHours = 6;
			AssertEquals("5 days 6 hours", transitTime1.TransitTimeFormatted);

			var transitTime2 = collection.AddNew();
			transitTime2.TransitDays = 2;
			transitTime2.TransitHours = 18;
			AssertEquals("2 days 18 hours", transitTime2.TransitTimeFormatted);

			var transitTime3 = collection.AddNew();
			transitTime3.TransitDays = 14;
			transitTime3.TransitHours = 2;
			AssertEquals("14 days 2 hours", transitTime3.TransitTimeFormatted);

			collection.ApplySort(RefTransitTime.Schema.TransitTimeFormatted, ListSortDirection.Ascending);

			AssertEquals(transitTime2, collection[0]);
			AssertEquals(transitTime1, collection[1]);
			AssertEquals(transitTime3, collection[2]);

			collection.ApplySort(RefTransitTime.Schema.TransitTimeFormatted, ListSortDirection.Descending);

			AssertEquals(transitTime3, collection[0]);
			AssertEquals(transitTime1, collection[1]);
			AssertEquals(transitTime2, collection[2]);
		}
	}
}
