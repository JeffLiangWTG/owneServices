using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingRoutingCollection))]
	class SterlingRoutingCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingRoutingCollection>
	{
		#region Test Overrides

		protected override SterlingRoutingCollection GetCollectionToTest()
		{
			SterlingCommerceConsolAndShipmentExporter master = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());
			return new SterlingRoutingCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingRouting();
		}

		#endregion

		public void TestSterlingRoutingCollection()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.PlannedLeg routing = shipment.ShipmentDetails.TransportPlan.AddNew();
			routing = shipment.ShipmentDetails.TransportPlan.AddNew();
			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(2, sterling.RoutingInfo.Count);
			Xsd.Consol consol = new Xsd.Consol();
			routing = consol.ConsolDetail.PlannedLegs.AddNew();
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, consol, shipment, new Xsd.InterchangeInfo());
			AssertEquals(3, sterling.RoutingInfo.Count);
		}
	}
}
