using NUnit.Framework;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class FTZMessageStatusListTest : TestCase
	{
		public void TestIStatusList()
		{
			var list = new FTZMessageStatusList();

			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.ClearFTZAdmissionAdd));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.ClearFTZAdmissionAmend));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.ClearFTZAdmissionDelete));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAmend));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.ClearConcurrence));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.ClearDeliveryOfGoods));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.ClearGoodsArrival));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.ClearPermitToTransfer));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.PermitToTransferArrived));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.ClearUnconcurrence));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.PermitToTransferCancelAccepted));
			AssertEquals("IsStatusClear", true, list.IsStatusClear(FTZMessageStatusList.Codes.PermitToTransferUnArrived));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(FTZMessageStatusList.Codes.ErrorDeliveryOfGoods));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(FTZMessageStatusList.Codes.PermitToTransferCancelUnauthorized));
			AssertEquals("IsStatusClear", false, list.IsStatusClear(FTZMessageStatusList.Codes.PermitToTransferArrivalCancelUnauthorized));

			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAdd));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(FTZMessageStatusList.Codes.AwaitingFTZAdmissionAmend));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(FTZMessageStatusList.Codes.AwaitingFTZAdmissionDelete));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(FTZMessageStatusList.Codes.AwaitingCancelPermitToTransfer));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(FTZMessageStatusList.Codes.AwaitingPermitToTransferUnArrival));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(FTZMessageStatusList.Codes.AwaitingPermitToTransfer));
			AssertEquals("IsWaitingForResponse", true, list.IsWaitingForResponse(FTZMessageStatusList.Codes.AwaitingPermitToTransferArrival));
			AssertEquals("IsWaitingForResponse", false, list.IsWaitingForResponse(FTZMessageStatusList.Codes.ClearFTZAdmissionAmend));
			AssertEquals("IsWaitingForResponse", false, list.IsWaitingForResponse(FTZMessageStatusList.Codes.ClearGoodsArrival));
		}

		public void TestAcceptedStatusToCancelRejectStatusInterested()
		{
			var list = new FTZMessageStatusList();
			AssertEquals("AcceptedStatusToCancelRejectStatusInterested", 12, list.AcceptedStatusToCancelRejectStatusInterested.Count);
			AssertEquals("ClearFTZAdmissionAdd", FTZMessageStatusList.Codes.ClearFTZAdmissionAdd, list.AcceptedStatusToCancelRejectStatusInterested[0]);
			AssertEquals("ClearFTZAdmissionAdd", FTZMessageStatusList.Codes.ClearFTZAdmissionAddWithWarnings, list.AcceptedStatusToCancelRejectStatusInterested[1]);
			AssertEquals("ClearFTZAdmissionAmend", FTZMessageStatusList.Codes.ClearFTZAdmissionAmend, list.AcceptedStatusToCancelRejectStatusInterested[2]);
			AssertEquals("ClearFTZAdmissionDelete", FTZMessageStatusList.Codes.ClearFTZAdmissionDelete, list.AcceptedStatusToCancelRejectStatusInterested[3]);
			AssertEquals("ClearConcurrence", FTZMessageStatusList.Codes.ClearConcurrence, list.AcceptedStatusToCancelRejectStatusInterested[4]);
			AssertEquals("ClearDeliveryOfGoods", FTZMessageStatusList.Codes.ClearDeliveryOfGoods, list.AcceptedStatusToCancelRejectStatusInterested[5]);
			AssertEquals("ClearGoodsArrival", FTZMessageStatusList.Codes.ClearGoodsArrival, list.AcceptedStatusToCancelRejectStatusInterested[6]);
			AssertEquals("ClearPermitToTransfer", FTZMessageStatusList.Codes.ClearPermitToTransfer, list.AcceptedStatusToCancelRejectStatusInterested[7]);
			AssertEquals("PermitToTransferArrived", FTZMessageStatusList.Codes.PermitToTransferArrived, list.AcceptedStatusToCancelRejectStatusInterested[8]);
			AssertEquals("ClearUnconcurrence", FTZMessageStatusList.Codes.ClearUnconcurrence, list.AcceptedStatusToCancelRejectStatusInterested[9]);
			AssertEquals("PermitToTransferCancelAccepted", FTZMessageStatusList.Codes.PermitToTransferCancelAccepted, list.AcceptedStatusToCancelRejectStatusInterested[10]);
			AssertEquals("PermitToTransferUnArrived", FTZMessageStatusList.Codes.PermitToTransferUnArrived, list.AcceptedStatusToCancelRejectStatusInterested[11]);
		}

		public void TestRejectStatusInterested()
		{
			var list = new FTZMessageStatusList();
			AssertEquals("RejectStatusInterested", 11, list.RejectStatusInterested.Count);
			AssertEquals("ErrorFTZAdmissionAdd", FTZMessageStatusList.Codes.ErrorFTZAdmissionAdd, list.RejectStatusInterested[0]);
			AssertEquals("ErrorFTZAdmissionAmend", FTZMessageStatusList.Codes.ErrorFTZAdmissionAmend, list.RejectStatusInterested[1]);
			AssertEquals("ErrorFTZAdmissionDelete", FTZMessageStatusList.Codes.ErrorFTZAdmissionDelete, list.RejectStatusInterested[2]);
			AssertEquals("ErrorConcurrence", FTZMessageStatusList.Codes.ErrorConcurrence, list.RejectStatusInterested[3]);
			AssertEquals("ErrorDeliveryOfGoods", FTZMessageStatusList.Codes.ErrorDeliveryOfGoods, list.RejectStatusInterested[4]);
			AssertEquals("ErrorGoodsArrival", FTZMessageStatusList.Codes.ErrorGoodsArrival, list.RejectStatusInterested[5]);
			AssertEquals("ErrorPermitToTransfer", FTZMessageStatusList.Codes.ErrorPermitToTransfer, list.RejectStatusInterested[6]);
			AssertEquals("ErrorPermitToTransferArrival", FTZMessageStatusList.Codes.ErrorPermitToTransferArrival, list.RejectStatusInterested[7]);
			AssertEquals("ErrorUnconcurrence", FTZMessageStatusList.Codes.ErrorUnconcurrence, list.RejectStatusInterested[8]);
			AssertEquals("PermitToTransferCancelUnauthorized", FTZMessageStatusList.Codes.PermitToTransferCancelUnauthorized, list.RejectStatusInterested[9]);
			AssertEquals("PermitToTransferArrivalCancelUnauthorized", FTZMessageStatusList.Codes.PermitToTransferArrivalCancelUnauthorized, list.RejectStatusInterested[10]);
		}
	}
}
