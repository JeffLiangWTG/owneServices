using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingEventCollection))]
	class SterlingEventCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingEventCollection>
	{
		#region Test Overrides

		protected override SterlingEventCollection GetCollectionToTest()
		{
			SterlingCommerceConsolAndShipmentExporter master = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());
			return new SterlingEventCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingEvent();
		}

		#endregion

		public void TestSterlingEventCollection()
		{
			Xsd.InterchangeInfo interchange = new Xsd.InterchangeInfo();
			interchange.Source.Purpose = "APP";
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.Event @event = shipment.Events.Event.AddNew();
			@event = shipment.Events.Event.AddNew();
			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, interchange);
			AssertEquals("Purpose is not EVT - all events are in collection", 2, sterling.EventInfo.Count);
			interchange.Source.Purpose = "EVT";
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, interchange);
			AssertEquals("No triggered events in collection and purpose is EVT - no Event Info", 0, sterling.EventInfo.Count);
			@event.TriggeredBy = true;
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, interchange);
			AssertEquals("triggered events in collection - 1 Event Info", 1, sterling.EventInfo.Count);
		}
	}
}
