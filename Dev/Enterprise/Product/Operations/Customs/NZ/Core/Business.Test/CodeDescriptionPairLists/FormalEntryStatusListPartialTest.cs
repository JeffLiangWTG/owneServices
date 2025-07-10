// -----------------------------------------------------------------------
// <copyright file="FormalEntryStatusListPartial.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Enterprise.Customs.NZ.Business.Testing
{
	using CargoWise.EntityFramework.Testing;

	class FormalEntryStatusListTest : TestCaseWithFactory
	{
		public void TestIsDeliveryStatusCleared()
		{
			AssertEquals(true, FormalEntryStatusList.IsDeliveryStatusCleared(FormalEntryStatusList.Codes.DeliveryOnPayment));
			AssertEquals(true, FormalEntryStatusList.IsDeliveryStatusCleared(FormalEntryStatusList.Codes.DeliveryOrderReceived));
			AssertEquals(true, FormalEntryStatusList.IsDeliveryStatusCleared(FormalEntryStatusList.Codes.DOSentToRecipient));

			AssertEquals(false, FormalEntryStatusList.IsDeliveryStatusCleared(FormalEntryStatusList.Codes.EntryCancelled));
			AssertEquals(false, FormalEntryStatusList.IsDeliveryStatusCleared(FormalEntryStatusList.Codes.AgencyResponsePending));
			AssertEquals(false, FormalEntryStatusList.IsDeliveryStatusCleared(FormalEntryStatusList.Codes.InspectionsAuditRequirements));
		}
	}
}
