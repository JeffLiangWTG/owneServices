using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class AMSMessageSubTypeListTest : TestCaseWithFactory
	{
		public void TestGetSubTypeFromActionCode()
		{
			AssertEquals(AMSMessageSubTypeList.Codes.AmendingAdd, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.AmendingAdd));
			AssertEquals(AMSMessageSubTypeList.Codes.AmendingUpdate, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.AmendingUpdate));
			AssertEquals(AMSMessageSubTypeList.Codes.AmendingDelete, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.AmendingDelete));
			AssertEquals(AMSMessageSubTypeList.Codes.Creating, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.Creating));
			AssertEquals(AMSMessageSubTypeList.Codes.Equipment, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.Equipment));
			AssertEquals(AMSMessageSubTypeList.Codes.GeneralOrderStatus, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.GeneralOrderStatus));
			AssertEquals(AMSMessageSubTypeList.Codes.InBondArrival, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.InBondArrival));
			AssertEquals(AMSMessageSubTypeList.Codes.PermitToTransfer, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.PermitToTransfer));
			AssertEquals(AMSMessageSubTypeList.Codes.CancelPermitToTransfer, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.CancelPermitToTransfer));
			AssertEquals(AMSMessageSubTypeList.Codes.VesselArrival, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.VesselArrival));
			AssertEquals(AMSMessageSubTypeList.Codes.VesselDeparture, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.VesselDeparture));
			AssertEquals(AMSMessageSubTypeList.Codes.ChangeEstimatedDateOfArrival, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.ChangeEstDateOfArrival));
			AssertEquals(AMSMessageSubTypeList.Codes.SubsequentInBondAmendment, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.SubsequentInBondAmendment));
			AssertEquals(AMSMessageSubTypeList.Codes.SubsequentInBondDelete, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.SubsequentInBondDelete));
			AssertEquals(AMSMessageSubTypeList.Codes.SubsequentInBondOriginal, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.SubsequentInBondOriginal));
			AssertEquals(AMSMessageSubTypeList.Codes.InBondDiversion, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.InBondDiversion));
			AssertEquals(AMSMessageSubTypeList.Codes.InBondExportation, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.InBondExportation));
			AssertEquals(AMSMessageSubTypeList.Codes.InBondTransferOfLiability, AMSMessageSubTypeList.GetSubTypeFromActionCode(ActionCode.InBondTransferOfLiability));
		}

		public void TestGetActionCodeFromSubType()
		{
			AssertEquals(ActionCode.AmendingAdd, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.AmendingAdd));
			AssertEquals(ActionCode.AmendingUpdate, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.AmendingUpdate));
			AssertEquals(ActionCode.AmendingDelete, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.AmendingDelete));
			AssertEquals(ActionCode.Creating, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.Creating));
			AssertEquals(ActionCode.Equipment, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.Equipment));
			AssertEquals(ActionCode.GeneralOrderStatus, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.GeneralOrderStatus));
			AssertEquals(ActionCode.InBondArrival, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.PaperlessInBondOrVesselArrival));
			AssertEquals(ActionCode.PermitToTransfer, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.PermitToTransfer));
			AssertEquals(ActionCode.SubsequentInBondAmendment, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.SubsequentInBondAmendment));
			AssertEquals(ActionCode.SubsequentInBondDelete, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.SubsequentInBondDelete));
			AssertEquals(ActionCode.SubsequentInBondOriginal, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.SubsequentInBondOriginal));
			AssertEquals(ActionCode.InBondArrival, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.InBondArrival));
			AssertEquals(ActionCode.InBondDiversion, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.InBondDiversion));
			AssertEquals(ActionCode.InBondExportation, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.InBondExportation));
			AssertEquals(ActionCode.InBondTransferOfLiability, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.InBondTransferOfLiability));
			AssertEquals(ActionCode.CancelPermitToTransfer, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.CancelPermitToTransfer));
			AssertEquals(ActionCode.VesselArrival, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.VesselArrival));
			AssertEquals(ActionCode.VesselDeparture, AMSMessageSubTypeList.GetActionCodeFromSubType(AMSMessageSubTypeList.Codes.VesselDeparture));
		}

		public void TestIsVesselArrivalEventRelevent()
		{
			AssertEquals(true, AMSMessageSubTypeList.IsVesselArrivalEventRelevent(AMSMessageSubTypeList.Codes.ChangeEstimatedDateOfArrival));
			AssertEquals(true, AMSMessageSubTypeList.IsVesselArrivalEventRelevent(AMSMessageSubTypeList.Codes.VesselArrival));
			AssertEquals(false, AMSMessageSubTypeList.IsVesselArrivalEventRelevent(AMSMessageSubTypeList.Codes.VesselDeparture));
		}
	}
}
