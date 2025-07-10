using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.US.Business.Testing
{
	sealed class USCTariffRuleLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestRuleList()
		{
			var ruleList = lookups.RuleList;
			var expectedCodes = "A99, AES, AAU, CLP, TBC, CFE, AGO, ATP, CAF, CBT, STN, FDE, TBF, HTH, HMF, EG1, EG2, MO1, MO2, MO4, MO5, MO6, MO7, MO8, PN1, I99, LCY, NSP, TBP, R98, FME, R99, SGL, SGP, TEM, TPR, VLT, WLE";
			CombineAssertions(() =>
			{
				AssertEquals("Codes", expectedCodes, ruleList.CodesAsString);
				AssertSame("Cached", Factory.GetCachedValue<TariffRuleList>(), ruleList);
			});
		}

		public void TestTariffs()
		{
			AssertType<USCTariffCollection>(lookups.Tariffs);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var tariffRule = Factory.New<USCTariffRule>();
			lookups = new USCTariffRuleLookups(tariffRule);
		}
		USCTariffRuleLookups lookups;
	}
}
