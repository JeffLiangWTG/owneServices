using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.Lists;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceSubTypeCodesAndListsTest : TestCase
	{
		public void TestGetTaxInvoiceRuleList()
		{
			var list = GetTaxInvoiceRuleList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => null));
			CombineAssertions("Test GetTaxInvoiceRuleList when input null", () =>
			{
				AssertEquals("GetTaxInvoiceRuleList count", 16, list.Count);
				Assert("should contain 'AMT'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAnAmountOfTax));
				Assert("should contain 'TID'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID));
				Assert("should contain 'TXN'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT));
				Assert("should contain 'TNE'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL));
				Assert("should contain 'TXX'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax));
				Assert("should contain 'TXA'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax));
				Assert("should contain 'TXR'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs));
				Assert("should contain 'RVS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly));
				Assert("should contain 'SUS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID));
				Assert("should contain 'TXS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended));
				Assert("should contain 'EXL'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly));
				Assert("should contain 'ATZ'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount));
				Assert("should contain 'STI'", list.ContainsCode(TaxInvoiceRuleCodes.SpecificTaxIDs));
				Assert("should contain 'EXT'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithExemptTaxIDs));
				Assert("should contain 'NOT'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithNoReportTaxIDs));
				Assert("should contain 'NON'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsNoTaxIDs));
			});

			list = GetTaxInvoiceRuleList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => new CodeDescriptionPairList()));
			CombineAssertions("Test GetTaxInvoiceRuleList when input new empty object", () =>
			{
				AssertEquals("GetTaxInvoiceRuleList count", 16, list.Count);
				Assert("should contain 'AMT'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAnAmountOfTax));
				Assert("should contain 'TID'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID));
				Assert("should contain 'TXN'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT));
				Assert("should contain 'TXE'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL));
				Assert("should contain 'TXX'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax));
				Assert("should contain 'TXA'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax));
				Assert("should contain 'TXR'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs));
				Assert("should contain 'RVS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly));
				Assert("should contain 'SUS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID));
				Assert("should contain 'TXS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended));
				Assert("should contain 'EXL'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly));
				Assert("should contain 'ATZ'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount));
				Assert("should contain 'STI'", list.ContainsCode(TaxInvoiceRuleCodes.SpecificTaxIDs));
				Assert("should contain 'EXT'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithExemptTaxIDs));
				Assert("should contain 'NOT'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithNoReportTaxIDs));
				Assert("should contain 'NON'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsNoTaxIDs));
			});

			list = GetTaxInvoiceRuleList(distinctDataProvider);
			CombineAssertions("Test GetTaxInvoiceRuleList when input no empty object", () =>
			{
				AssertEquals("GetTaxInvoiceRuleList count", 17, list.Count);
				Assert("should contain 'AMT'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAnAmountOfTax));
				Assert("should contain 'TID'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxID));
				Assert("should contain 'TXN'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOT));
				Assert("should contain 'TXN'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingNOTAndEXL));
				Assert("should contain 'TXX'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndNoAmountOfTax));
				Assert("should contain 'TXA'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDAndAmountOfTax));
				Assert("should contain 'TXR'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingReverseChargeTaxIDs));
				Assert("should contain 'RVS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsReverseChargeTaxIDsOnly));
				Assert("should contain 'SUS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneSuspendedTaxID));
				Assert("should contain 'TXS'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsAtLeastOneTaxIDExcludingSuspended));
				Assert("should contain 'EXL'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsExcludeChargeTaxIDsOnly));
				Assert("should contain 'ATZ'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithRatedTaxIDAndZeroTaxAmount));
				Assert("should contain 'STI'", list.ContainsCode(TaxInvoiceRuleCodes.SpecificTaxIDs));
				Assert("should contain 'EXT'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithExemptTaxIDs));
				Assert("should contain 'NOT'", list.ContainsCode(TaxInvoiceRuleCodes.AllWithNoReportTaxIDs));
				Assert("should contain 'NON'", list.ContainsCode(TaxInvoiceRuleCodes.ContainsNoTaxIDs));
				Assert("should contain 'ABC'", list.ContainsCode("ABC"));
			});
		}

		public void TestGetLedgerTypeList()
		{
			var list = GetLedgerTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => null));
			AssertEquals("GetLedgerTypeList", 2, list.Count);
			Assert("The LedgerTypeList should contain 'AR'", list.ContainsCode(LedgerTypes.AccountsReceivable));
			Assert("The LedgerTypeList should contain 'AP'", list.ContainsCode(LedgerTypes.AccountsPayable));

			list = GetLedgerTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => new CodeDescriptionPairList()));
			AssertEquals("GetLedgerTypeList", 2, list.Count);
			Assert("The LedgerTypeList should contain 'AR'", list.ContainsCode(LedgerTypes.AccountsReceivable));
			Assert("The LedgerTypeList should contain 'AP'", list.ContainsCode(LedgerTypes.AccountsPayable));

			list = GetLedgerTypeList(distinctDataProvider);
			AssertEquals("GetLedgerTypeList", 3, list.Count);
			Assert("The LedgerTypeList should contain 'AR'", list.ContainsCode(LedgerTypes.AccountsReceivable));
			Assert("The LedgerTypeList should contain 'AP'", list.ContainsCode(LedgerTypes.AccountsPayable));
			Assert("The LedgerTypeList should contain 'DEF'", list.ContainsCode("DEF"));
		}

		public void TestGetInvoiceTypeList()
		{
			var list = GetInvoiceTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => null));
			AssertEquals("GetInvoiceTypeList", 0, list.Count);

			list = GetInvoiceTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => new CodeDescriptionPairList()));
			AssertEquals("GetInvoiceTypeList", 0, list.Count);

			list = GetInvoiceTypeList(distinctDataProvider);
			AssertEquals("GetInvoiceTypeList", 1, list.Count);
			Assert("The InvoiceTypeList should contain 'GHI'", list.ContainsCode("GHI"));
		}

		public void TestGetTaxRegistrationTypeList()
		{
			var list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => null), Core.Constants.CountryCodes.Argentina);
			AssertEquals("GetTaxRegistrationTypeList", 2, list.Count);
			Assert("The TaxRegistrationTypeList should contain 'REC'", list.ContainsCode(TaxRegistrationTypeCodes.Recoverable));
			Assert("The TaxRegistrationTypeList should contain 'NOT'", list.ContainsCode(TaxRegistrationTypeCodes.NotRecoverable));

			list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => null), Core.Constants.CountryCodes.China);
			AssertEquals("GetTaxRegistrationTypeList", 2, list.Count);
			Assert("The TaxRegistrationTypeList should contain 'REC'", list.ContainsCode(TaxRegistrationTypeCodes.Recoverable));
			Assert("The TaxRegistrationTypeList should contain 'NOT'", list.ContainsCode(TaxRegistrationTypeCodes.NotRecoverable));

			list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => null), Core.Constants.CountryCodes.Australia);
			AssertEquals("GetTaxRegistrationTypeList", 0, list.Count);

			list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => null), ZString.Empty);
			AssertEquals("GetTaxRegistrationTypeList", 0, list.Count);

			list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => new CodeDescriptionPairList()), Core.Constants.CountryCodes.Argentina);
			AssertEquals("GetTaxRegistrationTypeList", 2, list.Count);
			Assert("The TaxRegistrationTypeList should contain 'REC'", list.ContainsCode(TaxRegistrationTypeCodes.Recoverable));
			Assert("The TaxRegistrationTypeList should contain 'NOT'", list.ContainsCode(TaxRegistrationTypeCodes.NotRecoverable));

			list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => new CodeDescriptionPairList()), Core.Constants.CountryCodes.China);
			AssertEquals("GetTaxRegistrationTypeList", 2, list.Count);
			Assert("The TaxRegistrationTypeList should contain 'REC'", list.ContainsCode(TaxRegistrationTypeCodes.Recoverable));
			Assert("The TaxRegistrationTypeList should contain 'NOT'", list.ContainsCode(TaxRegistrationTypeCodes.NotRecoverable));

			list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => new CodeDescriptionPairList()), Core.Constants.CountryCodes.Australia);
			AssertEquals("GetTaxRegistrationTypeList", 0, list.Count);

			list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => null), Core.Constants.CountryCodes.Turkey);
			AssertEquals("GetTaxRegistrationTypeList", 0, list.Count);

			list = GetTaxRegistrationTypeList(new DummyComplianceSubTypeListAdditionalSameDataProvider(() => new CodeDescriptionPairList()), ZString.Empty);
			AssertEquals("GetTaxRegistrationTypeList", 0, list.Count);

			list = GetTaxRegistrationTypeList(distinctDataProvider, Core.Constants.CountryCodes.Argentina);
			AssertEquals("GetTaxRegistrationTypeList", 3, list.Count);
			Assert("The TaxRegistrationTypeList should contain 'REC'", list.ContainsCode(TaxRegistrationTypeCodes.Recoverable));
			Assert("The TaxRegistrationTypeList should contain 'NOT'", list.ContainsCode(TaxRegistrationTypeCodes.NotRecoverable));
			Assert("The TaxRegistrationTypeList should contain 'JKL'", list.ContainsCode("JKL"));

			list = GetTaxRegistrationTypeList(distinctDataProvider, Core.Constants.CountryCodes.China);
			AssertEquals("GetTaxRegistrationTypeList", 3, list.Count);
			Assert("The TaxRegistrationTypeList should contain 'REC'", list.ContainsCode(TaxRegistrationTypeCodes.Recoverable));
			Assert("The TaxRegistrationTypeList should contain 'NOT'", list.ContainsCode(TaxRegistrationTypeCodes.NotRecoverable));
			Assert("The TaxRegistrationTypeList should contain 'JKL'", list.ContainsCode("JKL"));

			list = GetTaxRegistrationTypeList(distinctDataProvider, Core.Constants.CountryCodes.Australia);
			AssertEquals("GetTaxRegistrationTypeList", 1, list.Count);
			Assert("The TaxRegistrationTypeList should contain 'JKL'", list.ContainsCode("JKL"));

			list = GetTaxRegistrationTypeList(distinctDataProvider, ZString.Empty);
			AssertEquals("GetTaxRegistrationTypeList", 1, list.Count);
			Assert("The TaxRegistrationTypeList should contain 'JKL'", list.ContainsCode("JKL"));
		}

		public void TestOriginalRuleList()
		{
			var list = OriginalRuleList;
			AssertEquals("OriginalRuleList", 5, list.Count);
			Assert("The OriginalRuleList should contain 'ALL'", list.ContainsCode(OriginalRuleCodes.AllTransactions));
			Assert("The OriginalRuleList should contain 'ATO'", list.ContainsCode(OriginalRuleCodes.AmendingTransactionOnly));
			Assert("The OriginalRuleList should contain 'RTO'", list.ContainsCode(OriginalRuleCodes.ReversalTransactionOnly));
			Assert("The OriginalRuleList should contain 'ARO'", list.ContainsCode(OriginalRuleCodes.AmendingReversalOnly));
			Assert("The OriginalRuleList should contain 'OTO'", list.ContainsCode(OriginalRuleCodes.OriginalTransactionOnly));
		}

		public void TestDisbursementRuleList()
		{
			var list = DisbursementRuleList;
			AssertEquals("DisbursementRuleList", 3, list.Count);
			Assert("The DisbursementRuleList should contain 'ALL'", list.ContainsCode(DisbursementRuleCodes.AllTransactions));
			Assert("The DisbursementRuleList should contain 'DSB'", list.ContainsCode(DisbursementRuleCodes.DisbursementOnly));
			Assert("The DisbursementRuleList should contain 'NDB'", list.ContainsCode(DisbursementRuleCodes.NonDisbursementOnly));
		}

		public void TestSelfBillingRuleList()
		{
			var list = SelfBillingRuleList;
			AssertEquals("SelfBillingRuleList", 2, list.Count);
			Assert("The SelfBillingRuleList should contain 'SBI'", list.ContainsCode(SelfBillingRuleCodes.SelfBillingTransactions));
			Assert("The SelfBillingRuleList should contain 'STD'", list.ContainsCode(SelfBillingRuleCodes.StandardTransactions));
		}

		public void TestVATGroupList()
		{
			var list = VATGroupList;
			AssertEquals("VATGroupList", 2, list.Count);
			Assert("The VATGroupList should contain 'VGM'", list.ContainsCode(VatGroupListCodes.VATGroupMember));
			Assert("The VATGroupList should contain 'EVG'", list.ContainsCode(VatGroupListCodes.ExcludeVATGroupMembers));
		}

		public void TestExporterExemptionList()
		{
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					(ZString)ExporterExemptionCodes.Exempt,
					(ZString)ExporterExemptionCodes.NotExempt,
				},
				ExporterExemptionList.GetAllCodesZString()
			);
		}

		public void TestSubTypeList()
		{
			var country = Core.Constants.CountryCodes.UnitedKingdom;
			var listToAssert = GetSubTypeList(country);
			var referenceList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(country);
			AssertEquals("GetSubTypeList", referenceList.Count, listToAssert.Count);

			foreach (var code in referenceList)
			{
				Assert("SubTypeList should contain code", listToAssert.ContainsCode(code));
			}
		}

		public void TestCountryList()
		{
			var factory = new BusinessObjectFactory();
			var listToAssert = GetCountryList(factory);
			var referenceList = new RefCountryCollection(factory);
			AssertEquals("GetCountryList", referenceList.Count, listToAssert.Count);

			foreach (var country in referenceList)
			{
				Assert("CountryList should contain code", listToAssert.Contains(country));
			}
		}

		public void TestTaxRegistrationLocationRuleList()
		{
			var factory = new BusinessObjectFactory();
			var country = ZString.Empty;
			var list = GetTaxRegistrationLocationRuleList(country, factory);
			AssertEquals("TaxRegistrationLocationRuleList should be empty if country is not set", 0, list.Count);

			country = Core.Constants.CountryCodes.KoreaSouth; // Non EU Country
			list = GetTaxRegistrationLocationRuleList(country, factory);
			AssertEquals("TaxRegistrationLocationRuleList.Count", 1, list.Count);
			Assert("The TaxRegistrationLocationRuleList should contain the country being configured", list.ContainsCode(country));

			country = Core.Constants.CountryCodes.Italy; // EU Country
			list = GetTaxRegistrationLocationRuleList(country, factory);
			AssertEquals("TaxRegistrationLocationRuleList.Count", 3, list.Count);
			Assert("The TaxRegistrationLocationRuleList should contain the country being configured", list.ContainsCode(country));
			Assert("The TaxRegistrationLocationRuleList should contain EUX", list.ContainsCode("EUX"));
			Assert("The TaxRegistrationLocationRuleList should contain NEU", list.ContainsCode("NEU"));

			country = "ABC";
			list = GetTaxRegistrationLocationRuleList(country, factory);
			AssertEquals("TaxRegistrationLocationRuleList.Count", 0, list.Count);
		}

		public void TestOrganisationLocationList()
		{
			var factory = new BusinessObjectFactory();
			var country = ZString.Empty;
			var list = GetOrganisationLocationList(country, factory);
			AssertEquals("OrganisationLocationList should be empty if country is not set", 0, list.Count);

			country = Core.Constants.CountryCodes.Italy; // EU Country
			list = GetTaxRegistrationLocationRuleList(country, factory);
			AssertEquals("GetOrganisationLocationList.Count", 3, list.Count);
			Assert("The OrganisationLocationList should contain the country being configured", list.ContainsCode(country));
			Assert("The OrganisationLocationList should contain EUX", list.ContainsCode("EUX"));
			Assert("The OrganisationLocationList should contain NEU", list.ContainsCode("NEU"));

			country = Core.Constants.CountryCodes.KoreaSouth; // Non EU Country
			var listToAssert = GetOrganisationLocationList(country, factory);
			var referenceList = AccountingTaxLocations.GetCountries(factory);
			AssertEquals("Lookups.OrganisationLocationList.Count", referenceList.Count, listToAssert.Count);

			foreach (var code in referenceList)
			{
				Assert("Lookups.OrganisationLocationList should contain code", listToAssert.ContainsCode(code));
			}

			country = "ABC";
			list = GetOrganisationLocationList(country, factory);
			AssertEquals("TaxRegistrationLocationRuleList.Count", 0, list.Count);
		}

		readonly DummyComplianceSubTypeListAdditionalDistinctDataProvider distinctDataProvider = new DummyComplianceSubTypeListAdditionalDistinctDataProvider("ABC", "DEF", "GHI", "JKL");

		class DummyComplianceSubTypeListAdditionalSameDataProvider : IComplianceSubTypeListAdditionalDataProvider
		{
			public DummyComplianceSubTypeListAdditionalSameDataProvider(Func<CodeDescriptionPairList> func)
			{
				this.func = func;
			}
			readonly Func<CodeDescriptionPairList> func;

			CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalInvoiceTypeList() => func();

			CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalLedgerTypeList() => func();

			CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalTaxInvoiceRuleList() => func();

			CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalTaxRegistrationTypeList() => func();
		}

		class DummyComplianceSubTypeListAdditionalDistinctDataProvider : IComplianceSubTypeListAdditionalDataProvider
		{
			public DummyComplianceSubTypeListAdditionalDistinctDataProvider(string code1, string code2, string code3, string code4)
			{
				this.code1 = code1;
				this.code2 = code2;
				this.code3 = code3;
				this.code4 = code4;
			}
			readonly string code1;
			readonly string code2;
			readonly string code3;
			readonly string code4;

			CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalTaxInvoiceRuleList() => GetCodeDescList(code1);

			CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalLedgerTypeList() => GetCodeDescList(code2);

			CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalInvoiceTypeList() => GetCodeDescList(code3);

			CodeDescriptionPairList IComplianceSubTypeListAdditionalDataProvider.GetAdditionalTaxRegistrationTypeList() => GetCodeDescList(code4);

			CodeDescriptionPairList GetCodeDescList(string code)
			{
				var result = new CodeDescriptionPairList();
				result.AddPair(code, string.Format("{0} Desc", code));
				return result;
			}
		}
	}
}
