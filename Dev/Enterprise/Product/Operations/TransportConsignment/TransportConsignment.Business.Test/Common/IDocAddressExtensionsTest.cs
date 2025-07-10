using Enterprise.MasterFiles.Integration;

namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class IDocAddressExtensionsTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestGetAddressUniqueKey

		public void TestGetAddressUniqueKey()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			AssertEquals("HONDA MOTORCYCLES1/2SOME OTHER STREETMELBOURNE3039VIC", consignment.PickupInstruction.Address.GetAddressUniqueKey());
			AssertEquals("", ((IDocAddress)null).GetAddressUniqueKey());
		}

		#endregion
	}
}
