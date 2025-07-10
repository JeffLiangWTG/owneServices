using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Moq;

namespace Enterprise.Freight.Common.Business.Testing
{
	sealed class JobCartageAddressHelperTest : TestCaseWithFactory
	{
		public void TestGetCartagePickupAddressForNullLegTypeReturnsNull()
		{
			var cartage = new Mock<IDocAddresses>().Object;
			AssertNull(CommonCartageAddressHelper.GetCartagePickupAddress(cartage, null));
		}

		public void TestGetCartageWaitPointAddressForNullLegTypeReturnsNull()
		{
			var cartage = new Mock<IDocAddresses>().Object;
			AssertNull(CommonCartageAddressHelper.GetCartageWaitPointAddress(cartage, null));
		}

		public void TestGetCartageDeliveryAddressForNullLegTypeReturnsNull()
		{
			var cartage = new Mock<IDocAddresses>().Object;
			AssertNull(CommonCartageAddressHelper.GetCartageDeliveryAddress(cartage, null));
		}
	}
}
