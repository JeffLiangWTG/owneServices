using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.AWB.Testing
{
	[TestedType(typeof(ShipmentExportAWBOtherChargesCollection))]
	sealed class ShipmentExportAWBOtherChargesCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ShipmentExportAWBHeader aWBHeader = Factory.New<ShipmentExportAWBHeader>();
			return new ShipmentExportAWBOtherChargesCollection(aWBHeader);
		}
	}
}
