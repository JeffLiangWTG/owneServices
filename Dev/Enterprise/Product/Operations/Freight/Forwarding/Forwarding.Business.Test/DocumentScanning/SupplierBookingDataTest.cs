using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Orders.Business;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	sealed class SupplierBookingDataTest : TestCaseWithFactory
	{
		public void TestGetBusinessObjectCollection()
		{
			var booking1 = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking1.JSB_TransportMode = "AIR";
			booking1.JSB_LoadMode = "CY";
			booking1.JSB_Status = "PLN";

			var booking2 = Factory.NewWithValidTestData<JobSupplierBooking>();
			booking2.JSB_TransportMode = "AIR";
			booking2.JSB_LoadMode = "CY";
			booking2.JSB_Status = "PLN";

			Factory.Save();

			var collection = new SupplierBookingData().GetBusinessObjectCollection(new BusinessObjectFactory());

			AssertEquals(2, collection.Count);
			AssertNotNull(collection.FindByPK(booking1.PK));
			AssertNotNull(collection.FindByPK(booking2.PK));
		}
	}
}
