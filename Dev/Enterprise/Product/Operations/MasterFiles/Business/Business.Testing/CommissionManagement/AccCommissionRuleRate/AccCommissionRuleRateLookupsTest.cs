using CargoWise.EntityFramework.Testing;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class AccCommissionRuleRateLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestComissionTypes()
		{
			AssertEquals("FIX, PCT", lookups.CommissionTypes.CodesAsString);
		}

		public void TestCommissionPeriods()
		{
			AssertEquals("NEW, EXS", lookups.CommissionPeriods.CodesAsString);
		}

		protected override void SetUp()
		{
			base.SetUp();
			var accCommissionRuleRate = Factory.New<AccCommissionRuleRate>();
			lookups = new AccCommissionRuleRateLookups(accCommissionRuleRate);
		}
		AccCommissionRuleRateLookups lookups;
	}
}
