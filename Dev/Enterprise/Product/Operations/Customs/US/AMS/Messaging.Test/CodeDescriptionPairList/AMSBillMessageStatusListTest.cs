using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	sealed class AMSBillMessageStatusListTest : TestCaseWithFactory
	{
		public void TestGetInBondMessageStatusList()
		{
			var list1 = AMSBillMessageStatusList.GetInBondMessageStatusList(Factory);
			var list2 = AMSBillMessageStatusList.GetInBondMessageStatusList(Factory);
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals(13, list1.Count);
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingArrival, AMSBillMessageStatusList.Descriptions.AwaitingArrival, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingDeparture, AMSBillMessageStatusList.Descriptions.AwaitingDeparture, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingDeparture));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingDiversion, AMSBillMessageStatusList.Descriptions.AwaitingDiversion, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingDiversion));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingExportation, AMSBillMessageStatusList.Descriptions.AwaitingExportation, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingExportation));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability, AMSBillMessageStatusList.Descriptions.AwaitingTransferOfLiability, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearArrival, AMSBillMessageStatusList.Descriptions.ClearArrival, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDeparture, AMSBillMessageStatusList.Descriptions.ClearDeparture, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearDeparture));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDiversion, AMSBillMessageStatusList.Descriptions.ClearDiversion, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearDiversion));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearExportation, AMSBillMessageStatusList.Descriptions.ClearExportation, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearExportation));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearTransferOfLiability, AMSBillMessageStatusList.Descriptions.ClearTransferOfLiability, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearTransferOfLiability));
			AssertEquals(AMSBillMessageStatusList.Codes.Deleted, AMSBillMessageStatusList.Descriptions.Deleted, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Deleted));
			AssertEquals(AMSBillMessageStatusList.Codes.Deleting, AMSBillMessageStatusList.Descriptions.Deleting, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Deleting));
			AssertEquals(AMSBillMessageStatusList.Codes.Error, AMSBillMessageStatusList.Descriptions.Error, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Error));
		}

		public void TestGetVesselMessageStatusList()
		{
			var list1 = AMSBillMessageStatusList.GetVesselMessageStatusList(Factory);
			var list2 = AMSBillMessageStatusList.GetVesselMessageStatusList(Factory);
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals(7, list1.Count);
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingArrival, AMSBillMessageStatusList.Descriptions.AwaitingArrival, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingDeparture, AMSBillMessageStatusList.Descriptions.AwaitingDeparture, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingDeparture));
			AssertEquals(AMSBillMessageStatusList.Codes.Updating, AMSBillMessageStatusList.Descriptions.Updating, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Updating));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearArrival, AMSBillMessageStatusList.Descriptions.ClearArrival, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDeparture, AMSBillMessageStatusList.Descriptions.ClearDeparture, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearDeparture));
			AssertEquals(AMSBillMessageStatusList.Codes.Updated, AMSBillMessageStatusList.Descriptions.Updated, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Updated));
			AssertEquals(AMSBillMessageStatusList.Codes.Error, AMSBillMessageStatusList.Descriptions.Error, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Error));
		}

		public void TestGetInBondStatusList()
		{
			var list1 = AMSBillMessageStatusList.GetInBondStatusList(Factory);
			var list2 = AMSBillMessageStatusList.GetInBondStatusList(Factory);
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals(6, list1.Count);
			AssertEquals(AMSBillMessageStatusList.Codes.ClearArrival, AMSBillMessageStatusList.Descriptions.ClearArrival, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDeparture, AMSBillMessageStatusList.Descriptions.ClearDeparture, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearDeparture));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDiversion, AMSBillMessageStatusList.Descriptions.ClearDiversion, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearDiversion));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearExportation, AMSBillMessageStatusList.Descriptions.ClearExportation, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearExportation));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearTransferOfLiability, AMSBillMessageStatusList.Descriptions.ClearTransferOfLiability, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearTransferOfLiability));
			AssertEquals(AMSBillMessageStatusList.Codes.Deleted, AMSBillMessageStatusList.Descriptions.Deleted, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Deleted));
		}

		public void TestGetSubTypeFromActionCode()
		{
			AssertEquals(AMSBillMessageStatusList.Codes.Added, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.AmendingAdd));
			AssertEquals(AMSBillMessageStatusList.Codes.Updated, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.AmendingUpdate));
			AssertEquals(AMSBillMessageStatusList.Codes.Deleted, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.AmendingDelete));
			AssertEquals(AMSBillMessageStatusList.Codes.Deleted, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.CancelPermitToTransfer));
			AssertEquals(AMSBillMessageStatusList.Codes.Added, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.Creating));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDeparture, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.SubsequentInBondOriginal));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDeparture, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.VesselDeparture));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearArrival, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.InBondArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearArrival, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.VesselArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDiversion, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.InBondDiversion));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearExportation, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.InBondExportation));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearTransferOfLiability, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.InBondTransferOfLiability));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDiversion, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.InBondDiversion));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearPermitToTransfer, AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.PermitToTransfer));
			AssertEquals("", AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.Equipment));
			AssertEquals("", AMSBillMessageStatusList.GetTypeFromActionCode(ActionCode.GeneralOrderStatus));
		}

		public void TestGetInBondCodeList()
		{
			var list1 = AMSBillMessageStatusList.GetInBondCodeList(Factory);
			var list2 = AMSBillMessageStatusList.GetInBondCodeList(Factory);
			AssertEquals(true, object.ReferenceEquals(list1, list2));
			AssertEquals(13, list1.Count);
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingArrival, AMSBillMessageStatusList.Descriptions.AwaitingArrival, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingDeparture, AMSBillMessageStatusList.Descriptions.AwaitingDeparture, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingDeparture));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingDiversion, AMSBillMessageStatusList.Descriptions.AwaitingDiversion, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingDiversion));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingExportation, AMSBillMessageStatusList.Descriptions.AwaitingExportation, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingExportation));
			AssertEquals(AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability, AMSBillMessageStatusList.Descriptions.AwaitingTransferOfLiability, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearArrival, AMSBillMessageStatusList.Descriptions.ClearArrival, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearArrival));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDeparture, AMSBillMessageStatusList.Descriptions.ClearDeparture, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearDeparture));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearDiversion, AMSBillMessageStatusList.Descriptions.ClearDiversion, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearDiversion));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearExportation, AMSBillMessageStatusList.Descriptions.ClearExportation, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearExportation));
			AssertEquals(AMSBillMessageStatusList.Codes.ClearTransferOfLiability, AMSBillMessageStatusList.Descriptions.ClearTransferOfLiability, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.ClearTransferOfLiability));
			AssertEquals(AMSBillMessageStatusList.Codes.Deleted, AMSBillMessageStatusList.Descriptions.Deleted, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Deleted));
			AssertEquals(AMSBillMessageStatusList.Codes.Deleting, AMSBillMessageStatusList.Descriptions.Deleting, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Deleting));
			AssertEquals(AMSBillMessageStatusList.Codes.Error, AMSBillMessageStatusList.Descriptions.Error, list1.GetDescriptionFromCode(AMSBillMessageStatusList.Codes.Error));
		}

		public void TestIsMessagingInProgressType()
		{
			foreach (var code in new[]
			{
				AMSBillMessageStatusList.Codes.Adding,
				AMSBillMessageStatusList.Codes.AwaitingArrival,
				AMSBillMessageStatusList.Codes.Deleting,
				AMSBillMessageStatusList.Codes.AwaitingDeparture,
				AMSBillMessageStatusList.Codes.AwaitingDiversion,
				AMSBillMessageStatusList.Codes.AwaitingExportation,
				AMSBillMessageStatusList.Codes.AwaitingTransferOfLiability,
				AMSBillMessageStatusList.Codes.AwaitingPermitToTransfer,
				AMSBillMessageStatusList.Codes.Updating
			})
			{
				AssertEquals(code, true, AMSBillMessageStatusList.IsMessagingInProgressType(code));
			}
		}
	}
}
