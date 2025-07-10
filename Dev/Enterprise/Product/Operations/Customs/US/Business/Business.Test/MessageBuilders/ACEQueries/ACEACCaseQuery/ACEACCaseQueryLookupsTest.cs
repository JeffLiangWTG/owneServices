using CargoWise.EntityFramework.Testing;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class ACEACCaseQueryLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCompanyCaseStatusList()
		{
			var companyCaseStatusList = lookups.CompanyCaseStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "A, I, B", companyCaseStatusList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue("CompanyCaseStatusList", () => new CodeDescriptionPairList()), companyCaseStatusList);
			});
		}

		public void TestCountryList()
		{
			AssertType<USCCountryCollection>(lookups.CountryList);
		}

		public void TestTariffList()
		{
			AssertType<USCTariffCollection>(lookups.TariffList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var aceacCaseQuery = new ACEACCaseQuery(Factory);
			lookups = new ACEACCaseQueryLookups(aceacCaseQuery);
		}
		ACEACCaseQueryLookups lookups;
	}
}
