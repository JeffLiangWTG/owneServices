using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Freight.Forwarding.DataTransfer.Testing
{
	[TestedType(typeof(SterlingPackageCollection))]
	class SterlingPackageCollectionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<SterlingPackageCollection>
	{
		#region Test Overrides

		protected override SterlingPackageCollection GetCollectionToTest()
		{
			SterlingCommerceConsolAndShipmentExporter master = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), new Xsd.Shipment(), new Xsd.InterchangeInfo());
			return new SterlingPackageCollection(master);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new SterlingPackage();
		}

		#endregion

		public void TestSterlingPackageCollection()
		{
			Xsd.Shipment shipment = new Xsd.Shipment();
			Xsd.Package package = shipment.ShipmentDetails.Packages.AddNew();

			SterlingCommerceConsolAndShipmentExporter sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(1, sterling.PackageInfo.Count);

			package = shipment.ShipmentDetails.Packages.AddNew();

			sterling = new SterlingCommerceConsolAndShipmentExporter(Factory, new Xsd.Consol(), shipment, new Xsd.InterchangeInfo());
			AssertEquals(2, sterling.PackageInfo.Count);
		}
	}
}
