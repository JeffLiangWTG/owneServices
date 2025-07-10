using CargoWise.EntityFramework.Testing;

namespace Enterprise.Recruiter.Business.Testing
{
	class HROnBoardingEntitlementLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCurrencies()
		{
			var onboardingEntitlement = Factory.New<HROnBoardingEntitlement>();
			AssertNotNull("Currencies should not be null", onboardingEntitlement.Lookups.Currencies);
		}

		public void TestOnBoardings()
		{
			var onboardingEntitlement = Factory.New<HROnBoardingEntitlement>();
			AssertNotNull("HROnBoardings should not be null", onboardingEntitlement.Lookups.HROnBoardings);
		}
	}
}

