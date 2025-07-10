using Enterprise.TransportCommon.Shared;
using Enterprise.TransportConsignment.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.TransportConsignment.GUI.Testing
{
	class DtbRoutePlannerDirectModeDetailsUserControlTest : DtbBookingConsignmentTestCaseWithFactory
	{
		#region TestSelectedConfirmations

		public void TestSelectedConfirmations()
		{
			var consignment1 = Helper.CreateBookingConsignmentWithTemplate();
			var pickupConfirmation1 = consignment1.PickupInstruction.PickupConfirmation;
			var deliveryConfirmation1 = consignment1.DeliveryInstruction.DeliveryConfirmation;

			var consignment2 = Helper.CreateBookingConsignment();
			var pickupInstruction2 = Helper.CreateInstruction(consignment2, InstructionTypes.Codes.PickUp);
			var deliveryInstruction2 = Helper.CreateInstruction(consignment2, InstructionTypes.Codes.Delivery);
			var pickupConfirmation2 = pickupInstruction2.Confirmations[0];
			var deliveryConfirmation2 = deliveryInstruction2.Confirmations[0];

			var address = Helper.CreateOrganisation("123").MainAddress;
			var addressPoint = Helper.CreateAddressPoint(address, pickupConfirmation1, pickupConfirmation2);

			using (var form = new ZForm(addressPoint))
			{
				var control = new TestDtbRoutePlannerDetailsUserControl();
				form.Controls.Add(control);
				form.Show();
				AssertEquals("Precondition", 0, control.SelectedConfirmations.Length);

				control.ConfirmationGridExposed.SelectAllElements();
				AssertContainsExactElementsInAnyOrder(new[] { pickupConfirmation1, deliveryConfirmation1, pickupConfirmation2, deliveryConfirmation2 }, control.SelectedConfirmations);
				control.ConfirmationGridExposed.SelectSingleElement(pickupConfirmation1);
				AssertContainsExactElementsInAnyOrder(new[] { pickupConfirmation1, deliveryConfirmation1 }, control.SelectedConfirmations);
			}
		}

		#endregion

		#region Implementation

		class TestDtbRoutePlannerDetailsUserControl : DtbRoutePlannerDirectModeDetailsUserControl
		{
			public ZGrid ConfirmationGridExposed
			{
				get { return ConfirmationsGrid; }
			}
		}

		#endregion
	}
}
