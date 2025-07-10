using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.Tracking.Business;
using Enterprise.Tracking.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Enterprise.ZArchitecture.Web.Business;
using Enterprise.ZArchitecture.Web.Business.Testing;
using Enterprise.ZArchitecture.Web.GUI;
using Enterprise.ZArchitecture.Web.GUI.Testing;
using Enterprise.ZArchitecture.Web.GUI.WebControls;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.Tracking.Web.Testing
{
	[TestedType(typeof(ARAPInvoicingColumnProvider))]
	[HttpContextEnabledTest]
	class ARAPInvoicingColumnProviderTest : GridColumnProviderTest
	{
		public void TestComplianceNumberColumn()
		{
			var company = Factory.NewWithValidTestData<GlbCompany>();
			var companyData = Factory.NewWithValidTestData<OrgCompanyData>();
			companyData.OB_GC = company.PK;
			companyData.OB_OH = LoggedSiteUser.LoggedInOrganisation.PK;
			companyData.OB_IsDebtor = true;
			company.GC_RN_NKCountryCode = Core.Constants.CountryCodes.Italy;
			var anotherCompany = Factory.NewWithValidTestData<GlbCompany>();
			var anotherCompanyData = Factory.NewWithValidTestData<OrgCompanyData>();
			anotherCompanyData.OB_GC = anotherCompany.PK;
			anotherCompanyData.OB_OH = LoggedSiteUser.LoggedInOrganisation.PK;
			anotherCompanyData.OB_IsDebtor = true;
			anotherCompany.GC_RN_NKCountryCode = Core.Constants.CountryCodes.UnitedStates;
			Factory.Save();

			var collection = new ComplianceSubTypeAttributionRuleConfigurationCollection();
			var configuration = collection.AddNew();
			configuration.Country = Core.Constants.CountryCodes.Italy;
			configuration.SubType = ItalyComplianceInfo.ComplianceSubTypeCodes.ARS;
			configuration.LedgerType = LedgerTypes.AccountsReceivable;
			configuration.InvoiceType = TransactionTypes.Invoice;
			configuration.TaxInvoiceRule = TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID;
			configuration.DisbursementRule = DisbursementRuleCodes.AllTransactions;
			configuration.OriginalRule = OriginalRuleCodes.AllTransactions;
			configuration.TaxRegistrationType = ZString.Empty;
			configuration.OrganisationLocation = ZString.Empty;
			configuration.SelfBillingRule = SelfBillingRuleCodes.SelfBillingTransactions;
			configuration.VATGroupRule = VatGroupListCodes.ExcludeVATGroupMembers;

			using (AccountingMasterFilesRegistry.Instance.ComplianceSubTypeAttributionRuleConfiguration.SetTemporaryValue(company.PK.ToGuid(), Guid.Empty, Guid.Empty, collection))
			{
				var companies = LoggedSiteUser.GetTransactionCompanies(Factory);
				AssertEquals("Precondition", 2, companies.Count);

				var provider = new ARAPInvoicingColumnProvider(LoggedSiteUser);
				provider.CustomizeDictionary();

				Assert("Contains Compliance Number column when related company's country has Compliance Sequence Module enabled", provider.DefaultColumns.Contains((int)WebTracker.Grids.ARAPInvoicing.ComplianceNumber));
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0002:Simplify Member Access", Justification = "Simplified access could change context here")]
		protected override void SetupColumnsCore()
		{
			base.SetupColumnsCore();
			ZLinkButtonColumn invoiceNumColumn = new ZLinkButtonColumn("Invoice #", "InvoiceNumber") { ColumnKey = WebTracker.Grids.ARAPInvoicing.InvoiceNumber };
			invoiceNumColumn.Command = "ViewInvoice";
			if (LoggedSiteUser != null && LoggedSiteUser.IsShipmentQuickViewUser)
			{
				invoiceNumColumn.ClientClickHandler = BasePage.ShipmentQuickViewUserLoginRequest;
			}
			AddDefaultsColumn(invoiceNumColumn);
			AddDefaultsColumn(new ZFindBoxColumn("Issuer", InvoicingBase.Schema.AH_GC, "Lookups.Companies", typeof(InvoicingBase))
			{
				ColumnKey = WebTracker.Grids.ARAPInvoicing.Issuer,
				DisplayStyle = OComboBoxDropDownStyle.DescriptionOnly
			});

			AddDefaultsColumn(new ZTextEditColumn("Type", InvoicingBase.Schema.AH_TransactionType) { ColumnKey = WebTracker.Grids.ARAPInvoicing.Type });
			AddDefaultsColumn(new ZTextEditColumn("Terms", InvoicingBase.Schema.AH_InvoiceTerm) { ColumnKey = WebTracker.Grids.ARAPInvoicing.Terms });
			AddDefaultsColumn(new ZDateTimeColumn("Inv. Date", InvoicingBase.Schema.AH_InvoiceDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.ARAPInvoicing.InvoiceDate });
			AddDefaultsColumn(new ZDateTimeColumn("Due Date", InvoicingBase.Schema.AH_DueDate, ZDateTimePickerFormat.Short) { ColumnKey = WebTracker.Grids.ARAPInvoicing.DueDate });
			AddDefaultsColumn(new ZCodeFindBoxColumn("Currency", InvoicingBase.Schema.AH_RX_NKTransactionCurrency, "Lookups.TransactionCurrencies", typeof(InvoicingBase)) { ColumnKey = WebTracker.Grids.ARAPInvoicing.Currency });
			AddDefaultsColumn(new ZCalcEditColumn("Amount", InvoicingBase.Schema.AH_OSTotalAmount, InvoicingBase.Schema.AH_Calc_RXDecimals, AccTransactionHeaderSchema.AH_OSTotal.Name) { ColumnKey = WebTracker.Grids.ARAPInvoicing.Amount });
			AddDefaultsColumn(new ZCalcEditColumn("Outstanding Amt.", InvoicingBase.Schema.AH_Calc_OSOutstandingAmount, InvoicingBase.Schema.AH_Calc_RXDecimals, AccTransactionHeaderSchema.AH_OutstandingAmount.Name) { ColumnKey = WebTracker.Grids.ARAPInvoicing.OutstandingAmount });
			AddDefaultsColumn(new ZDateTimeColumn("Paid Date", InvoicingBase.Schema.AH_FullyPaidDate, ZDateTimePickerFormat.Long) { ColumnKey = WebTracker.Grids.ARAPInvoicing.PaidDate });
		}

		protected override bool SupportsOldLayoutFix
		{
			get { return false; }
		}

		protected TrackingSiteUser LoggedSiteUserForTest => LoggedSiteUser;

		TrackingSiteUser LoggedSiteUser
		{
			get
			{
				return WebEnv.AppInstance.SiteUser as TrackingSiteUser;
			}
		}

		protected override GridColumnProvider GetNewTestProvider()
		{
			return new ARAPInvoicingColumnProvider(LoggedSiteUser);
		}

		protected override void SetUp()
		{
			var testHelper = new TestHelper(Factory);
			testHelper.TestSiteUser.Login(testHelper.TestOrg.OH_Code, testHelper.TestContact.OC_Email, testHelper.TestContact.PasswordForTesting);
			base.SetUp();
		}
	}
}
