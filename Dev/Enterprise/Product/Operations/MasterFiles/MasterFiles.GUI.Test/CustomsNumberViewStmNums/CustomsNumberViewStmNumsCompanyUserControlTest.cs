using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(ZChildForm))]
	sealed class CustomsNumberViewStmNumsCompanyUserControlTest : ZFormBasherTest
	{
		public void TestAddButtonVisibility()
		{
			providerSetup?.Dispose();
			providerSetup = null;
			var company = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			using (var setup = new CustomsNumberViewStmNumsGuiProviderForTestingSetUp(true, true))
			using (var testForm = new ZChildForm(company))
			{
				testForm.CaptionRenderingEnabled = true;
				var control = new CustomsNumberViewStmNumsCompanyUserControl(company.CustomsNumberProvider.CustomsNumberWrappers);
				control.Dock = DockStyle.Fill;
				testForm.Controls.Add(control);
				testForm.SetDataBinding(company, "");
				testForm.Show();
				var numberRangesAddCompanyButtonPanel = control.Controls.Find("NumberRangesAddButtonPanel", true).OfType<ZPanel>().First();
				AssertEquals("NumberRangesAddButtonPanel", true, numberRangesAddCompanyButtonPanel.Visible);
				var numberRangesAddCompanyButton = control.Controls.Find("NumberRangesAddButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesAddButton", true, numberRangesAddCompanyButton.Visible);
				AssertEquals("NumberRangesAddButton.CaptionResourceString", "Add &Company", numberRangesAddCompanyButton.CaptionResourceString.Caption);
				var numberRangesAddBranchButtonPanel = control.Controls.Find("NumberRangesAdditionalButtonsPanel", true).OfType<ZPanel>().First();
				AssertEquals("NumberRangesAdditionalButtonsPanel", true, numberRangesAddBranchButtonPanel.Visible);
				var numberRangesAddBranchButton = control.Controls.Find("NumberRangesAddBranchButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesAddBranchButton", true, numberRangesAddBranchButton.Visible);
				AssertEquals("NumberRangesAddBranchButton.CaptionResourceString", "Add &Branch", numberRangesAddBranchButton.CaptionResourceString.Caption);
			}

			company = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			using (var setup = new CustomsNumberViewStmNumsGuiProviderForTestingSetUp(true, false))
			using (var testForm = new ZChildForm(company))
			{
				testForm.CaptionRenderingEnabled = true;
				var control = new CustomsNumberViewStmNumsCompanyUserControl(company.CustomsNumberProvider.CustomsNumberWrappers);
				control.Dock = DockStyle.Fill;
				testForm.Controls.Add(control);
				testForm.SetDataBinding(company, "");
				testForm.Show();
				var numberRangesAddCompanyButtonPanel = control.Controls.Find("NumberRangesAddButtonPanel", true).OfType<ZPanel>().First();
				AssertEquals("NumberRangesAddButtonPanel", true, numberRangesAddCompanyButtonPanel.Visible);
				var numberRangesAddCompanyButton = control.Controls.Find("NumberRangesAddButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesAddButton", true, numberRangesAddCompanyButton.Visible);
				AssertEquals("NumberRangesAddButton.CaptionResourceString", "&Add", numberRangesAddCompanyButton.CaptionResourceString.Caption);
				var numberRangesAddBranchButtonPanel = control.Controls.Find("NumberRangesAdditionalButtonsPanel", true).OfType<ZPanel>().First();
				AssertEquals("NumberRangesAdditionalButtonsPanel", false, numberRangesAddBranchButtonPanel.Visible);
				var numberRangesAddBranchButton = control.Controls.Find("NumberRangesAddBranchButton", true).OfType<ZButton>().FirstOrDefault();
				AssertNull("When branch level is not enabled, we don't even create the add branch button.", numberRangesAddBranchButton);
			}

			company = new BusinessObjectFactory().Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			using (var setup = new CustomsNumberViewStmNumsGuiProviderForTestingSetUp(false, true))
			using (var testForm = new ZChildForm(company))
			{
				testForm.CaptionRenderingEnabled = true;
				var control = new CustomsNumberViewStmNumsCompanyUserControl(company.CustomsNumberProvider.CustomsNumberWrappers);
				control.Dock = DockStyle.Fill;
				testForm.Controls.Add(control);
				testForm.SetDataBinding(company, "");
				testForm.Show();
				var numberRangesAddCompanyButtonPanel = control.Controls.Find("NumberRangesAddButtonPanel", true).OfType<ZPanel>().First();
				AssertEquals("NumberRangesAddButtonPanel", false, numberRangesAddCompanyButtonPanel.Visible);
				var numberRangesAddCompanyButton = control.Controls.Find("NumberRangesAddButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesAddButton", false, numberRangesAddCompanyButton.Visible);
				AssertEquals("NumberRangesAddButton.CaptionResourceString", "Add &Company", numberRangesAddCompanyButton.CaptionResourceString.Caption);
				var numberRangesAddBranchButtonPanel = control.Controls.Find("NumberRangesAdditionalButtonsPanel", true).OfType<ZPanel>().First();
				AssertEquals("NumberRangesAdditionalButtonsPanel", true, numberRangesAddBranchButtonPanel.Visible);
				var numberRangesAddBranchButton = control.Controls.Find("NumberRangesAddBranchButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesAddBranchButton", true, numberRangesAddBranchButton.Visible);
				AssertEquals("NumberRangesAddBranchButton.CaptionResourceString", "&Add", numberRangesAddBranchButton.CaptionResourceString.Caption);
			}
		}

		[RequiresSTA]
		public void TestNumberRangesAddCompanyButton_Click()
		{
			using (var testForm = new ZChildForm(Company))
			{
				testForm.CaptionRenderingEnabled = true;
				var provider = (CustomsNumberViewStmNumsGuiProviderForTesting)Company.CustomsNumberProvider;
				provider.getEditorFormForTesting = (x, y) => new CustomsNumberViewStmNumsEditorForm(y);
				var control = new CustomsNumberViewStmNumsCompanyUserControl(provider.CustomsNumberWrappers);
				control.Dock = DockStyle.Fill;
				testForm.Controls.Add(control);
				testForm.SetDataBinding(Company, "");
				testForm.Show();
				var numberRangesAddCompanyButton = control.Controls.Find("NumberRangesAddButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesAddButton", true, numberRangesAddCompanyButton.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				numberRangesAddCompanyButton.PerformClick();
				var lastFormShown = (CustomsNumberViewStmNumsEditorForm)ZFormModaliser.LastFormShownDialogForTest;
				var wrapper = (CustomsNumberViewStmNumsWrapper)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("IsInDatabase", false, wrapper.StmNums.IsInDatabase);
				AssertEquals("SN_Type", ZString.Empty, wrapper.StmNums.SN_Type);
				AssertEquals("SN_Owner", Company.PK, wrapper.StmNums.SN_Owner);
				AssertEquals("CustomsNumberWrappers.Count", 0, provider.CustomsNumberWrappers.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				numberRangesAddCompanyButton.PerformClick();
				lastFormShown = (CustomsNumberViewStmNumsEditorForm)ZFormModaliser.LastFormShownDialogForTest;
				wrapper = (CustomsNumberViewStmNumsWrapper)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("IsInDatabase", true, wrapper.StmNums.IsInDatabase);
				AssertEquals("SN_Type", ZString.Empty, wrapper.StmNums.SN_Type);
				AssertEquals("SN_Owner", Company.PK, wrapper.StmNums.SN_Owner);
				AssertEquals("CustomsNumberWrappers.Count", 1, provider.CustomsNumberWrappers.Count);
				AssertEquals("Wrapper.SN_Type", ZString.Empty, provider.CustomsNumberWrappers[0].SN_Type);
				AssertEquals("Wrapper.SN_Owner", Company.PK, provider.CustomsNumberWrappers[0].SN_Owner);
			}
		}

		public void TestNumberRangesAddBranchButton_Click()
		{
			var companyER = Factory.New<GlbCompany>();
			companyER.GC_Code = "@#@";
			companyER.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			companyER.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var branch1 = companyER.Branches.AddNew();
			branch1.GB_Code = "1$#";
			branch1.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branch1.GB_IsActive = false;
			var branch2 = companyER.Branches.AddNew();
			branch2.GB_Code = "2B#";
			branch2.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var branch3 = companyER.Branches.AddNew();
			branch3.GB_Code = "2A#";
			branch3.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			var branch4 = companyER.Branches.AddNew();
			branch4.GB_Code = "3$#";
			branch4.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			Factory.Save();
			using (var testForm = new ZChildForm(companyER))
			{
				testForm.CaptionRenderingEnabled = true;
				var provider = (CustomsNumberViewStmNumsGuiProviderForTesting)companyER.CustomsNumberProvider;
				provider.getEditorFormForTesting = (x, y) => new CustomsNumberViewStmNumsEditorForm(y);
				var control = new CustomsNumberViewStmNumsCompanyUserControl(provider.CustomsNumberWrappers);
				control.Dock = DockStyle.Fill;
				testForm.Controls.Add(control);
				testForm.SetDataBinding(companyER, "");
				testForm.Show();
				var numberRangesAddBranchButton = control.Controls.Find("NumberRangesAddBranchButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesAddBranchButton", true, numberRangesAddBranchButton.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				numberRangesAddBranchButton.PerformClick();
				var lastFormShown = (CustomsNumberViewStmNumsEditorForm)ZFormModaliser.LastFormShownDialogForTest;
				var wrapper = (CustomsNumberViewStmNumsWrapper)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("IsInDatabase", false, wrapper.StmNums.IsInDatabase);
				AssertEquals("SN_Type", ZString.Empty, wrapper.StmNums.SN_Type);
				AssertEquals("SN_Owner", branch3.PK, wrapper.StmNums.SN_Owner);
				AssertEquals("CustomsNumberWrappers.Count", 0, provider.CustomsNumberWrappers.Count);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				numberRangesAddBranchButton.PerformClick();
				lastFormShown = (CustomsNumberViewStmNumsEditorForm)ZFormModaliser.LastFormShownDialogForTest;
				wrapper = (CustomsNumberViewStmNumsWrapper)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("IsInDatabase", true, wrapper.StmNums.IsInDatabase);
				AssertEquals("SN_Type", ZString.Empty, wrapper.StmNums.SN_Type);
				AssertEquals("SN_Owner", branch3.PK, wrapper.StmNums.SN_Owner);
				AssertEquals("CustomsNumberWrappers.Count", 1, provider.CustomsNumberWrappers.Count);
				AssertEquals("Wrapper.SN_Type", ZString.Empty, provider.CustomsNumberWrappers[0].SN_Type);
				AssertEquals("Wrapper.SN_Owner", branch3.PK, provider.CustomsNumberWrappers[0].SN_Owner);
			}
		}

		public void TestNumberRangesEditButton_Click()
		{
			var companyER = Factory.New<GlbCompany>();
			companyER.GC_Code = "@#@";
			companyER.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			companyER.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var branchER = companyER.Branches.AddNew();
			branchER.GB_Code = "1$#";
			branchER.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branchER.GB_IsActive = false;
			Factory.Save();
			var provider = new CustomsNumberViewStmNumsGuiProviderForTesting(Factory, Core.Constants.CountryCodes.Eritrea, companyER.PK);
			var stmNums = provider.NewCustomsNumber();
			stmNums.SN_MinimumValue = 10000L;
			stmNums.SN_MaximumValue = 20000L;
			stmNums.Factory.Save();
			companyER = new BusinessObjectFactory().Load<GlbCompany>(companyER.PK);
			provider = (CustomsNumberViewStmNumsGuiProviderForTesting)companyER.CustomsNumberProvider;
			using (var testForm = new ZChildForm(companyER))
			{
				testForm.CaptionRenderingEnabled = true;
				provider.getEditorFormForTesting = (x, y) =>
				{
					y.StmNums.SN_Value = 11000L;
					return new CustomsNumberViewStmNumsEditorForm(y);
				};
				var control = new CustomsNumberViewStmNumsCompanyUserControl(provider.CustomsNumberWrappers);
				control.Dock = DockStyle.Fill;
				testForm.Controls.Add(control);
				testForm.SetDataBinding(companyER, "");
				testForm.Show();
				var numberRangesGrid = control.Controls.Find("NumberRangesGrid", true).OfType<ZGrid>().First();
				numberRangesGrid.Select(0);
				var id = provider.CustomsNumbers[0].SN_ID;
				var numberRangesEditButton = control.Controls.Find("NumberRangesEditButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesEditButton", true, numberRangesEditButton.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				provider.getReasonForNotAbleToModifyForTesting = (x) => new ZString("Hello World");
				numberRangesEditButton.PerformClick();
				AssertEquals("Hello World", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);

				provider.getReasonForNotAbleToModifyForTesting = null;
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.Cancel;
				numberRangesEditButton.PerformClick();
				AssertEquals(typeof(CustomsNumberViewStmNumsEditorForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				var wrapper = (CustomsNumberViewStmNumsWrapper)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("SN_ID", id, wrapper.StmNums.SN_ID);
				AssertEquals("SN_Value", 11000L, wrapper.StmNums.SN_Value);
				AssertEquals("StmNums.HasChanges", true, wrapper.StmNums.HasChanges);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("SN_Value", 10000L, provider.CustomsNumbers[0].SN_Value);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				ZFormModaliser.ResultToReturnFromShowDialog = DialogResult.OK;
				numberRangesEditButton.PerformClick();
				AssertEquals(typeof(CustomsNumberViewStmNumsEditorForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				wrapper = (CustomsNumberViewStmNumsWrapper)ZFormModaliser.LastIBusinessShownOnDialogForTest;
				AssertEquals("SN_ID", id, wrapper.StmNums.SN_ID);
				AssertEquals("SN_Value", 11000L, wrapper.StmNums.SN_Value);
				AssertEquals("StmNums.HasChanges", false, wrapper.StmNums.HasChanges);
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("SN_Value", 11000L, provider.CustomsNumbers[0].SN_Value);
			}
		}

		public void TestNumberRangesDeleteButton_Click()
		{
			var companyER = Factory.New<GlbCompany>();
			companyER.GC_Code = "@#@";
			companyER.GC_OH_OrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
			companyER.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Eritrea;
			var branchER = companyER.Branches.AddNew();
			branchER.GB_Code = "1$#";
			branchER.GB_OH_OrgProxy = GlbBranch.CurrentBranch.GB_OH_OrgProxy;
			branchER.GB_IsActive = false;
			Factory.Save();
			var provider = new CustomsNumberViewStmNumsGuiProviderForTesting(Factory, Core.Constants.CountryCodes.Eritrea, companyER.PK);
			var stmNums = provider.NewCustomsNumber();
			stmNums.SN_MinimumValue = 10000L;
			stmNums.SN_MaximumValue = 20000L;
			stmNums.Factory.Save();
			var pk = provider.CustomsNumbers.First(x => x.PK != stmNums.PK).PK;
			companyER = new BusinessObjectFactory().Load<GlbCompany>(companyER.PK);
			provider = (CustomsNumberViewStmNumsGuiProviderForTesting)companyER.CustomsNumberProvider;
			provider.WrapperTypeForTesting = typeof(CustomsNumberViewStmNumsCompanyWrapperForTesting);
			var wrapper = (CustomsNumberViewStmNumsCompanyWrapperForTesting)provider.CustomsNumberWrappers[0];
			wrapper.ReasonForNotAbleToDeleteForTesting = "Because you cannot";
			wrapper.CanDeleteForTesting = false;
			using (var testForm = new ZChildForm(companyER))
			{
				testForm.CaptionRenderingEnabled = true;
				provider.getEditorFormForTesting = (x, y) => new CustomsNumberViewStmNumsEditorForm(y);
				var control = new CustomsNumberViewStmNumsCompanyUserControl(provider.CustomsNumberWrappers);
				control.Dock = DockStyle.Fill;
				testForm.Controls.Add(control);
				testForm.SetDataBinding(companyER, "");
				testForm.Show();
				var numberRangesGrid = control.Controls.Find("NumberRangesGrid", true).OfType<ZGrid>().First();
				AssertEquals(1, provider.CustomsNumberWrappers.Count);
				numberRangesGrid.Select(0);
				var numberRangesDeleteButton = control.Controls.Find("NumberRangesDeleteButton", true).OfType<ZButton>().First();
				AssertEquals("NumberRangesDeleteButton", true, numberRangesDeleteButton.Visible);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				numberRangesDeleteButton.PerformClick();
				AssertEquals("Because you cannot", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasError);
				AssertEquals(1, provider.CustomsNumberWrappers.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				wrapper.CanDeleteForTesting = true;
				numberRangesDeleteButton.PerformClick();
				AssertEquals("This would delete range 'Owner: Company - @#@ - Company, Range Type: CEN, Name: BOB NUMBER'. Continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(1, provider.CustomsNumberWrappers.Count);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				numberRangesDeleteButton.PerformClick();
				AssertEquals("This would delete range 'Owner: Company - @#@ - Company, Range Type: CEN, Name: BOB NUMBER'. Continue?", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals(0, provider.CustomsNumberWrappers.Count);
			}
		}

		protected override Form GetFormToBashCore() => Form;

		protected override void SetUp()
		{
			base.SetUp();
			countrySetter = GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.Eritrea);
			providerSetup = providerSetup ?? new CustomsNumberViewStmNumsGuiProviderForTestingSetUp();
		}
		static IDisposable providerSetup;
		IDisposable countrySetter;

		protected override void TearDown()
		{
			if (providerSetup != null)
			{
				providerSetup.Dispose();
				providerSetup = null;
			}
			if (form != null)
			{
				form.Dispose();
				form = null;
			}
			if (countrySetter != null)
			{
				countrySetter.Dispose();
				countrySetter = null;
			}
			base.TearDown();
		}

		ZChildForm Form
		{
			get
			{
				if (form == null)
				{
					form = new ZChildForm(Company);
					form.Size = new System.Drawing.Size(1024, 768);
					form.CaptionRenderingEnabled = true;
					var control = new CustomsNumberViewStmNumsCompanyUserControl(Company.CustomsNumberProvider.CustomsNumberWrappers);
					control.Dock = DockStyle.Fill;
					form.Controls.Add(control);
					form.SetDataBinding(Company, "");
				}
				return form;
			}
		}
		ZChildForm form;

		GlbCompany Company => company ?? (company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK));
		GlbCompany company;

		sealed class CustomsNumberViewStmNumsCompanyWrapperForTesting : CustomsNumberViewStmNumsCompanyWrapper
		{
			public CustomsNumberViewStmNumsCompanyWrapperForTesting(CustomsNumberViewStmNums stmNums)
				: base(stmNums)
			{ }

			public override bool CanDelete => CanDeleteForTesting;
			public bool CanDeleteForTesting = true;

			public override MultilingualString ReasonForNotAbleToDelete => (NoResString)ReasonForNotAbleToDeleteForTesting;
			public string ReasonForNotAbleToDeleteForTesting = "";
		}
	}
}
