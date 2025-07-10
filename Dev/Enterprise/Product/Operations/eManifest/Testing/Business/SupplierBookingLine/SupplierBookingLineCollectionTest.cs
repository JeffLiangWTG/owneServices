
namespace Enterprise.eManifest.Business.Testing
{
	using CargoWise.EntityFramework.Testing;
	using Enterprise.Freight.Business;
	using NUnit.Framework;

	[TestedType(typeof(SupplierBookingLineCollection))]
	internal class SupplierBookingLineCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override CargoWise.EntityFramework.BusinessObjectCollection GetCollectionToTest()
		{
			var shipment = Factory.NewWithValidTestData<CommonShipment>();

			return new SupplierBookingLineCollection(shipment);
		}
	}
}
