using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.MasterFiles.GUI.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class ReceivablesUserControlTest : TestCaseWithFactory
	{
		#region Tax Configuration Template

		public void TestCompanyData_ARTaxTemplateInfo_ValueChanged()
		{
			TestCompanyData_ARTaxTemplateInfo_ValueChanged(Template.PK, ZGuid.Empty, true);
			TestCompanyData_ARTaxTemplateInfo_ValueChanged(ZGuid.Empty, ZGuid.Empty, false);
			TestCompanyData_ARTaxTemplateInfo_ValueChanged(ZGuid.Empty, Template.PK, false);
			TestCompanyData_ARTaxTemplateInfo_ValueChanged(Template.PK, ZGuid.NewZGuid(), false);

			void TestCompanyData_ARTaxTemplateInfo_ValueChanged(ZGuid oldValue, ZGuid newValue, bool shouldShowQuestion)
			{
				TestCompanyData_ARTaxTemplateInfo_ValueChanged_WithAnswer(ZDialogResult.Yes);
				TestCompanyData_ARTaxTemplateInfo_ValueChanged_WithAnswer(ZDialogResult.No);

				void TestCompanyData_ARTaxTemplateInfo_ValueChanged_WithAnswer(ZDialogResult dialogResult)
				{
					TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
					{
						header.CompanyData.OB_OCT_ARTaxTemplate = oldValue;
						Factory.Save();

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(dialogResult);

						header.CompanyData.OB_OCT_ARTaxTemplate = newValue;

						if (shouldShowQuestion)
						{
							AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
							AssertEquals(@"You have removed the Tax Configuration Template recorded against this organization and chosen to leave this field empty. Removing the Template from this organization does NOT make any changes to the Tax Configurations currently populated against this organization. 
Do you want to proceed?
· Select Yes to proceed and save without a related Template. This change would mean that Tax Configurations on this organization will now need to be maintained manually.
· Select No to cancel this action.", UnitTestUserNotification.Instance.LastMessage.Text);

							AssertEquals(dialogResult == ZDialogResult.Yes ? newValue : oldValue, header.CompanyData.OB_OCT_ARTaxTemplate);
						}
						else
						{
							AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasNone);
						}
					});
				}
			}
		}

		public void TestRedefaultFromTemplateButton_Click_ShowQuestion()
		{
			TestRedefaultOrgTaxConfigurationFormTemplate(ZDialogResult.Yes);
			TestRedefaultOrgTaxConfigurationFormTemplate(ZDialogResult.No);

			void TestRedefaultOrgTaxConfigurationFormTemplate(ZDialogResult dialogResult)
			{
				TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
				{
					AssertEquals("Precondition", Template.PK, Header.CompanyData.OB_OCT_ARTaxTemplate);
					AssertNoErrors("Precondition", Header.CompanyData.OB_OCT_ARTaxTemplateInfo);

					UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
					UnitTestUserNotification.Instance.AddAnswer(dialogResult);
					redefaultFromTemplateButton.PerformClick();
					AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
					AssertEquals(@"The Tax Configurations populated in the grid below will be updated using the values defined against the Template you have chosen.
Do you want to proceed? 
· Select Yes to re-default Tax Configuration settings on this organization.
· Select No to cancel this action.", UnitTestUserNotification.Instance.LastMessage.Text);

					var times = dialogResult == ZDialogResult.Yes ? Times.Once() : Times.Never();
					mockITemplateDataHelper.Verify(x => x.UpdateTaxConfigurationsForOrgnization
					(
						It.Is<AccOrgTaxConfigurationCollectionByLedger>((target) => target == Header.CompanyData.AROrgTaxConfigurations),
						It.Is<IEnumerable<AccOrgTaxConfiguration>>((source) => source == Header.CompanyData.ARTaxTemplate.AccOrgTaxConfigurations)
					), times);
				});
			}
		}

		public void TestRedefaultFromTemplateButton_Click_ShowWarningWhenPropertyInfoHasErrors()
		{
			Header.CompanyData.OB_OCT_ARTaxTemplate = ZGuid.Empty;
			Template.OCT_IsActive = false;
			Factory.Save();

			TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
			{
				Header.CompanyData.OB_OCT_ARTaxTemplate = Template.PK;
				AssertHasError(Header.CompanyData.OB_OCT_ARTaxTemplateInfo, "This AR Tax Configuration Template is inactive - it may not be used.");
				AssertHasError(Header.CompanyData.OB_OCT_ARTaxTemplateInfo, "Enter a valid AR Tax Configuration Template.");

				UnitTestUserNotification.Instance.ClearMessages();
				redefaultFromTemplateButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("This AR Tax Configuration Template is inactive - it may not be used.\nEnter a valid AR Tax Configuration Template.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRedefaultFromTemplateButton_Click_ShowWarningWhenTemplateIsNull()
		{
			TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
			{
				Header.CompanyData.OB_OCT_ARTaxTemplate = ZGuid.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				redefaultFromTemplateButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(@"Re-defaulting the Tax Configuration values on this organization is not possible because the Tax Configuration Template field is empty.
Please link a Tax Configuration Template to this organization before attempting to re-default", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRedefaultFromTemplateButton_WhenNotEnableTaxConfigurationTemplate()
		{
			var ex = AssertExceptionThrown<InvalidOperationException>(() => TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) => { }, false));
			AssertEquals("Should not show Tax Config Tab Page when there is no active tax config", "taxConfigurationTabPage was null or disposed", ex.Message);

			TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
			{
				AssertEquals(true, redefaultFromTemplateButton.Visible);
			});
		}

		[RequiresSTA]
		public void TestRedefaultFromTemplateButtonSecurity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			CombineAssertions(() =>
			{
				AssertReadOnly(true);
				AssertReadOnly(false);
			});

			void AssertReadOnly(bool allowed)
			{
				var checkpoint = Env.Security.OrgReceivablesModifyTaxConfigurationTemplate;
				var isAllowed = checkpoint.IsAllowed;
				checkpoint.IsAllowed = allowed;

				using (var form = new ZForm(org))
				using (var control = new ReceivablesUserControl())
				{
					form.Controls.Add(control);
					control.ARTabControl.SelectedTab = control.GetField("taxConfigurationTabPage") as ZTabPage;
					form.Show();

					var button = control.GetField("redefaultFromTemplateButton") as ZButton;

					try
					{
						AssertEquals("RedefaultFromTemplate enabled from security checkpoint", allowed, button.Enabled);
					}
					finally
					{
						checkpoint.IsAllowed = isAllowed;
					}
				}
			}
		}

		void TestRedefaultFromTemplateCore(Action<OrgHeader, AccOrgTaxConfigurationTemplate, ZButton, Mock<IAccOrgTaxConfigurationTemplateDataHelper>> assertAction, bool hasActiveTaxConfig = true)
		{
			var mockProviders = SetupAccountingMasterFilesDependencyIOrgTaxConfigurationTemplateMocks(hasActiveTaxConfig);
			using (ObjectFactory.Substitute(mockProviders.mockIAccountingMasterFilesDependencyFactory.Object))
			using (var form = new ZForm(Header))
			using (var control = new ReceivablesUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var taxConfigTabPage = control.GetField("taxConfigurationTabPage") as ZTabPage;
				if (taxConfigTabPage == null || taxConfigTabPage.IsDisposed)
				{
					throw new InvalidOperationException("taxConfigurationTabPage was null or disposed");
				}
				control.ARTabControl.SelectedTab = taxConfigTabPage;
				var redefaultFromTemplateButton = control.GetField("redefaultFromTemplateButton") as ZButton;
				assertAction.Invoke(Header, Template, redefaultFromTemplateButton, mockProviders.mockIAccTaxConfigurationTemplateHelper);
			}
		}

		(Mock<IAccountingMasterFilesDependencyFactory> mockIAccountingMasterFilesDependencyFactory, Mock<IAccOrgTaxConfigurationTemplateDataHelper> mockIAccTaxConfigurationTemplateHelper) SetupAccountingMasterFilesDependencyIOrgTaxConfigurationTemplateMocks(bool hasActiveTaxConfig = true)
		{
			var mockITemplateDataHelper = new Mock<IAccOrgTaxConfigurationTemplateDataHelper>();
			mockITemplateDataHelper.Setup(x => x.UpdateTaxConfigurationsForOrgnization(It.IsAny<AccOrgTaxConfigurationCollectionByLedger>(), It.IsAny<IEnumerable<AccOrgTaxConfiguration>>()));
			var mockITaxFrameworkConfigurationHelper = new Mock<ITaxFrameworkConfigurationHelper>();
			mockITaxFrameworkConfigurationHelper.Setup(x => x.HasAnyActiveAccTaxConfiguration(It.IsAny<BusinessObjectFactory>(), It.IsAny<GlbCompany>(), It.IsAny<ZString>())).Returns(hasActiveTaxConfig);
			var mockIAccountingMasterFilesDependencyFactory = new Mock<IAccountingMasterFilesDependencyFactory>();
			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetAccOrgTaxConfigurationTemplateDataHelper()).Returns(mockITemplateDataHelper.Object);
			mockIAccountingMasterFilesDependencyFactory.Setup(x => x.GetTaxFrameworkConfigurationHelper()).Returns(mockITaxFrameworkConfigurationHelper.Object);

			return (mockIAccountingMasterFilesDependencyFactory, mockITemplateDataHelper);
		}

		#endregion

		public void TestPayablesUserControl_Has_TaxConfigurationTabPage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;

			using (OrgFormForTest form = new OrgFormForTest(org))
			{
				form.Show();
				form.OrgTabControl.SelectedTab = form.ReceivablesTabPage;
				bool contains = false;
				contains = form.Controls.Find("taxConfigurationTabPage", true).Length == 1;
				AssertEquals(false, contains);
			}

			var collection = new AccTaxConfigurationCollectionForCompanyOrBranch(org.CompanyData.Company);
			var config1 = collection.AddNew();
			config1.ETC_Ledger = LedgerTypes.AccountsReceivable;
			config1.ETC_IsActive = true;

			using (OrgFormForTest form = new OrgFormForTest(org))
			{
				form.Show();
				form.OrgTabControl.SelectedTab = form.ReceivablesTabPage;
				bool contains = false;
				contains = form.Controls.Find("taxConfigurationTabPage", true).Length == 1;
				AssertEquals(true, contains);
			}
		}

		public void TestTaxConfigurationRecordsTabTaxConfigurationPage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			var collection = new AccTaxConfigurationCollectionForCompanyOrBranch(org.CompanyData.Company);

			CombineAssertions(() =>
			{
				AssertTaxConfigurationRecordsTabTaxConfigurationPage(false);

				var config1 = collection.AddNew();
				config1.ETC_Ledger = LedgerTypes.AccountsReceivable;
				config1.ETC_IsActive = false;

				AssertTaxConfigurationRecordsTabTaxConfigurationPage(false);

				var config2 = collection.AddNew();
				config2.ETC_Ledger = LedgerTypes.AccountsReceivable;
				config2.ETC_IsActive = true;

				AssertTaxConfigurationRecordsTabTaxConfigurationPage(true);
			});

			void AssertTaxConfigurationRecordsTabTaxConfigurationPage(bool hasActiveRecords)
			{
				using (OrgFormForTest form = new OrgFormForTest(org))
				{
					form.Show();
					form.OrgTabControl.SelectedTab = form.ReceivablesTabPage;

					bool contains = form.Controls.Find("taxConfigurationTabPage", true).Length == 1;
					AssertEquals(String.Format("Tax configuration tab must be {0}", (hasActiveRecords) ? "visible" : "hidden"), hasActiveRecords, contains);
				}
			}
		}

		public void TestARTaxConfigurationGridVisibleColumns()
		{
			using (var form = new ZForm(Header))
			using (var control = new ReceivablesUserControl())
			{
				form.Controls.Add(control);
				control.ARTabControl.SelectedTab = control.GetField("taxConfigurationTabPage") as ZTabPage;
				form.Show();

				var accOrgTaxRateGrid = control.GetField("accOrgTaxRateGrid") as ZGrid;
				var expectedListOfColumns = new[]
				{
					$"{AccOrgTaxRate.Schema.OTR_StartDate} (ZDateEditColumnStyleInfo) IsVisible:True",
					$"{AccOrgTaxRate.Schema.OTR_EndDate} (ZDateEditColumnStyleInfo) IsVisible:True",
					$"{AccOrgTaxRate.Schema.OTR_Source} (ZDropEditColumnStyleInfo) IsVisible:True",
					$"{AccOrgTaxRate.Schema.OTR_RateNumerator} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"{AccOrgTaxRate.Schema.OTR_RateDenominator} (ZCalcEditColumnStyleInfo) IsVisible:True",
					$"Rate (ZCalcEditColumnStyleInfo) IsVisible:True"
				};
				var listOfColumns = accOrgTaxRateGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x.ToString()} IsVisible:{x.IsVisible}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, listOfColumns);
			}
		}

		[RequiresSTA]
		public void TestTaxConfigurationsGrid()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsDebtor = true;
			using (var form = new ZForm(org))
			using (var control = new ReceivablesUserControl())
			{
				form.Controls.Add(control);
				control.ARTabControl.SelectedTab = control.GetField("taxConfigurationTabPage") as ZTabPage;
				form.Show();

				var grid = control.GetField("accOrgTaxConfigurationGrid") as ZGrid;

				var expectedListOfColumns = new[]
				{
						$"{AccOrgTaxConfiguration.Schema.OTC_ETC} (ZGuidDropEditColumnStyleInfo) IsVisible:True",
						$"OTC_ETC_Description (ZTextBoxColumnStyleInfo) IsVisible:True",
						$"{AccOrgTaxConfiguration.Schema.OTC_IsActive} (ZCheckBoxColumnStyleInfo) IsVisible:True",
						$"{AccOrgTaxConfiguration.Schema.OTC_RecoverTax} (ZCheckBoxColumnStyleInfo) IsVisible:True",
						$"OTC_IsThresholdUsed (ZCheckBoxColumnStyleInfo) IsVisible:True",
				};
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		public void TestTaxConfigurationsGridSecurity()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();

			CombineAssertions(() =>
			{
				AssertReadOnly(true);
				AssertReadOnly(false);
			});

			void AssertReadOnly(bool allowed)
			{
				var checkpoint = Env.Security.OrgReceivablesModifyTaxConfigurationGrid;
				var isAllowed = checkpoint.IsAllowed;
				checkpoint.IsAllowed = allowed;

				using (var form = new ZForm(org))
				using (var control = new ReceivablesUserControl())
				{
					form.Controls.Add(control);
					control.ARTabControl.SelectedTab = control.GetField("taxConfigurationTabPage") as ZTabPage;
					form.Show();

					var grid = control.GetField("accOrgTaxConfigurationGrid") as ZGrid;

					try
					{
						AssertEquals("RedefaultFromTemplate enabled from security checkpoint", allowed, !grid.ReadOnly);
					}
					finally
					{
						checkpoint.IsAllowed = isAllowed;
					}
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Template = TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("TEST", true, null, true);
			Header = Factory.NewWithValidTestData<OrgHeader>();
			Header.OH_IsDebtor = true;
			Header.CompanyData.OB_OCT_ARTaxTemplate = Template.PK;
			Factory.Save();
		}

		public OrgHeader Header;

		public AccOrgTaxConfigurationTemplate Template;

		public AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
