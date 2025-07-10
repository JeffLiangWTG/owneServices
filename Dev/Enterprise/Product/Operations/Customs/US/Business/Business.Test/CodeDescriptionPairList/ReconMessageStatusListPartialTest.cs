using CargoWise.EntityFramework;
using Enterprise.Customs.Common.US;
using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ReconMessageStatusListTest : TestCase
	{
		public void TestIStatusList()
		{
			IStatusList list = new ReconMessageStatusList();

			AssertEquals("IsStatusClear", true, list.IsStatusClear(ReconMessageStatusList.Codes.ClearReconDelete));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(ReconMessageStatusList.Codes.ErrorReconDelete));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(ReconMessageStatusList.Codes.ClearReconOriginal));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(ReconMessageStatusList.Codes.ErrorReconOriginal));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(ReconMessageStatusList.Codes.ClearReconReplace));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(ReconMessageStatusList.Codes.ErrorReconReplace));

			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(ReconMessageStatusList.Codes.AwaitingReconDelete));
			AssertEquals("IsWaitingForResponse", false, list.IsWaitingForResponse(ReconMessageStatusList.Codes.ErrorReconDelete));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(ReconMessageStatusList.Codes.AwaitingReconOriginal));
			AssertEquals("IsWaitingForResponse", false, list.IsWaitingForResponse(ReconMessageStatusList.Codes.ErrorReconOriginal));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(ReconMessageStatusList.Codes.AwaitingReconReplace));
			AssertEquals("IsWaitingForResponse", false, list.IsWaitingForResponse(ReconMessageStatusList.Codes.ErrorReconReplace));

			AssertEquals("IsWithdrawalStatus", false, list.IsWithdrawnStatus(ReconMessageStatusList.Codes.AwaitingReconDelete));
			AssertEquals("IsWithdrawalStatus", false, list.IsWithdrawnStatus(ReconMessageStatusList.Codes.AwaitingReconOriginal));
			AssertEquals("IsWithdrawalStatus", false, list.IsWithdrawnStatus(ReconMessageStatusList.Codes.ErrorReconDelete));
			AssertEquals("IsWithdrawalStatus", false, list.IsWithdrawnStatus(ReconMessageStatusList.Codes.ErrorReconOriginal));
			AssertEquals("IsWithdrawalStatus", true, list.IsWithdrawnStatus(ReconMessageStatusList.Codes.ClearReconDelete));
			AssertEquals("IsWithdrawalStatus", false, list.IsWithdrawnStatus(ReconMessageStatusList.Codes.ErrorReconReplace));
		}

		public void TestAcceptedStatusToCancelRejectStatusInterested()
		{
			var list = new ReconMessageStatusList();
			AssertEquals("AcceptedStatusToCancelRejectStatusInterested", 3, list.AcceptedStatusToCancelRejectStatusInterested.Count);
			AssertEquals("Should have ClearReconDelete status", ReconMessageStatusList.Codes.ClearReconDelete, list.AcceptedStatusToCancelRejectStatusInterested[0]);
			AssertEquals("Should have ClearReconOriginal status", ReconMessageStatusList.Codes.ClearReconOriginal, list.AcceptedStatusToCancelRejectStatusInterested[1]);
			AssertEquals("Should have ClearReconReplace status", ReconMessageStatusList.Codes.ClearReconReplace, list.AcceptedStatusToCancelRejectStatusInterested[2]);
		}

		public void TestRejectStatusInterested()
		{
			var list = new ReconMessageStatusList();
			AssertEquals("RejectStatusInterested", 3, list.RejectStatusInterested.Count);
			AssertEquals("Should have ErrorReconDelete status", ReconMessageStatusList.Codes.ErrorReconDelete, list.RejectStatusInterested[0]);
			AssertEquals("Should have ErrorReconOriginal status", ReconMessageStatusList.Codes.ErrorReconOriginal, list.RejectStatusInterested[1]);
			AssertEquals("Should have ErrorReconReplace status", ReconMessageStatusList.Codes.ErrorReconReplace, list.RejectStatusInterested[2]);
		}

		public void TestGetCachedReconMessageStatusList()
		{
			var factory = new BusinessObjectFactory();
			var list1 = ReconMessageStatusList.GetCachedReconMessageStatusList(factory);
			var list2 = ReconMessageStatusList.GetCachedReconMessageStatusList(factory);
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			var list3 = ReconMessageStatusList.GetCachedReconMessageStatusList(new BusinessObjectFactory());
			AssertEquals(false, object.ReferenceEquals(list1, list3));
		}
	}
}
