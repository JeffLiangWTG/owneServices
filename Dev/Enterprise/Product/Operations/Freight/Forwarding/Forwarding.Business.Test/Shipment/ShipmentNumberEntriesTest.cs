using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ShipmentNumberEntries))]
	sealed class ShipmentNumberEntriesTest : NonPersistentBusinessObjectCollectionTestCase<ShipmentNumberEntries>
	{
		#region AllowNew

		public void TestAllowNew()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ShipmentNumberEntries entries = new ShipmentNumberEntries(consol, Factory);
			Assert(!entries.AllowNew);
		}

		#endregion

		#region Load

		public void TestLoad()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ForwardingShipment shipment1 = consol.Shipments.AddNew();
			ShipmentNumberEntries entries = new ShipmentNumberEntries(consol, Factory);
			AssertEquals("Should be 0", 0, entries.Count);

			entries.Load();
			AssertEquals("Should be 1", 1, entries.Count);
			AssertEquals("Should be the first shipment", shipment1.PK, entries[0].Shipment.PK);

			ForwardingShipment shipment2 = consol.Shipments.AddNew();
			AssertEquals("Should be 1", 1, entries.Count);
			AssertEquals("Should be the first shipment", shipment1.PK, entries[0].Shipment.PK);

			entries.Load();
			AssertEquals("Should be 2 now", 2, entries.Count);
		}

		#endregion

		#region Implementation

		protected override ShipmentNumberEntries GetCollectionToTest()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			consol.Shipments.AddNew();

			return new ShipmentNumberEntries(consol, Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new ShipmentNumberEntry(Factory.New<ForwardingShipment>());
		}

		#endregion
	}
}
