using System.Linq;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Warehouse.Transit.Business.Testing.ReceiveExpectedPacking
{
	[TestedType(typeof(WhsItemReceiveASNCollection))]
	class WhsItemReceiveASNCollectionTest : ActiveBusinessObjectCollectionTestCase<WhsItemReceiveASNCollection>
	{
		#region TestASNsFromReceiveTransportationUnit

		public void TestASNsFromReceiveTransportationUnit()
		{
			var warehouse = Helper.CreateTRWWarehouse();
			var row = Helper.CreateRowAndGenerateLocations(warehouse, "Dock", 2, 2);
			var location = row.Locations.First(l => l.ToLocationString() == "Dock-1-1");
			var asn1 = Helper.CreateReceiveASN("ASN1", warehouse.PK);
			var asn2 = Helper.CreateReceiveASN("ASN2", warehouse.PK);
			var receiveTransportationUnit = Helper.CreateReceiveTransportationUnit("RTU1", warehouse.PK, location.PK);
			Helper.CreateReceiveASNRTUPivot(receiveTransportationUnit.PK, asn1.PK);
			Helper.CreateReceiveASNRTUPivot(receiveTransportationUnit.PK, asn2.PK);

			Factory.Save();

			AssertEquals("Find ASNs under RTU", 2, new WhsItemReceiveASNCollection(Factory, receiveTransportationUnit).Count);
			AssertContainsExactElementsInAnyOrder(new[] { asn1, asn2 }, new WhsItemReceiveASNCollection(Factory, receiveTransportationUnit));
		}

		#endregion

		#region Implementation

		protected override WhsItemReceiveASNCollection GetCollectionToTest()
		{
			return new WhsItemReceiveASNCollection(Factory);
		}

		WhsTransitTestHelper Helper => new WhsTransitTestHelper(Factory);

		#endregion
	}
}
