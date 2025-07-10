using Enterprise.MasterFiles.Integration;
using Enterprise.TransportConsignment.Business;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class DtbConsignmentRunSheetDetailsUserControlTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestHookContextMenuShowSignature

		public void TestHookContextMenuShowSignature()
		{
			var runSheet = Helper.CreateRunSheet();
			var runSheetInstruction = Helper.CreateRunSheetInstruction(runSheet, Helper.CreateBookingConsignmentWithTemplateAndAddresses().PickupInstruction.PickupConfirmation);

			using (var form = new ZForm(runSheet))
			{
				var userControl = new DtbConsignmentRunSheetDetailsUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var menu = userControl.ConfirmationsGrid.ContextMenu.MenuItems.FindByText("Show Signature");
				AssertNotNull(menu);
			}
		}

		#endregion

		#region TestHideConfirmationTab_ShowActionTab

		public void TestHideConfirmationTab_ShowActionTab()
		{
			var helperLTC = new TransportConsignmentTestHelper(Factory);
			var consignment = helperLTC.CreateConsignment("LTC001");
			var pickupAddress = helperLTC.CreateConsignmentAddressWithAction(consignment, ConsignmentAddressTypes.Codes.PickUp, ActionTypes.Codes.PickUp, DocAddressType.LocalCartageCFS);
			var runSheet = helperLTC.CreateRunSheet();
			var runSheetInstruction = helperLTC.CreateRunSheetInstruction(runSheet, pickupAddress.Actions[0]);

			using (var form = new ZForm(runSheet))
			{
				var userControl = new DtbConsignmentRunSheetDetailsUserControl();
				form.Controls.Add(userControl);
				form.Show();

				AssertEquals(true, userControl.ActionsTabPage.TabVisible);
				AssertEquals(false, userControl.ConfirmationsTabPage.TabVisible);
			}
		}

		#endregion

		#region TestHideActionTab_ShowConfirmationTab

		public void TestHideActionTab_ShowConfirmationTab()
		{
			var runSheet = Helper.CreateRunSheet();
			var runSheetInstruction = Helper.CreateRunSheetInstruction(runSheet, Helper.CreateBookingConsignmentWithTemplateAndAddresses().PickupInstruction.PickupConfirmation);

			using (var form = new ZForm(runSheet))
			{
				var userControl = new DtbConsignmentRunSheetDetailsUserControl();
				form.Controls.Add(userControl);
				form.Show();

				AssertEquals(true, userControl.ConfirmationsTabPage.TabVisible);
				AssertEquals(false, userControl.ActionsTabPage.TabVisible);
			}
		}

		#endregion
	}
}
