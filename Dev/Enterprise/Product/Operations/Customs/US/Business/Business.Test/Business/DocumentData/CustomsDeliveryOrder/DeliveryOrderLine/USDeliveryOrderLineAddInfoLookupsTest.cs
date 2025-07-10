using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.Messaging.Business;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USDeliveryOrderLineAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestPackageTypeList()
		{
			var deliveryOrderLine = Factory.New<DeliveryOrderLine>();
			AssertType<ShippingOrPackingingUnitList>(deliveryOrderLine.AddInfoLookups.PackageTypeList);
		}
	}
}
