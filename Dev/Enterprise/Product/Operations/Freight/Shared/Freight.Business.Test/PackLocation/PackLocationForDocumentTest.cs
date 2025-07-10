using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	[TestedType(typeof(PackLocationForDocument))]
	sealed class PackLocationForDocumentTest : NonPersistentBusinessObjectTestCase
	{
		protected override BusinessObject GetNewBusinessObject()
		{
			return new PackLocationForDocument("S00001023", "AUSYD", "The Shipping Company", 5, "PKG", "Warehouse1");
		}

		public void TestPackLocationForDocument()
		{
			PackLocationForDocument location = new PackLocationForDocument("S00001023", "AUSYD", "The Shipping Company", 5, "PKG", "Warehouse1");
			AssertEquals("ShipmentNumber", "S00001023", location.ShipmentNumber);
			AssertEquals("Destination", "AUSYD", location.Destination);
			AssertEquals("ConsignorName", "The Shipping Company", location.ConsignorName);
			AssertEquals("PackCount", 5, location.PackCount);
			AssertEquals("PackType", "PKG", location.PackType);
			AssertEquals("WhsLocation", "Warehouse1", location.WhsLocation);
		}
	}
}
