using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using static Enterprise.Registry.Business.ComplianceSubTypeCodesAndLists.CodesAndDescriptions;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceSubTypeAttributionRuleConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxRegistrationTypeList()
		{
			configuration.Country = Core.Constants.CountryCodes.Argentina;
			AssertEquals("TaxRegistrationTypeList.Count", 2, Lookups.TaxRegistrationTypeList.Count);
			AssertEquals("The TaxRegistrationTypeList should contain 'REC'", true, Lookups.TaxRegistrationTypeList.ContainsCode("REC"));
			AssertEquals("The TaxRegistrationTypeList should contain 'NOT'", true, Lookups.TaxRegistrationTypeList.ContainsCode("NOT"));

			configuration.Country = Core.Constants.CountryCodes.China;
			AssertEquals("TaxRegistrationTypeList.Count", 4, Lookups.TaxRegistrationTypeList.Count);
			AssertEquals("The TaxRegistrationTypeList should contain 'REC'", true, Lookups.TaxRegistrationTypeList.ContainsCode("REC"));
			AssertEquals("The TaxRegistrationTypeList should contain 'NOT'", true, Lookups.TaxRegistrationTypeList.ContainsCode("NOT"));
			AssertEquals("The TaxRegistrationTypeList should contain 'FOR'", true, Lookups.TaxRegistrationTypeList.ContainsCode("FOR"));

			configuration.Country = Core.Constants.CountryCodes.Australia;
			AssertEquals("TaxRegistrationTypeList.Count", 0, Lookups.TaxRegistrationTypeList.Count);

			configuration.Country = Core.Constants.CountryCodes.Peru;
			AssertEquals("TaxRegistrationTypeList.Count", 2, Lookups.TaxRegistrationTypeList.Count);
			AssertEquals("The TaxRegistrationTypeList should contain 'DNI'", true, Lookups.TaxRegistrationTypeList.ContainsCode("DNI"));
			AssertEquals("The TaxRegistrationTypeList should contain 'RUC'", true, Lookups.TaxRegistrationTypeList.ContainsCode("RUC"));

			configuration.Country = Core.Constants.CountryCodes.ElSalvador;
			AssertEquals("TaxRegistrationTypeList.Count", 1, Lookups.TaxRegistrationTypeList.Count);
			AssertEquals("The TaxRegistrationTypeList should contain 'NRC'", true, Lookups.TaxRegistrationTypeList.ContainsCode("NRC"));

			configuration.Country = Core.Constants.CountryCodes.SriLanka;
			AssertEquals("TaxRegistrationTypeList.Count", 3, Lookups.TaxRegistrationTypeList.Count);
			AssertEquals("The TaxRegistrationTypeList should contain 'ALL'", true, Lookups.TaxRegistrationTypeList.ContainsCode("ALL"));
			AssertEquals("The TaxRegistrationTypeList should contain 'SVT'", true, Lookups.TaxRegistrationTypeList.ContainsCode("SVT"));
			AssertEquals("The TaxRegistrationTypeList should contain 'NON'", true, Lookups.TaxRegistrationTypeList.ContainsCode("NON"));

			configuration.Country = Core.Constants.CountryCodes.Turkey;
			Assert("The TaxRegistrationTypeList should contain the country being configured", Lookups.TaxRegistrationLocationRuleList.ContainsCode(configuration.Country));
			AssertEquals("TaxRegistrationTypeList.Count", 3, Lookups.TaxRegistrationTypeList.Count);
			AssertEquals("The TaxRegistrationTypeList should contain 'ERO'", true, Lookups.TaxRegistrationTypeList.ContainsCode("ERO"));
			AssertEquals("The description of ERO should be without Turkish word.", "e-Invoice Registered Organization", Lookups.TaxRegistrationTypeList.GetDescriptionFromCode("ERO"));
			AssertEquals("The TaxRegistrationTypeList should contain 'NER'", true, Lookups.TaxRegistrationTypeList.ContainsCode("NER"));
			AssertEquals("The TaxRegistrationTypeList should contain 'ALL'", true, Lookups.TaxRegistrationTypeList.ContainsCode("ALL"));

			using (AccountingMasterFilesRegistry.Instance.EnableNewTurkeyARComplianceFeaturesFrom.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.ToDateTime()))
			{
				AssertEquals("TaxRegistrationTypeList.Count", 5, Lookups.TaxRegistrationTypeList.Count);
				AssertEquals("The TaxRegistrationTypeList should contain 'ERO'", true, Lookups.TaxRegistrationTypeList.ContainsCode("ERO"));
				AssertEquals("The description of ERO should be with Turkish word.", "e-Invoice Registered Organization (Temel/Basic)", Lookups.TaxRegistrationTypeList.GetDescriptionFromCode("ERO"));
				AssertEquals("The TaxRegistrationTypeList should contain 'ERC'", true, Lookups.TaxRegistrationTypeList.ContainsCode("ERC"));
				AssertEquals("The description of ERO should be with Turkish word.", "e-Invoice Registered Organization (Ticari/Commercial)", Lookups.TaxRegistrationTypeList.GetDescriptionFromCode("ERC"));
				AssertEquals("The TaxRegistrationTypeList should contain 'ERC'", true, Lookups.TaxRegistrationTypeList.ContainsCode("ERG"));
				AssertEquals("The description of ERG should be with Turkish word.", "e-Invoice Registered Organization (Temel veya Ticari/Basic or Commercial)", Lookups.TaxRegistrationTypeList.GetDescriptionFromCode("ERG"));
				AssertEquals("The TaxRegistrationTypeList should contain 'NER'", true, Lookups.TaxRegistrationTypeList.ContainsCode("NER"));
				AssertEquals("The TaxRegistrationTypeList should contain 'ALL'", true, Lookups.TaxRegistrationTypeList.ContainsCode("ALL"));
			}
		}

		public void TestTaxRegistrationLocationRuleList()
		{
			AssertEquals("TaxRegistrationLocationRuleList should be empty if country is not set", 0, Lookups.TaxRegistrationLocationRuleList.Count);

			configuration.Country = Core.Constants.CountryCodes.KoreaSouth; // Non EU Country
			AssertEquals("TaxRegistrationLocationRuleList.Count", 1, Lookups.TaxRegistrationLocationRuleList.Count);
			Assert("The TaxRegistrationLocationRuleList should contain the country being configured", Lookups.TaxRegistrationLocationRuleList.ContainsCode(configuration.Country));

			configuration.Country = Core.Constants.CountryCodes.Italy; // EU Country
			AssertEquals("TaxRegistrationLocationRuleList.Count", 3, Lookups.TaxRegistrationLocationRuleList.Count);
			Assert("The TaxRegistrationLocationRuleList should contain the country being configured", Lookups.TaxRegistrationLocationRuleList.ContainsCode(configuration.Country));
			Assert("The TaxRegistrationLocationRuleList should contain EUX", Lookups.TaxRegistrationLocationRuleList.ContainsCode("EUX"));
			Assert("The TaxRegistrationLocationRuleList should contain NEU", Lookups.TaxRegistrationLocationRuleList.ContainsCode("NEU"));

			configuration.Country = Core.Constants.CountryCodes.Germany; // EU Country
			AssertEquals("TaxRegistrationLocationRuleList.Count", 3, Lookups.TaxRegistrationLocationRuleList.Count);
			Assert("The TaxRegistrationLocationRuleList should contain the country being configured", Lookups.TaxRegistrationLocationRuleList.ContainsCode(configuration.Country));
			Assert("The TaxRegistrationLocationRuleList should contain EUX", Lookups.TaxRegistrationLocationRuleList.ContainsCode("EUX"));
			Assert("The TaxRegistrationLocationRuleList should contain NEU", Lookups.TaxRegistrationLocationRuleList.ContainsCode("NEU"));
		}

		public void TestOrganisationLocationList()
		{
			AssertEquals("OrganisationLocationList should be empty if country is not set", 0, Lookups.OrganisationLocationList.Count);

			configuration.Country = Core.Constants.CountryCodes.Italy; // EU Country
			AssertEquals("OrganisationLocationList.Count", 3, Lookups.OrganisationLocationList.Count);
			Assert("The OrganisationLocationList should contain the country being configured", Lookups.OrganisationLocationList.ContainsCode(configuration.Country));
			Assert("The OrganisationLocationList should contain EUX", Lookups.OrganisationLocationList.ContainsCode("EUX"));
			Assert("The OrganisationLocationList should contain NEU", Lookups.OrganisationLocationList.ContainsCode("NEU"));

			configuration.Country = Core.Constants.CountryCodes.KoreaSouth; // Non EU Country
			var listToAssert = Lookups.OrganisationLocationList;
			var referenceList = AccountingTaxLocations.GetCountries(Factory);
			AssertEquals("Lookups.OrganisationLocationList.Count", referenceList.Count, listToAssert.Count);

			foreach (var code in referenceList)
			{
				Assert("Lookups.OrganisationLocationList should contain code", listToAssert.ContainsCode(code));
			}
		}

		public void TestVATGroupList()
		{
			AssertEquals("VAT Group List", 2, Lookups.VATGroupList.Count);
			Assert("VAT Group List should contain 'VGM'", Lookups.VATGroupList.ContainsCode("VGM"));
			Assert("VAT Group List should contain 'EVG'", Lookups.VATGroupList.ContainsCode("EVG"));
		}

		public void TestLedgerTypeList()
		{
			AssertEquals("LedgerTypeList.Count", 2, Lookups.LedgerTypeList.Count);
			AssertEquals("The LedgerTypeList should contain 'AR'", true, Lookups.LedgerTypeList.ContainsCode("AR"));
			AssertEquals("The LedgerTypeList should contain 'AP'", true, Lookups.LedgerTypeList.ContainsCode("AP"));
		}

		public void TestExporterExemptionList()
		{
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					ExporterExemptionCodes.Exempt,
					ExporterExemptionCodes.NotExempt,
				},
				Lookups.ExporterExemptionList.GetAllCodesZString());
		}

		public void TestInvoiceTypeList()
		{
			AssertEquals("InvoiceTypeList.Count", 3, Lookups.InvoiceTypeList.Count);
			AssertEquals("The InvoiceTypeList should contain 'INV'", true, Lookups.InvoiceTypeList.ContainsCode("INV"));
			AssertEquals("The InvoiceTypeList should contain 'CRD'", true, Lookups.InvoiceTypeList.ContainsCode("CRD"));
			AssertEquals("The InvoiceTypeList should contain 'ADJ'", true, Lookups.InvoiceTypeList.ContainsCode("ADJ"));
		}

		public void TestTaxInvoiceRuleList()
		{
			AssertEquals("TaxInvoiceRuleList.Count", 16, Lookups.TaxInvoiceRuleList.Count);
			AssertEquals("The TaxInvoiceRuleList should contain 'NON'", true, Lookups.TaxInvoiceRuleList.ContainsCode("NON"));
			AssertEquals("The TaxInvoiceRuleList should contain 'TNE'", true, Lookups.TaxInvoiceRuleList.ContainsCode("TNE"));
			AssertEquals("The TaxInvoiceRuleList should contain 'AMT'", true, Lookups.TaxInvoiceRuleList.ContainsCode("AMT"));
			AssertEquals("The TaxInvoiceRuleList should contain 'TID'", true, Lookups.TaxInvoiceRuleList.ContainsCode("TID"));
			AssertEquals("The TaxInvoiceRuleList should contain 'TXN'", true, Lookups.TaxInvoiceRuleList.ContainsCode("TXN"));
			AssertEquals("The TaxInvoiceRuleList should contain 'TXX'", true, Lookups.TaxInvoiceRuleList.ContainsCode("TXX"));
			AssertEquals("The TaxInvoiceRuleList should contain 'TXA'", true, Lookups.TaxInvoiceRuleList.ContainsCode("TXA"));
			AssertEquals("The TaxInvoiceRuleList should contain 'RVS'", true, Lookups.TaxInvoiceRuleList.ContainsCode("RVS"));
			AssertEquals("The TaxInvoiceRuleList should contain 'TXR'", true, Lookups.TaxInvoiceRuleList.ContainsCode("TXR"));
			AssertEquals("The TaxInvoiceRuleList should contain 'EXL'", true, Lookups.TaxInvoiceRuleList.ContainsCode("EXL"));
			AssertEquals("The TaxInvoiceRuleList should contain 'ATZ'", true, Lookups.TaxInvoiceRuleList.ContainsCode("ATZ"));
			AssertEquals("The TaxInvoiceRuleList should contain 'TXS'", true, Lookups.TaxInvoiceRuleList.ContainsCode("TXS"));
			AssertEquals("The TaxInvoiceRuleList should contain 'SUS'", true, Lookups.TaxInvoiceRuleList.ContainsCode("SUS"));
			AssertEquals("The TaxInvoiceRuleList should contain 'STI'", true, Lookups.TaxInvoiceRuleList.ContainsCode("STI"));
			AssertEquals("The TaxInvoiceRuleList should contain 'EXT'", true, Lookups.TaxInvoiceRuleList.ContainsCode("EXT"));
			AssertEquals("The TaxInvoiceRuleList should contain 'NOT'", true, Lookups.TaxInvoiceRuleList.ContainsCode("NOT"));
		}

		public void TestOriginalRuleList()
		{
			AssertEquals("OriginalRuleList.Count", 5, Lookups.OriginalRuleList.Count);
			AssertEquals("The OriginalRuleList should contain 'ALL'", true, Lookups.OriginalRuleList.ContainsCode("ALL"));
			AssertEquals("The OriginalRuleList should contain 'ATO'", true, Lookups.OriginalRuleList.ContainsCode("ATO"));
			AssertEquals("The OriginalRuleList should contain 'RTO'", true, Lookups.OriginalRuleList.ContainsCode("RTO"));
			AssertEquals("The OriginalRuleList should contain 'ARO'", true, Lookups.OriginalRuleList.ContainsCode("ARO"));
			AssertEquals("The OriginalRuleList should contain 'OTO'", true, Lookups.OriginalRuleList.ContainsCode("OTO"));
		}

		public void TestDisbursementRuleList()
		{
			AssertEquals("DisbursementRuleList.Count", 3, Lookups.DisbursementRuleList.Count);
			AssertEquals("The DisbursementRuleList should contain 'ALL'", true, Lookups.DisbursementRuleList.ContainsCode("ALL"));
			AssertEquals("The DisbursementRuleList should contain 'DSB'", true, Lookups.DisbursementRuleList.ContainsCode("DSB"));
			AssertEquals("The DisbursementRuleList should contain 'NDB'", true, Lookups.DisbursementRuleList.ContainsCode("NDB"));
		}

		public void TestSelfBillingRuleList()
		{
			AssertEquals("SelfBillingRuleList.Count", 2, Lookups.SelfBillingRuleList.Count);
			AssertEquals("The SelfBillingRuleList should contain 'SBI'", true, Lookups.SelfBillingRuleList.ContainsCode("SBI"));
			AssertEquals("The SelfBillingRuleList should contain 'STD'", true, Lookups.SelfBillingRuleList.ContainsCode("STD"));
		}

		public void TestSubTypeList()
		{
			configuration.Country = Core.Constants.CountryCodes.UnitedKingdom;
			var listToAssert = Lookups.SubTypeList;
			var referenceList = AccComplianceSequenceLookups.SequenceClassListBaseOnCountryCode(configuration.Country);
			AssertEquals("Lookups.SubTypeList.Count", referenceList.Count, listToAssert.Count);

			foreach (var code in referenceList)
			{
				Assert("Lookups.SubTypeList should contain code", listToAssert.ContainsCode(code));
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

			AssertContainsExactElementsInAnyOrder(expectedCodes, Lookups.OrganisationCategoryList.GetAllCodes());
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			configuration = new ComplianceSubTypeAttributionRuleConfiguration();
			Lookups = new ComplianceSubTypeAttributionRuleConfigurationLookups(configuration);
		}
		ComplianceSubTypeAttributionRuleConfigurationLookups Lookups;
		ComplianceSubTypeAttributionRuleConfiguration configuration;

		#endregion
	}
}
