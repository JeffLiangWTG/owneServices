using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingOrderReferenceCollection))]
	class SterlingOrderReferenceCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingOrderReferenceCollection>
	{
		#region Test Overrides

		protected override SterlingOrderReferenceCollection GetCollectionToTest()
		{
			SterlingCommerceConsolAndShipmentExporter master = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());
			return new SterlingOrderReferenceCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingOrderReference();
		}

		#endregion

		public void TestSterlingOrderReferenceCollection()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();

			string[] references2 = { "Reference1", "Reference2" };
			shipment.ShipmentDetails.OrderReferences = references2;

			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(2, sterling.OrderReferenceInfo.Count);

			string[] references3 = { "Reference1", "Reference2", "Reference3" };
			shipment.ShipmentDetails.OrderReferences = references3;

			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(3, sterling.OrderReferenceInfo.Count);
		}
	}
}
