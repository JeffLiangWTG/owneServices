using System.Windows.Forms;
using Enterprise.Customs.US.eManifest.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Customs.US.eManifest.GUI.Testing
{
	[TestedType(typeof(ManifestForm))]
	sealed class ManifestFormBasherTest : ZFormBasherTest
	{
		public void TestTabOrder()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				var tabControl = ((IFormPlugInsProvider)form).TopLevelTabControl;
				AssertNotNull("TopLevelTabControl", tabControl);
				AssertEquals("AllTabPages.Count", 10, tabControl.Controls.Count);
				AssertEquals("Trip", tabControl.Controls[0].Text);
				AssertEquals("Shipments", tabControl.Controls[1].Text);
				AssertEquals("Messages", tabControl.Controls[2].Text);
				AssertEquals("Status", tabControl.Controls[3].Text);
				AssertEquals("Workflow && Tracking", tabControl.Controls[4].Text);
				AssertEquals("Billing", tabControl.Controls[5].Text);
				AssertEquals("Doc Data", tabControl.Controls[6].Text);
				AssertEquals("eDocs", tabControl.Controls[7].Text);
				AssertEquals("Notes", tabControl.Controls[8].Text);
				AssertEquals("Logs", tabControl.Controls[9].Text);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var trip = Factory.New<Trip>();
			var conveyance = trip.Conveyance;
			var crew = trip.CrewMembers.AddNew(); // to make the "mark as needing validation" test shut up
			Factory.Save();
			var mainForm = new ManifestForm(trip)
			{ ControllerID = ControllerIDs.Customs.US.eManifest };
			mainForm.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1600, 900, true);
			return mainForm;
		}

		protected override bool ExpectedExecuteAllFetchHintsBeforeValidateAll => false;
	}
}
