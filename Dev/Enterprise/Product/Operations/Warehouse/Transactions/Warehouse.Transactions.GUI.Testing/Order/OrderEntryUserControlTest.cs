using System;
using System.Windows.Forms;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.MasterFiles.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.Warehouse.Transactions.Business.Testing;
using Enterprise.Warehouse.Transactions.CodeLists;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.GUI.Testing
{
	class OrderEntryUserControlTest : WhsGuiTestCaseWithFactory
	{
		#region TestAuthorisedToLeaveFlagIsNotSetOnFormLoad

		public void TestAuthorisedToLeaveFlagIsNotSetOnFormLoad()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.ConsigneeDocAddress.E2_AddressOverride = true;
			order.ConsigneeDocAddress.E2_CompanyName = "Bob's things";
			order.ConsigneeDocAddress.E2_Address1 = "123 Street";
			order.ConsigneeDocAddress.E2_RN_NKCountryCode = "AU";
			order.ConsigneeDocAddress.E2_State = "NSW";
			order.ConsigneeDocAddress.E2_Postcode = "1111";
			order.ConsigneeDocAddress.ValidationStatus = AddressValidationStatus.ToBeVerified;
			order.WD_IsAuthorisedToLeave = true;
			Factory.Save();

			using (DataRegistry.Instance.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			using (var form = new OrderEntryTestForm(order))
			{
				form.Show();
				Application.DoEvents();
				AssertEquals("Form Binding should not have set ATL to false.", true, order.WD_IsAuthorisedToLeave);
			}

			// Address Validation hooks a couple of Tasks on Form Closing/Controls losing focus. Application.DoEvents() lets all these task finish and prevent a test failure
			Application.DoEvents();
		}

		#endregion

		#region TestIsLoadingRequiredCheckBox

		public void TestIsLoadingRequiredCheckBox()
		{
			using (var form = new OrderEntryTestForm(Factory.New<WhsOrder>()))
			{
				form.Show();
				var userControl = form.UserControl;
				var checkBox = GUITestHelper.FindControl<ZCheckBox>(userControl.Controls, "IsLoadingRequiredCheckBox");
				AssertEquals("Is Loading should be visible.", true, checkBox.Visible);
			}
		}

		#endregion

		#region Properties

		public void TestShowBottomPanel()
		{
			using (var form = new OrderEntryTestForm(Factory.New<WhsOrder>()))
			{
				form.Show();
				var userControl = form.UserControl;
				var detailTabControl = GUITestHelper.FindControl<ZTemplateTabControl>(userControl.Controls, "DetailTabControl");
				var linesTabPage = GUITestHelper.FindControl<ZTabPage>(detailTabControl.Controls, "LinesTabPage");
				detailTabControl.SelectedTab = linesTabPage;

				var bottomPanel = GUITestHelper.FindControl<Panel>(linesTabPage.Controls, "bottomPanel");
				var splitter = GUITestHelper.FindControl<KSplitter>(linesTabPage.Controls, "splitter1");
				AssertEquals(true, bottomPanel.Visible);
				AssertEquals(true, splitter.Visible);

				userControl.ShowBottomPanel = false;
				AssertEquals(false, bottomPanel.Visible);
				AssertEquals(false, splitter.Visible);
			}
		}

		#endregion

		#region Events

		#region TestSetupInventoryFilterStripUserControl

		public void TestSetupInventoryFilterStripUserControl()
		{
			var order = Factory.New<WhsOrder>();

			AssertSetupInventoryFilterStripUserControl(order, DocketStatus.Codes.New, true);
			AssertSetupInventoryFilterStripUserControl(order, DocketStatus.Codes.Entered, true);
			AssertSetupInventoryFilterStripUserControl(order, DocketStatus.Codes.AttachedToPick, true);
			AssertSetupInventoryFilterStripUserControl(order, DocketStatus.Codes.Picking, true);
			AssertSetupInventoryFilterStripUserControl(order, DocketStatus.Codes.Finalised, false);
			AssertSetupInventoryFilterStripUserControl(order, DocketStatus.Codes.Cancelled, false);
		}

		void AssertSetupInventoryFilterStripUserControl(WhsOrder order, ZString docketStatus, bool shouldCreate)
		{
			order.WD_DocketStatus = docketStatus;
			using (var form = new OrderEntryTestForm(order))
			{
				form.Show();
				AssertNotNull(form.UserControl);
				if (shouldCreate)
				{
					AssertNotNull(form.UserControl.inventoryFilterStripUserControl);
				}
				else
				{
					AssertNull(form.UserControl.inventoryFilterStripUserControl);
				}
			}
		}

		#endregion

		#region TestDistributionCentreTabControlCaption

		public void TestDistributionCentreTabControlCaption()
		{
			var orgWithAddressShortCodeTooBig = Helper.CreateClient("O1");
			var orgWithAddressShortCode = Helper.CreateClient("O2");
			var orgWithAddressShortCodeSmall = Helper.CreateClient("O2");
			var orgWithoutAddressShortCode = Helper.CreateClient("O2");
			orgWithAddressShortCodeTooBig.MainAddress.OA_Code = "ShortCode";
			orgWithAddressShortCode.MainAddress.OA_Code = "ShortC";
			orgWithAddressShortCodeSmall.MainAddress.OA_Code = "Sho";
			orgWithoutAddressShortCode.MainAddress.OA_Code = "";

			var order = Factory.New<WhsOrder>();
			using (var form = new OrderEntryTestForm(order))
			{
				form.Show();
				var distributionCentreTabPage = FindControl<ZTabPage>(form.Controls, "DistributionCentreTabPage");
				AssertEquals("Distribution Center", distributionCentreTabPage.Text);

				order.DistributionCentreDocAddress.OrganisationPK = orgWithAddressShortCodeTooBig.PK;
				AssertEquals("DC - ShortC...", distributionCentreTabPage.Text);

				order.DistributionCentreDocAddress.OrganisationPK = orgWithAddressShortCode.PK;
				AssertEquals("DC - ShortC", distributionCentreTabPage.Text);

				order.DistributionCentreDocAddress.OrganisationPK = orgWithAddressShortCodeSmall.PK;
				AssertEquals("DC - Sho", distributionCentreTabPage.Text);

				order.DistributionCentreDocAddress.OrganisationPK = orgWithoutAddressShortCode.PK;
				AssertEquals("Distribution Center", distributionCentreTabPage.Text);

				order.DistributionCentreDocAddress.OrganisationPK = orgWithAddressShortCodeTooBig.PK;
				AssertEquals("DC - ShortC...", distributionCentreTabPage.Text);
				order.DistributionCentreDocAddress.E2_AddressOverride = true;
				AssertEquals("Distribution Center", distributionCentreTabPage.Text);
			}
		}

		static T FindControl<T>(Control.ControlCollection controlCollection, string name) where T : Control
		{
			foreach (Control control in controlCollection)
			{
				if (control is T && control.Name == name)
				{
					return (T)control;
				}
				else
				{
					var matchingControl = FindControl<T>(control.Controls, name);
					if (matchingControl != null)
					{
						return matchingControl;
					}
				}
			}

			return null;
		}

		#endregion

		#endregion

		#region TestExcludeFromTotePickingCheckBox

		public void TestExcludeFromTotePickingCheckBox()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);

			using (var form = new OrderEntryTestForm(order))
			{
				form.Show();

				var excludeFromTotePickingCheckBox = form.FindSingle<ZCheckBox>("ExcludeFromTotePickingCheckBox");
				AssertNotNull(excludeFromTotePickingCheckBox);
				Assert(excludeFromTotePickingCheckBox.Visible);
			}
		}

		#endregion

		#region TestUsePackingConsolidationCheckBox

		public void TestUsePackingConsolidationCheckBox()
		{
			using (var form = new OrderEntryTestForm(Factory.New<WhsOrder>()))
			{
				form.Show();
				var userControl = form.UserControl;
				var checkBox = GUITestHelper.FindControl<ZCheckBox>(userControl.Controls, "UsePackingConsolidationCheckBox");
				AssertEquals("Use Packing Consolidation CheckBox should be visible.", true, checkBox.Visible);
			}
		}

		#endregion

		#region TestAwaitCustomsResponseLabelVisibility

		public void TestAwaitCustomsResponseLabelVisibility()
		{
			var data = new TestDataSimpleEnvironment(Factory);
			Helper.EnableWarehouseForBond(data.Whs1, true);
			var order = Helper.CreateWhsOrder(data.Org1, data.Whs1);
			order.WD_DocketSubType = OrderType.Codes.Customs;
			Factory.Save();

			using (var form = new OrderEntryTestForm(order))
			{
				form.Show();
				var userControl = form.UserControl;
				AssertEquals("", userControl.AwaitingResponseLabelForTest.Text);

				order.Logs.AddNew(Events.HoldTheWarehouseOrder);
				form.FireSaveButton();
				AssertEquals("Awaiting Customs Response", userControl.AwaitingResponseLabelForTest.Text);

				// we want to set the utc date to be later then the 'hold' event's utc date
				var allowFinaliseLog = order.Logs.AddNew(Events.WarehouseJobCanNowBeFinalised);
				Helper.SetLogUTCTimeOnFactorySave(TestConnection, allowFinaliseLog, ZDateTime.Now.AddDays(1));

				form.FireSaveButton();
				AssertEquals("Precondition", true, order.IsFinaliseAllowed);
				order.AwaitingCustomsResponseStatusInfo.RefreshBinding();
				AssertEquals("", userControl.AwaitingResponseLabelForTest.Text);
			}
		}

		#endregion

		#region TestForm

		protected class OrderEntryTestForm : ZForm
		{
			public OrderEntryTestForm(WhsDocket docket)
				: base(docket)
			{
			}

			public OrderEntryUserControl UserControl;

			protected override void InitializeComponent()
			{
				this.UserControl = GetNewOrderEntryUserControl();
				this.Controls.Add(this.UserControl);
				this.DataSourceAssemblyName = "Enterprise.Warehouse.Transactions.Business";
				this.DataSourceTypeName = "Enterprise.Warehouse.Transactions.Business.WhsDocket";
			}

			protected OrderEntryUserControl GetNewOrderEntryUserControl()
			{
				return new OrderEntryUserControl();
			}
		}

		#endregion
	}
}
