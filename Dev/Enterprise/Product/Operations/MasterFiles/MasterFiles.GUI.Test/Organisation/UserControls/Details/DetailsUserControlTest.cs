using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Windows.UI;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(MainDetailsUserControl))]
	sealed class DetailsUserControlTest : OrganisationSecurityContainerControlBaseTest
	{
		#region TestDetailsControl

		public class TestDetailsControl : MainDetailsUserControl
		{
			public TestDetailsControl()
				: base()
			{
				OH_IsConsignorBoundCheckEdit = base.OH_IsConsignorBoundCheckEdit;
				OH_IsConsigneeBoundCheckEdit = base.OH_IsConsigneeBoundCheckEdit;
				OH_IsDebtorBoundCheckEdit = base.OH_IsDebtorBoundCheckEdit;
			}

			public new ZCheckBox OH_IsConsignorBoundCheckEdit;
			public new ZCheckBox OH_IsConsigneeBoundCheckEdit;
			public new ZCheckBox OH_IsDebtorBoundCheckEdit;

			public void SetSalesCheckEdit(bool @checked)
			{
				OH_IsSalesLeadBoundCheckEdit.Checked = @checked;
			}

			public bool OrgTypeGroupBoxVisible
			{
				get { return OrgTypeGroupBox.Visible; }
			}
		}

		public class TestCustomFieldsControl : CustomFieldsUserControl
		{
			public TestCustomFieldsControl()
				: base()
			{
			}
		}

		#endregion

		public void TestBoundCheckTextCSTermin()
		{
			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			AssertText(FreightDataRegistry.Instance.ConsignorShipperTerminology.DefaultValue);

			FreightDataRegistry.Instance.ConsignorShipperTerminology.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, "TestTestTest");
			AssertText("TestTestTest");
		}

		void AssertText(string expectedValue)
		{
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				AssertEquals("Caption should be is " + expectedValue, expectedValue, ctrl.OH_IsConsignorBoundCheckEdit.Text);
			}
		}

		public void TestBusinessRegNoLabelWithCountry()
		{
			Guid singBranchPK = new Guid("EF8CBDB5-9F53-4360-921E-C2929BF77A85");
			Guid aUBranchPK = new Guid("27A55065-AC88-4EC3-8BED-E575E79172CB");

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, singBranchPK, Env.CurrentDepartment.PK))
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				Assert("Business Reg No. (SG):", ctrl.OrgTypeGroupBoxVisible);
			}

			using (Env.SetTemporaryUserContext(Env.CurrentUser.LoginName, aUBranchPK, Env.CurrentDepartment.PK))
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				Assert("Business Reg No. (AU):", ctrl.OrgTypeGroupBoxVisible);
			}
		}

		[RequiresSTA]
		public void TestClientInteligenceGroupBoxVisibility()
		{
			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (ZForm testForm = new ZForm(testHeader))
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				testForm.Controls.Add(ctrl);
				testForm.Show();
				Assert("OrgTypeGroupBox.Visible = true", ctrl.OrgTypeGroupBoxVisible);
			}

			using (ZCompetitorIntelligenceForm testForm = new ZCompetitorIntelligenceForm(testHeader))
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				testForm.Controls.Add(ctrl);
				testForm.Show();
				Assert("OrgTypeGroupBox.Visible = false", !ctrl.OrgTypeGroupBoxVisible);
			}
		}

		public void TestToggleOrgTypesForClientIntelligence()
		{
			Env.Registry.SetOrgShowConsigneeConsignorTab(true);
			Env.Registry.SetOrgShowARTab(true);

			OrgHeader testHeader = Factory.NewWithValidTestData<OrgHeader>();

			using (ZClientIntelligenceForm testForm = new ZClientIntelligenceForm(testHeader))
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				testForm.Controls.Add(ctrl);
				testForm.Show();
				Assert("Consignee Org type check box should be Enabled", ctrl.OH_IsConsigneeBoundCheckEdit.Enabled);
				Assert("Consignor Org type check box should be Enabled", ctrl.OH_IsConsignorBoundCheckEdit.Enabled);
				Assert("Receivables Org type check box should be Enabled", ctrl.OH_IsDebtorBoundCheckEdit.Enabled);
			}

			Env.Registry.SetOrgShowConsigneeConsignorTab(false);
			Env.Registry.SetOrgShowARTab(true);

			using (ZClientIntelligenceForm testForm = new ZClientIntelligenceForm(testHeader))
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				testForm.Controls.Add(ctrl);
				testForm.Show();
				Assert("Consignee Org type check box should not be Enabled", !ctrl.OH_IsConsigneeBoundCheckEdit.Enabled);
				Assert("Consignor Org type check box should not be Enabled", !ctrl.OH_IsConsignorBoundCheckEdit.Enabled);
				Assert("Receivables Org type check box should be Enabled", ctrl.OH_IsDebtorBoundCheckEdit.Enabled);
			}

			Env.Registry.SetOrgShowConsigneeConsignorTab(true);
			Env.Registry.SetOrgShowARTab(false);

			using (ZClientIntelligenceForm testForm = new ZClientIntelligenceForm(testHeader))
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				testForm.Controls.Add(ctrl);
				testForm.Show();
				Assert("Consignee Org type check box should be Enabled", ctrl.OH_IsConsigneeBoundCheckEdit.Enabled);
				Assert("Consignor Org type check box should be Enabled", ctrl.OH_IsConsignorBoundCheckEdit.Enabled);
				Assert("Receivables Org type check box should not be Enabled", !ctrl.OH_IsDebtorBoundCheckEdit.Enabled);
			}

			Env.Registry.SetOrgShowConsigneeConsignorTab(false);
			Env.Registry.SetOrgShowARTab(false);

			using (ZClientIntelligenceForm testForm = new ZClientIntelligenceForm(testHeader))
			using (TestDetailsControl ctrl = new TestDetailsControl())
			{
				testForm.Controls.Add(ctrl);
				testForm.Show();
				Assert("Consignee Org type check box should not be Enabled", !ctrl.OH_IsConsigneeBoundCheckEdit.Enabled);
				Assert("Consignor Org type check box should not be Enabled", !ctrl.OH_IsConsignorBoundCheckEdit.Enabled);
				Assert("Receivables Org type check box should not be Enabled", !ctrl.OH_IsDebtorBoundCheckEdit.Enabled);
			}
		}

		public void TestStaffAssignmentsTabPageWithOrgDetailsViewCompanysStaffAssignmentsNotAllowed()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			Env.Security.OrgDetailsViewCompanysStaffAssignments.IsAllowed = false;

			using (var testForm = new ZForm(testHeader))
			using (var ctrl = new TestDetailsControl())
			{
				testForm.Controls.Add(ctrl);
				testForm.Show();
				var tabPages = ctrl.Controls.Find("StaffAssignmentsTabPage", true);
				AssertEquals(1, tabPages.Length);
				AssertEquals("StaffAssignmentsTabPage", tabPages[0].Name);
				AssertEquals("coveringLabel", tabPages[0].Controls[0].Name);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> Master Data -> Organization -> View -> View Company's Staff Assignments
", tabPages[0].Controls[0].Text);

				testForm.Close();
			}
		}

		public void TestCustomFieldsDisabledIfUserNotAllowed()
		{
			var testHeader = Factory.NewWithValidTestData<OrgHeader>();
			Env.Security.OrgDetailsModifyCustomFields.IsAllowed = false;
			Factory.Save();

			using (var testForm = new ZForm(testHeader))
			using (var ctrl = new TestDetailsControl())
			using (var customFields = new TestCustomFieldsControl())
			{
				ctrl.Controls.Add(customFields);
				testForm.Controls.Add(ctrl);
				testForm.Show();
				var tabPages = ctrl.Controls.Find("CustomFieldsTabPage", true);
				AssertEquals(1, tabPages.Length);
				AssertEquals("CustomFieldsTabPage", tabPages[0].Name);
				AssertEquals(1, ctrl.Controls.Find("CustomFieldsUserControl", true).Length);
				var customFieldsUserControl = (CustomFieldsUserControl)ctrl.Controls.Find("CustomFieldsUserControl", true)[0];
				AssertNotNull(customFieldsUserControl.processTemplateCustomFieldsControl);
				AssertEquals(true, ((ICustomFieldProvider)testHeader).GetCustomBusinessObject().ReadOnly);

				testForm.Close();
			}
		}

		public void TestCustomFieldControlIsForcedBinding()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			template.GlobalTemplate = true;

			var templateDefinition = template.GenCustomColumnDefinitions.AddNew();
			templateDefinition.XC_Name = "CustomString";
			templateDefinition.XC_Type = AddOnColumnDataType.Codes.String;

			var testHeader = Factory.NewWithValidTestData<OrgHeader>();

			var customAddOnValue = Factory.NewWithValidTestData<GenCustomAddOnValue>();
			customAddOnValue.XV_Data = "TestString";
			customAddOnValue.XV_Name = "CustomString";
			customAddOnValue.XV_Type = AddOnColumnDataType.Codes.String;
			customAddOnValue.XV_ParentID = testHeader.PK;
			customAddOnValue.XV_ParentTableCode = "OH";

			Factory.Save();

			using (var testForm = new ZForm(testHeader))
			using (var ctrl = new TestDetailsControl())
			{
				testForm.Controls.Add(ctrl);
				testForm.Show();
				testForm.FireSaveButton();

				var customBusinessObject = ((ICustomFieldProvider)testHeader).GetCustomBusinessObject();
				AssertNotNull(customBusinessObject);
				AssertEquals("TestString", customBusinessObject["__CUSTOMSTRING__prop__ZString"]);
				Assert(((IBusiness)testHeader).Children.Contains(customBusinessObject));
				testForm.Close();
			}
		}

		#region ProductivityWise

		public void TestProductivityWiseMode_ShouldRemoveSupplyChainRelatedOrgTypes()
		{
			OrganisationsDataRegistry.Instance.EnableControllingCustomerFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true); // makes the last two checkboxes appear.
			OrganisationsDataRegistry.Instance.EnableControllingAgentFunctionalityAndValidations.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);    // makes the last two checkboxes appear.

			var org = Factory.NewWithValidTestData<OrgHeader>();

			void AssertOrganizationTypesVisible(string message, params string[] expectedVisibleTypes)
			{
				using (var form = new ZOrganisationsForm(org))
				{
					form.Show();
					Application.DoEvents();

					var mainControl = form.FindSingle<MainDetailsUserControl>();
					var flowLayoutPanel = mainControl.FindSingle<KFlowLayoutPanel>("orgTypesflowLayoutPanel");
					var visibleControls = flowLayoutPanel.Controls.Cast<Control>().Where(x => x.Visible).OrderBy(x => x.Top).Select(x => x.Text);
					AssertSequencesEqual(message, expectedVisibleTypes, visibleControls);
				}
			}

			var divider = string.Empty;
			AssertOrganizationTypesVisible("Organization Types should be visible when ProductivityWise mode is not enabled. SAD!", "Active Client", "National Account", "Global Supplier", "Temporary Acct", divider, "Receivables", "Payables", divider, "Consignor", "Consignee", "Transport Client", "Warehouse", divider, "Carrier", "Forwarder/Agent", "Broker", "Services", "Competitor", divider, "Sales", divider, "Controlling Customer", "Controlling Agent");

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertOrganizationTypesVisible("Only Accounting-related Organization Types should be visible when ProductivityWise mode is enabled. SAD!", "Active Client", "Receivables", "Payables", "Sales");
		}

		[RequiresSTA]
		public void TestProductivityWiseMode_ShouldHideLogisticsTabs()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			void AssertTabs(string message, params string[] expectedTabs)
			{
				using (var form = new ZOrganisationsForm(org))
				{
					form.Show();
					Application.DoEvents();

					var mainControl = form.FindSingle<MainDetailsUserControl>();
					var tabControl = mainControl.DetailsTabControl;
					var tabs = tabControl.AllTabPages.Select(x => x.Text);

					AssertContainsExactElementsInAnyOrder(message, expectedTabs, tabs);
				}
			}

			AssertTabs("All tabs should be visible when ProductivityWise mode is not enabled. SAD!", "Details", "Staff Assignments", "Web Security", "Rating", "Supply Chain Security (AU)", "Config", "Related Parties", "Web", "Custom Fields");

			DataRegistry.Instance.ProductivityWiseModeEnabled = true;
			AssertTabs("Logistics-related tabs should not be visible when ProductivityWise mode is enabled. SAD!", "Details", "Staff Assignments", "Web Security", "Config", "Web", "Custom Fields");
		}

		#endregion

		public void TestAutoScrollForOrganisationTypesPanel()
		{
			using (var ctrl = new TestDetailsControl())
			{
				AssertEquals("AutoScroll is true", true, ctrl.orgTypesflowLayoutPanel.AutoScroll);
				AssertEquals("AutoScrollMinSize is expected", CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 560, true), ctrl.orgTypesflowLayoutPanel.AutoScrollMinSize);
			}
		}

		protected override OrganisationSecurityContainerControl GetNewControlForTesting()
		{
			return new MainDetailsUserControl();
		}

		protected override string[] SecurityContainerPropertiesEnabledForThisControl
		{
			get { return new string[] { "IsModifyDetails", "IsModifyDetailsNameAndAddress", "IsModifyDetailsOrganisationType", "IsModifyDetailsPhFaxWebDetails", "IsModifyDetailsRatingAndTariffs", "IsModifyDetailsStaffAssignments", "IsModifyDetailsWebSecurity", "IsNewDetailsWebSecurity", "IsModifyConfig", "IsModifyConfigBrandsAndCompanyNames", "IsModifyConfigEDICodeMapping", "IsModifyConfigGeneral", "IsModifyConfigRegistrationNumbers", "IsModifyDetailsCustomFields" }; }
		}
	}
}
