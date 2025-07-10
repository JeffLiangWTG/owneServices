using CargoWise.EntityFramework.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ARAPDefaultTaxRecognitionRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestTaxRecognitionCodeList()
		{
			var configuration = new ARAPDefaultTaxRecognitionRule();
			ARAPDefaultTaxRecognitionRuleLookups lookups = new ARAPDefaultTaxRecognitionRuleLookups(configuration);
			AssertNotNull("CodeList should not be null", lookups.TaxRecognitionCodeList);
			AssertEquals("Code.Count", 2, lookups.TaxRecognitionCodeList.Count);
			AssertContainsExactElementsInAnyOrder(new CodeDescriptionPairList { AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.Default, AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.NotApplicable }, lookups.TaxRecognitionCodeList);
		}

		public void TestLoginCompanyCountryCodeList()
		{
			var configuration = new ARAPDefaultTaxRecognitionRule();
			ARAPDefaultTaxRecognitionRuleLookups lookups = new ARAPDefaultTaxRecognitionRuleLookups(configuration);

			AssertNotNull("LoginCompanyCountryCodeList should not be null", lookups.LoginCompanyCountryRuleCodeList);
			AssertEquals("LoginCompanyCountryCodeList.Count", 2, lookups.LoginCompanyCountryRuleCodeList.Count);

			AssertEquals(ARAPDefaultTaxRecognitionRuleLookups.LoginCompanyCountryRuleCode.IEU, lookups.LoginCompanyCountryRuleCodeList[0].Code);
			AssertEquals("European Union Country/Region", lookups.LoginCompanyCountryRuleCodeList[0].Description);
			AssertEquals(ARAPDefaultTaxRecognitionRuleLookups.LoginCompanyCountryRuleCode.OEU, lookups.LoginCompanyCountryRuleCodeList[1].Code);
			AssertEquals("Outside the European Union", lookups.LoginCompanyCountryRuleCodeList[1].Description);
		}

		public void TestOrganizationCountryCodeList()
		{
			var configuration = new ARAPDefaultTaxRecognitionRule();
			ARAPDefaultTaxRecognitionRuleLookups lookups = new ARAPDefaultTaxRecognitionRuleLookups(configuration);

			AssertNotNull("OrganizationCountryCodeList should not be null", lookups.OrganizationCountryRuleCodeList);
			AssertEquals("OrganizationCountryCodeList.Count", 4, lookups.OrganizationCountryRuleCodeList.Count);

			AssertEquals(ARAPDefaultTaxRecognitionRuleLookups.OrganizationCountryRuleCode.IEU, lookups.OrganizationCountryRuleCodeList[0].Code);
			AssertEquals("European Union Country/Region", lookups.OrganizationCountryRuleCodeList[0].Description);
			AssertEquals(ARAPDefaultTaxRecognitionRuleLookups.OrganizationCountryRuleCode.OEU, lookups.OrganizationCountryRuleCodeList[1].Code);
			AssertEquals("Outside the European Union", lookups.OrganizationCountryRuleCodeList[1].Description);
			AssertEquals(ARAPDefaultTaxRecognitionRuleLookups.OrganizationCountryRuleCode.SAL, lookups.OrganizationCountryRuleCodeList[2].Code);
			AssertEquals("Same country/region as login Country/Region", lookups.OrganizationCountryRuleCodeList[2].Description);
			AssertEquals(ARAPDefaultTaxRecognitionRuleLookups.OrganizationCountryRuleCode.DTL, lookups.OrganizationCountryRuleCodeList[3].Code);
			AssertEquals("Different country/region from login country/region", lookups.OrganizationCountryRuleCodeList[3].Description);
		}
	}
}
