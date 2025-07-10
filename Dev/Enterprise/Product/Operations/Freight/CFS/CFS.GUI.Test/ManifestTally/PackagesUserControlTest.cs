using System.Reflection;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core;
using Enterprise.Freight.CFS.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.Freight.CFS.GUI.Testing
{
	sealed class PackagesUserControlTest : TestCaseWithFactory
	{
		public void TestNilOutturn()
		{
			var container = Factory.New<TallyContainer>();
			var shipment = container.PackUnpackShipments.AddNew();

			AssertEquals(1, shipment.OuterPackLines.Count);

			var line = shipment.OuterPackLines[0];
			line.JL_PackageCount = 7;
			line.JL_Outturn = 0;

			using (ZForm form = new ZForm(container))
			using (PackagesUserControl control = new PackagesUserControl())
			{
				form.Controls.Add(control);
				control.SetDataBinding(container, string.Empty);
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);

				var nilOutturnButton = control.FindSingle<ZButton>(ctrl => ctrl.Name == "NilOutturnButton");
				nilOutturnButton.PerformClick();
				AssertEquals(0, line.JL_Outturn);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				nilOutturnButton.PerformClick();
				AssertEquals(7, line.JL_Outturn);
			}
		}

		public void TestPlusBindingIsBrokenSoUseDotBinding()
		{
			TallyContainer container = Factory.New<TallyContainer>();

			using (ManifestTallyForm form = new ManifestTallyForm(container))
			{
				form.Show();
				Application.DoEvents();

				PackagesUserControl packagesUserControl = (PackagesUserControl)typeof(ManifestTallyForm).GetField("PackagesUserControl", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
				ZDateEdit control = (ZDateEdit)typeof(PackagesUserControl).GetField("JC_LCLUnpackDateEdit", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(packagesUserControl);
				AssertEquals("Refreshing dont Work when we use Plus Binding", "JC_LCLUnpack", control.BindTo);
			}
		}

		public void TestCanadaNumbersVisible()
		{
			TallyContainer container = Factory.New<TallyContainer>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				using (ManifestTallyForm form = new ManifestTallyForm(container))
				{
					form.Show();
					Application.DoEvents();

					PackagesUserControl packagesUserControl = (PackagesUserControl)typeof(ManifestTallyForm).GetField("PackagesUserControl", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);

					ZTextBox cCNNumTextBox = (ZTextBox)typeof(PackagesUserControl).GetField("CCNNumTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(packagesUserControl);
					ZTextBox pCNNumTextBox = (ZTextBox)typeof(PackagesUserControl).GetField("PCNNumTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(packagesUserControl);

					Assert("CCNNumTextBox should be visible", cCNNumTextBox.Visible);
					Assert("PCNNumTextBox should be visible", pCNNumTextBox.Visible);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				using (ManifestTallyForm form = new ManifestTallyForm(container))
				{
					form.Show();
					Application.DoEvents();

					PackagesUserControl packagesUserControl = (PackagesUserControl)typeof(ManifestTallyForm).GetField("PackagesUserControl", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(form);
					ZTextBox cCNNumTextBox = (ZTextBox)typeof(PackagesUserControl).GetField("CCNNumTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(packagesUserControl);
					ZTextBox pCNNumTextBox = (ZTextBox)typeof(PackagesUserControl).GetField("PCNNumTextBox", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(packagesUserControl);

					Assert("CCNNumTextBox should not be visible", !cCNNumTextBox.Visible);
					Assert("PCNNumTextBox should not be visible", !pCNNumTextBox.Visible);
				}
			}
		}

		public void TestExternalColumnsInShipmentsGrid()
		{
			TallyContainer container = Factory.New<TallyContainer>();

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				using (ManifestTallyForm form = new ManifestTallyForm(container))
				{
					form.Show();
					Application.DoEvents();

					ManifestTallyShipmentsGrid shipmentGrid = (ManifestTallyShipmentsGrid)form.Controls.Find("ShipmentsGrid", true)[0];

					AssertNull("Column RNSReleaseStatus should not be added", shipmentGrid.GetColumnStyle("RNSReleaseStatus"));
					AssertNull("Column RNSReleaseDate should not be added", shipmentGrid.GetColumnStyle("RNSReleaseDate"));

					AssertNull("Column ArrivalCertificationStatus should not be added", shipmentGrid.GetColumnStyle("ArrivalCertificationStatus"));
					AssertNull("Column ArrivalCertificationDate should not be added", shipmentGrid.GetColumnStyle("ArrivalCertificationDate"));

					AssertNull("Column CanadaHouseCCN should be removed", shipmentGrid.GetColumnStyle("CanadaHouseCCN"));
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				using (ManifestTallyForm form = new ManifestTallyForm(container))
				{
					form.Show();
					Application.DoEvents();

					ManifestTallyShipmentsGrid shipmentGrid = (ManifestTallyShipmentsGrid)form.Controls.Find("ShipmentsGrid", true)[0];

					AssertNotNull("Column RNSReleaseStatus should be added", shipmentGrid.GetColumnStyle("RNSReleaseStatus"));
					AssertNotNull("Column RNSReleaseDate should be added", shipmentGrid.GetColumnStyle("RNSReleaseDate"));

					AssertNotNull("Column ArrivalCertificationStatus should be added", shipmentGrid.GetColumnStyle("ArrivalCertificationStatus"));
					AssertNotNull("Column ArrivalCertificationDate should be added", shipmentGrid.GetColumnStyle("ArrivalCertificationDate"));

					AssertNotNull("Column CanadaHouseCCN should not be removed", shipmentGrid.GetColumnStyle("CanadaHouseCCN"));
				}
			}
		}

		public void TestShipmentRNSMFTabPage()
		{
			var consol = Factory.New<CFSLoadListConsol>();
			var container = Factory.New<TallyContainer>();
			consol.Containers.Add(container);

			var shipment = container.PackUnpackShipments.AddNew();

			Assert("Precondition:", !container.IsImport());

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.Canada))
			{
				using (var form = new ManifestTallyForm(container))
				{
					form.Show();

					var tabControl = form.Controls.Find("ShipmentDetailsTabControl", true)[0] as ZTabControl;
					var rNSMFTallyShipmentsPlugIn = tabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSMFTallyShipmentsPlugIn);
					Assert("RNSMFTallyShipmentsPlugIn should be disabled for shipment", !rNSMFTallyShipmentsPlugIn.Enabled);

					container.Consol.JK_RL_NKLoadPort = "USAAA";
					container.Consol.JK_RL_NKDischargePort = "CABBB";
					Assert("Precondition:", container.IsImport());

					Assert("RNSMFTallyShipmentsPlugIn should be enabled for shipment", rNSMFTallyShipmentsPlugIn.Enabled);
				}
			}

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Constants.CountryCodes.China))
			{
				using (ManifestTallyForm form = new ManifestTallyForm(container))
				{
					form.Show();

					var tabControl = form.Controls.Find("ShipmentDetailsTabControl", true)[0] as ZTabControl;
					var rNSMFTallyShipmentsPlugIn = tabControl.PlugIns.GetPlugIn(ControllerIDs.Customs.CA.RNSMFTallyShipmentsPlugIn);
					AssertNull("RNSMFTallyShipmentsPlugIn should be invisible for shipment", rNSMFTallyShipmentsPlugIn);
				}
			}
		}
	}
}
