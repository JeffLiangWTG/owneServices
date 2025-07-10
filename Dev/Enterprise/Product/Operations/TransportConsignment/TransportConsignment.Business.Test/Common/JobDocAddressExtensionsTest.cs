namespace Enterprise.TransportConsignment.Business.Testing
{
	sealed class JobDocAddressExtensionsTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestGetAddressLine

		public void TestGetAddressLine()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplateAndAddresses();
			AssertEquals("1/2 SOME OTHER STREET MELBOURNE VIC 3039", consignment.PickupInstruction.Address.GetAddressLine(Factory));
		}

		#endregion
	}
}
