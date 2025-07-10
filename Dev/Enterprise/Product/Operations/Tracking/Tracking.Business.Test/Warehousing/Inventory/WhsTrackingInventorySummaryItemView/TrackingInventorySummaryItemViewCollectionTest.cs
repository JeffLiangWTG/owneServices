using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Tracking.Business.Testing
{
	[TestedType(typeof(WhsTrackingInventorySummaryItemViewCollection))]
	sealed class TrackingInventorySummaryItemViewCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest() => new WhsTrackingInventorySummaryItemViewCollection(Factory);

		public override void TestAddAndCancelOfElementAsThoughBinding()
		{
			Assert("Deletion is not supported", true);
		}

		public override void TestAddAndDeleteOfElementAsThoughBinding()
		{
			Assert("Deletion is not supported", true);
		}
	}
}
