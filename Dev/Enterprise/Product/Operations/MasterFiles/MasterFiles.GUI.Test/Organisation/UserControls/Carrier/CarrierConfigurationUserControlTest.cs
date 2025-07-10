using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class CarrierConfigurationUserControlTest : TestCaseWithFactory
	{
		public void TestAirlineTabPage()
		{
			using (var form = new ZForm(TestHeader))
			using (var userControl = new CarrierConfigurationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var airlineTabPage = userControl.Controls.Find("AirlineTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(airlineTabPage);

				var iataTabPage = airlineTabPage.Controls.Find("IATATabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(iataTabPage);

				var airlineAccountNumbersTabPage = airlineTabPage.Controls.Find("AirlineAccountNumbersTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(airlineAccountNumbersTabPage);

				var mawbStockManagementTabPage = airlineTabPage.Controls.Find("MAWBStockManagementTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(mawbStockManagementTabPage);
			}
		}

		public void TestAirlineAccountNumberTabPage()
		{
			using (var form = new ZForm(TestHeader))
			using (var userControl = new CarrierConfigurationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var airlineAccountNumberTabPage = userControl.Controls.Find("AirlineAccountNumbersTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(airlineAccountNumberTabPage);

				airlineAccountNumberTabPage.Show();

				var airlineAccountNumberTextBox = airlineAccountNumberTabPage.Controls.Find("AirlineAccountNumberTextBox", true).FirstOrDefault() as ZArchitecture.ZTextBox;
				AssertNotNull(airlineAccountNumberTextBox);

				var airlineAccountNumberGrid = airlineAccountNumberTabPage.Controls.Find("AirlineAccountNumberGrid", true).FirstOrDefault() as ZArchitecture.ZGrid;
				AssertNotNull(airlineAccountNumberGrid);

				var columns = airlineAccountNumberGrid.ColumnStyles.Cast<ZGridColumnInfo>();
				var branchColumnStyleInfo = columns.FirstOrDefault(c => c.ColumnName == "OAA_GB_Branch");
				AssertNotNull(branchColumnStyleInfo);

				var accountNumberColumnStyleInfo = columns.FirstOrDefault(c => c.ColumnName == "OAA_APAirlineAccountNumber");
				AssertNotNull(accountNumberColumnStyleInfo);
			}
		}

		public void TestNamedAccountMappingTabPage()
		{
			using(RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm(TestHeader))
			using (var userControl = new CarrierConfigurationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var namedAccountClientsTabPage = userControl.Controls.Find("NamedAccountClientsTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(namedAccountClientsTabPage);

				namedAccountClientsTabPage.Show();

				var control = namedAccountClientsTabPage.Controls.Find("OrgCarrierNamedAccountUserControl", true).FirstOrDefault() as OrgCarrierNamedAccountUserControl;
				AssertNotNull(control);
			}

			using (RatingDataRegistry.Instance.EnableUrsIntegration.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm(TestHeader))
			using (var userControl = new CarrierConfigurationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				var namedAccountClientsTabPage = userControl.Controls.Find("NamedAccountClientsTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNull(namedAccountClientsTabPage);
			}
		}

		public void TestDirectionColumnInCTOAndCYTabs()
		{
			using (var form = new ZForm(TestHeader))
			using (var userControl = new CarrierConfigurationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				void AssertDirectionColumnInGrid(string message, string tabName, string gridName)
				{
					var tabPage = userControl.Controls.Find(tabName, true).FirstOrDefault() as ZTabPage;
					AssertNotNull(tabName, tabPage);

					tabPage.Show();

					var grid = tabPage.Controls.Find(gridName, true).FirstOrDefault() as ZArchitecture.ZGrid;
					AssertNotNull(gridName, grid);

					var agentDirectionColumnStyleInfo = grid.ColumnStyles.Cast<ZGridColumnInfo>().FirstOrDefault(c => c.ColumnName == "O5_AgentDirection");
					AssertNotNull(message, agentDirectionColumnStyleInfo);
					Assert(message, agentDirectionColumnStyleInfo.IsVisible);
				}

				AssertDirectionColumnInGrid("Sea/CTO", "StevedoreTabPage", "AppointedCarrierPortszGrid");
				AssertDirectionColumnInGrid("Air/CTO", "AirTabPage", "AirCTOGrid");
				AssertDirectionColumnInGrid("CY/Park", "ContainerYardTabPage", "CYPGrid");
			}
		}

		public void TestCarrierPackageGroupingVisible()
		{
			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			using (var form = new ZForm())
			{
				var userControl = new CarrierConfigurationUserControl();

				form.Controls.Add(userControl);
				form.Show();

				var shippingLineTab = userControl.Controls.Find("RefShippingLineTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(shippingLineTab);
				shippingLineTab.Show();

				var carrierPackageGroupingDropEdit = shippingLineTab.Controls.Find("CarrierPackageGroupingDropEdit", true).FirstOrDefault() as ZDropEdit;
				Assert(!carrierPackageGroupingDropEdit.Visible);
			}

			using (FreightDataRegistry.Instance.EnablePackageGrouping.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new ZForm())
			{
				var userControl = new CarrierConfigurationUserControl();

				form.Controls.Add(userControl);
				form.Show();

				var shippingLineTab = userControl.Controls.Find("RefShippingLineTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(shippingLineTab);
				shippingLineTab.Show();

				var carrierPackageGroupingDropEdit = shippingLineTab.Controls.Find("CarrierPackageGroupingDropEdit", true).FirstOrDefault() as ZDropEdit;
				Assert(carrierPackageGroupingDropEdit.Visible);
			}
		}

		public void TestShippingLineFieldsAreNotBlank()
		{
			var shippingLine = Factory.NewWithValidTestData<RefShippingLine>();
			shippingLine.RSL_CarrierName = "Test Ship";
			shippingLine.RSL_StandardCarrierAlphaCode = "ABCD";

			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsShippingLine = true;
			org.OH_RSL_ShippingLine = shippingLine.PK;

			Factory.Save();

			// Need to load another instance of org as issue occurs only when shipping line is initially set
			// (without going through the OH_RSL_ShippingLine setter method for the new instance)
			var newFactory = new BusinessObjectFactory();
			var loadedOrg = newFactory.Load<OrgHeader>(org.PK);

			using (var form = new ZForm(loadedOrg))
			using (var userControl = new CarrierConfigurationUserControl())
			{
				form.Controls.Add(userControl);
				form.Show();

				// Show tab which contains the shipping line fields
				var shippingLineTab = userControl.Controls.Find("RefShippingLineTabPage", true).FirstOrDefault() as ZTabPage;
				AssertNotNull(shippingLineTab);
				shippingLineTab.Show();

				// Ensure the carrier name and SCAC code fields match the shipping line details
				var carrierNameField = shippingLineTab.Controls.Find("RefShippingLineCarrierName", true).FirstOrDefault() as ZArchitecture.ZTextBox;
				var scacCodeField = shippingLineTab.Controls.Find("RefShippingLineSCACCode", true).FirstOrDefault() as ZArchitecture.ZTextBox;
				AssertEquals("TEST SHIP", carrierNameField.Text);
				AssertEquals("ABCD", scacCodeField.Text);
			}
		}

		#region Implementation

		public OrgHeader TestHeader
		{
			get
			{
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.OH_IsAirLine = true;
				return orgHeader;
			}
		}

		#endregion
	}
}
