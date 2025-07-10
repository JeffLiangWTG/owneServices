using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(AccGLAccountDescriptorForm))]
	sealed class AccGLAccountDescriptorTestCase : ZFormBasherTest
	{
		public void TestPivotCollectionTabPage()
		{
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			descriptor.AJ_Language = Core.Constants.Languages.English;
			descriptor.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ParentGLHeaderPK = gLHeader.PK;
			Factory.Save();

			using (AccGLAccountDescriptorForm form = new AccGLAccountDescriptorForm(descriptor))
			{
				form.Show();
				Assert("Report Setup Tab is hidden", !form.PivotCollectionTabPage.TabVisible);
			}
			descriptor.AJ_Language = Core.Constants.Languages.English;
			descriptor.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ParentGLHeaderPK = gLHeader.PK;
			using (AccGLAccountDescriptorForm form = new AccGLAccountDescriptorForm(descriptor))
			{
				form.Show();

				Assert("Report Setup Tab is hidden", !form.PivotCollectionTabPage.TabVisible);
			}

			descriptor.AJ_Language = Core.Constants.Languages.ChineseSimplified;
			descriptor.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ParentGLHeaderPK = gLHeader.PK;
			using (AccGLAccountDescriptorForm form = new AccGLAccountDescriptorForm(descriptor))
			{
				form.Show();
				Assert("Report Setup Tab is displayed", form.PivotCollectionTabPage.TabVisible);
			}

			descriptor.ParentGLHeaderPK = Guid.Empty;
			using (AccGLAccountDescriptorForm form = new AccGLAccountDescriptorForm(descriptor))
			{
				form.Show();
				Assert("Report Setup Tab is hidden", !form.PivotCollectionTabPage.TabVisible);
			}

			descriptor.ParentGLHeaderPK = gLHeader.PK;
			descriptor.AJ_Language = Core.Constants.Languages.ChineseSimplified;
			descriptor.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			using (AccGLAccountDescriptorForm form = new AccGLAccountDescriptorForm(descriptor))
			{
				form.Show();
				Assert("Report Setup Tab is hidden", !form.PivotCollectionTabPage.TabVisible);
			}

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		public void TestReportConfigurationPivot()
		{
			ZString country = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
			AccGLAccountDescriptor descriptor = Factory.NewWithValidTestData<AccGLAccountDescriptor>();
			AccGLHeader gLHeader = Factory.NewWithValidTestData<AccGLHeader>();
			descriptor.AJ_Language = Core.Constants.Languages.ChineseSimplified;
			descriptor.AJ_RN_NKCountryOfCompliance = Core.Constants.CountryCodes.China;
			descriptor.AJ_ReportCategory = AccountTypeComboBoxConstants.BalanceSheetAccount;
			descriptor.ParentGLHeaderPK = gLHeader.PK;
			Factory.Save();
			using (AccGLAccountDescriptorForm form = new AccGLAccountDescriptorForm(descriptor))
			{
				form.Show();
				AccGLAccountDescriptor[] descriptors = Factory.Load<AccGLAccountDescriptor>(new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, descriptor.AJ_LocalAccountNumber));
				AssertEquals("AccGLAccountDescriptor should be 1", 1, descriptors.Length);
				AssertEquals("AccGLAccountDescriptor type should be COA", "COA", descriptors[0].AJ_ReportType);

				form.MainTabControl.SelectedTab = form.PivotCollectionTabPage;
				form.PivotCollectionGrid.BeginEdit(form.PivotCollectionGrid.Columns[0].ColumnStyle, 0);
				descriptors = Factory.Load<AccGLAccountDescriptor>(new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, descriptor.AJ_LocalAccountNumber));
				AssertEquals("Report Setup has be set a new default row.", 1, descriptor.ReportConfigurationPivotCollection.Count);
				AssertEquals("AccGLAccountDescriptor should be cloned a record, the records should be 2", 2, descriptors.Length);
				AssertNotEquals("The cloned record type should be Non-COA", "COA", descriptors[1].AJ_ReportType);

				form.PivotCollectionGrid.EndEdit(form.PivotCollectionGrid.Columns[0].ColumnStyle, 0, true);
				form.MainTabControl.SelectedTab = form.DetailsTabPage;
				descriptors = Factory.Load<AccGLAccountDescriptor>(new ZQuery(AccGLAccountDescriptorSchema.AJ_LocalAccountNumber, descriptor.AJ_LocalAccountNumber));
				AssertEquals("Report Setup has be canceled", 0, descriptor.ReportConfigurationPivotCollection.Count);
				AssertEquals("The cloned record should be deleted, the record should be 1", 1, descriptors.Length);
				AssertEquals("AccGLAccountDescriptor type should be COA", "COA", descriptors[0].AJ_ReportType);
			}

			GlbCompany.CurrentCompany.GC_RN_NKCountryCode = country;
		}

		public void TestRestrictLocalAccountTextToNumericAndDotWhenLanguageIsChinese()
		{
			AccGLAccountDescriptor descriptor = Factory.New<AccGLAccountDescriptor>();
			descriptor.AJ_Language = Core.Constants.Languages.English;
			using (AccGLAccountDescriptorForm form = new AccGLAccountDescriptorForm(descriptor))
			{
				form.Show();

				SimulateLocalAccountNumberTextBoxKeyPress(form, 'D');
				AssertEquals("D", form.LocalAccountTextBox.Text);

				SimulateLocalAccountNumberTextBoxKeyPress(form, '1');
				AssertEquals("D1", form.LocalAccountTextBox.Text);

				descriptor.AJ_Language = Core.Constants.Languages.ChineseSimplified;
				descriptor.RefreshBinding();
				AssertEquals("", form.LocalAccountTextBox.Text);

				SimulateLocalAccountNumberTextBoxKeyPress(form, '1');
				AssertEquals("1", form.LocalAccountTextBox.Text);

				SimulateLocalAccountNumberTextBoxKeyPress(form, 'D');
				AssertEquals("1", form.LocalAccountTextBox.Text);

				descriptor.AJ_Language = Core.Constants.Languages.ChineseTraditional;
				AssertEquals("", form.LocalAccountTextBox.Text);

				SimulateLocalAccountNumberTextBoxKeyPress(form, '1');
				AssertEquals("1", form.LocalAccountTextBox.Text);

				SimulateLocalAccountNumberTextBoxKeyPress(form, '.');
				AssertEquals("1.", form.LocalAccountTextBox.Text);

				SimulateLocalAccountNumberTextBoxKeyPress(form, 'E');
				AssertEquals("1.", form.LocalAccountTextBox.Text);
			}
		}

		public void TestAuditPluginIsAdded()
		{
			using (var form = (AccGLAccountDescriptorForm)GetFormToBashCore())
			{
				AssertNotNull("Form should have audit plugin", form.PlugIns.IsPlugInAvailable(ControllerIDs.Audit));
			}
		}

		void SimulateLocalAccountNumberTextBoxKeyPress(AccGLAccountDescriptorForm form, char keyChar)
		{
			KeySender.SendKeyPress(form.LocalAccountTextBox, form.LocalAccountTextBox.Handle, keyChar);
		}

		protected override Form GetFormToBashCore()
		{
			AccGLAccountDescriptor testGLDescriptor = Factory.New<AccGLAccountDescriptor>();
			return new AccGLAccountDescriptorForm(testGLDescriptor);
		}
	}
}
