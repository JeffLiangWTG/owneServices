using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(AWBHeaderManager<ForwardingShipment>))]
	sealed class AWBHeaderManagerTestForForwardingShipment : ActiveBusinessObjectCollectionTestCase<AWBHeaderManager<ForwardingShipment>>
	{
		#region Implementation

		ForwardingShipment Shipment
		{
			get
			{
				if (shipment == null)
				{
					shipment = Factory.New<ForwardingShipment>();
					shipment.JS_TransportMode = Constants.TransportModes.Air;
				}

				return shipment;
			}
		}

		ForwardingShipment shipment;

		protected override AWBHeaderManager<ForwardingShipment> GetCollectionToTest()
		{
			return new AWBHeaderManager<ForwardingShipment>(Shipment);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			var exportAWBHeader = Factory.New<ShipmentExportAWBHeader>();
			exportAWBHeader.EH_ParentID = Shipment.PK;

			return exportAWBHeader;
		}

		public void TestAWBHeaderOnlyReturnedForAirTransportModes()
		{
			var manager = new AWBHeaderManager<ForwardingShipment>(Shipment);
			AssertNotNull("Manager should created an AWBHeader", manager.AWBHeader);
			Factory.Save();

			var initalAWB = manager.AWBHeader;

			Shipment.JS_TransportMode = Constants.TransportModes.Road;
			AssertNull("Should not return an AWBHeader for a non-air parent", manager.AWBHeader);
			Factory.Save();

			Shipment.JS_TransportMode = Constants.TransportModes.AirSea;
			AssertNotNull(manager.AWBHeader);

			Shipment.JS_TransportMode = Constants.TransportModes.Air;
			AssertNotNull(manager.AWBHeader);
			Factory.Save();

			AssertEquals(initalAWB, manager.AWBHeader);
		}

		#endregion

		public override void TestAdd()
		{
			int initialCount = Collection.Count;

			var bizO1 = (ShipmentExportAWBHeader)GetNewElementToAddToTheCollection();

			AssertEquals("Collection count", initialCount + 1, Collection.Count);
			Assert("Contains element 1", Collection.Contains(bizO1));

			var bizO2 = (ShipmentExportAWBHeader)GetNewElementToAddToTheCollection();
			AssertEquals("Collection count", 0, Collection.Count);
		}

		public override void TestDelete()
		{
			int initialCount = Collection.Count;
			var bizO = (ShipmentExportAWBHeader)GetNewElementToAddToTheCollection();
			AssertEquals("Precondition : Collection count", initialCount + 1, Collection.Count);

			Collection.Delete(bizO);

			AssertEquals("Collection count", initialCount, Collection.Count);
			Assert("Doesn't contain element", !Collection.Contains(bizO));
			Assert("Element was removed, and deleted", bizO.IsDeleted);
		}
	}
}
