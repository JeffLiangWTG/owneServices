using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.AMS.Messaging.Business.Testing
{
	class AMSBillSendingActionCodeListTest : TestCaseWithFactory
	{
		public void TestGetBillActionCodeList()
		{
			foreach (var actionCode in new[] { ActionCode.SubsequentInBondOriginal, ActionCode.SubsequentInBondAmendment, ActionCode.SubsequentInBondDelete })
			{
				var list = AMSBillSendingActionCodeList.GetBillActionCodeList(Factory, actionCode);
				AssertEquals("Should Be Cached", AMSBillSendingActionCodeList.GetBillActionCodeList(Factory, actionCode), list);
				AssertEquals(2, list.Count);
				AssertEquals(AMSBillSendingActionCodeList.Codes.AddInBondMovement, AMSBillSendingActionCodeList.Descriptions.AddInBondMovement, list.GetDescriptionFromCode(AMSBillSendingActionCodeList.Codes.AddInBondMovement));
				AssertEquals(AMSBillSendingActionCodeList.Codes.CancelSubInBond, AMSBillSendingActionCodeList.Descriptions.CancelSubInBond, list.GetDescriptionFromCode(AMSBillSendingActionCodeList.Codes.CancelSubInBond));
			}

			foreach (var actionCode in new[] { ActionCode.AmendingAdd, ActionCode.AmendingDelete, ActionCode.AmendingUpdate })
			{
				var list = AMSBillSendingActionCodeList.GetBillActionCodeList(Factory, actionCode);
				AssertEquals("Should Be Cached", AMSBillSendingActionCodeList.GetBillActionCodeList(Factory, actionCode), list);
				AssertEquals(4, list.Count);
				AssertEquals(AMSBillSendingActionCodeList.Codes.AddBill, AMSBillSendingActionCodeList.Descriptions.AddBill, list.GetDescriptionFromCode(AMSBillSendingActionCodeList.Codes.AddBill));
				AssertEquals(AMSBillSendingActionCodeList.Codes.DeleteBill, AMSBillSendingActionCodeList.Descriptions.DeleteBill, list.GetDescriptionFromCode(AMSBillSendingActionCodeList.Codes.DeleteBill));
				AssertEquals(AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity, AMSBillSendingActionCodeList.Descriptions.ReplaceManifestQuantity, list.GetDescriptionFromCode(AMSBillSendingActionCodeList.Codes.ReplaceManifestQuantity));
				AssertEquals(AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd, AMSBillSendingActionCodeList.Descriptions.ReplaceEntireBillDeleteAndAdd, list.GetDescriptionFromCode(AMSBillSendingActionCodeList.Codes.ReplaceEntireBillDeleteAndAdd));
			}
		}
	}
}
