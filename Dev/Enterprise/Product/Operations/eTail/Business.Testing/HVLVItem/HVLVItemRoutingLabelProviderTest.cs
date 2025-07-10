using CargoWise.EntityFramework.Testing;

namespace Enterprise.eTail.Business.Testing
{
	class HVLVItemRoutingLabelProviderTest : TestCaseWithFactory
	{
		public void TestHVLVItemRoutingLabelProvider_RoutingLabel()
		{
			var bookingHeader = Factory.New<HVLVBookingHeader>();
			bookingHeader.FillWithValidTestData();
			var item = bookingHeader.Consignments.AddNew().Items.AddNew();

			Factory.Save();

			var routingLabel = new HVLVItemRoutingLabelProvider(item.PK.ToGuid()).RoutingLabel;

			AssertNotNull(routingLabel);
		}
	}
}
