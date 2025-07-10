using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(ShipmentSummaryCollection))]
	sealed class ShipmentSummaryCollectionTest : NonPersistentBusinessObjectCollectionTestCase<ShipmentSummaryCollection>
	{
		protected override ShipmentSummaryCollection GetCollectionToTest()
		{
			return new ShipmentSummaryCollection();
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return ShipmentSummaryTest.GetNewShipmentSummary();
		}
	}
}
