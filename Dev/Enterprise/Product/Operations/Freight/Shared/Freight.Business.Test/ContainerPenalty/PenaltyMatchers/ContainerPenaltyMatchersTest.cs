using NUnit.Framework;

namespace Enterprise.Freight.Business.Testing
{
	public class ContainerPenaltyMatchersTest : TestCase
	{
		public void TestPriorityOrder()
		{
			var matchers = ContainerPenaltyMatchers.PriorityOrderedPenaltyMatchers;

			AssertEquals("Please verify a new matcher is a legitimate requirement.", matchers.Count, 4);
			
			AssertType<CarrierContractPenaltyMatcher>("Priority of matchers changed.", matchers[0]);
			AssertType<ClientContractPenaltyMatcher>("Priority of matchers changed.", matchers[1]);
			AssertType<OrgContainerPenaltyMatcher>("Priority of matchers changed.", matchers[2]);
			AssertType<RegistryPenaltyMatcher>("Priority of matchers changed.", matchers[3]);
		}
	}
}
