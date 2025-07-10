using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class DeniedPartyScreeningMonitoringFormTest : TestCaseWithFactory
	{
		public void TestDeniedPartyScreeningMessageWriter()
		{
			using (var form = GetShownFormWithNotTrackingStatus())
			{
				try
				{
					form.ButtonToggleStartStop.PerformClick();
					DeniedPartyScreenerAsync.WriteMessage("I don't know why I can't write messages before..");
					Assert("Message written", form.TextBoxStackTrace.Text.Contains("I don't know why I can't write messages before.."));

					form.ButtonToggleStartStop.PerformClick();
					DeniedPartyScreenerAsync.WriteMessage("Wait! Not again!");
					Assert("Message not written", !form.TextBoxStackTrace.Text.Contains("Wait! Not again!"));
				}
				finally
				{
					DeniedPartyScreenerAsync.MessageWriter = null;
				}
			}
		}

		public void TestMonitorFormToggleStartAndStopTrackingWithOneButton()
		{
			using (var form = GetShownFormWithNotTrackingStatus())
			{
				try
				{
					form.MessageWriter.WriteMessage("test");
					AssertEquals("Start tracking Denied Party Screening", form.ButtonToggleStartStop.Text);
					AssertEquals("Should not tracking message", false, DeniedPartyScreeningMonitoringForm.IsTrackingMessage);
					AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);

					form.ButtonToggleStartStop.PerformClick();
					form.MessageWriter.WriteMessage("test");
					AssertEquals("Stop tracking messages", form.ButtonToggleStartStop.Text);
					AssertEquals("Should tracking message", true, DeniedPartyScreeningMonitoringForm.IsTrackingMessage);
					AssertContains("Should have message", "Start tracking Denied Party Screening at", form.TextBoxStackTrace.Text);

					form.ButtonClear.PerformClick();
					AssertEquals("Should not have tracking message", string.Empty, form.TextBoxStackTrace.Text);

					form.ButtonToggleStartStop.PerformClick();
					form.MessageWriter.WriteMessage("test");
					AssertEquals("Start tracking Denied Party Screening", form.ButtonToggleStartStop.Text);
					AssertEquals("Should not tracking message", false, DeniedPartyScreeningMonitoringForm.IsTrackingMessage);
					AssertContains("Should have message", "Tracking stopped at", form.TextBoxStackTrace.Text);
					AssertNotContains("Should not have message", "test", form.TextBoxStackTrace.Text);
				}
				finally
				{
					DeniedPartyScreenerAsync.MessageWriter = null;
				}
			}
		}

		DeniedPartyScreeningMonitoringForm GetShownFormWithNotTrackingStatus()
		{
			var form = new DeniedPartyScreeningMonitoringForm();
			form.Show();

			if (DeniedPartyScreeningMonitoringForm.IsTrackingMessage)
			{
				form.ButtonToggleStartStop.PerformClick();
				form.TextBoxStackTrace.Clear();
			}

			return form;
		}
	}
}
