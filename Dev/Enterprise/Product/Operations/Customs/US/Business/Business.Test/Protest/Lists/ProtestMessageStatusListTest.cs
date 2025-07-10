using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ProtestMessageStatusListTest : TestCase
	{
		public void TestIStatusList()
		{
			IStatusList list = new ProtestMessageStatusList();

			AssertEquals("IsStatusClear", true, list.IsStatusClear(ProtestMessageStatusList.Codes.ClearInitialFiling));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(ProtestMessageStatusList.Codes.ClearInitialFilingWithWarnings));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(ProtestMessageStatusList.Codes.ClearAmendment));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(ProtestMessageStatusList.Codes.ClearAddenda));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(ProtestMessageStatusList.Codes.AwaitingAmendment));

			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(ProtestMessageStatusList.Codes.AwaitingInitialFiling));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(ProtestMessageStatusList.Codes.AwaitingAmendment));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(ProtestMessageStatusList.Codes.AwaitingAddenda));
			AssertEquals("IsWaitingForResponse", false, list.IsWaitingForResponse(ProtestMessageStatusList.Codes.ErrorAmendment));
		}

		public void TestAcceptedStatusToCancelRejectStatusInterested()
		{
			var list = new ProtestMessageStatusList();
			AssertEquals("AcceptedStatusToCancelRejectStatusInterested", 4, list.AcceptedStatusToCancelRejectStatusInterested.Count);
			AssertEquals("ClearInitialFiling", ProtestMessageStatusList.Codes.ClearInitialFiling, list.AcceptedStatusToCancelRejectStatusInterested[0]);
			AssertEquals("ClearInitialFilingWithWarnings", ProtestMessageStatusList.Codes.ClearInitialFilingWithWarnings, list.AcceptedStatusToCancelRejectStatusInterested[1]);
			AssertEquals("ClearAmendment", ProtestMessageStatusList.Codes.ClearAmendment, list.AcceptedStatusToCancelRejectStatusInterested[2]);
			AssertEquals("ClearAddenda", ProtestMessageStatusList.Codes.ClearAddenda, list.AcceptedStatusToCancelRejectStatusInterested[3]);
		}
	}
}
