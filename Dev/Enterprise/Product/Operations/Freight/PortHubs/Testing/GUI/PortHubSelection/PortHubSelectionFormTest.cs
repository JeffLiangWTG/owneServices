using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Freight.PortHubs.Business;
using Enterprise.Freight.PortHubs.GUI;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.PortHubs.Testing
{
	[TestedType(typeof(PortHubSelectionForm))]
	public class PortHubSelectionFormTest : ZFormBasherTest
	{
		protected override Form GetFormToBashCore()
		{
			return new PortHubSelectionForm(new PortHubSelectionCollectionWrapper(Factory));
		}

		public void TestFormHeading()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				AssertEquals("Port and Depot Selection", form.FormHeading);
				form.Show();
				AssertEquals("Port and Depot Selection", form.Text);
			}
		}

		public void TestFormControlsAutoResizing()
		{
			using (var form = GetFormToBashCore() as ZForm)
			{
				form.Show();
				var portHubFilterStripControl = form.FindSingle<UserControl>("portHubFilterStripControl");
				Assert(portHubFilterStripControl.AutoScroll);
				Assert(portHubFilterStripControl.AutoSize);
				AssertEquals(new System.Drawing.Size(0, 70), portHubFilterStripControl.MinimumSize);
				var portsGrid = form.FindSingle<Control>("PortsGrid");
				AssertEquals(System.Windows.Forms.DockStyle.Fill, portsGrid.Dock);
				var zonesGroupBox = form.FindSingle<Control>("ZonesGroupBox");
				AssertEquals(System.Windows.Forms.DockStyle.Fill, zonesGroupBox.Dock);
			}
		}

		public void TestSecurity()
		{
			Env.Security.PortDepotSelectionModify.IsAllowed = false;
			using (var form = new PortHubSelectionForm(new PortHubSelectionCollectionWrapper(Factory)))
			{
				var grid = form.FindSingle<ZGrid>("PortsGrid");
				Assert(grid.ReadOnly);
			}

			Env.Security.PortDepotSelectionModify.IsAllowed = true;
			using (var form = new PortHubSelectionForm(new PortHubSelectionCollectionWrapper(Factory)))
			{
				var grid = form.FindSingle<ZGrid>("PortsGrid");
				Assert(!grid.ReadOnly);
			}
		}

		public void TestCollectionOnSaveWithFilters()
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
			portHubSelection1.TY_Direction = PortHubSelectionDirectionList.Codes.Pickup;
			portHubSelection1.TY_OA_DepotAddress = depot1.MainAddress.PK;

			PortHubSelection portHubSelection2 = collection.AddNew();
			portHubSelection2.TY_Direction = PortHubSelectionDirectionList.Codes.Delivery;
			portHubSelection2.TY_OA_DepotAddress = depot2.MainAddress.PK;
			Factory.Save();

			using (PortHubSelectionForm form = new PortHubSelectionForm(collectionWrapper))
			{
				form.Show();

				var filterBizO = new PortHubFilterStripBusinessObject();
				var filters = new FilterBusinessObjectDefaults();
				filters.Add(new FilterBusinessObjectDefault(PortHubFilterStripBusinessObject.Descriptions.Direction, "Property", (ZString)PortHubSelectionDirectionList.Codes.Pickup));

				filterBizO.SetExternalDefaults(filters);
				filterBizO.LoadLayout(null);

				var strips = filterBizO.FilterStrips;
				AssertEquals(1, strips.Count);

				var portHubFilterStripControl = form.Controls.Find("portHubFilterStripControl", true).First();
				var filterPanel = portHubFilterStripControl.Controls.Find("filterStripPanel", false).First();
				var stripControl = filterPanel.Controls.OfType<PortHubStripControl>().First();

				stripControl.FilterBusinessObject.FilterStrips.RemoveAndDeleteAll();
				stripControl.FilterBusinessObject.FilterStrips.Add(strips[0]);

				portHubSelection2.TY_OA_DepotAddress = depot1.MainAddress.PK;

				Assert("pre-requisite", strips[0].CurrentModuleFilter.IsActive);
				form.FireSaveButton();

				AssertContainsExactElementsInAnyOrder("Collection is loaded on save", new[] { portHubSelection1, portHubSelection2 }, collection);
				Assert("Current filters are cleared on save", strips[0].CurrentModuleFilter.IsEmpty);
			}
		}
	}
}
