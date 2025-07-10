using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	class DtbRoutePlannerDetailsUserControlTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestSelectedConfirmations

		public void TestSelectedConfirmations()
		{
			var consignment = Helper.CreateBookingConsignmentWithTemplate();
			var address = Helper.CreateOrganisation("123").MainAddress;
			var addressPoint = Helper.CreateAddressPoint(address, consignment.PickupInstruction.PickupConfirmation, consignment.DeliveryInstruction.DeliveryConfirmation);

			using (var form = new ZForm(addressPoint))
			{
				var control = new TestDtbRoutePlannerDetailsUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", 0, control.SelectedConfirmations.Length);

				control.ConfirmationGridExposed.SelectAllElements();
				AssertContainsExactElementsInAnyOrder(new[] { consignment.PickupInstruction.PickupConfirmation, consignment.DeliveryInstruction.DeliveryConfirmation }, control.SelectedConfirmations);
				control.ConfirmationGridExposed.SelectSingleElement(consignment.PickupInstruction.PickupConfirmation);
				AssertContainsExactElementsInAnyOrder(new[] { consignment.PickupInstruction.PickupConfirmation }, control.SelectedConfirmations);
			}
		}

		#endregion

		#region Implementation

		class TestDtbRoutePlannerDetailsUserControl : DtbRoutePlannerDetailsUserControl
		{
			public ZGrid ConfirmationGridExposed
			{
				get { return ConfirmationsGrid; }
			}
		}

		#endregion
	}
}
