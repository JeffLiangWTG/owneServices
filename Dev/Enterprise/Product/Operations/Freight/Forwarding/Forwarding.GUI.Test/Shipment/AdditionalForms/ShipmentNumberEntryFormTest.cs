using System.Reflection;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class ShipmentNumberEntryFormTest : BaseFreightTest
	{
		#region TestShowForm

		public void TestShowForm()
		{
			ForwardingShipment shipment = GetShipment();
			bool result = ShipmentNumberEntryForm.ShowForm(shipment);

			AssertEquals("Was not accepted", false, result);
			AssertNotNull("Should have shown a form", ZFormModaliser.LastFormShownDialogForTest);
			AssertEquals("Should have shown the correct form.", typeof(ShipmentNumberEntryForm), ZFormModaliser.LastFormShownDialogForTest.GetType());

			ShipmentNumberEntry.AutoAcceptNextForTesting = true;
			result = ShipmentNumberEntryForm.ShowForm(shipment);
			AssertEquals("Was accepted", true, result);
		}

		#endregion

		#region TestCancelButton

		public void TestCancelButton()
		{
			ShipmentNumberEntry entry = GetEntry();

			using (ShipmentNumberEntryForm form = new ShipmentNumberEntryForm(entry))
			{
				form.Show();

				PressCancelButton(form);

				AssertEquals("Should not be accepted", false, entry.Accepted);
				AssertEquals("Form should be closed", false, form.Visible);
			}
		}

		#endregion

		#region TestOkButton_HasErrors

		public void TestOkButton_HasErrors()
		{
			ShipmentNumberEntry entry = GetEntry();

			using (ShipmentNumberEntryForm form = new ShipmentNumberEntryForm(entry))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				PressOkButton(form);

				AssertEquals("Should have Displayed an error message", true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals("Should not be accepted", false, entry.Accepted);
				AssertEquals("Form should not be closed", true, form.Visible);
			}
		}

		#endregion

		#region TestOkButton_NoErrors

		public void TestOkButton_NoErrors()
		{
			string uniqueConsignerRef = "BlahBlahBlah";

			ShipmentNumberEntry entry = GetEntry();
			using (ShipmentNumberEntryForm form = new ShipmentNumberEntryForm(entry))
			{
				form.Show();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				entry.ShipmentNumber = uniqueConsignerRef;
				PressOkButton(form);

				AssertEquals("Should be accepted", true, entry.Accepted);
				AssertEquals("Form should be closed", false, form.Visible);
			}
		}

		#endregion

		#region Implementation

		const string Ok_Button = "Ok_Button";
		const string Cancel_Button = "Cancel_Button";

		ForwardingShipment GetShipment()
		{
			return Factory.New<ForwardingShipment>();
		}

		ShipmentNumberEntry GetEntry()
		{
			return new ShipmentNumberEntry(GetShipment());
		}

		void PressCancelButton(ShipmentNumberEntryForm form)
		{
			ZButton button = (ZButton)typeof(ShipmentNumberEntryForm).GetField(Cancel_Button, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
			button.PerformClick();
		}

		void PressOkButton(ShipmentNumberEntryForm form)
		{
			ZButton button = (ZButton)typeof(ShipmentNumberEntryForm).GetField(Ok_Button, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
			button.PerformClick();
		}

		#endregion
	}
}
