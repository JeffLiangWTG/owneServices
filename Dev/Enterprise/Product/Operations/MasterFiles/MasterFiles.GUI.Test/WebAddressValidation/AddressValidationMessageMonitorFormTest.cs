using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business.Testing;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class AddressValidationMessageMonitorFormTest : TestCaseWithFactory
	{
		public void TestAddressValidationServiceMessageWriter()
		{
			using (AddressValidationMessageMonitorForm form = new AddressValidationMessageMonitorForm())
			{
				form.Show();
				form.ButtonToggleStartStop.PerformClick();
				AddressValidationService.WriteMessage("I don't know why I can't write messages before..");
				Assert("Message written", form.TextBoxStackTrace.Text.Contains("I don't know why I can't write messages before.."));

				form.ButtonToggleStartStop.PerformClick();
				AddressValidationService.WriteMessage("Wait! Not again!");
				Assert("Message not written", !form.TextBoxStackTrace.Text.Contains("Wait! Not again!"));
				AddressValidationService.MessageWriter = null;
				AddressValidationMessageMonitorForm.MessageWriter = null;
			}
		}

		public void TestMonitorFormToggleStartAndStopTrackingWithOneButton()
		{
			using (OrganisationsDataRegistryTestHelper.SetTemporaryValueForAddressValidationWebServiceURIs(value =>
			{
				value.Primary.ServiceUri = "https://testprimary.webservice.com/";
				value.Secondary.ServiceUri = "https://testsecondary.webservice.com/";
			}, Factory))
			using (AddressValidationMessageMonitorForm form = new AddressValidationMessageMonitorForm())
			{
				AddressValidationMessageMonitorForm.IsTrackingMessage = false;

				try
				{
					form.Show();

					AddressValidationMessageMonitorForm.MessageWriter.WriteMessage("test");
					AssertEquals("Start tracking address validation messages", form.ButtonToggleStartStop.Text);
					AssertEquals("Should not tracking message", false, AddressValidationMessageMonitorForm.IsTrackingMessage);
					AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);

					form.ButtonToggleStartStop.PerformClick();
					AddressValidationMessageMonitorForm.MessageWriter.WriteMessage("test");
					AssertEquals("Stop tracking messages", form.ButtonToggleStartStop.Text);
					AssertEquals("Should tracking message", true, AddressValidationMessageMonitorForm.IsTrackingMessage);
					AssertContains("Should have message", "Start tracking address validation messages at", form.TextBoxStackTrace.Text);
					AssertContains(
						"Should have message",
						@"Address validation web service Primary URI: https://testprimary.webservice.com/
Address validation web service Secondary URI: https://testsecondary.webservice.com/

test",
						form.TextBoxStackTrace.Text);

					form.ButtonClear.PerformClick();
					AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);

					form.ButtonToggleStartStop.PerformClick();
					AddressValidationMessageMonitorForm.MessageWriter.WriteMessage("test");
					AssertEquals("Start tracking address validation messages", form.ButtonToggleStartStop.Text);
					AssertEquals("Should not tracking message", false, AddressValidationMessageMonitorForm.IsTrackingMessage);
					AssertContains("Should have message", "Tracking stopped at", form.TextBoxStackTrace.Text);
					AssertNotContains("Should not have message", "test", form.TextBoxStackTrace.Text);
				}
				finally
				{
					AddressValidationMessageMonitorForm.MessageWriter = null;
					AddressValidationService.MessageWriter = null;
					AddressValidationMessageMonitorForm.IsTrackingMessage = false;
				}
			}
		}

		public void TestFormCaption()
		{
			using (var form = new AddressValidationMessageMonitorForm())
			{
				AssertEquals("Address Validation Message Monitor", form.Text);
				AddressValidationMessageMonitorForm.MessageWriter = null;
			}
		}
	}
}
