using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration.Compliance;
using Enterprise.MasterFiles.Business.CountryCompliance;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;
using static Enterprise.Core.Constants;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class ComplianceSubTypeAllocationOverrideConfigurationLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestSubTypeLists()
		{
			var brConfiguration = GetNewObjectToTest(CountryCodes.Brazil);
			AssertEquals("BR", brConfiguration.Country);
			var expectedBrCodes = GetExpectedCodes(new BrazilComplianceInfo());
			AssertContainsExactElementsInAnyOrder(expectedBrCodes, brConfiguration.Lookups.SubTypeList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(expectedBrCodes, brConfiguration.Lookups.SubTypeInLocalLanguageList.GetAllCodes());

			var inConfiguration = GetNewObjectToTest(CountryCodes.India);
			AssertEquals("IN", inConfiguration.Country);
			var expectedInCodes = GetExpectedCodes(new IndiaComplianceInfo());
			AssertContainsExactElementsInAnyOrder(expectedInCodes, inConfiguration.Lookups.SubTypeList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(expectedInCodes, inConfiguration.Lookups.SubTypeInLocalLanguageList.GetAllCodes());

			var noCountryConfiguration = GetNewObjectToTest(CountryCodes._TemplateCountryName_);
			var expectedNoCountryCodes = Array.Empty<string>();
			AssertContainsExactElementsInAnyOrder(expectedNoCountryCodes, noCountryConfiguration.Lookups.SubTypeList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(expectedNoCountryCodes, noCountryConfiguration.Lookups.SubTypeInLocalLanguageList.GetAllCodes());

			var argentinaConfiguration = GetNewObjectToTest(CountryCodes.Argentina);
			AssertEquals("AR", argentinaConfiguration.Country);
			var expectedArCodes = GetExpectedCodes(new ArgentinaComplianceInfo());
			AssertContainsExactElementsInAnyOrder(expectedArCodes, argentinaConfiguration.Lookups.SubTypeList.GetAllCodes());
			AssertContainsExactElementsInAnyOrder(expectedArCodes, argentinaConfiguration.Lookups.SubTypeInLocalLanguageList.GetAllCodes());

			ZString[] GetExpectedCodes(IComplianceSubTypeCodeProvider provider)
				=> provider.GetComplianceSubTypes()
					.Where(x => x.Ledger == LedgerOfUse.ALL || x.Ledger == LedgerOfUse.AR)
					.Select(x => x.Code)
					.ToArray();
		}

		public void TestAllocationList()
		{
			var expectedCodes = AccComplianceSequenceLookups.ComplianceDocumentNumberAllocationSettingList.GetAllCodes();

			var brConfiguration = GetNewObjectToTest(CountryCodes.Brazil);
			AssertEquals("BR", brConfiguration.Country);
			AssertContainsExactElementsInAnyOrder(expectedCodes, brConfiguration.Lookups.AllocationMethodList.GetAllCodes());

			var inConfiguration = GetNewObjectToTest(CountryCodes.India);
			AssertEquals("IN", inConfiguration.Country);
			AssertContainsExactElementsInAnyOrder(expectedCodes, inConfiguration.Lookups.AllocationMethodList.GetAllCodes());

			var noCountryConfiguration = GetNewObjectToTest(CountryCodes._TemplateCountryName_);
			AssertContainsExactElementsInAnyOrder(expectedCodes, noCountryConfiguration.Lookups.AllocationMethodList.GetAllCodes());
		}

		public void TestBranchList()
		{
			var currentCompanyFallback = new FallbackLevel(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var item = new ComplianceSubTypeAllocationOverrideConfiguration(currentCompanyFallback, Factory, CountryCodes._TemplateCountryName_);
			var expectedBranchCodes = GlbCompany.CurrentCompany.Branches.Select(b => b.GB_Code);
			var actualBranchCodes = item.Lookups.BranchList.Select(b => b.GB_Code);
			AssertContainsExactElementsInAnyOrder(expectedBranchCodes, actualBranchCodes);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			var branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = company.PK;
			var branch2 = Factory.NewWithValidTestData<GlbBranch>();
			branch2.GB_GC = company.PK;
			Factory.Save();

			var otherCompanyFallback = new FallbackLevel(company.PK.ToGuid(), Guid.Empty, Guid.Empty);
			var otherCompanyItem = new ComplianceSubTypeAllocationOverrideConfiguration(otherCompanyFallback, Factory, CountryCodes._TemplateCountryName_);
			expectedBranchCodes = new[] { branch1.GB_Code, branch2.GB_Code };
			actualBranchCodes = otherCompanyItem.Lookups.BranchList.Select(b => b.GB_Code);
			AssertContainsExactElementsInAnyOrder(expectedBranchCodes, actualBranchCodes);

			var nullCompanyItem = new ComplianceSubTypeAllocationOverrideConfiguration();
			expectedBranchCodes = Enumerable.Empty<ZString>();
			actualBranchCodes = nullCompanyItem.Lookups.BranchList.Select(b => b.GB_Code);
			AssertContainsExactElementsInAnyOrder(expectedBranchCodes, actualBranchCodes);
		}

		#region Implementation

		ComplianceSubTypeAllocationOverrideConfiguration GetNewObjectToTest(string countryCode)
		{
			var collection = new ComplianceSubTypeAllocationOverrideConfigurationCollection(null, Factory, countryCode ?? CountryCodes.Brazil);
			var item = collection.AddNew();
			return item;
		}

		#endregion
	}
}
