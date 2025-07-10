using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;
using Enterprise.MasterFiles.Business.Testing.Accounting.Helpers;
using Enterprise.Registry.Business;
using Enterprise.Registry.Business.Testing;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Tests
{
	class PayablesUserControlTest : TestCaseWithFactory
	{
		#region Tax Configuration Template

		public void TestCompanyData_APTaxTemplateInfo_ValueChanged()
		{
			TestCompanyData_APTaxTemplateInfo_ValueChanged(Template.PK, ZGuid.Empty, true);
			TestCompanyData_APTaxTemplateInfo_ValueChanged(ZGuid.Empty, ZGuid.Empty, false);
			TestCompanyData_APTaxTemplateInfo_ValueChanged(ZGuid.Empty, Template.PK, false);
			TestCompanyData_APTaxTemplateInfo_ValueChanged(Template.PK, ZGuid.NewZGuid(), false);

			void TestCompanyData_APTaxTemplateInfo_ValueChanged(ZGuid oldValue, ZGuid newValue, bool shouldShowQuestion)
			{
				TestCompanyData_APTaxTemplateInfo_ValueChanged_WithAnswer(ZDialogResult.Yes);
				TestCompanyData_APTaxTemplateInfo_ValueChanged_WithAnswer(ZDialogResult.No);

				void TestCompanyData_APTaxTemplateInfo_ValueChanged_WithAnswer(ZDialogResult dialogResult)
				{
					TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
					{
						header.CompanyData.OB_OCT_APTaxTemplate = oldValue;
						Factory.Save();

						UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
						UnitTestUserNotification.Instance.AddAnswer(dialogResult);

						header.CompanyData.OB_OCT_APTaxTemplate = newValue;

						if (shouldShowQuestion)
						{
							AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasQuestion);
							AssertEquals(@"You have removed the Tax Configuration Template recorded against this organization and chosen to leave this field empty. Removing the Template from this organization does NOT make any changes to the Tax Configurations currently populated against this organization. 
Do you want to proceed?
· Select Yes to proceed and save without a related Template. This change would mean that Tax Configurations on this organization will now need to be maintained manually.
· Select No to cancel this action.", UnitTestUserNotification.Instance.LastMessage.Text);

							AssertEquals(dialogResult == ZDialogResult.Yes ? newValue : oldValue, header.CompanyData.OB_OCT_APTaxTemplate);
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
					AssertEquals("Precondition", Template.PK, Header.CompanyData.OB_OCT_APTaxTemplate);
					AssertNoErrors("Precondition", Header.CompanyData.OB_OCT_APTaxTemplateInfo);

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
						It.Is<AccOrgTaxConfigurationCollectionByLedger>((target) => target == Header.CompanyData.APOrgTaxConfigurations),
						It.Is<IEnumerable<AccOrgTaxConfiguration>>((source) => source == Header.CompanyData.APTaxTemplate.AccOrgTaxConfigurations)
					), times);
				});
			}
		}

		public void TestRedefaultFromTemplateButton_Click_ShowWarningWhenPropertyInfoHasErrors()
		{
			Header.CompanyData.OB_OCT_APTaxTemplate = ZGuid.Empty;
			Template.OCT_IsActive = false;
			Factory.Save();

			TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
			{
				Header.CompanyData.OB_OCT_APTaxTemplate = Template.PK;
				AssertHasError(Header.CompanyData.OB_OCT_APTaxTemplateInfo, "This AP Tax Configuration Template is inactive - it may not be used.");
				AssertHasError(Header.CompanyData.OB_OCT_APTaxTemplateInfo, "Enter a valid AP Tax Configuration Template.");

				UnitTestUserNotification.Instance.ClearMessages();
				redefaultFromTemplateButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals("This AP Tax Configuration Template is inactive - it may not be used.\nEnter a valid AP Tax Configuration Template.", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		public void TestRedefaultFromTemplateButton_Click_ShowWarningWhenTemplateIsNull()
		{
			TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
			{
				Header.CompanyData.OB_OCT_APTaxTemplate = ZGuid.Empty;
				UnitTestUserNotification.Instance.ClearMessages();
				redefaultFromTemplateButton.PerformClick();

				AssertEquals(true, UnitTestUserNotification.Instance.LastMessage.WasWarning);
				AssertEquals(@"Re-defaulting the Tax Configuration values on this organization is not possible because the Tax Configuration Template field is empty.
Please link a Tax Configuration Template to this organization before attempting to re-default", UnitTestUserNotification.Instance.LastMessage.Text);
			});
		}

		[RequiresSTA]
		public void TestRedefaultFromTemplateButton_WhenNotEnableTaxConfigurationTemplate()
		{
			var ex = AssertExceptionThrown<InvalidOperationException>(() => TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) => { }, false));
			AssertEquals("Should not show Tax Config Tab Page when there is no active tax config", "taxConfigurationTabPage was null or disposed", ex.Message);

			TestRedefaultFromTemplateCore((header, template, redefaultFromTemplateButton, mockITemplateDataHelper) =>
			{
				AssertEquals(true, redefaultFromTemplateButton.Visible);
			});
		}

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
				var checkpoint = Env.Security.OrgPayablesModifyTaxConfigurationTemplate;
				var isAllowed = checkpoint.IsAllowed;
				checkpoint.IsAllowed = allowed;

				using (var form = new ZForm(org))
				using (var control = new PayablesUserControl())
				{
					form.Controls.Add(control);
					control.PayablesDetailsTabControl.SelectedTab = control.GetField("taxConfigurationTabPage") as ZTabPage;
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
			using (var control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				form.Show();

				var payablesDetailsTabControl = control.GetField("PayablesDetailsTabControl") as ZTemplateTabControl;
				var taxConfigTabPage = control.GetField("taxConfigurationTabPage") as ZTabPage;
				if (taxConfigTabPage == null || taxConfigTabPage.IsDisposed)
				{
					throw new InvalidOperationException("taxConfigurationTabPage was null or disposed");
				}
				payablesDetailsTabControl.SelectedTab = taxConfigTabPage;
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

		#region Credit Reports Visibility

		public void TestCreditReportsVisibility()
		{
			CombineAssertions(() =>
			{
				AssertCreditReportsVisibility("org1", false, true, true, true);
				AssertCreditReportsVisibility("org2", false, true, false, true);
				AssertCreditReportsVisibility("org3", false, false, true, true);
				AssertCreditReportsVisibility("org4", false, false, false, true);
				AssertCreditReportsVisibility("org5", true, true, true, false);
				AssertCreditReportsVisibility("org6", true, true, false, true);
				AssertCreditReportsVisibility("org7", true, false, true, true);
				AssertCreditReportsVisibility("org8", true, false, false, true);
			});
		}

		void AssertCreditReportsVisibility(string orgName, bool hasUrl, bool enableCreditReports, bool currentCompanyCountryAvailable, bool expectedPanel1Collapsed)
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_FullName = orgName;
			Factory.Save();

			var creditReportItemCollection = new CreditReportItemCollection() {
				CreditReportItemTest.CreateCreditReportItemForTest(Env.CurrentCompany.Country.Code) };
			creditReportItemCollection[0].CountryEnabledForCompany = currentCompanyCountryAvailable;

			var collection = new CodeDescriptionBoolWithSingleTrueCollection();
			if (hasUrl)
			{
				collection.Add(new CodeDescriptionBoolWithSingleTrue() { CodeMaxLength = 4, Code = "SYD", Description = (NoResString)"https://www.baidu.com", Bool = true });
			}

			using (OrganisationsDataRegistry.Instance.EnableCreditReportsPerCountryOrganisationAndCompany.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, creditReportItemCollection))
			using (OrganisationsDataRegistry.Instance.CreditCheckServiceURLs.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, collection))
			using (OrganisationsDataRegistry.Instance.EnableCreditReports.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, enableCreditReports))
			using (var testForm = new ZForm(org))
			using (var control = new PayablesUserControl())
			{
				testForm.Controls.Add(control);
				testForm.Show();

				var splitContainer = (KSplitContainer)control.Controls.Find("PayablesDetailsContainer", true).First();
				AssertEquals(expectedPanel1Collapsed, splitContainer.Panel1Collapsed);
			}
		}

		#endregion

		public void TestOB_APPrintContractorFormCheckBox()
		{
			var header = Factory.New<OrgHeader>();

			AssertResult(Constants.CountryCodes.Uruguay, false);
			AssertResult(Constants.CountryCodes.UnitedStates, true, "Eligible IRS 1099-MISC Form Org.");
			AssertResult(Constants.CountryCodes.Australia, true, "Include in TPAR Reporting", true);
			AssertResult(Constants.CountryCodes.Australia, false, isTparRegistryEnabled: false);

			void AssertResult(string countryCode, bool expectedVisibility, string expectedText = "", bool isTparRegistryEnabled = false)
			{
				using (GlbCompany.TemporaryLoginInNewCompanyForCountry(countryCode))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isTparRegistryEnabled))
				using (ZForm form = new ZForm(header))
				using (PayablesUserControl control = new PayablesUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					AssertEquals(expectedVisibility, control.OB_APPrintContractorFormCheckBox.Visible);
					if (expectedVisibility)
					{
						AssertEquals(expectedText, control.OB_APPrintContractorFormCheckBox.Text);
					}
				}
			}
		}

		public void TestOB_APExcludeFromPaymentReportsCheckBox()
		{
			var header = Factory.New<OrgHeader>();

			AssertResult(Constants.CountryCodes.Uruguay, false, false);
			AssertResult(Constants.CountryCodes.Australia, true, true);
			AssertResult(Constants.CountryCodes.Australia, false, false);

			void AssertResult(string countryCode, bool expectedVisibility, bool isTparRegistryEnabled)
			{
				using (GlbCompany.TemporaryLoginInNewCompanyForCountry(countryCode))
				using (AccountingMasterFilesRegistry.Instance.EnableComplianceReportsForAustralia.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, isTparRegistryEnabled))
				using (ZForm form = new ZForm(header))
				using (PayablesUserControl control = new PayablesUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					AssertEquals(expectedVisibility, control.OB_APExcludeFromPaymentReportsCheckBox.Visible);
					AssertEquals("Exclude from PTRS Reporting", control.OB_APExcludeFromPaymentReportsCheckBox.Text);
				}
			}
		}

		public void TestOB_GSTRegisteredCheckboxVisibility()
		{
			GlbCompany.CurrentCompany.GC_IsGSTRegistered = true;
			OrgHeader header = Factory.New<OrgHeader>();
			using (ZForm form = new ZForm(header))
			using (PayablesUserControl control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.OB_APVATConfigDropEdit.Visible);
				AssertEquals(true, control.OB_APVATConfigLabel.Visible);
			}

			GlbCompany.CurrentCompany.GC_IsGSTRegistered = false;
			using (ZForm form = new ZForm(header))
			using (PayablesUserControl control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.OB_APVATConfigDropEdit.Visible);
				AssertEquals(false, control.OB_APVATConfigLabel.Visible);
			}
		}

		public void TestAccountDetailsGridVisibility()
		{
			CheckGridVisibility(true, false);
			CheckGridVisibility(false, true);
		}

		public void TestTransCreationRestrictionDropEditVisibility()
		{
			var cachedValue = Env.Security.OrgPayablesModifyConfigTransCreationRestriction.IsAllowed;

			foreach (var isAllowed in new bool[] { true, false })
			{
				using (new DisposableAction(
						() => Env.Security.OrgPayablesModifyConfigTransCreationRestriction.IsAllowed = isAllowed,
						() => Env.Security.OrgPayablesModifyConfigTransCreationRestriction.IsAllowed = cachedValue))
				{
					var header = Factory.NewWithValidTestData<OrgHeader>();
					using (var form = new ZForm(header))
					using (var control = new PayablesUserControl())
					{
						form.Controls.Add(control);
						form.Show();
						Assert(control.zDropEdit_TransCreationRestriction.Visible);
						AssertEquals("Control ReadOnly should follow the relative security right",
									 isAllowed, !control.zDropEdit_TransCreationRestriction.ReadOnly);
					}
				}
			}
		}

		void CheckGridVisibility(bool allowFunction, bool asserText)
		{
			Env.Security.OrgPayablesViewAccountDetails.IsAllowed = allowFunction;
			OrgHeader header = Factory.New<OrgHeader>();
			using (ZForm form = new ZForm(header))
			{
				using (PayablesUserControl control = new PayablesUserControl())
				{
					form.Controls.Add(control);
					form.Show();
					AssertEquals(!allowFunction, control.lblNotAllowedToSeeAccDetails.Visible);
					AssertEquals(allowFunction, control.AccountDetailsGrid.Visible);

					if (asserText)
					{
						AssertEquals("Message", "You do not have the appropriate security rights to run this function.\r\n\r\nIf you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to: \r\n\r\nMaintain > Reference Files  > Organization > View Payables > View Account Details", control.lblNotAllowedToSeeAccDetails.Text);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestOB_APCreateVATComplianceDocumentOnPostingBoundCheckEditVisibility()
		{
			OrgHeader header = Factory.New<OrgHeader>();
			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (ZForm form = new ZForm(header))
			using (PayablesUserControl control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(true, control.OB_APCreateVATComplianceDocumentOnPostingLabel.Visible);
				AssertEquals(true, control.OB_APCreateVATComplianceDocumentOnPostingDropEdit.Visible);
			}

			using (AccountingMasterFilesRegistry.Instance.EnableComplianceDocumentModule.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			using (ZForm form = new ZForm(header))
			using (PayablesUserControl control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				AssertEquals(false, control.OB_APCreateVATComplianceDocumentOnPostingLabel.Visible);
				AssertEquals(false, control.OB_APCreateVATComplianceDocumentOnPostingDropEdit.Visible);
			}
		}

		public void TestAPTaxConfigurationGridVisibleColumns()
		{
			using (var form = new ZForm(Header))
			using (var control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				control.PayablesDetailsTabControl.SelectedTab = control.GetField("taxConfigurationTabPage") as ZTabPage;
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

		public void TestTaxConfigurationsGrid()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = true;

			using (var form = new ZForm(org))
			using (var control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				control.PayablesDetailsTabControl.SelectedTab = control.GetField("taxConfigurationTabPage") as ZTabPage;
				form.Show();

				var taxConfigGrid = control.GetField("accOrgTaxConfigurationGrid") as ZGrid;
				var expectedListOfColumns = new[]
				{
						$"{AccOrgTaxConfiguration.Schema.OTC_ETC} (ZGuidDropEditColumnStyleInfo) IsVisible:True",
						$"OTC_ETC_Description (ZTextBoxColumnStyleInfo) IsVisible:True",
						$"{AccOrgTaxConfiguration.Schema.OTC_IsActive} (ZCheckBoxColumnStyleInfo) IsVisible:True",
						$"OTC_IsThresholdUsed (ZCheckBoxColumnStyleInfo) IsVisible:True",
					};
				var realListOfColumns = taxConfigGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible}").ToArray();
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
				var checkpoint = Env.Security.OrgPayablesModifyTaxConfigurationGrid;
				var isAllowed = checkpoint.IsAllowed;
				checkpoint.IsAllowed = allowed;

				using (var form = new ZForm(org))
				using (var control = new PayablesUserControl())
				{
					form.Controls.Add(control);
					control.PayablesDetailsTabControl.SelectedTab = control.GetField("taxConfigurationTabPage") as ZTabPage;
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

		public void TestPayablesUserControl_Has_TaxConfigurationTabPage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = true;

			using (ZForm form = new ZForm(org))
			using (PayablesUserControl control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				bool contains = false;
				contains = form.Controls.Find("taxConfigurationTabPage", true).Length == 1;
				AssertEquals(false, contains);
			}

			var collection = new AccTaxConfigurationCollectionForCompanyOrBranch(org.CompanyData.Company);
			var config1 = collection.AddNew();
			config1.ETC_Ledger = LedgerTypes.AccountsPayable;
			config1.ETC_IsActive = true;

			using (ZForm form = new ZForm(org))
			using (PayablesUserControl control = new PayablesUserControl())
			{
				form.Controls.Add(control);
				form.Show();
				bool contains = false;
				contains = form.Controls.Find("taxConfigurationTabPage", true).Length == 1;
				AssertEquals(true, contains);
			}
		}

		public void TestTaxConfigurationRecordsTabTaxConfigurationPage()
		{
			var org = Factory.NewWithValidTestData<OrgHeader>();
			org.OH_IsCreditor = true;
			var collection = new AccTaxConfigurationCollectionForCompanyOrBranch(org.CompanyData.Company);

			CombineAssertions(() =>
			{
				AssertTaxConfigurationRecordsTabTaxConfigurationPage(false);

				var config1 = collection.AddNew();
				config1.ETC_Ledger = LedgerTypes.AccountsPayable;
				config1.ETC_IsActive = false;

				AssertTaxConfigurationRecordsTabTaxConfigurationPage(false);

				var config2 = collection.AddNew();
				config2.ETC_Ledger = LedgerTypes.AccountsPayable;
				config2.ETC_IsActive = true;

				AssertTaxConfigurationRecordsTabTaxConfigurationPage(true);
			});

			void AssertTaxConfigurationRecordsTabTaxConfigurationPage(bool hasActiveRecords)
			{
				using (ZForm form = new ZForm(org))
				using (PayablesUserControl control = new PayablesUserControl())
				{
					form.Controls.Add(control);
					form.Show();

					bool contains = form.Controls.Find("taxConfigurationTabPage", true).Length == 1;
					AssertEquals(String.Format("Tax configuration tab must be {0}", (hasActiveRecords) ? "visible" : "hidden"), hasActiveRecords, contains);
				}
			}
		}

		protected override void SetUp()
		{
			base.SetUp();

			Template = TestObjectCreator.CreateAccOrgTaxConfigurationTemplate("TEST", false, null, true);
			Header = Factory.NewWithValidTestData<OrgHeader>();
			Header.OH_IsCreditor = true;
			Header.CompanyData.OB_OCT_APTaxTemplate = Template.PK;
			Factory.Save();
		}

		public OrgHeader Header;

		public AccOrgTaxConfigurationTemplate Template;

		public AccountingTestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new AccountingTestObjectCreator(Factory));
		AccountingTestObjectCreator testObjectCreator;
	}
}
