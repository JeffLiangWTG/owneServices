using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting.CountryCompliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.MasterFiles.GUI.WebAddressValidation;
using Enterprise.MasterFiles.Integration;
using Enterprise.Registry.Business;
using Enterprise.ServiceManager.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using ServiceManager.Integration.Abstractions;
using ServiceManager.Shared.Abstractions;
using WTG.AddressCleansing.Common;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbCompanyForm))]
	sealed class GlbCompanyFormTest : ZFormBasherTest
	{
		#region Cash Advance

		[RequiresSTA]
		public void TestCashAdvanceTabPageVisibilityIsBasedOnEnableCashAdvanceFunctionalityRegistry()
		{
			var mockFunctionalityChecker = new Mock<IAccCashAdvanceFunctionalityChecker>();
			mockFunctionalityChecker.Setup(x => x.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(false);
			ObjectFactory.Substitute(mockFunctionalityChecker.Object);

			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
			using (var form = new GlbCompanyForm(company))
			{
				form.Show();
				var accountingTabControl = form.Controls.Find("AccConfigTabControl", true)[0] as ZTemplateTabControl;
				var cashAdvanceTabPage = accountingTabControl.Controls.Find("CashAdvanceTabPage", true);
				AssertEquals(nameof(cashAdvanceTabPage.Length), 0, cashAdvanceTabPage.Length);
			}

			mockFunctionalityChecker.Setup(x => x.IsReceivablesCashAdvanceFunctionalityEnabled).Returns(true);
			using (var form = new GlbCompanyForm(company))
			{
				form.Show();
				var accountingTabControl = form.Controls.Find("AccConfigTabControl", true)[0] as ZTemplateTabControl;
				var cashAdvanceTabPage = accountingTabControl.Controls.Find("CashAdvanceTabPage", true);
				AssertEquals(nameof(cashAdvanceTabPage.Length), 1, cashAdvanceTabPage.Length);
			}
		}

		#endregion

		#region Tax Configuration

		[RequiresSTA]
		public void TestTaxConfigurationsGrid()
		{
			var company = Factory.New<GlbCompany>();
			var expectedListOfColumns = new[]
			{
				$"{AccTaxConfiguration.Schema.ETC_Code} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_Description} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_RN_NKCountry} (ZTextBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_TaxAuthorityCode} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_TaxSystemCode} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_Ledger} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_TaxRealisationMethod} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_RecoveryMethod} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_ThresholdMethod} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_ThresholdAmount} (ZCalcEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_IsActive} (ZCheckBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_TaxAmountRounding} (ZDropEditColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_AG_LedgerControlAccount} (ZGuidFindBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_AG_TaxControlAccount} (ZGuidFindBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_AG_TaxExpenseAccount} (ZGuidFindBoxColumnStyleInfo) IsVisible:True IsUnavailable:False",
				$"{AccTaxConfiguration.Schema.ETC_AG_TaxPendingControlAccount} (ZGuidFindBoxColumnStyleInfo) IsVisible:True IsUnavailable:False"
			};

			using (var form = new GlbCompanyForm(company))
			{
				var grid = form.GetField("taxConfigurationsGrid") as ZGrid;
				var realListOfColumns = grid.ColumnStyles.Cast<ZGridColumnInfo>().Select(x => $"{x} IsVisible:{x.IsVisible} IsUnavailable:{x.IsUnavailable}").ToArray();
				AssertArrayEqualsByElements(expectedListOfColumns, realListOfColumns);
			}
		}

		public void TestTaxConfigurationControlsAreVisible()
		{
			var helperMock = TestMockObjectCreator.CreateAndRegisterITaxFrameworkConfigurationHelper();

			const string tabPageTaxConfig = "taxConfigurationTabPage";
			const string gridTaxConfig = "taxConfigurationsGrid";

			GlbCompany company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			helperMock.Setup(x => x.IsCompanyLevelTaxSystemConfigured(company)).Returns(() => false);
			using (var form = new GlbCompanyForm(company))
			{
				form.Show();
				var taxConfigurationTabControlCount = form.Controls.Find(tabPageTaxConfig, true).Length;
				var taxConfigurationGridControlCount = form.Controls.Find(gridTaxConfig, true).Length;

				AssertEquals("Should not exists in form.", 0, taxConfigurationTabControlCount);
			}

			helperMock.Setup(x => x.IsCompanyLevelTaxSystemConfigured(company)).Returns(() => true);
			using (var form = new GlbCompanyForm(company))
			{
				form.Show();
				var taxConfigurationTabControlCount = form.Controls.Find(tabPageTaxConfig, true).Length;
				var taxConfigurationGridControlCount = form.Controls.Find(gridTaxConfig, true).Length;

				AssertEquals("taxConfigurationTabControlCount should be 1.", 1, taxConfigurationTabControlCount);
			}
		}

		[RequiresSTA]
		public void TestSignatureCredentialTab()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.SetCountry(Core.Constants.CountryCodes.Australia);
				using (var companyForm = new GlbCompanyForm(company))
				{
					companyForm.Show();
					var tabControl = companyForm.Controls.Find("AccConfigTabControl", true)[0] as ZTemplateTabControl;
					var signatureCredentialTabPages = tabControl.Controls.Find("EInvoiceCredentialsForTurkeyTabPage", true);
					AssertEquals(nameof(signatureCredentialTabPages.Length), 0, signatureCredentialTabPages.Length);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.SetCountry(Core.Constants.CountryCodes.Turkey);
				using (var companyForm = new GlbCompanyForm(company))
				{
					companyForm.Show();
					var tabControl = companyForm.Controls.Find("AccConfigTabControl", true)[0] as ZTemplateTabControl;
					var signatureCredentialTabPages = tabControl.Controls.Find("EInvoiceCredentialsForTurkeyTabPage", true);
					AssertEquals(nameof(signatureCredentialTabPages.Length), 0, signatureCredentialTabPages.Length);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.SetCountry(Core.Constants.CountryCodes.Turkey);
				using (var companyForm = new GlbCompanyForm(company))
				{
					companyForm.Show();
					var tabControl = companyForm.Controls.Find("AccConfigTabControl", true)[0] as ZTemplateTabControl;
					var signatureCredentialTabPages = tabControl.Controls.Find("EInvoiceCredentialsForTurkeyTabPage", true);
					AssertEquals(nameof(signatureCredentialTabPages.Length), 1, signatureCredentialTabPages.Length);
					var signatureCredentialTabPage = signatureCredentialTabPages[0] as ZTabPage;
					AssertEquals(nameof(signatureCredentialTabPage.TabVisible), true, signatureCredentialTabPage.TabVisible);
				}
			}
		}

		public void TestARInvoiceTemplateTab()
		{
			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.SetCountry(Core.Constants.CountryCodes.Australia);
				using (var companyForm = new GlbCompanyForm(company))
				{
					companyForm.Show();
					var tabControl = companyForm.Controls.Find("AccConfigTabControl", true)[0] as ZTemplateTabControl;
					var aRInvTemplateTabPages = tabControl.Controls.Find("ARInvTemplateTabPage", true);
					AssertEquals(nameof(aRInvTemplateTabPages.Length), 0, aRInvTemplateTabPages.Length);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(1).ToDateTime()))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.SetCountry(Core.Constants.CountryCodes.Turkey);
				using (var companyForm = new GlbCompanyForm(company))
				{
					companyForm.Show();
					var tabControl = companyForm.Controls.Find("AccConfigTabControl", true)[0] as ZTemplateTabControl;
					var aRInvTemplateTabPages = tabControl.Controls.Find("ARInvTemplateTabPage", true);
					AssertEquals(nameof(aRInvTemplateTabPages.Length), 0, aRInvTemplateTabPages.Length);
				}
			}

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.UtcNow.AddDays(-1).ToDateTime()))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.SetCountry(Core.Constants.CountryCodes.Turkey);
				using (var companyForm = new GlbCompanyForm(company))
				{
					companyForm.Show();
					var tabControl = companyForm.Controls.Find("AccConfigTabControl", true)[0] as ZTemplateTabControl;
					var aRInvTemplateTabPages = tabControl.Controls.Find("ARInvTemplateTabPage", true);
					AssertEquals(nameof(aRInvTemplateTabPages.Length), 1, aRInvTemplateTabPages.Length);
					var aRInvTemplateTabPage = aRInvTemplateTabPages[0] as ZTabPage;
					AssertEquals(nameof(aRInvTemplateTabPage.TabVisible), true, aRInvTemplateTabPage.TabVisible);
				}
			}
		}

		#endregion

		public void TestPhoneFaxColumnOfBranchGridValidation()
		{
			var company = Factory.New<GlbCompany>();
			var branch = company.Branches.AddNew();
			using (var form = new GlbCompanyForm(company))
			{
				form.Show();
				var branchesGrid = (form.Controls.Find("GlbBranchModuleButtonGrid", true)[0] as ZModuleButtonGrid).InnerGrid as ZGrid;

				var phoneColumnStyleInfo = branchesGrid.GetColumnStyle("GB_Phone_Formatted");
				AssertNotNull(phoneColumnStyleInfo);
				var phoneColumnStyle = branchesGrid.Columns["GB_Phone_Formatted"].ColumnStyle as ZTextBoxColumnStyle;
				phoneColumnStyle.Width = 80;
				AssertEquals("Phone", phoneColumnStyle.HeaderText);
				phoneColumnStyle.Width = 200;
				AssertEquals("Phone Number", phoneColumnStyle.HeaderText);

				var faxColumnStyleInfo = branchesGrid.GetColumnStyle("GB_Fax_Formatted");
				AssertNotNull(faxColumnStyleInfo);
				var faxColumnStyle = branchesGrid.Columns["GB_Fax_Formatted"].ColumnStyle as ZTextBoxColumnStyle;
				faxColumnStyle.Width = 30;
				AssertEquals("Fax", faxColumnStyle.HeaderText);
				faxColumnStyle.Width = 200;
				AssertEquals("Fax Number", faxColumnStyle.HeaderText);
			}
		}

		#region Delete Current Company

		public void TestDeleteCurrentCompany()
		{
			GlbCompany company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
			Assert("Current Company should not be null.", company != null);

			using (var form = new GlbCompanyFormForTest(company))
			{
				try
				{
					form.FormDelete();
				}
				catch (Exception)
				{
				}
				company = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK);
				Assert("Current Company could not be deleted.", company != null);
			}
		}

		#endregion

		#region GC_RegNo1AndGC_RegNo2

		public void TestGC_RegNo1AndGC_RegNo2()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			using (var form = new GlbCompanyFormForTest(company))
			{
				company.GC_RN_NKCountryCode = ZString.Empty;
				form.Show();
				var businessRegNoTextBox = form.Controls.Find("GC_BusinessRegNoTextBox", true)[0];
				var businessRegNo2TextBox = form.Controls.Find("GC_BusinessRegNo2TextBox", true)[0];

				var country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Australia);
				if (country != null)
				{
					company.GC_RN_NKCountryCode = country.Code;
					AssertEquals("Australian Business Number", businessRegNoTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					AssertEquals("Australian Company Number", businessRegNo2TextBox.GetExtension<LabelCaptionRenderer>().Caption);
				}
				country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Indonesia);
				if (country != null)
				{
					company.GC_RN_NKCountryCode = country.Code;
					AssertEquals("PPN Reg No", businessRegNoTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					AssertEquals("Company Reg No", businessRegNo2TextBox.GetExtension<LabelCaptionRenderer>().Caption);
				}
				country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.Thailand);
				if (country != null)
				{
					company.GC_RN_NKCountryCode = country.Code;
					AssertEquals("VAT Reg No", businessRegNoTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					AssertEquals("Company Reg No", businessRegNo2TextBox.GetExtension<LabelCaptionRenderer>().Caption);
				}
				country = Factory.LoadFromNaturalKey<RefCountry>(RefCountrySchema.RN_Code, Core.Constants.CountryCodes.UnitedStates);
				if (country != null)
				{
					company.GC_RN_NKCountryCode = country.Code;
					AssertEquals("GST Reg No", businessRegNoTextBox.GetExtension<LabelCaptionRenderer>().Caption);
					AssertEquals("Company Reg No", businessRegNo2TextBox.GetExtension<LabelCaptionRenderer>().Caption);
				}
			}
		}

		#endregion

		#region New Company

		public void TestDoesNotShowChangeMessageForNewCompany()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			using (var companyForm = new GlbCompanyFormForTest(company))
			{
				company.GC_IsReciprocal = true;
				companyForm.FormSaveInternal();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCompanyCode()
		{
			GlbCompany company = Factory.NewWithValidTestData<GlbCompany>();
			using (var companyForm = new GlbCompanyFormForTest(company))
			{
				Assert("company code should be editable", !company.GC_Code_ReadOnly);
				GlbStaff.CurrentUser.GS_LoginName = "TestLogin";
				Assert("company code should be editable", !company.GC_Code_ReadOnly);
				company.GC_Code = "DDE";
				AssertEquals("Company code cannot be changed once saved, except by support. Please ensure the code is a suitable value to identify this company.", UnitTestUserNotification.Instance.LastMessage.Text);
				Factory.Save();
			}

			using (var companyForm = new GlbCompanyFormForTest(company))
			{
				Assert("company code should not be editable", company.GC_Code_ReadOnly);
				GlbStaff.CurrentUser.GS_LoginName = User.SupportUserName;
				Assert("company code should be editable", !company.GC_Code_ReadOnly);
			}
		}

		void AssertExchangeRates(RefExchangeRate exchangeRateBuy, decimal expectedBuySellRate,
				RefExchangeRate exchangeRateSell, decimal expectedSellSellRate,
				RefExchangeRate exchangeRateCus, decimal expectedCusSellRate)
		{
			AssertEquals("Expected BUY ExRateType", Core.Constants.ExchangeRateTypes.Code.BuyRate, exchangeRateBuy.RE_ExRateType);
			AssertEquals("Expected SellRate", expectedBuySellRate, exchangeRateBuy.RE_SellRate);
			AssertEquals("Expected SEL ExRateType", Core.Constants.ExchangeRateTypes.Code.SellRate, exchangeRateSell.RE_ExRateType);
			AssertEquals("Expected SellRate", expectedSellSellRate, exchangeRateSell.RE_SellRate);
			AssertEquals("Expected CUS ExRateType", Core.Constants.ExchangeRateTypes.Code.CustomsRate, exchangeRateCus.RE_ExRateType);
			AssertEquals("Expected SellRate", expectedCusSellRate, exchangeRateCus.RE_SellRate);
		}

		[RequiresSTA]
		public void TestChangeIsReciprocal()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "DDD";
			company.GC_IsReciprocal = true;

			using (var companyForm = new GlbCompanyFormForTest(company))
			{
				RefCurrency currency = Factory.NewWithValidTestData<RefCurrency>();
				RefExchangeRate exchangeRateBuy = currency.ExchangeRates.AddNew();
				RefExchangeRate exchangeRateSell = currency.ExchangeRates.AddNew();
				RefExchangeRate exchangeRateCus = currency.ExchangeRates.AddNew();

				exchangeRateBuy.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.BuyRate;
				exchangeRateBuy.RE_SellRate = 9.0M;
				exchangeRateBuy.RE_GC = company.PK;
				exchangeRateBuy.RE_StartDate = new ZDateTime(2000, 1, 1);
				exchangeRateBuy.RE_ExpiryDate = new ZDateTime(2030, 12, 31);

				exchangeRateSell.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.SellRate;
				exchangeRateSell.RE_SellRate = 18.0M;
				exchangeRateSell.RE_GC = company.PK;
				exchangeRateSell.RE_StartDate = new ZDateTime(2000, 1, 1);
				exchangeRateSell.RE_ExpiryDate = new ZDateTime(2030, 12, 31);

				exchangeRateCus.RE_ExRateType = Core.Constants.ExchangeRateTypes.Code.CustomsRate;
				exchangeRateCus.RE_SellRate = 0.125M;
				exchangeRateCus.RE_GC = company.PK;
				exchangeRateCus.RE_StartDate = new ZDateTime(2000, 1, 1);
				exchangeRateCus.RE_ExpiryDate = new ZDateTime(2030, 12, 31);

				Factory.Save();

				exchangeRateBuy.Reload();
				exchangeRateSell.Reload();
				exchangeRateCus.Reload();
				decimal expectedBuySellRate = 9.0M;
				decimal expectedSellSellRate = 18.0M;
				decimal expectedCusSellRate = 0.125M;
				AssertExchangeRates(exchangeRateBuy, expectedBuySellRate, exchangeRateSell, expectedSellSellRate, exchangeRateCus, expectedCusSellRate);

				exchangeRateBuy.RE_SellRate = 9.0M;
				exchangeRateSell.RE_SellRate = 18.0M;
				exchangeRateCus.RE_SellRate = 0.125M;
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				company.GC_IsReciprocal = false;
				AssertEquals("Is reciprocal should be unchanged if answer no", true, company.GC_IsReciprocal);

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				company.GC_IsReciprocal = false;
				AssertEquals("Is reciprocal is changed", false, company.GC_IsReciprocal);
				companyForm.FormSaveInternal();

				exchangeRateBuy.Reload();
				exchangeRateSell.Reload();
				exchangeRateCus.Reload();
				expectedBuySellRate = 0.111111M;
				expectedSellSellRate = 0.055556M;
				AssertExchangeRates(exchangeRateBuy, expectedBuySellRate, exchangeRateSell, expectedSellSellRate, exchangeRateCus, expectedCusSellRate);

				//NOTE: Enterprise.Accounting.Business.UpdateReciprocalExchangeRatesTestCase implements all the reciprocal exchange rate unit tests
			}
		}
		#endregion

		#region Phone Numbers

		[RequiresSTA]
		public void TestPhoneNumberControls()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			using (var companyForm = new GlbCompanyFormForTest(company))
			{
				Assert(!companyForm.PhoneNumberControl.ShowToolTip);
				Assert(companyForm.PhoneNumberControl.ShowDiallerControl);
				Assert(companyForm.PhoneNumberControl.ShowLocalNumberLabel);
				Assert(!companyForm.PhoneNumberControl.ShowPublishedCheckBox);
				Assert(companyForm.PhoneNumberControl.EnableValidStateColor);
				Assert(!companyForm.FaxNumberControl.ShowToolTip);
				Assert(!companyForm.FaxNumberControl.ShowDiallerControl);
				Assert(!companyForm.FaxNumberControl.ShowLocalNumberLabel);
				Assert(!companyForm.FaxNumberControl.ShowPublishedCheckBox);
				Assert(companyForm.FaxNumberControl.EnableValidStateColor);
			}
		}

		#endregion

		public void TestGC_IsGSTCheckBoxesUseLocalTaxName()
		{
			foreach (var countryCode in Country.LicenceKeyBuilderSupportedCountryCodes)
			{
				GlbCompany company = Factory.New<GlbCompany>();
				company.GC_RN_NKCountryCode = countryCode;
				if (company.Country == null)
				{
					continue;
				}

				var taxName = Country.GetConsumptionTaxDescription(countryCode);
				using (var form = new GlbCompanyForm(company))
				{
					form.Show();
					var gC_IsGSTRegisteredCheckBox = form.Controls.Find("GC_IsGSTRegisteredCheckBox", true)[0];
					if (string.IsNullOrEmpty(taxName))
					{
						// default value is "VAT"
						AssertEquals(string.Format("{0} Registered", "VAT"), gC_IsGSTRegisteredCheckBox.GetExtension<LabelCaptionRenderer>().Caption);
					}
					else
					{
						AssertEquals(string.Format("{0} Registered", taxName), gC_IsGSTRegisteredCheckBox.GetExtension<LabelCaptionRenderer>().Caption);
					}

					var gC_IsGSTCashBasisCheckBox = form.Controls.Find("GC_IsGSTCashBasisCheckBox", true)[0];
					if (string.IsNullOrEmpty(taxName))
					{
						// default value is "VAT"
						AssertEquals(string.Format("Cash Basis {0} Enabled", "VAT"), gC_IsGSTCashBasisCheckBox.GetExtension<LabelCaptionRenderer>().Caption);
					}
					else
					{
						AssertEquals(string.Format("Cash Basis {0} Enabled", taxName), gC_IsGSTCashBasisCheckBox.GetExtension<LabelCaptionRenderer>().Caption);
					}
				}
			}
		}

		[RequiresSTA]
		public void TestBranchesGroupBoxIsReadOnlyWhenBranchModifyIsAllowed()
		{
			var company = CreateNewCompanyWithBranches();

			Env.Security.BranchModify.IsAllowed = true;

			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();

				var branchesGroupBox = form.Controls.Find("BranchesGroupBox", true)[0];

				AssertAllEquals(false, branchesGroupBox.Controls.Cast<Control>().Select(control => control.GetReadOnly()).ToList());
			}

			// Branch Modify check is done inside form constructor so setting permission has to be done outside form creation.
			Env.Security.BranchModify.IsAllowed = false;

			using (var form2 = new GlbCompanyFormForTest(company))
			{
				form2.Show();

				var branchesGroupBox = form2.Controls.Find("BranchesGroupBox", true)[0];

				AssertAllEquals(true, branchesGroupBox.Controls.Cast<Control>().Select(control => control.GetReadOnly()).ToList());
			}
		}

		[RequiresSTA]
		public void TestCompanyAccSurchargeConfigurationAndAccSurchargeBasisIsReadOnlyWhenCompaniesModifySurchargeConfigurationIsNotAllowed()
		{
			var company = Factory.New<GlbCompany>();

			Env.Security.CompaniesModifySurchargeConfiguration.IsAllowed = true;

			using (var form1 = new GlbCompanyFormForTest(company))
			{
				form1.Show();

				var accSurchargeConfiguration = form1.Controls.Find("accSurchargeConfiguration", true)[0];
				AssertAllEquals(false, accSurchargeConfiguration.Controls.Cast<Control>().Select(control => control.GetReadOnly()).ToList());

				var accSurchargeBasis = form1.Controls.Find("accSurchargeBasis", true)[0];
				AssertAllEquals(false, accSurchargeBasis.Controls.Cast<Control>().Select(control => control.GetReadOnly()).ToList());
			}

			Env.Security.CompaniesModifySurchargeConfiguration.IsAllowed = false;

			using (var form2 = new GlbCompanyFormForTest(company))
			{
				form2.Show();

				var accSurchargeConfiguration = form2.Controls.Find("accSurchargeConfiguration", true)[0];
				AssertAllEquals(true, accSurchargeConfiguration.Controls.Cast<Control>().Select(control => control.GetReadOnly()).ToList());

				var accSurchargeBasis = form2.Controls.Find("accSurchargeBasis", true)[0];
				AssertAllEquals(true, accSurchargeBasis.Controls.Cast<Control>().Select(control => control.GetReadOnly()).ToList());
			}
		}

		[RequiresSTA]
		public void TestCompanyAccSurchargeApplicationGridIsReadOnlyWhenCompaniesModifySurchargeApplicationIsNotAllowed()
		{
			var company = Factory.New<GlbCompany>();

			Env.Security.CompaniesModifySurchargeApplication.IsAllowed = true;

			using (var form1 = new GlbCompanyFormForTest(company))
			{
				form1.Show();

				var accSurchargeApplicationGrid = form1.Controls.Find("accSurchargeApplicationGrid", true)[0];
				AssertAllEquals(false, accSurchargeApplicationGrid.Controls.Cast<Control>().Select(control => control.GetReadOnly()).ToList());
			}

			Env.Security.CompaniesModifySurchargeApplication.IsAllowed = false;

			using (var form2 = new GlbCompanyFormForTest(company))
			{
				form2.Show();

				var accSurchargeApplicationGrid = form2.Controls.Find("accSurchargeApplicationGrid", true)[0];
				AssertAllEquals(true, accSurchargeApplicationGrid.Controls.Cast<Control>().Select(control => control.GetReadOnly()).ToList());
			}
		}

		[RequiresSTA]
		public void TestGC_StateVisibility()
		{
			var company = Factory.New<GlbCompany>();

			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();

				var stateTextBox = form.Controls.Find("GC_StateTextBox", true)[0];
				var stateDropEdit = form.Controls.Find("GC_StateDropEdit", true)[0];

				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.HongKong;
				Assert("TextBox.Visible = true", stateTextBox.Visible);
				Assert("DropEdit.Visible = false", !stateDropEdit.Visible);

				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;
				Assert("TextBox.Visible = false", !stateTextBox.Visible);
				Assert("DropEdit.Visible = true", stateDropEdit.Visible);

				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.NewZealand;
				Assert("TextBox.Visible = false", !stateTextBox.Visible);
				Assert("DropEdit.Visible = true", stateDropEdit.Visible);
			}
		}

		[RequiresSTA]
		public void TestAddressValidationButtonHasClickEvent()
		{
			var company = Factory.LoadTop1<GlbCompany>(new ZQuery());

			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();

				var propertyInfo = typeof(Button).GetProperty("Events", BindingFlags.Instance | BindingFlags.NonPublic);
				var eventHandlerList = (EventHandlerList)propertyInfo.GetValue(form.ValidateButton, null);
				var fieldInfo = typeof(Control).GetField("EventClick", BindingFlags.Static | BindingFlags.NonPublic);
				var delegates = eventHandlerList[fieldInfo.GetValue(null)].GetInvocationList();
				AssertEquals(1, delegates.Length);
				AssertEquals("ValidateAddressButton_Click", delegates[0].Method.Name);
			}
		}

		[RequiresSTA]
		public void TestDefalutValues()
		{
			var company = Factory.New<GlbCompany>();
			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();

				var gC_IsGSTRegisteredCheckBox = form.Controls.Find("GC_IsGSTRegisteredCheckBox", true).First() as ZCheckBox;
				var gC_IsGSTCashBasisCheckBox = form.Controls.Find("GC_IsGSTCashBasisCheckBox", true).First() as ZCheckBox;
				var gC_IsReciprocalCheckBox = form.Controls.Find("GC_IsReciprocalCheckEdit", true).First() as ZCheckBox;

				Assert("Default country code should be the CurrentCompany's country code.", form.CountryCodeFindBox.Text.Equals(GlbCompany.CurrentCompany.GC_RN_NKCountryCode));
				Assert("Default currency code should be the CurrentCompany's country currency.", form.CurrencyCodeFindBox.Text.Equals(GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency));
				Assert("Default country's IsGSTRegistered should be the CurrentCompany's IsGSTRegistered", gC_IsGSTRegisteredCheckBox.Checked.Equals(GlbCompany.CurrentCompany.GC_IsGSTRegistered));
				Assert("Default country's IsGSTCashBasis should be the CurrentCompany's IsGSTCashBasis", gC_IsGSTCashBasisCheckBox.Checked.Equals(GlbCompany.CurrentCompany.GC_IsGSTCashBasis));
				Assert("Default country's IsReciprocal should be the CurrentCompany's IsReciprocal", gC_IsReciprocalCheckBox.Checked.Equals(GlbCompany.CurrentCompany.GC_IsReciprocal));
			}
		}

		[RequiresSTA]
		public void TestLoadingCountryRelatedValues()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.AccountingCountry = Core.Constants.CountryCodes.Australia;
			company.GC_IsGSTRegistered = false;
			company.GC_IsReciprocal = true;

			bool savedIsGSTRegistered = company.GC_IsGSTRegistered;
			bool savedIsGSTCashBasis = company.GC_IsGSTCashBasis;
			bool savedIsReciprocal = company.GC_IsReciprocal;

			AssertEquals(Core.Constants.CountryCodes.Australia, company.GC_RN_NKCountryCode);
			AssertEquals(Core.Constants.CurrencyCodes.Australia, company.GC_RX_NKLocalCurrency);
			Assert(!savedIsGSTRegistered);
			Assert(!savedIsGSTCashBasis);
			Assert(savedIsReciprocal);

			Factory.Save();

			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();

				var loadedIsGSTRegistered = form.Controls.Find("GC_IsGSTRegisteredCheckBox", true).First() as ZCheckBox;
				var loadedIsGSTCashBasis = form.Controls.Find("GC_IsGSTCashBasisCheckBox", true).First() as ZCheckBox;
				var loadedIsReciprocal = form.Controls.Find("GC_IsReciprocalCheckEdit", true).First() as ZCheckBox;

				Assert("Current country should be Australia", form.CountryCodeFindBox.Text.Equals(Core.Constants.CountryCodes.Australia));
				AssertEquals(loadedIsGSTRegistered.Checked, savedIsGSTRegistered);
				AssertEquals(loadedIsGSTCashBasis.Checked, savedIsGSTCashBasis);
				AssertEquals(loadedIsReciprocal.Checked, savedIsReciprocal);
			}
		}

		public void TestUSCredentialsVisibility()
		{
			AssertCompanyCredentialsPluginVisibility(Core.Constants.CountryCodes.UnitedStates, true);
		}

		public void TestPRCredentialsVisibility()
		{
			AssertCompanyCredentialsPluginVisibility(Core.Constants.CountryCodes.PuertoRico, true);
		}

		public void TestAUCredentialsVisibility()
		{
			AssertCompanyCredentialsPluginVisibility(Core.Constants.CountryCodes.Australia, true);
		}

		public void TestCredentialsVisibility()
		{
			AssertCompanyCredentialsPluginVisibility(Core.Constants.CountryCodes.Eritrea, true);
		}

		public void TestCompanyInvoiceCertificateCredentialsTab()
		{
			var credentialsMock = new Mock<IEInvoicingCertificateCredentialSettings>();
			var settingsMock = new Mock<ICountryEInvoicingObjectFactorySettings>();
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(settingsMock.Object);
			settingsMock.Setup(x => x.Credentials).Returns(credentialsMock.Object);
			ObjectFactory.Substitute(globalFactoryMock.Object);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(false);
			using (var companyForm = new GlbCompanyForm(company))
			{
				companyForm.Show();
				var tabControl = companyForm.Controls.Find("CompanyTabControl", true)[0] as ZTemplateTabControl;
				var companyCredentialTabPages = tabControl.Controls.Find("EInvoiceCertificatesTabPage", true);
				AssertEquals(nameof(companyCredentialTabPages.Length), 0, companyCredentialTabPages.Length);
			}

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			using (var companyForm = new GlbCompanyForm(company))
			{
				companyForm.Show();
				var tabControl = companyForm.Controls.Find("CompanyTabControl", true)[0] as ZTemplateTabControl;
				var companyCredentialTabPages = tabControl.Controls.Find("EInvoiceCertificatesTabPage", true);
				AssertEquals(nameof(companyCredentialTabPages.Length), 1, companyCredentialTabPages.Length);

				var companyCredentialTabPage = companyCredentialTabPages[0] as ZTabPage;
				AssertEquals(nameof(companyCredentialTabPage.TabVisible), true, companyCredentialTabPage.TabVisible);

				var companyCredentialsControls = companyCredentialTabPage.Controls.Find("CompanyEInvoicingCredentialUserControl", true);
				AssertEquals(nameof(companyCredentialsControls.Length), 1, companyCredentialsControls.Length);

				var companyCredentialsControl = companyCredentialsControls[0];
				AssertType<GlbCompanyForm_CredentialUserControl>("Control should be for Mexico company credentials", companyCredentialsControl);
			}
		}

		public void TestCompanyCredentialTab_With_IEInvoiceCredentialsProvider()
		{
			var credentialsMock = new Mock<IEInvoicingCredentialSettings>();
			var settingsMock = new Mock<ICountryEInvoicingObjectFactorySettings>();
			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			globalFactoryMock.Setup(x => x.GetCountryEInvoicingObjectFactorySettings(It.IsAny<ZString>())).Returns(settingsMock.Object);
			settingsMock.Setup(x => x.Credentials).Returns(credentialsMock.Object);
			ObjectFactory.Substitute(globalFactoryMock.Object);

			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();
			var eInvoiceCredentialsProvider = mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>();

			mockICountryComplianceFactory
				.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			ObjectFactory.Substitute(mockICountryComplianceFactory.Object);

			var company = Factory.NewWithValidTestData<GlbCompany>();

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(false);
			eInvoiceCredentialsProvider.Setup(x => x.ShouldShowCertificatesTab(It.IsAny<ICompany>())).Returns(false);
			AssertTabNotExists();

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(false);
			eInvoiceCredentialsProvider.Setup(x => x.ShouldShowCertificatesTab(It.IsAny<ICompany>())).Returns(true);
			AssertTabExists();

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			eInvoiceCredentialsProvider.Setup(x => x.ShouldShowCertificatesTab(It.IsAny<ICompany>())).Returns(false);
			AssertTabNotExists();

			credentialsMock.Setup(x => x.IsCompanyCredentialsRequired).Returns(true);
			eInvoiceCredentialsProvider.Setup(x => x.ShouldShowCertificatesTab(It.IsAny<ICompany>())).Returns(true);
			AssertTabExists();

			void AssertTabNotExists()
			{
				using (var companyForm = new GlbCompanyForm(company))
				{
					companyForm.Show();
					var tabControl = companyForm.Controls.Find("CompanyTabControl", true)[0] as ZTemplateTabControl;
					var companyCredentialTabPages = tabControl.Controls.Find("EInvoiceCertificatesTabPage", true);
					AssertEquals(nameof(companyCredentialTabPages.Length), 0, companyCredentialTabPages.Length);
				}
			}

			void AssertTabExists()
			{
				using (var companyForm = new GlbCompanyForm(company))
				{
					companyForm.Show();
					var tabControl = companyForm.Controls.Find("CompanyTabControl", true)[0] as ZTemplateTabControl;
					var companyCredentialTabPages = tabControl.Controls.Find("EInvoiceCertificatesTabPage", true);
					AssertEquals(nameof(companyCredentialTabPages.Length), 1, companyCredentialTabPages.Length);

					var companyCredentialTabPage = companyCredentialTabPages[0] as ZTabPage;
					AssertEquals(nameof(companyCredentialTabPage.TabVisible), true, companyCredentialTabPage.TabVisible);

					var companyCredentialsControls = companyCredentialTabPage.Controls.Find("CompanyEInvoicingCredentialUserControl", true);
					AssertEquals(nameof(companyCredentialsControls.Length), 1, companyCredentialsControls.Length);

					var companyCredentialsControl = companyCredentialsControls[0];
					AssertType<GlbCompanyForm_CredentialUserControl>("Control should be for Mexico company credentials", companyCredentialsControl);
				}
			}
		}

		public void TestEInvoiceCredentialsTabPageVisibility()
		{
			var company = Factory.New<GlbCompany>();
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;

			var tabControlName = "CompanyTabControl";
			var tabPageName = "EInvoiceOAuthAuthorizationTabPage";

			var mockICountryComplianceFactory = new Mock<ICountryComplianceFactory>();
			var mockICountryComplianceInfoBase = new Mock<ICountryComplianceInfoBase>();

			mockICountryComplianceInfoBase.As<IEInvoiceCredentialsProvider>()
				.SetupSequence(x => x.ShouldShowCredentialsTab(It.IsAny<ICompany>()))
				.Returns(false)
				.Returns(true);

			mockICountryComplianceFactory.Setup(x => x.GetICountryComplianceInfoBase(It.IsAny<ZString>()))
				.Returns(mockICountryComplianceInfoBase.Object);

			using (ObjectFactory.Substitute(mockICountryComplianceFactory.Object))
			{
				using (var form = new GlbCompanyFormForTest(company))
				{
					form.Show();

					var companyTabControl = (ZTemplateTabControl)form.Controls.Find(tabControlName, searchAllChildren: true)[0];
					var tabPages = companyTabControl.Controls.Find(tabPageName, searchAllChildren: true);
					AssertEquals("Should not have the credential tab page", 0, tabPages.Length);
				}

				using (var form = new GlbCompanyFormForTest(company))
				{
					form.Show();

					var companyTabControl = (ZTemplateTabControl)form.Controls.Find(tabControlName, searchAllChildren: true)[0];
					var tabPages = companyTabControl.Controls.Find(tabPageName, searchAllChildren: true);
					AssertEquals("Should have credential tab page", 1, tabPages.Length);
					AssertEquals("The credential tab page should be visible", true, ((ZTabPage)tabPages[0]).TabVisible);

					var userControls = tabPages[0].Controls.Find("EInvoicingOAuthAuthorizationUserControl", searchAllChildren: true);
					AssertEquals("Should have the credential control", 1, userControls.Length);
					AssertType<GlbCompany_EInvoicingOAuthAuthorizationUserControl>(userControls[0]);
				}
			}
		}

		[RequiresSTA]
		public void TestAccountingCredentialsVisibility_Hungary()
		{
			var company = Factory.New<GlbCompany>();

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Australia;    // a country that does not have company credentials
			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();
				var companyTabControl = (ZTemplateTabControl)form.Controls.Find("CompanyTabControl", true).First();
				var eInvoiceCredentialsForHungaryTabPages = companyTabControl.Controls.Find("EInvoiceCredentialsForHungaryTabPage", true);
				AssertEquals(nameof(eInvoiceCredentialsForHungaryTabPages.Length), 0, eInvoiceCredentialsForHungaryTabPages.Length);
			}

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Hungary;
			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();
				var companyTabControl = (ZTemplateTabControl)form.Controls.Find("CompanyTabControl", true).First();
				var eInvoiceCredentialsForHungaryTabPages = companyTabControl.Controls.Find("EInvoiceCredentialsForHungaryTabPage", true);
				AssertEquals(nameof(eInvoiceCredentialsForHungaryTabPages.Length), 1, eInvoiceCredentialsForHungaryTabPages.Length);
				var eInvoiceCredentialsForHungaryTabPage = eInvoiceCredentialsForHungaryTabPages[0] as ZTabPage;
				AssertEquals(nameof(eInvoiceCredentialsForHungaryTabPage.TabVisible), true, eInvoiceCredentialsForHungaryTabPage.TabVisible);
				var eInvoiceCredentialsForHungaryUserControls = eInvoiceCredentialsForHungaryTabPage.Controls.Find("EInvoiceCredentialsForHungaryUserControl", true);
				AssertEquals(nameof(eInvoiceCredentialsForHungaryUserControls.Length), 1, eInvoiceCredentialsForHungaryUserControls.Length);
				var eInvoiceCredentialsForHungaryUserControl = eInvoiceCredentialsForHungaryUserControls[0];
				AssertType<GlbCompany_HungaryCredentialUserControl>("Control should be for Hungary company credentials", eInvoiceCredentialsForHungaryUserControl);
			}
		}

		[RequiresSTA]
		public void TestAccountingCredentialsVisibility_Philippines()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			AssertNotEquals("Pre-condition", Core.Constants.CountryCodes.Philippines, company.GC_RN_NKCountryCode);

			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();
				var companyTabControl = (ZTemplateTabControl)form.Controls.Find("CompanyTabControl", true).First();
				var eInvoiceCredentialsForPhilippinesTabPages = companyTabControl.Controls.Find("EInvoiceCredentialsForPhilippinesTabPage", true);
				AssertEquals(nameof(eInvoiceCredentialsForPhilippinesTabPages.Length), 0, eInvoiceCredentialsForPhilippinesTabPages.Length);
			}

			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Philippines;
			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();
				var companyTabControl = (ZTemplateTabControl)form.Controls.Find("CompanyTabControl", true).First();
				var eInvoiceCredentialsForPhilippinesTabPages = companyTabControl.Controls.Find("EInvoiceCredentialsForPhilippinesTabPage", true);
				AssertEquals(nameof(eInvoiceCredentialsForPhilippinesTabPages.Length), 1, eInvoiceCredentialsForPhilippinesTabPages.Length);
				var eInvoiceCredentialsForPhilippinesTabPage = eInvoiceCredentialsForPhilippinesTabPages[0] as ZTabPage;
				AssertEquals(nameof(eInvoiceCredentialsForPhilippinesTabPage.TabVisible), true, eInvoiceCredentialsForPhilippinesTabPage.TabVisible);
				var eInvoiceCredentialsForPhilippinesUserControls = eInvoiceCredentialsForPhilippinesTabPage.Controls.Find("EInvoiceCredentialsForPhilippinesUserControl", true);
				AssertEquals(nameof(eInvoiceCredentialsForPhilippinesUserControls.Length), 1, eInvoiceCredentialsForPhilippinesUserControls.Length);
				var eInvoiceCredentialsForPhilippinesUserControl = eInvoiceCredentialsForPhilippinesUserControls[0];
				AssertType<GlbCompany_PhilippinesCredentialUserControl>("Control should be for Philippines company credentials", eInvoiceCredentialsForPhilippinesUserControl);
			}
		}

		#region Deactivate Branch From Branch Grid At Company Form

		public void TestDeactivatingBranchFromBranchGrid()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_IsActive = true;

				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				branch1.GB_IsActive = true;
				company.Branches.Add(branch1);

				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				branch2.GB_IsActive = true;
				company.Branches.Add(branch2);
				company.GC_IsActive = true;

				var task1 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task1.S5_GB = branch1.PK;
				task1.S5_IsActive = true;

				var task2 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task2.S5_GB = branch2.PK;
				task2.S5_IsActive = true;

				Factory.Save();

				// Act
				ShowFormAndDeactivateBranch(company, 0);

				// Assert
				AssertEquals("We should show the switching form when the user deactivates a branch with active service tasks", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());

				// Act
				ShowFormAndDeactivateBranch(company, 1);

				// Assert
				AssertEquals("We should show the switching form when the user deactivates a branch with active service tasks", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
			}
		}

		public void TestDeactivatingBranchFromBranchGrid_WithStmServiceTasks()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("~1Z", "~2Z")))
			using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock("~1Z", "~2Z")))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_IsActive = true;

				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				branch1.GB_IsActive = true;
				company.Branches.Add(branch1);

				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				branch2.GB_IsActive = true;
				company.Branches.Add(branch2);
				company.GC_IsActive = true;

				var task1 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task1.S5_GB = branch1.PK;
				task1.S5_IsActive = true;

				var task2 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task2.S5_GB = branch2.PK;
				task2.S5_IsActive = true;

				_ = GetStmServiceTask("~1Z", branch1.PK);
				_ = GetStmServiceTask("~2Z", branch2.PK);

				Factory.Save();

				// Act
				ShowFormAndDeactivateBranch(company, 0);

				// Assert
				AssertEquals("We should show the switching form when the user deactivates a branch with active service tasks", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());

				// Act
				ShowFormAndDeactivateBranch(company, 1);

				// Assert
				AssertEquals("We should show the switching form when the user deactivates a branch with active service tasks", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
			}
		}

		[RequiresSTA]
		public void TestDeactivatingBranchFromBranchGrid_WithActiveStaff()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();

			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch1);
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch2);

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_GB_HomeBranch = branch1.PK;

			Factory.Save();

			ShowFormAndDeactivateBranch(company, 0);
			AssertEquals("We should show the switching form when the user deactivates a branch with active staff.", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		static void ShowFormAndDeactivateBranch(GlbCompany company, int rowNumberInGrid)
		{
			using (var companyForm = new GlbCompanyForm(company))
			{
				companyForm.Show();
				Application.DoEvents();
				var branchModuleButtonGrid = (ZModuleButtonGrid)companyForm.Controls.Find("GlbBranchModuleButtonGrid", true).Single();
				AssertEquals(2, branchModuleButtonGrid.InnerGrid.VisibleRowCount);

				branchModuleButtonGrid.InnerGrid.BeginEdit(branchModuleButtonGrid.InnerGrid.Columns[GlbBranchSchema.GB_IsActive.Name].ColumnStyle, rowNumberInGrid);
				var isAllowedColumn = branchModuleButtonGrid.InnerGrid.Columns[GlbBranchSchema.GB_IsActive.Name];
				if (isAllowedColumn != null)
				{
					((CheckBox)((ZCheckBoxColumnStyle)isAllowedColumn.ColumnStyle).EditControl).Checked = false;
				}
				branchModuleButtonGrid.InnerGrid.EndEdit(branchModuleButtonGrid.InnerGrid.Columns[GlbBranchSchema.GB_IsActive.Name].ColumnStyle, rowNumberInGrid, false);
			}
		}

		#endregion

		public void TestReloadNumberRangesTabPage()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.AccountingCountry = Core.Constants.CountryCodes.Italy;

			Factory.Save();

			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();
				form.NumberRangesTabPage.Select();
				form.NumberRangesTabPage.Show();

				Assert(form.NumberRangesTabPage.TabVisible);
				var itNumberRangesControl = form.customsNumberViewStmNumsTabPageUserControl.Controls.Find("NumberRangesControl", true).First();
				AssertNotNull("IT NumberRangesControl", itNumberRangesControl);

				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;

				var sgNumberRangesControl = form.customsNumberViewStmNumsTabPageUserControl.Controls.Find("NumberRangesControl", true).First();
				AssertNotNull("SG NumberRangesControl", sgNumberRangesControl);

				AssertNotEquals(sgNumberRangesControl.GetType().FullName, itNumberRangesControl.GetType().FullName);

				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.China;
				Assert(!form.NumberRangesTabPage.TabVisible);

				company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Singapore;
				Assert(form.NumberRangesTabPage.TabVisible);
			}
		}

		#region Deactivate Company

		[RequiresSTA]
		public void TestDeactivatingCompanyPrompts_WithNoServiceTaskAttached()
		{
			var company = CreateCompanyWithServiceTask(false);

			DeactivateOrReactiveCompanyThenShowForm(company, false);
			AssertEquals(typeof(DeactivateCompanyMessageBox), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		public void TestDeactivatingCompanyPrompts_WithInactiveServiceTaskAttached()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var company = CreateCompanyWithServiceTask(true, false);

				// Act
				DeactivateOrReactiveCompanyThenShowForm(company, false);

				// Assert
				AssertEquals(typeof(DeactivateCompanyMessageBox), ZFormModaliser.LastFormShownDialogForTest?.GetType());
			}
		}

		[RequiresSTA]
		public void TestDeactivatingCompanyPrompts_WithActiveServiceTaskAttached()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var company = CreateCompanyWithServiceTask(true, true);

				// Act
				DeactivateOrReactiveCompanyThenShowForm(company, false);

				// Assert
				AssertEquals("We should show the switching form when the user deactivates a branch with active service tasks", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
			}
		}

		[RequiresSTA]
		public void TestDeactivatingCompanyPrompts_WithInactiveStmServiceTaskAttached()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("~1Z", "~2Z")))
			using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock("~1Z", "~2Z")))
			{
				var company = CreateCompanyWithStmServiceTask(true, false);

				// Act
				DeactivateOrReactiveCompanyThenShowForm(company, false);

				// Assert
				AssertEquals(typeof(DeactivateCompanyMessageBox), ZFormModaliser.LastFormShownDialogForTest?.GetType());
			}
		}

		public void TestDeactivatingCompanyPrompts_WithActiveStmServiceTaskAttached()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("~1Z", "~2Z")))
			using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock("~1Z", "~2Z")))
			{
				var company = CreateCompanyWithStmServiceTask(true, true);

				// Act
				DeactivateOrReactiveCompanyThenShowForm(company, false);

				// Assert
				AssertEquals("We should show the switching form when the user deactivates a branch with active service tasks", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
			}
		}

		GlbCompany CreateCompanyWithServiceTask(bool createServiceTask, bool isActiveServiceTask = false)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;
			company.Branches.Add(branch);

			if (createServiceTask)
			{
				var task = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task.S5_GB = branch.PK;
				task.S5_IsActive = isActiveServiceTask;
			}

			Factory.Save();

			return company;
		}

		GlbCompany CreateCompanyWithStmServiceTask(bool createServiceTask, bool isActiveServiceTask = false)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_IsActive = true;
			company.Branches.Add(branch);

			if (createServiceTask)
			{
				var oldTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				oldTask.S5_GB = branch.PK;
				oldTask.S5_IsActive = isActiveServiceTask;

				var task = GetStmServiceTask("~1Z", branch.PK);
				task.SST_Active = isActiveServiceTask;
			}

			Factory.Save();

			return company;
		}

		public void TestDeactivatingCompanyPrompts_WithNoStaffAttached()
		{
			var company = CreateCompanyWithActiveStaff(false);

			DeactivateOrReactiveCompanyThenShowForm(company, false);
			AssertEquals(typeof(DeactivateCompanyMessageBox), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		public void TestDeactivatingCompanyPrompts_WithInactiveStaffAttached()
		{
			var company = CreateCompanyWithActiveStaff(true, false);

			DeactivateOrReactiveCompanyThenShowForm(company, false);
			AssertEquals(typeof(DeactivateCompanyMessageBox), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		public void TestDeactivatingCompanyPrompts_WithActiveStaffAttached()
		{
			var company = CreateCompanyWithActiveStaff(true, true);

			DeactivateOrReactiveCompanyThenShowForm(company, false);
			AssertEquals("We should show the switching form when the user deactivates a branch with active staff.", typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		GlbCompany CreateCompanyWithActiveStaff(bool createStaff, bool isActiveStaff = false)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			company.Branches.Add(branch);

			if (createStaff)
			{
				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_GB_HomeBranch = branch.PK;
				staff.GS_IsActive = isActiveStaff;
			}

			Factory.Save();

			return company;
		}

		[RequiresSTA]
		public void TestWarningPopupWhenDeactivatingCompany_WithNoBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				AssertNoWarningPopup(company);
			}
		}

		[RequiresSTA]
		public void TestWarningPopupWhenDeactivatingCompany_WithAnActiveBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_IsActive = true;
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				company.Branches.Add(branch1);

				AssertWarningPopup(company, DialogResult.No, false);
				AssertEquals($"Branch {branch1.GB_Code} should still be active", true, branch1.GB_IsActive);
				AssertWarningPopup(company, DialogResult.Yes, false);
			}
		}

		[RequiresSTA]
		public void TestWarningPopupWhenDeactivatingCompany_WithAnInactiveBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_IsActive = false;
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				company.Branches.Add(branch1);

				AssertNoWarningPopup(company);
				AssertEquals($"Branch {branch1.GB_Code} should still be de-activated", false, branch1.GB_IsActive);
			}
		}

		public void TestWarningPopupWhenDeactivatingCompany_WithAnActiveBranchAndServiceTask()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_IsActive = true;
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				branch1.GB_IsActive = true;

				Factory.Save();

				using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					company.Branches.Add(branch1);

					var task = Factory.NewWithValidTestData<ServiceTaskSchedule>();
					task.S5_GB = branch1.PK;
					task.S5_IsActive = true;

					// Assert
					AssertWarningPopup(company, DialogResult.No, true);
					AssertEquals($"Branch {branch1.GB_Code} should still be active", true, branch1.GB_IsActive);
					AssertWarningPopup(company, DialogResult.Yes, true);
				}
			}
		}

		public void TestWarningPopupWhenDeactivatingCompany_WithAnActiveBranchAndStmServiceTask()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("~1Z", "~2Z")))
			using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock("~1Z", "~2Z")))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_IsActive = true;
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				branch1.GB_IsActive = true;

				Factory.Save();

				using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					company.Branches.Add(branch1);

					var oldTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
					oldTask.S5_GB = branch1.PK;
					oldTask.S5_IsActive = true;

					_ = GetStmServiceTask("~1Z", branch1.PK);

					// Assert
					AssertWarningPopup(company, DialogResult.No, true);
					AssertEquals($"Branch {branch1.GB_Code} should still be active", true, branch1.GB_IsActive);
					AssertWarningPopup(company, DialogResult.Yes, true);
				}
			}
		}

		[RequiresSTA]
		public void TestWarningPopupWhenDeactivatingCompany_WithAnActiveBranchAndStaff_And_TrueBulkBranchStatusUpdateOnCompanyDeactivationAndActivationRegistry()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			{
				company.Branches.Add(branch1);

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_GB_HomeBranch = branch1.PK;

				AssertWarningPopup(company, DialogResult.No, true);
				Assert($"Branch {branch1.GB_Code} should still be active", branch1.GB_IsActive);
				AssertWarningPopup(company, DialogResult.Yes, true);
			}
		}

		[RequiresSTA]
		public void TestWarningPopupWhenDeactivatingCompany_WithAnActiveBranchAndStaff_And_FalseBulkBranchStatusUpdateOnCompanyDeactivationAndActivationRegistry()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();

			Factory.Save();

			using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				company.Branches.Add(branch1);

				var staff = Factory.NewWithValidTestData<GlbStaff>();
				staff.GS_GB_HomeBranch = branch1.PK;

				AssertWarningPopup(company, DialogResult.No, true);
				Assert($"Branch {branch1.GB_Code} should still be active", branch1.GB_IsActive);
				AssertWarningPopup(company, DialogResult.Yes, true);
			}
		}

		[RequiresSTA]
		public void TestWarningPopupWhenDeactivatingCompany_WithMultipleBranches_And_TrueBulkBranchStatusUpdateOnCompanyDeactivationAndActivationRegistry()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_IsActive = true;
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				branch1.GB_IsActive = true;
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				branch2.GB_IsActive = false;
				Factory.Save();

				using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					company.Branches.Add(branch1);
					company.Branches.Add(branch2);

					var task = Factory.NewWithValidTestData<ServiceTaskSchedule>();
					task.S5_GB = branch1.PK;
					task.S5_IsActive = true;

					// Assert
					AssertWarningPopup(company, DialogResult.No, true);
					AssertEquals($"Branch {branch1.GB_Code} should still be active", true, branch1.GB_IsActive);
					AssertEquals($"Branch {branch2.GB_Code} should still be de-activated", false, branch2.GB_IsActive);
					AssertWarningPopup(company, DialogResult.Yes, true);
				}
			}
		}

		[RequiresSTA]
		public void TestWarningPopupWhenDeactivatingCompany_WithMultipleBranches_And_TrueBulkBranchStatusUpdateOnCompanyDeactivationAndActivationRegistry_WithStmServiceTask()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("~1Z", "~2Z")))
			using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock("~1Z", "~2Z")))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();
				company.GC_IsActive = true;
				var branch1 = Factory.NewWithValidTestData<GlbBranch>();
				branch1.GB_IsActive = true;
				var branch2 = Factory.NewWithValidTestData<GlbBranch>();
				branch2.GB_IsActive = false;
				Factory.Save();

				using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
				{
					// Act
					company.Branches.Add(branch1);
					company.Branches.Add(branch2);

					var oldTask = Factory.NewWithValidTestData<ServiceTaskSchedule>();
					oldTask.S5_GB = branch1.PK;
					oldTask.S5_IsActive = true;

					_ = GetStmServiceTask("~1Z", branch1.PK);

					// Assert
					AssertWarningPopup(company, DialogResult.No, true);
					AssertEquals($"Branch {branch1.GB_Code} should still be active", true, branch1.GB_IsActive);
					AssertEquals($"Branch {branch2.GB_Code} should still be de-activated", false, branch2.GB_IsActive);
					AssertWarningPopup(company, DialogResult.Yes, true);
				}
			}
		}

		public void TestWarningPopupWhenDeactivatingCompany_WithMultipleBranches_And_FalseBulkBranchStatusUpdateOnCompanyDeactivationAndActivationRegistry()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_IsActive = true;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_IsActive = false;
			Factory.Save();

			using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				company.Branches.Add(branch1);
				company.Branches.Add(branch2);

				AssertNoWarningPopup(company);
			}
		}

		void AssertWarningPopup(GlbCompany company, DialogResult dialogResult, bool hasActiveTaskOrStaff)
		{
			using (var companyForm = new GlbCompanyForm(company))
			{
				companyForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(dialogResult);
				var isActiveCheckbox = (ZCheckBox)companyForm.Controls.Find("GC_IsActiveCheckBox", true).Single();
				isActiveCheckbox.Checked = false;

				if (dialogResult == DialogResult.Yes)
				{
					if (hasActiveTaskOrStaff)
					{
						AssertEquals(typeof(ChangingServiceTaskBranchForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
						AssertEquals($"Company {company.GC_Name} should be active", true, company.GC_IsActive);
					}
					else
					{
						AssertEquals(typeof(DeactivateCompanyMessageBox), ZFormModaliser.LastFormShownDialogForTest?.GetType());
						AssertEquals($"Company {company.GC_Name} should be de-activated", false, company.GC_IsActive);

						if (company.Branches?.Any() ?? false)
						{
							foreach (var branch in company.Branches)
							{
								AssertEquals($"Branch {branch.GB_Code} should be de-activated", false, branch.GB_IsActive);
							}
						}
					}
				}
				else
				{
					AssertEquals($"Company {company.GC_Name} should be active", true, company.GC_IsActive);
				}
			}
		}

		void AssertNoWarningPopup(GlbCompany company)
		{
			using (var companyForm = new GlbCompanyForm(company))
			{
				companyForm.Show();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				var isActiveCheckbox = (ZCheckBox)companyForm.Controls.Find("GC_IsActiveCheckBox", true).Single();
				isActiveCheckbox.Checked = false;
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestCompanyBranchSwitcherRefreshAfterServiceMoveToOtherBranch()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, false))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();

				var oldBranch1 = Factory.NewWithValidTestData<GlbBranch>();
				oldBranch1.GB_GC = company.PK;
				var task1 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task1.S5_GB = oldBranch1.PK;

				var oldBranch2 = Factory.NewWithValidTestData<GlbBranch>();
				oldBranch2.GB_GC = company.PK;
				var task2 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task2.S5_GB = oldBranch2.PK;

				var newBranch = Factory.NewWithValidTestData<GlbBranch>();
				Factory.Save();

				using (var companyForm = new GlbCompanyFormForTest(company))
				{
					// Act
					companyForm.SwitchCompanyBranchForTest();
					var lastShownForm = (ChangingServiceTaskBranchForm)ZFormModaliser.LastFormShownDialogForTest;

					// Assert
					AssertEquals(2, ((BranchSwitcherBusinessObject)lastShownForm.LastDataSourceForTest).ServiceTasks.Count);

					// Act
					ZFormModaliser.LastFormShownDialogForTest = null;
					task2.S5_GB = newBranch.PK;
					companyForm.SwitchCompanyBranchForTest();
					lastShownForm = (ChangingServiceTaskBranchForm)ZFormModaliser.LastFormShownDialogForTest;

					// Assert
					AssertEquals(1, ((BranchSwitcherBusinessObject)lastShownForm.LastDataSourceForTest).ServiceTasks.Count);

					// Act
					ZFormModaliser.LastFormShownDialogForTest = null;
					task1.S5_GB = newBranch.PK;
					companyForm.SwitchCompanyBranchForTest();

					// Assert
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		public void TestCompanyBranchSwitcherRefreshAfterStmServiceTaskMoveToOtherBranch()
		{
			// Arrange
			using (SystemDataRegistry.Instance.SwitchToNewServiceTasksModule.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, true))
			using (ObjectFactory.Substitute(GetServiceTaskStatusProviderMock("~1Z", "~2Z")))
			using (ObjectFactory.Substitute(GetClientHostedServiceAttributeProviderMock("~1Z", "~2Z")))
			{
				var company = Factory.NewWithValidTestData<GlbCompany>();

				var oldBranch1 = Factory.NewWithValidTestData<GlbBranch>();
				oldBranch1.GB_GC = company.PK;
				var task1 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task1.S5_GB = oldBranch1.PK;

				var oldBranch2 = Factory.NewWithValidTestData<GlbBranch>();
				oldBranch2.GB_GC = company.PK;
				var task2 = Factory.NewWithValidTestData<ServiceTaskSchedule>();
				task2.S5_GB = oldBranch2.PK;

				var newBranch = Factory.NewWithValidTestData<GlbBranch>();
				var stmTask1 = GetStmServiceTask("~1Z", oldBranch1.PK);
				var stmTask2 = GetStmServiceTask("~2Z", oldBranch2.PK);

				Factory.Save();

				using (var companyForm = new GlbCompanyFormForTest(company))
				{
					// Act
					companyForm.SwitchCompanyBranchForTest();

					var lastShownForm = (ChangingServiceTaskBranchForm)ZFormModaliser.LastFormShownDialogForTest;

					// Assert
					AssertEquals(2, ((BranchSwitcherBusinessObject)lastShownForm.LastDataSourceForTest).StmServiceTasks.Count);

					// Act
					ZFormModaliser.LastFormShownDialogForTest = null;
					stmTask2.SST_GB_Branch = newBranch.PK;
					companyForm.SwitchCompanyBranchForTest();
					lastShownForm = (ChangingServiceTaskBranchForm)ZFormModaliser.LastFormShownDialogForTest;

					// Assert
					AssertEquals(1, ((BranchSwitcherBusinessObject)lastShownForm.LastDataSourceForTest).StmServiceTasks.Count);

					// Act
					ZFormModaliser.LastFormShownDialogForTest = null;
					stmTask1.SST_GB_Branch = newBranch.PK;
					companyForm.SwitchCompanyBranchForTest();

					// Assert
					AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				}
			}
		}

		StmServiceTask GetStmServiceTask(string code, ZGuid branchPk)
		{
			var stmTask = Factory.NewWithValidTestData<StmServiceTask>();
			stmTask.SST_GB_Branch = branchPk; //oldBranch1.PK;
			stmTask.SST_Active = true;
			stmTask.SST_ServiceTaskCode = code;
			stmTask.SST_Configuration = "<ScheduleConfig><NextRunTimeCalculatorWorkingDays ScheduledRunTime =\"12:00:00\" /></ScheduleConfig>";
			_ = stmTask.StaticServiceAttributes.DefaultSchedule;

			return stmTask;
		}

		IServiceTaskScheduleStatusProvider GetServiceTaskStatusProviderMock(params string[] codes)
		{
			var webStatus = new Dictionary<string, TaskInstanceStatus>();

			foreach (var code in codes)
			{
				webStatus.Add(code, new TaskInstanceStatus { StatusString = "status", PlaceInQueueString = "place in queue", ProcessIDsString = "process id", RegisteredOnHosts = "registered on hosts", RunningCount = 2, SecondsInQueueString = "seconds in queue", SecondsRunningString = "seconds running" });
			}

			return Mock.Of<IServiceTaskScheduleStatusProvider>(o => o.GetServiceStatus() == webStatus);
		}

		IClientHostedServiceAttributeProvider GetClientHostedServiceAttributeProviderMock(params string[] codes)
		{
			var hostedServiceConfigMocks = codes.Select(GetHostedServiceAttributeMock).ToArray();
			var hostedServiceProviderMock = new Mock<IClientHostedServiceAttributeProvider>();

			foreach (var mock in hostedServiceConfigMocks)
			{
				hostedServiceProviderMock
					.Setup(p => p.GetClientHostedServiceAttribute(mock.Code))
					.Returns(mock);
			}

			hostedServiceProviderMock
				.Setup(p => p.GetClientHostedServiceAttributes())
				.Returns(hostedServiceConfigMocks);

			return hostedServiceProviderMock.Object;

			IHostedServiceAttribute GetHostedServiceAttributeMock(string code)
			{
				return Mock.Of<IHostedServiceAttribute>(o =>
					o.Code == code &&
					o.Description == "Dummy Description" &&
					o.Category == "TST" &&
					o.MutuallyExclusiveTaskGroup == MutuallyExclusiveServiceTaskGroups.NoGroup &&
					o.DefaultSchedule == Mock.Of<IDefaultSchedule>(d => d.RunEvery == "15minutes"));
			}
		}

		public void TestCompanyBranchSwitcherRefresh_AfterStaffMoveToOtherBranch()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var newBranch = Factory.NewWithValidTestData<GlbBranch>();

			var oldBranch1 = Factory.NewWithValidTestData<GlbBranch>();
			oldBranch1.GB_GC = company.PK;
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_GB_HomeBranch = oldBranch1.PK;

			var oldBranch2 = Factory.NewWithValidTestData<GlbBranch>();
			oldBranch2.GB_GC = company.PK;
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_GB_HomeBranch = oldBranch2.PK;

			Factory.Save();

			using (var companyForm = new GlbCompanyFormForTest(company))
			{
				companyForm.SwitchCompanyBranchForTest();
				var lastShownForm = (ChangingServiceTaskBranchForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(2, ((BranchSwitcherBusinessObject)lastShownForm.LastDataSourceForTest).Staff.Count);

				ZFormModaliser.LastFormShownDialogForTest = null;
				staff2.GS_GB_HomeBranch = newBranch.PK;
				companyForm.SwitchCompanyBranchForTest();
				lastShownForm = (ChangingServiceTaskBranchForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals(1, ((BranchSwitcherBusinessObject)lastShownForm.LastDataSourceForTest).Staff.Count);

				ZFormModaliser.LastFormShownDialogForTest = null;
				staff1.GS_GB_HomeBranch = newBranch.PK;
				companyForm.SwitchCompanyBranchForTest();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		#endregion

		#region Reactivate Company

		[RequiresSTA]
		public void TestReactivatingCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = false;
			var branch = Factory.NewWithValidTestData<GlbBranch>();
			Factory.Save();

			DeactivateOrReactiveCompanyThenShowForm(company, true);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest?.GetType());

			company.GC_IsActive = false;
			branch.GB_IsActive = true;
			company.Branches.Add(branch);
			DeactivateOrReactiveCompanyThenShowForm(company, true);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest?.GetType());

			company.GC_IsActive = false;
			branch.GB_IsActive = false;
			company.Branches.Add(branch);
			DeactivateOrReactiveCompanyThenShowForm(company, true, false);
			AssertNull(ZFormModaliser.LastFormShownDialogForTest?.GetType());

			company.GC_IsActive = false;
			DeactivateOrReactiveCompanyThenShowForm(company, true);
			AssertEquals(typeof(ReactivateBranchesOrAddressesForm), ZFormModaliser.LastFormShownDialogForTest?.GetType());
		}

		#endregion

		static void DeactivateOrReactiveCompanyThenShowForm(GlbCompany company, bool activeState, bool registryItemState = true)
		{
			using (OrganisationsDataRegistry.Instance.BulkBranchStatusUpdateOnCompanyDeactivationAndActivation.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, registryItemState))
			using (var companyForm = new GlbCompanyForm(company))
			{
				companyForm.Show();
				var isActiveCheckbox = (ZCheckBox)companyForm.Controls.Find("GC_IsActiveCheckBox", true).Single();
				isActiveCheckbox.Checked = activeState;
			}
		}

		public void TestNoErrorOccurWhenDeleteCompany()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_IsActive = true;
			Factory.Save();
			ErrorReporter.Clear();

			using (var companyForm = new GlbCompanyForm(company))
			{
				companyForm.Show();
				company.Delete();
			}

			AssertEquals(0, ExceptionReporterTestListener.Instance.Count);
		}

		#region FixedPlaceOfSupplyConfigurationTab

		public void TestFixedPlaceOfSupplyConfigurationTab_IsNotVisible_WhenAllRegistryValuesAreDisabled()
		{
			var company = Factory.New<GlbCompany>();
			var allDisabled = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			foreach (CodeDescriptionBool item in allDisabled)
			{
				item.Bool = false;
			}

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, allDisabled))
			{
				using (var form = new GlbCompanyFormForTest(company))
				{
					form.Show();
					var companyTabControl = (ZTemplateTabControl)form.Controls.Find("CompanyTabControl", true).First();
					var configurationTabPages = companyTabControl.Controls.Find("PlaceOfSupplyConfigurationTabPage", true);
					AssertEquals(nameof(configurationTabPages.Length), 0, configurationTabPages.Length);
				}
			}
		}

		public void TestFixedPlaceOfSupplyConfigurationTab_IsVisible_WhenAtLeastOneRegistryValueIsActive()
		{
			var company = Factory.New<GlbCompany>();
			var someEnabled = new CodeDescriptionBoolCollection(AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.DefaultValue);
			foreach (CodeDescriptionBool item in someEnabled)
			{
				item.Bool = false;
			}
			someEnabled[0].Bool = true;

			using (AccountingMasterFilesRegistry.Instance.FixedPlaceOfSupplyConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, someEnabled))
			{
				using (var form = new GlbCompanyFormForTest(company))
				{
					form.Show();
					var companyTabControl = (ZTemplateTabControl)form.Controls.Find("CompanyTabControl", true).First();
					var configurationTabPages = companyTabControl.Controls.Find("PlaceOfSupplyConfigurationTabPage", true);
					AssertEquals(nameof(configurationTabPages.Length), 1, configurationTabPages.Length);
					var configurationTabPage = configurationTabPages[0] as ZTabPage;
					AssertEquals(nameof(configurationTabPage.TabVisible), true, configurationTabPage.TabVisible);
					var configurationUserControls = configurationTabPage.Controls.Find("PlaceOfSupplyConfiguration", true);
					AssertEquals(nameof(configurationUserControls.Length), 1, configurationUserControls.Length);
					var configurationUserControl = configurationUserControls[0];
					AssertType<AccPlaceOfSupplyConfigurationControl>("Control should be for Place of Supply Configuration", configurationUserControl);
				}
			}
		}

		#endregion

		[RequiresSTA]
		public void TestSurchargeTabPageExists()
		{
			GlbCompany company = Factory.New<GlbCompany>();

			var accSurchargeConfiguration = "accSurchargeConfiguration";
			var accSurchargeBasis = "accSurchargeBasis";

			using (var form = new GlbCompanyForm(company))
			{
				form.Show();
				Application.DoEvents();

				AssertNotNull("Has accSurchargeConfiguration", form.Controls.Find(accSurchargeConfiguration, true).First());
				AssertNotNull("Has accSurchargeBasis", form.Controls.Find(accSurchargeBasis, true).First());
			}
		}

		public void TestValidateAddressButton()
		{
			Env.Instance.Registry.EnableAddressValidationWebService = true;
			var company = Factory.New<GlbCompanyForTest>();
			company.GC_Address1 = "xxxx";
			company.GC_RN_NKCountryCode = "AU";
			company.GC_State = "AST";
			company.GC_City = "Sydney";
			using (var form = new GlbCompanyForm(company))
			{
				form.Show();

				form.ValidateButton.PerformClick();
				var suggestionControl = form.FindSingleOrDefault<AddressSuggestionControl>("AddressSuggestionControl");
				AssertNotNull("AddressSuggestionControl", suggestionControl);
				var infoLabel = suggestionControl.FindSingleOrDefault<ZLabel>("InfoLabel");
				AssertNotNull("InfoLabel", infoLabel);
				AssertEquals("No suggestions have been found for the address that you entered. Please confirm as original or amend the address to receive suggestions.", infoLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestNoCreatedChangesNotificationExceptionThrown_WhenOnLoad()
		{
			using (DataRegistry.Instance.RawRegistry.EnableAddressValidationWebService.SetTemporaryValue(EnvProxy.Instance.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var company = Factory.New<GlbCompanyForTest>();
				company.GC_Address1 = "xxxx";
				company.GC_RN_NKCountryCode = "AU";
				company.GC_State = "AST";
				company.GC_City = "Sydney";
				company.ValidationStatus = AddressValidationStatus.ToBeVerified;
				Factory.Save();

				AssertNoExceptionThrown(() =>
				{
					using (TestingState.SuspendIsRunningTests())
					using (var form = new GlbCompanyFormForTest(company))
					{
						form.Show();
						Assert("Change should not happen on the company when load", !company.HasChanges);
						AssertNotContains("Should NOT contain DeveloperNotificationException", "Created Changes Before Type (HasChanges: True, HasChangesNotIncludingChildren: False)", ErrorReporter.LastMessageReported);
						AssertEquals("Validation status has been changed to INV.", AddressValidationStatus.Invalid, company.ValidationStatus);
					}
				});
			}
		}

		[RequiresSTA]
		public void TestIfAccountInformationIsEnable()
		{
			GlbCompany company = Factory.New<GlbCompany>();
			using (var form = new GlbCompanyFormForTest(company))
			{
				var overrideCheckBox = form.Controls.Find("OverrideCheckbox", true).First() as CheckBox;

				var accountFeeRuleDropEdit = form.Controls.Find("AccountFeeRuleDropEdit", true)[0] as ZDropEdit;
				var accountBoundFindBox = form.Controls.Find("GLAccountBoundFindBox", true)[0] as ZGuidFindBox;
				var amountCalcFindBox = form.Controls.Find("AmountCalcFindBox", true)[0] as ZCalcFindBox;

				AssertEquals(overrideCheckBox.Checked, accountFeeRuleDropEdit.Enabled);
				AssertEquals(overrideCheckBox.Checked, accountBoundFindBox.Enabled);
				AssertEquals(overrideCheckBox.Checked, amountCalcFindBox.Enabled);

				overrideCheckBox.Checked = true;

				AssertEquals(overrideCheckBox.Checked, accountFeeRuleDropEdit.Enabled);
				AssertEquals(overrideCheckBox.Checked, accountBoundFindBox.Enabled);
				AssertEquals(overrideCheckBox.Checked, amountCalcFindBox.Enabled);
			}
		}

		#region Implementation

		protected override Form GetFormToBashCore()
		{
			var demoCompany = Factory.NewWithValidTestData<GlbCompany>();
			Factory.Save();

			var testForm = new GlbCompanyForm(demoCompany);
			return testForm;
		}

		GlbCompany CreateNewCompanyWithBranches()
		{
			var company = Factory.New<GlbCompany>();

			var branch = Factory.New<GlbBranch>();
			var branch1 = Factory.New<GlbBranch>();
			var branch2 = Factory.New<GlbBranch>();
			var branch3 = Factory.New<GlbBranch>();

			company.Branches.Add(branch);
			company.Branches.Add(branch1);
			company.Branches.Add(branch2);
			company.Branches.Add(branch3);

			return company;
		}

		void AssertAllEquals(bool expectedValue, List<bool> actualValues)
		{
			foreach (bool actualValue in actualValues)
			{
				AssertEquals(expectedValue, actualValue);
			}
		}

		void AssertCompanyCredentialsPluginVisibility(ZString countryCode, ZBool isVisible)
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_RN_NKCountryCode = countryCode;

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(countryCode))
			using (var form = new GlbCompanyFormForTest(company))
			{
				form.Show();
				var companyTabControl = form.FindSingle<ZTemplateTabControl>("CompanyTabControl");
				var companyCredentialsPlugIn = companyTabControl.PlugIns.GetPlugIn(ControllerIDs.CompanyCredentialsPlugIn);

				CombineAssertions(() =>
				{
					AssertNotNull("Found Plugin", companyCredentialsPlugIn);
					AssertEquals("Visible", isVisible, companyCredentialsPlugIn.Enabled);
				});
			}
		}

		class GlbCompanyForTest : GlbCompany, ISupportWebAddressValidation
		{
			public GlbCompanyForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
			{
			}

			public new Task<WebAddressValidationResult> ValidateAddressAsync(CancellationTokenSource cancellationToken, CleanseAction cleanseAction = CleanseAction.ValidateAndSuggest)
			{
				ValidationStatus = AddressValidationStatus.Invalid;
				return Task.FromResult(new WebAddressValidationResult());
			}
		}

		class GlbCompanyFormForTest : GlbCompanyForm
		{
			public GlbCompanyFormForTest(GlbCompany company)
				: base(company)
			{
			}

			public void FormSaveInternal()
			{
				base.SaveInternal();
			}

			public void FormDelete()
			{
				base.Delete();
			}

			public void SwitchCompanyBranchForTest()
			{
				SwitchCompanyBranch();
			}

			public ZCodeFindBox CountryCodeFindBox
			{
				get { return base.GC_RN_NKCountryCodeCodeFindBox; }
			}

			public ZCodeFindBox CurrencyCodeFindBox
			{
				get { return base.GC_RX_NKLocalCurrencyCodeFindBox; }
			}
		}

		#endregion
	}
}
