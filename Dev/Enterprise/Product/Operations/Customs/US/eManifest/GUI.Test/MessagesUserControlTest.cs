using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;
using EventReferenceParameters = CargoWise.EventReference.Constants.EventReferenceParameters;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	sealed class MessagesUserControlTest : TestCaseWithFactory
	{
		public void TestReorderMessageUserControlTabPages_WheneManifestCreatedFromHVLV()
		{
			var trip = Factory.New<Trip>();
			using (var form = new ZForm(trip))
			using (var messageUserControl = new MessagesUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				var zTabControl = form.Controls.Find("MessageTabControl", true)[0] as ZTabControl;

				AssertEquals("Text and Details should not reorder", "Message Details", zTabControl.TabPages[0].Text);
				AssertEquals("Text and Details should not reorder", "Message Text", zTabControl.TabPages[1].Text);
			}

			trip.Logs.AddNew(AutoEvents.Transferred, new KeyValuePair<string, string>[] {
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.Type, "HVL"),
						new KeyValuePair<string, string>(EventReferenceParameters.Codes.JobNumber, "job123")
			});
			Factory.Save();

			using (var form = new ZForm(trip))
			using (var messageUserControl = new MessagesUserControl())
			{
				form.Controls.Add(messageUserControl);
				form.Show();

				var zTabControl = form.Controls.Find("MessageTabControl", true)[0] as ZTabControl;

				AssertEquals("HVLV, Text and Details reorder", "Message Text", zTabControl.TabPages[0].Text);
				AssertEquals("HVLV, Text and Details should reorder", "Message Details", zTabControl.TabPages[1].Text);
			}
		}
	}
}
