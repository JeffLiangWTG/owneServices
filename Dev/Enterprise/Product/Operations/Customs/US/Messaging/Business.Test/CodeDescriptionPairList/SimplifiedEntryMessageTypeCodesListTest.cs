using NUnit.Framework;

namespace Enterprise.Customs.US.Messaging.Business.Testing
{
	sealed class SimplifiedEntryMessageTypeCodesListTest : TestCase
	{
		public void TestIsSEAccepted()
		{
			AssertEquals(true, SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(SimplifiedEntryMessageTypeCodesList.Codes.MessageAccepted));
			AssertEquals(true, SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(SimplifiedEntryMessageTypeCodesList.Codes.MessageAcceptedWithWarning));
			AssertEquals(true, SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(SimplifiedEntryMessageTypeCodesList.Codes.CancellationRequestPending));
			AssertEquals(false, SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(SimplifiedEntryMessageTypeCodesList.Codes.MessageRejected));
			AssertEquals(true, SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(SimplifiedEntryMessageTypeCodesList.Codes.RecordAcceptedWithWarning));
			AssertEquals(false, SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted(SimplifiedEntryMessageTypeCodesList.Codes.RecordRejected));
			AssertEquals(true, SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted("2GC"));
			AssertEquals(true, SimplifiedEntryMessageTypeCodesList.IsCargoReleaseAccepted("2A4"));
		}
	}
}
