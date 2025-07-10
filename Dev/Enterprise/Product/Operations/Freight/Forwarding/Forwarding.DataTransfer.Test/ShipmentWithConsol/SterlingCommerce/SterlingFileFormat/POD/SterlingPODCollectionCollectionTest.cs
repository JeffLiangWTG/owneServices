using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingPODCollection))]
	class SterlingPODCollectionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingPODCollection>
	{
		#region Test Overrides

		protected override SterlingPODCollection GetCollectionToTest()
		{
			SterlingCommerceConsolAndShipmentExporter master = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());
			return new SterlingPODCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingPOD();
		}

		#endregion

		public void TestSterlingPODCollection()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();

			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals("PODInfo count is same as DeliveryLegs", 0, sterling.PODInfo.Count);

			Xsd.ContainerLeg leg = shipment.ShipmentDetails.Deliver.DeliveryLegs.AddNew();
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals("PODInfo count is same as DeliveryLegs", 1, sterling.PODInfo.Count);

			leg = shipment.ShipmentDetails.Deliver.DeliveryLegs.AddNew();
			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals("PODInfo count is same as DeliveryLegs", 2, sterling.PODInfo.Count);
		}
	}
}
