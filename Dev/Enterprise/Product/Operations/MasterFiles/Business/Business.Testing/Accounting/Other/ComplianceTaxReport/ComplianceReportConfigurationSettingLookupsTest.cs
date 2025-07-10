using System.Linq;
using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using static Enterprise.Registry.Business.ComplianceReportConfigurationSettingLookups;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using LookupsClass = Enterprise.Registry.Business.ComplianceReportConfigurationSettingLookups;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceReportConfigurationSettingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLedgerTypeList()
		{
			ReportConfiguration.ReportBaseTablePrefix = "AH";
			AssertEquals("LedgerTypeList.Count", 3, Lookups.LedgerTypeList.Count);
			Assert("should contain AccountsReceivable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsReceivable));
			Assert("should contain AccountsPayable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsPayable));
			Assert("should contain CashBook", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.CashBook));

			ReportConfiguration.ReportBaseTablePrefix = "AL";
			AssertEquals("LedgerTypeList.Count", 3, Lookups.LedgerTypeList.Count);
			Assert("should contain AccountsReceivable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsReceivable));
			Assert("should contain AccountsPayable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsPayable));
			Assert("should contain AccountsPayable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.CashBook));

			ReportConfiguration.ReportBaseTablePrefix = "**";
			AssertEquals("LedgerTypeList.Count", 6, Lookups.LedgerTypeList.Count);
			Assert("should contain AccountsReceivable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsReceivable));
			Assert("should contain AccountsPayable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsPayable));
			Assert("should contain CashBook", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.CashBook));
			Assert("should contain GeneralLedger", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.General));
			Assert("should contain JobCosting", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.JobCosting));
			Assert("should contain CashBasisTax", Lookups.LedgerTypeList.ContainsCode(PseudoLedgerCodes.CashBasisTax));

			ReportConfiguration.ReportBaseTablePrefix = "GLD";
			AssertEquals("LedgerTypeList.Count", 6, Lookups.LedgerTypeList.Count);
			Assert("should contain AccountsReceivable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsReceivable));
			Assert("should contain AccountsPayable", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.AccountsPayable));
			Assert("should contain CashBook", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.CashBook));
			Assert("should contain GeneralLedger", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.General));
			Assert("should contain JobCosting", Lookups.LedgerTypeList.ContainsCode(LedgerTypes.JobCosting));
			Assert("should contain CashBasisTax", Lookups.LedgerTypeList.ContainsCode(PseudoLedgerCodes.CashBasisTax));
		}

		public void TestInvoicetypeListForAHBaseTable()
		{
			AssertInvoicetypeListForAHALBaseTable("AH");
		}

		public void TestInvoicetypeListForALBaseTable()
		{
			AssertInvoicetypeListForAHALBaseTable("AL");
		}

		void AssertInvoicetypeListForAHALBaseTable(string tablePrefix)
		{
			ReportConfiguration.ReportBaseTablePrefix = tablePrefix;
			AssertEquals("LedgerTypeList.Count", 3, Lookups.LedgerTypeList.Count);

			AssertInvoiceTypeListForARAPLedgers(LedgerTypes.AccountsReceivable);

			AssertInvoiceTypeListForARAPLedgers(LedgerTypes.AccountsPayable);

			AssertInvoiceTypeListForCashBook();
		}

		public void TestInvoiceTypeListForAllTransactions()
		{
			ReportConfiguration.ReportBaseTablePrefix = "**";
			AssertEquals("LedgerTypeList.Count", 6, Lookups.LedgerTypeList.Count);

			AssertInvoiceTypeListForARAPLedgers(LedgerTypes.AccountsReceivable);

			AssertInvoiceTypeListForARAPLedgers(LedgerTypes.AccountsPayable);

			Setting.LedgerType = LedgerTypes.CashBook;
			AssertNotNull("InvoiceTypeList", Lookups.InvoiceTypeList);
			AssertEquals("InvoiceTypeList.Count", 4, Lookups.InvoiceTypeList.Count);
			Assert("The InvoiceTypeList should contain 'EXX'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.ExchangeDifference));
			Assert("The InvoiceTypeList should contain 'TRF'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Transfer));
			Assert("The InvoiceTypeList should contain 'DPY'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.DirectPayment));
			Assert("The InvoiceTypeList should contain 'DRC'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.DirectReceipt));

			Setting.LedgerType = LedgerTypes.General;
			AssertNotNull("InvoiceTypeList", Lookups.InvoiceTypeList);
			AssertEquals("InvoiceTypeList.Count", 3, Lookups.InvoiceTypeList.Count);
			Assert("The InvoiceTypeList should contain 'AJL'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.GLAutoJournal));
			Assert("The InvoiceTypeList should contain 'RJL'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.GLReversingJournal));
			Assert("The InvoiceTypeList should contain 'GJL'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.GLStandardJournal));

			Setting.LedgerType = LedgerTypes.JobCosting;
			AssertNotNull("InvoiceTypeList", Lookups.InvoiceTypeList);
			AssertEquals("InvoiceTypeList.Count", 7, Lookups.InvoiceTypeList.Count);
			Assert("The InvoiceTypeList should contain 'JRJ'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.JobRevenueJournal));
			Assert("The InvoiceTypeList should contain 'JNL'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Journal));
			Assert("The InvoiceTypeList should contain 'ACR'", Lookups.InvoiceTypeList.ContainsCode(TransactionLineTypes.Accrual));
			Assert("The InvoiceTypeList should contain 'REV'", Lookups.InvoiceTypeList.ContainsCode(TransactionLineTypes.Revenue));
			Assert("The InvoiceTypeList should contain 'WIP'", Lookups.InvoiceTypeList.ContainsCode(TransactionLineTypes.WIP));
			Assert("The InvoiceTypeList should contain 'RAC'", Lookups.InvoiceTypeList.ContainsCode(LookupsClass.PseudoTransactionTypeCodes.ReversedAccrual));
			Assert("The InvoiceTypeList should contain 'RWI'", Lookups.InvoiceTypeList.ContainsCode(LookupsClass.PseudoTransactionTypeCodes.ReversedWIP));

			Setting.LedgerType = PseudoLedgerCodes.CashBasisTax;
			AssertNotNull("InvoiceTypeList", Lookups.InvoiceTypeList);
			AssertEquals("InvoiceTypeList.Count", 1, Lookups.InvoiceTypeList.Count);
			Assert("The InvoiceTypeList should contain 'CBT'", Lookups.InvoiceTypeList.ContainsCode(PseudoTransactionTypeCodes.CashBasisTax));
		}

		void AssertInvoiceTypeListForARAPLedgers(string ledger)
		{
			Setting.LedgerType = ledger;
			AssertNotNull("InvoiceTypeList", Lookups.InvoiceTypeList);
			AssertEquals("InvoiceTypeList.Count", 13, Lookups.InvoiceTypeList.Count);
			Assert("The InvoiceTypeList should contain 'CTR'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Contra));
			Assert("The InvoiceTypeList should contain 'CRD'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.CreditNote));
			Assert("The InvoiceTypeList should contain 'ADJ'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.AdjustmentNote));
			Assert("The InvoiceTypeList should contain 'DSC'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Discount));
			Assert("The InvoiceTypeList should contain 'EXX'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.ExchangeDifference));
			Assert("The InvoiceTypeList should contain 'INV'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Invoice));
			Assert("The InvoiceTypeList should contain 'JNL'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Journal));
			Assert("The InvoiceTypeList should contain 'OVP'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Overpayment));
			Assert("The InvoiceTypeList should contain 'PAY'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Payment));
			Assert("The InvoiceTypeList should contain 'REC'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Receipt));
			Assert("The InvoiceTypeList should contain 'TRF'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Transfer));
			Assert("The InvoiceTypeList should contain 'CST'", Lookups.InvoiceTypeList.ContainsCode(TransactionLineTypes.Cost));
			Assert("The InvoiceTypeList should contain 'REV'", Lookups.InvoiceTypeList.ContainsCode(TransactionLineTypes.Revenue));
		}

		void AssertInvoiceTypeListForCashBook()
		{
			Setting.LedgerType = LedgerTypes.CashBook;
			AssertNotNull("InvoiceTypeList", Lookups.InvoiceTypeList);
			AssertEquals("InvoiceTypeList.Count", 4, Lookups.InvoiceTypeList.Count);
			Assert("The InvoiceTypeList should contain 'EXX'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.ExchangeDifference));
			Assert("The InvoiceTypeList should contain 'TRF'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.Transfer));
			Assert("The InvoiceTypeList should contain 'DPY'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.DirectPayment));
			Assert("The InvoiceTypeList should contain 'DRC'", Lookups.InvoiceTypeList.ContainsCode(TransactionTypes.DirectReceipt));
		}

		public void TestTaxInvoiceRuleList()
		{
			AssertNotNull("TaxInvoiceRuleList", Lookups.TaxInvoiceRuleList);
			AssertEquals("TaxInvoiceRuleList.Count", 17, Lookups.TaxInvoiceRuleList.Count);
			Assert("The TaxInvoiceRuleList should contain 'ALL'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.All));
			Assert("The TaxInvoiceRuleList should contain 'AMT'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAnAmountOfTax));
			Assert("The TaxInvoiceRuleList should contain 'TID'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID));
			Assert("The TaxInvoiceRuleList should contain 'TXN'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT));
			Assert("The TaxInvoiceRuleList should contain 'TNE'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL));
			Assert("The TaxInvoiceRuleList should contain 'TXX'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax));
			Assert("The TaxInvoiceRuleList should contain 'TXA'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax));
			Assert("The TaxInvoiceRuleList should contain 'TXR'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs));
			Assert("The TaxInvoiceRuleList should contain 'RVS'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly));
			Assert("The TaxInvoiceRuleList should contain 'EXL'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly));
			Assert("The TaxInvoiceRuleList should contain 'NON'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsNoTaxIDs));
			Assert("The TaxInvoiceRuleList should contain 'ATZ'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount));
			Assert("The TaxInvoiceRuleList should contain 'SUS'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID));
			Assert("The TaxInvoiceRuleList should contain 'TXS'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended));
			Assert("The TaxInvoiceRuleList should contain 'STI'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.SpecificTaxIDs));
			Assert("The TaxInvoiceRuleList should contain 'EXT'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.AllWithExemptTaxIDs));
			Assert("The TaxInvoiceRuleList should contain 'NOT'", Lookups.TaxInvoiceRuleList.ContainsCode(TaxInvoiceRuleCodes.AllWithNoReportTaxIDs));
		}

		public void TestOriginalRuleList()
		{
			AssertNotNull("OriginalRuleList", Lookups.OriginalRuleList);
			AssertEquals("OriginalRuleList.Count", 5, Lookups.OriginalRuleList.Count);
			Assert("The OriginalRuleList should contain 'ALL'", Lookups.OriginalRuleList.ContainsCode(OriginalRuleCodes.AllTransactions));
			Assert("The OriginalRuleList should contain 'ATO'", Lookups.OriginalRuleList.ContainsCode(OriginalRuleCodes.AmendingTransactionOnly));
			Assert("The OriginalRuleList should contain 'RTO'", Lookups.OriginalRuleList.ContainsCode(OriginalRuleCodes.ReversalTransactionOnly));
			Assert("The OriginalRuleList should contain 'ARO'", Lookups.OriginalRuleList.ContainsCode(OriginalRuleCodes.AmendingReversalOnly));
			Assert("The OriginalRuleList should contain 'OTO'", Lookups.OriginalRuleList.ContainsCode(OriginalRuleCodes.OriginalTransactionOnly));
		}

		public void TestTaxRegistrationTypeList()
		{
			Setting.Country = Core.Constants.CountryCodes.Argentina;
			AssertNotNull("TaxRegistrationTypeList", Lookups.TaxRegistrationTypeList);
			AssertEquals("TaxRegistrationTypeList.Count", 2, Lookups.TaxRegistrationTypeList.Count);
			Assert("The TaxRegistrationTypeList should contain 'REC'", Lookups.TaxRegistrationTypeList.ContainsCode(TaxRegistrationTypeCodes.Recoverable));
			Assert("The TaxRegistrationTypeList should contain 'NOT'", Lookups.TaxRegistrationTypeList.ContainsCode(TaxRegistrationTypeCodes.NotRecoverable));

			Setting.Country = Core.Constants.CountryCodes.China;
			AssertNotNull("TaxRegistrationTypeList", Lookups.TaxRegistrationTypeList);
			AssertEquals("TaxRegistrationTypeList.Count", 2, Lookups.TaxRegistrationTypeList.Count);
			Assert("The TaxRegistrationTypeList should contain 'REC'", Lookups.TaxRegistrationTypeList.ContainsCode("REC"));
			Assert("The TaxRegistrationTypeList should contain 'NOT'", Lookups.TaxRegistrationTypeList.ContainsCode("NOT"));

			Setting.Country = Core.Constants.CountryCodes.Australia;
			AssertNotNull("TaxRegistrationTypeList", Lookups.TaxRegistrationTypeList);
			AssertEquals("TaxRegistrationTypeList.Count", 0, Lookups.TaxRegistrationTypeList.Count);

			Setting.Country = Core.Constants.CountryCodes.Turkey;
			AssertNotNull("TaxRegistrationTypeList", Lookups.TaxRegistrationTypeList);
			AssertEquals("TaxRegistrationTypeList.Count", 0, Lookups.TaxRegistrationTypeList.Count);

			Setting.Country = Core.Constants.CountryCodes.SriLanka;
			AssertNotNull("TaxRegistrationTypeList", Lookups.TaxRegistrationTypeList);
			AssertEquals("TaxRegistrationTypeList.Count", 0, Lookups.TaxRegistrationTypeList.Count);
		}

		public void TestDisbursementRuleList()
		{
			AssertNotNull("DisbursementRuleList", Lookups.DisbursementRuleList);
			AssertEquals("DisbursementRuleList.Count", 3, Lookups.DisbursementRuleList.Count);
			Assert("The DisbursementRuleList should contain 'ALL'", Lookups.DisbursementRuleList.ContainsCode(DisbursementRuleCodes.AllTransactions));
			Assert("The DisbursementRuleList should contain 'DSB'", Lookups.DisbursementRuleList.ContainsCode(DisbursementRuleCodes.DisbursementOnly));
			Assert("The DisbursementRuleList should contain 'NDB'", Lookups.DisbursementRuleList.ContainsCode(DisbursementRuleCodes.NonDisbursementOnly));
		}

		public void TestSelfBillingRuleList()
		{
			AssertNotNull("SelfBillingRuleList", Lookups.SelfBillingRuleList);
			AssertEquals("SelfBillingRuleList.Count", 2, Lookups.SelfBillingRuleList.Count);
			Assert("The SelfBillingRuleList should contain 'SBI'", Lookups.SelfBillingRuleList.ContainsCode(SelfBillingRuleCodes.SelfBillingTransactions));
			Assert("The SelfBillingRuleList should contain 'STD'", Lookups.SelfBillingRuleList.ContainsCode(SelfBillingRuleCodes.StandardTransactions));
		}

		public void TestVATGroupList()
		{
			AssertNotNull("VATGroupList", Lookups.VATGroupList);
			AssertEquals("VATGroupList.Count", 2, Lookups.VATGroupList.Count);
			Assert("The VATGroupList should contain 'VGM'", Lookups.VATGroupList.ContainsCode(VatGroupListCodes.VATGroupMember));
			Assert("The VATGroupList should contain 'EVG'", Lookups.VATGroupList.ContainsCode(VatGroupListCodes.ExcludeVATGroupMembers));
		}

		public void TestSubTypeList()
		{
			Setting.Country = Core.Constants.CountryCodes.UnitedKingdom;
			var listToAssert = Lookups.SubTypeList;
			var referenceList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(Setting.Country);
			AssertEquals("Lookups.SubTypeList.Count", referenceList.Count, listToAssert.Count);

			foreach (var code in referenceList)
			{
				Assert("Lookups.SubTypeList should contain code", listToAssert.ContainsCode(code));
			}
		}

		public void TestTaxRegistrationLocationRuleList()
		{
			AssertEquals("TaxRegistrationLocationRuleList should be empty if country is not set", 0, Lookups.TaxRegistrationLocationRuleList.Count);

			Setting.Country = Core.Constants.CountryCodes.KoreaSouth; // Non EU Country
			AssertEquals("TaxRegistrationLocationRuleList.Count", 1, Lookups.TaxRegistrationLocationRuleList.Count);
			Assert("The TaxRegistrationLocationRuleList should contain the country being configured", Lookups.TaxRegistrationLocationRuleList.ContainsCode(Setting.Country));

			Setting.Country = Core.Constants.CountryCodes.Italy; // EU Country
			AssertEquals("TaxRegistrationLocationRuleList.Count", 3, Lookups.TaxRegistrationLocationRuleList.Count);
			Assert("The TaxRegistrationLocationRuleList should contain the country being configured", Lookups.TaxRegistrationLocationRuleList.ContainsCode(Setting.Country));
			Assert("The TaxRegistrationLocationRuleList should contain EUX", Lookups.TaxRegistrationLocationRuleList.ContainsCode("EUX"));
			Assert("The TaxRegistrationLocationRuleList should contain NEU", Lookups.TaxRegistrationLocationRuleList.ContainsCode("NEU"));

			Setting.Country = Core.Constants.CountryCodes.Germany; // EU Country
			AssertEquals("TaxRegistrationLocationRuleList.Count", 3, Lookups.TaxRegistrationLocationRuleList.Count);
			Assert("The TaxRegistrationLocationRuleList should contain the country being configured", Lookups.TaxRegistrationLocationRuleList.ContainsCode(Setting.Country));
			Assert("The TaxRegistrationLocationRuleList should contain EUX", Lookups.TaxRegistrationLocationRuleList.ContainsCode("EUX"));
			Assert("The TaxRegistrationLocationRuleList should contain NEU", Lookups.TaxRegistrationLocationRuleList.ContainsCode("NEU"));
		}

		public void TestOrganisationLocationList()
		{
			AssertEquals("OrganisationLocationList should be empty if country is not set", 0, Lookups.OrganisationLocationList.Count);

			Setting.Country = Core.Constants.CountryCodes.Italy; // EU Country
			AssertEquals("OrganisationLocationList.Count", 3, Lookups.OrganisationLocationList.Count);
			Assert("The OrganisationLocationList should contain the country being configured", Lookups.OrganisationLocationList.ContainsCode(Setting.Country));
			Assert("The OrganisationLocationList should contain EUX", Lookups.OrganisationLocationList.ContainsCode("EUX"));
			Assert("The OrganisationLocationList should contain NEU", Lookups.OrganisationLocationList.ContainsCode("NEU"));

			Setting.Country = Core.Constants.CountryCodes.KoreaSouth; // Non EU Country
			var listToAssert = Lookups.OrganisationLocationList;
			var referenceList = AccountingTaxLocations.GetCountries(Factory);
			AssertEquals("Lookups.OrganisationLocationList.Count", referenceList.Count, listToAssert.Count);

			foreach (var code in referenceList)
			{
				Assert("Lookups.OrganisationLocationList should contain code", listToAssert.ContainsCode(code));
			}
		}

		public void TestCountryList()
		{
			var listToAssert = Lookups.CountryList;
			var referenceList = new RefCountryCollection(Factory);
			AssertEquals("Lookups.CountryList.Count", referenceList.Count, listToAssert.Count);

			foreach (var country in referenceList)
			{
				Assert("Lookups.CountryList should contain code", listToAssert.Contains(country));
			}
		}

		public void TestReportingDateList()
		{
			Setting.LedgerType = LedgerTypes.AccountsReceivable;

			AssertEquals("TaxRegistrationLocationRuleList.Count for AR", 4, Lookups.ReportingDateList.Count);
			Assert("The ReportingDateList for AR should contain POS code", Lookups.ReportingDateList.ContainsCode("POS"));
			Assert("The ReportingDateList for AR should contain INV code", Lookups.ReportingDateList.ContainsCode("INV"));
			Assert("The ReportingDateList for AR should contain TDE code", Lookups.ReportingDateList.ContainsCode("TDE"));
			Assert("The ReportingDateList for AR should contain TDL code", Lookups.ReportingDateList.ContainsCode("TDL"));

			Setting.LedgerType = LedgerTypes.AccountsPayable;

			AssertEquals("TaxRegistrationLocationRuleList.Count for AP", 5, Lookups.ReportingDateList.Count);
			Assert("The ReportingDateList for AP should contain POS code", Lookups.ReportingDateList.ContainsCode("POS"));
			Assert("The ReportingDateList for AR should contain INV code", Lookups.ReportingDateList.ContainsCode("INV"));
			Assert("The ReportingDateList for AP should contain DRD code", Lookups.ReportingDateList.ContainsCode("DRD"));
			Assert("The ReportingDateList for AP should contain TDE code", Lookups.ReportingDateList.ContainsCode("TDE"));
			Assert("The ReportingDateList for AP should contain TDL code", Lookups.ReportingDateList.ContainsCode("TDL"));

			var ledgersWithEmptyList = Setting.Lookups.LedgerTypeList.GetAllCodes().Except(new[] { LedgerTypes.AccountsReceivable, LedgerTypes.AccountsPayable });

			foreach (var ledger in ledgersWithEmptyList)
			{
				Setting.LedgerType = ledger;
				Assert(Setting.ReportingDate.IsEmpty);
				AssertEquals("Empty ReportingDateList for Ledger: " + ledger, 0, Lookups.ReportingDateList.Count);
			}
		}

		public void TestOrganisationCategoryList()
		{
			AssertNotNull("OrganisationCategoryList", Lookups.OrganisationCategoryList);

			var expectedCodes = new[]
			{
				OrgConstants.Category.Business,
				OrgConstants.Category.Government,
				OrgConstants.Category.NaturalPersonIndividual,
				OrgConstants.Category.NonGovernmentOrganisation
			};

			AssertContainsExactElementsInAnyOrder(expectedCodes, Lookups.OrganisationCategoryList.GetAllCodes().ToArray());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			ReportConfiguration = new ComplianceReportConfiguration(Factory);
			Setting = ReportConfiguration.Settings.AddNew();
			Lookups = new LookupsClass(Setting);
		}

		ComplianceReportConfiguration ReportConfiguration;
		ComplianceReportConfigurationSetting Setting;
		LookupsClass Lookups;

		#endregion
	}
}
