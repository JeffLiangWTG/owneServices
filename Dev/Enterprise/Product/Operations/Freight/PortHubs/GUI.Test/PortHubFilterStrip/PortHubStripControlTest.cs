using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.PortHubs.GUI.Testing
{
	sealed class PortHubStripControlTest : TestCaseWithFactory
	{
		public void TestFilterStripsPanelDocking()
		{
			using (var control = new PortHubStripControl(new PortHubSelectionCollectionWrapper(Factory)))
			{
				var innerFilterStripsPanel = control.FindSingle<Control>("FilterStripsPanel");
				AssertEquals("FilterStripsPanel should fill its container", DockStyle.Fill, innerFilterStripsPanel.Dock);
			}
		}

		public void TestStripControlFindButtonClick()
		{
			var depot1 = Factory.NewWithValidTestData<OrgHeader>();
			depot1.OH_Code = "DEPOT1";
			var depotAddress1 = depot1.Addresses.AddNew();
			depotAddress1.OA_Address1 = "depot address";

			var depot2 = Factory.NewWithValidTestData<OrgHeader>();
			depot2.OH_Code = "DEPOT2";
			var depotAddress2 = depot2.Addresses.AddNew();
			depotAddress2.OA_Address1 = "depot2 address";

			var collectionWrapper = new PortHubSelectionCollectionWrapper(Factory);
			PortHubSelectionCollection collection = collectionWrapper.Collection;

			PortHubSelection portHubSelection1 = collection.AddNew();
			portHubSelection1.TY_OA_DepotAddress = depot1.MainAddress.PK;

			PortHubSelection portHubSelection2 = collection.AddNew();
			portHubSelection2.TY_OA_DepotAddress = depot2.MainAddress.PK;
			Factory.Save();

			using (PortHubSelectionForm form = new PortHubSelectionForm(collectionWrapper))
			{
				form.Show();
				portHubSelection2.TY_OA_DepotAddress = depot1.MainAddress.PK;

				var portHubFilterStripControl = form.Controls.Find("portHubFilterStripControl", true).First();
				var filterPanel = portHubFilterStripControl.Controls.Find("filterStripPanel", false).First();
				var stripControl = filterPanel.Controls.OfType<PortHubStripControl>().First();

				ZToolStrip toolStrip = (ZToolStrip)stripControl.Controls.Find("ToolStrip", true).First();
				var button = toolStrip.Items.OfType<ZToolStripSplitButton>().First();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				button.PerformButtonClick();

				Assert("pre-requisite", collectionWrapper.HasChanges);
				AssertEquals("There are changes on this form. Please save and try again.", UnitTestUserNotification.Instance.LastMessage.Text);

				Factory.Save();

				Assert("pre-requisite", !collectionWrapper.HasChanges);
				button.PerformButtonClick();

				AssertContainsExactElementsInAnyOrder(new[] { portHubSelection1, portHubSelection2 }, collectionWrapper.Collection);
			}
		}
	}
}
