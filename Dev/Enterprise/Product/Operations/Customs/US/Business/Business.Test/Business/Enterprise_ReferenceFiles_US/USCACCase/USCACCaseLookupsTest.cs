using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCACCaseLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCaseStatusList()
		{
			var caseStatusList = lookups.CaseStatusList;
			CombineAssertions(() =>
			{
				AssertEquals("Codes", "AC, IC, ID, IF, IO, IT, IX", caseStatusList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<ACCaseStatusList>(), caseStatusList);
			});
		}

		public void TestCountryCodeList()
		{
			AssertType<USCCountryCollection>(lookups.CountryCodeList);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var cacCase = Factory.New<USCACCase>();
			lookups = new USCACCaseLookups(cacCase);
		}
		USCACCaseLookups lookups;
	}
}
