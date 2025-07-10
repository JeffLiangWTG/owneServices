using System.Reflection;
using Enterprise.Freight.Business.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Forwarding.GUI.Testing
{
	public class OrderManualShipmentNumberEntryFormTest : BaseFreightTest
	{
		#region TestCancelButton

		public void TestCancelButton()
		{
			ShipmentNumberEntry entry = GetEntry();

			using (var form = new OrderManualShipmentNumberEntryForm(entry))
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

			using (var form = new OrderManualShipmentNumberEntryForm(entry))
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
			string uniqueConsignerRef = "SHSHSHSHSHSHH";

			ShipmentNumberEntry entry = GetEntry();
			using (var form = new OrderManualShipmentNumberEntryForm(entry))
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

		void PressCancelButton(OrderManualShipmentNumberEntryForm form)
		{
			var button = (ZButton)typeof(OrderManualShipmentNumberEntryForm).GetField(Cancel_Button, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
			button.PerformClick();
		}

		void PressOkButton(OrderManualShipmentNumberEntryForm form)
		{
			var button = (ZButton)typeof(OrderManualShipmentNumberEntryForm).GetField(Ok_Button, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(form);
			button.PerformClick();
		}

		#endregion
	}
}
