using System.Linq;
using System.Windows.Forms;
using Enterprise.Customs.US.Business;
using Enterprise.Customs.US.LVS.Business;
using Enterprise.Customs.US.Messaging.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.US.LVS.GUI.Testing
{
	class EDIMenuForSingleBillTest : EDIMenuTest
	{
		public void TestOnlyUpdateSingleConsignmentActionWhenSendMessage()
		{
			var consignment2 = header.CusUSLVConsignments.AddNew();

			Factory.Save();

			using (var testMenu = GetEDIMenu())
			using (var form = new ZForm(consignment))
			{
				form.Menu.MenuItems.Add(testMenu);
				var item = GetSendOriginalMessageMenuItem(testMenu);
				AssertNotNull(item);

				consignment.CE_EntryStatus = "";
				item.PerformClick();

				AssertEquals(UpdateActionCode.Add, consignment.Action);
				AssertNull(consignment2.Action);
			}
		}

		public void TestCollectionContainsSingleConsignment()
		{
			var header = Factory.New<CusUSLVClearance>();
			var currentConsignment = header.CusUSLVConsignments.AddNew();
			var otherConsignment = header.CusUSLVConsignments.AddNew();
			header.PrepareCusUSLVConsignmentsForUpdateAction(UpdateActionCode.Replace);

			currentConsignment.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;
			otherConsignment.CE_EntryStatus = CRLReleaseStatusList.Codes.ADM;

			var testMenuForSingleConsignment = new EDIMenuForSingleBillForTesting { Consignment = currentConsignment };
			var clearanceMessageWrapper = testMenuForSingleConsignment.GetMessageWrapper_ForTest();
			var selectedConsignments = clearanceMessageWrapper.CusUSLVConsignmentsToSend.Cast<CusUSLVConsignmentForMessaging>().Select(c => c.Consignment);
			AssertContainsExactElementsInAnyOrder(new[] { currentConsignment }, selectedConsignments);
		}

		#region Implementation

		protected override MenuItem GetSendOriginalMessageMenuItem(EDIMenu ediMenu) => ediMenu.MenuItems.FindByText("Send Original Message");

		protected override MenuItem GetSendReplacementMessageMenuItem(EDIMenu ediMenu) => ediMenu.MenuItems.FindByText("Send Replacement Message");

		protected override MenuItem GetSendUpdateMessageMenuItem(EDIMenu ediMenu) => ediMenu.MenuItems.FindByText("Send Update Message");

		protected override MenuItem GetSendDeletionMessageMenuItem(EDIMenu ediMenu) => ediMenu.MenuItems.FindByText("Send Deletion Message");

		protected override EDIMenu GetEDIMenu()
		{
			return new EDIMenuForSingleBill() { Consignment = consignment };
		}

		class EDIMenuForSingleBillForTesting : EDIMenuForSingleBill
		{
			public EDIMenuForSingleBillForTesting()
				: base()
			{
			}

			public CusUSLVClearanceMessageWrapper GetMessageWrapper_ForTest() => base.GetMessageWrapper();
		}

		#endregion
	}
}
