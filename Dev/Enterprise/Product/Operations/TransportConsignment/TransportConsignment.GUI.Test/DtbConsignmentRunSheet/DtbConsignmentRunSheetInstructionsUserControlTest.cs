using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	public class DtbConsignmentRunSheetInstructionsUserControlTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestHookContextMenuShowSignature

		public void TestHookContextMenuShowSignature()
		{
			var runSheet = Helper.CreateRunSheet();
			var runSheetInstruction = Helper.CreateRunSheetInstruction(runSheet, Helper.CreateBookingConsignmentWithTemplateAndAddresses().PickupInstruction.PickupConfirmation);

			using (var form = new ZForm(runSheet))
			{
				var userControl = new DtbConsignmentRunSheetInstructionsUserControl();
				form.Controls.Add(userControl);
				form.Show();

				var menu = userControl.InstructionsGrid.ContextMenu.MenuItems.FindByText("Show Signature");
				AssertNotNull(menu);
			}
		}

		#endregion
	}
}
