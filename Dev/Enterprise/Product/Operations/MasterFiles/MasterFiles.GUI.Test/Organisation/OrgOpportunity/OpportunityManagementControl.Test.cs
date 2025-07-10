using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	public class OpportunityTasksControlTest : TestCaseWithFactory
	{
		public void TestReadOnly()
		{
			using (OpportunityManagementControl control = new OpportunityManagementControl())
			{
				var taskGrid = control.FindSingle<ZGrid>("TasksGrid");
				var opportunitiesGrid = control.FindSingle<ZModuleButtonGrid>("OpportunitiesGrid");

				AssertEquals("Edit", "Edit");
				Assert(opportunitiesGrid.Enabled);
				Assert(taskGrid.Enabled);

				control.SetControlReadOnly(true);
				Assert("Grid not ReadOnly", opportunitiesGrid.Enabled);
				Assert("Grid not ReadOnly", taskGrid.Enabled);

				control.SetControlReadOnly(false);
				Assert(opportunitiesGrid.Enabled);
				Assert(taskGrid.Enabled);
			}
		}

		public void TestNavigateToTask()
		{
			var testHeader = Factory.New<OrgHeader>();
			testHeader.OH_IsSalesLead = true;

			var opp1 = testHeader.SalesOpportunities.AddNew();
			var opp2 = testHeader.SalesOpportunities.AddNew();
			opp2.WorkflowItems.AddNew();
			var task = opp2.WorkflowItems.AddNew();

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationFormTest.OrgFormForTest form = new OrganisationFormTest.OrgFormForTest(testHeader))
			{
				form.Show();
				form.OrgTabControl.SelectedIndex = 11;
				var salesControl = form.OrgTabControl.FindSingle<ZUserControl>("SalesControl");
				var salesTabControl = (ZTemplateTabControl)salesControl.Controls[0];
				salesTabControl.SelectedIndex = 1;
				var oppControl = GetOppControl(form);

				oppControl.NavigateToWorkflowItem(task);

				var taskGrid = form.FindSingle<ZGrid>("TasksGrid");
				var opportunitiesGrid = form.FindSingle<ZModuleButtonGrid>("OpportunitiesGrid");

				AssertEquals(1, opportunitiesGrid.InnerGrid.CurrentRowIndex);
				AssertEquals(1, opportunitiesGrid.InnerGrid.ListManager.Position);
				AssertEquals(1, taskGrid.CurrentRowIndex);
				AssertEquals(1, taskGrid.ListManager.Position);
			}
		}

		[RequiresSTA]
		public void TestNavigateToWorkflow()
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, "ORG", WorkflowDescriptors.OpportunityWorkflowDescriptorCode);

			var testHeader = Factory.New<OrgHeader>();
			testHeader.OH_IsSalesLead = true;

			var opp1 = testHeader.SalesOpportunities.AddNew();
			var opp2 = testHeader.SalesOpportunities.AddNew();

			var opp2Header = helper.GetJobHeaderForParent(opp2, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = helper.CreateWorkflow(opp2Header, "Workflow 1");
			var workflow2 = helper.CreateWorkflow(opp2Header, "Workflow 2");
			_ = helper.CreateTask(workflow1);
			_ = helper.CreateTask(workflow1);
			_ = helper.CreateTask(workflow2);
			_ = helper.CreateTask(workflow2);

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrganisationFormTest.OrgFormForTest(testHeader))
			{
				form.Show();
				form.OrgTabControl.SelectedIndex = 11;
				var salesControl = form.OrgTabControl.FindSingle<ZUserControl>("SalesControl");
				var salesTabControl = (ZTemplateTabControl)salesControl.Controls[0];
				salesTabControl.SelectedIndex = 1;
				var oppControl = GetOppControl(form);

				oppControl.NavigateToWorkflowItem(workflow2);

				var taskGrid = form.FindSingle<ZGrid>("TasksGrid");
				var opportunitiesGrid = form.FindSingle<ZModuleButtonGrid>("OpportunitiesGrid");

				AssertEquals(1, opportunitiesGrid.InnerGrid.CurrentRowIndex);
				AssertEquals(1, opportunitiesGrid.InnerGrid.ListManager.Position);
				AssertEquals(2, taskGrid.CurrentRowIndex);
				AssertEquals(2, taskGrid.ListManager.Position);
			}
		}

		public void TestExtraCategoryColumnCaption()
		{
			var org = OrgHeader.New(Factory);
			org.OH_IsSalesLead = true;
			var opp = org.SalesOpportunities.AddNew();

			OrganisationsDataRegistry.Instance.ProductTypeLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "Zubs");
			OrganisationsDataRegistry.Instance.CurrentLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "CurrentABC");
			OrganisationsDataRegistry.Instance.PotentialLabel.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, "PotentialDEF");

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationFormTest.OrgFormForTest form = new OrganisationFormTest.OrgFormForTest(org))
			{
				form.Show();
				form.OrgTabControl.SelectedIndex = 11;
				var salesControl = form.OrgTabControl.FindSingle<ZUserControl>("SalesControl");
				var salesTabControl = (ZTemplateTabControl)salesControl.Controls[0];
				salesTabControl.SelectedIndex = 1;
				var oppControl = GetOppControl(form);

				var opportunitiesGrid = form.OrgTabControl.FindSingle<ZModuleButtonGrid>("OpportunitiesGrid");
				var colInfos = opportunitiesGrid.ColumnStyles.OfType<ZGridColumnInfo>();
				AssertEquals("Zubs", colInfos.First(x => x.ColumnName == OrgOpportunitySchema.P8_PackageType.Name).Caption);
				AssertEquals("CurrentABC", colInfos.First(x => x.ColumnName == OrgOpportunitySchema.P8_DiscountAmount.Name).Caption);
				AssertEquals("PotentialDEF", colInfos.First(x => x.ColumnName == OrgOpportunitySchema.P8_RentalMultiplier.Name).Caption);
			}
		}

		[RequiresSTA]
		public void TestFiltering()
		{
			var org = OrgHeader.New(Factory);
			org.OH_IsSalesLead = true;
			var opp = org.SalesOpportunities.AddNew();
			opp.P8_ClosedDateLocal = new ZDateTime(2006, 05, 05);

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (OrganisationFormTest.OrgFormForTest form = new OrganisationFormTest.OrgFormForTest(org))
			{
				form.Show();
				form.OrgTabControl.SelectedIndex = 11;
				var salesControl = form.FindSingle<ZUserControl>("SalesControl");
				var salesTabControl = (ZTemplateTabControl)salesControl.Controls[0];
				salesTabControl.SelectedIndex = 1;
				var oppControl = GetOppControl(form);
				var opportunitiesGrid = form.FindSingle<ZModuleButtonGrid>("OpportunitiesGrid");
				var findButton = form.FindSingle<ZButton>("FindButton");
				var clearButton = form.FindSingle<ZButton>("ClearButton");

				//Check Load
				AssertEquals("1 opportunities should be in the grid", 1, opportunitiesGrid.InnerGrid.ListManager.List.Count);

				org.OpportunitiesDateTypeToFilter = OrgHeaderLookups.LookupConstants.DateFilterListConstants.ClosedDate;
				org.OpportunityDateFrom = new ZDateTime(2007, 01, 01);
				findButton.PerformClick();
				AssertEquals("0 opportunities should be in the grid", 0, opportunitiesGrid.InnerGrid.ListManager.List.Count);

				//Check Clear
				clearButton.PerformClick();
				AssertEquals("Method to clear all fields should be called", ZDateTime.Empty, org.OpportunityDateFrom);
				AssertEquals("1 opportunity should be in the grid", 1, opportunitiesGrid.InnerGrid.ListManager.List.Count);

				//Check error
				org.OpportunityDateFrom = ZDateTime.Invalid;
				findButton.PerformClick();
				AssertEquals("Error message should have been displayed", "There are errors. Please correct these before searching.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestP8_Status_ChangeResultingInMakingOpportunityNonEffective_WithApprovedAgreements()
		{
			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_Status = "EFF";
			var approvedAgreement = opportunity.ApprovedCommissionAgreements.AddNew();
			approvedAgreement.FillWithValidTestData();
			var unapprovedAgreement = opportunity.CommissionAgreements.AddNew();
			unapprovedAgreement.FillWithValidTestData();

			var organization = OrgHeader.New(Factory);
			organization.OH_IsSalesLead = true;
			organization.SalesOpportunities.Add(opportunity);

			OrganisationsDataRegistry.Instance.OpportunityStatus.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, OpportunityCommissionChangeUtils.GetOpportunityStatusCollection());

			using (RatingDataRegistry.Instance.EnableQuotationDocumentsChargeGroupingSequencingAndRollup.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (var form = new OrganisationFormTest.OrgFormForTest(organization))
			{
				form.Show();
				form.OrgTabControl.SelectedIndex = 11;
				var salesControl = form.OrgTabControl.FindSingle<ZUserControl>("SalesControl");
				var salesTabControl = (ZTemplateTabControl)salesControl.Controls[0];
				salesTabControl.SelectedIndex = 1;
				var oppControl = GetOppControl(form);
				var opportunitiesGrid = form.OrgTabControl.FindSingle<ZModuleButtonGrid>("OpportunitiesGrid");
				opportunitiesGrid.InnerGrid.Select(0);
				var statusColumn = opportunitiesGrid.InnerGrid.Columns[OrgOpportunitySchema.P8_Status.Name];
				var statusControl = (ZDropEdit)(((ZDropEditColumnStyle)statusColumn.ColumnStyle).EditControl);
				statusControl.Text = "NON";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				oppControl.PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(statusControl, null);

				AssertEquals("EFF", statusControl.Text);
				AssertEquals(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(string.Format(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaptionText, "EFF", "NON"), UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Should have reverted to previous status", "EFF", statusControl.Text);

				statusControl.Text = "NON";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				oppControl.PromptUserToReverseAllAgreementsIfChangingOpportunityToNonEffective(statusControl, null);

				AssertEquals("NON", statusControl.Text);
				AssertEquals(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaption, UnitTestUserNotification.Instance.LastMessage.Caption);
				AssertEquals(string.Format(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeCaptionText, "EFF", "NON"), UnitTestUserNotification.Instance.LastMessage.Text);

				AssertEquals(true, approvedAgreement.HasDraft);
				AssertEquals(true, approvedAgreement.Draft.IsReversed);
				AssertEquals(true, unapprovedAgreement.IsReversed);

				var logs = approvedAgreement.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.StatusChangeCode).Select(x => x.SL_Reference).ToArray();
				AssertArrayEqualsByElements(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeLogs, logs);

				logs = unapprovedAgreement.Logs.Find(x => x.SL_SE_NKEvent == ZArchitecture.Business.Events.StatusChangeCode).Select(x => x.SL_Reference).ToArray();
				AssertArrayEqualsByElements(OpportunityCommissionChangeUtils.ExpectedOpportunityCommissionChangeLogs, logs);
			}
		}

		OpportunityManagementControl GetOppControl(ZForm form)
		{
			return ZTestFormUtilities.GetControlsRecursively<OpportunityManagementControl>(form)[0];
		}
	}
}
