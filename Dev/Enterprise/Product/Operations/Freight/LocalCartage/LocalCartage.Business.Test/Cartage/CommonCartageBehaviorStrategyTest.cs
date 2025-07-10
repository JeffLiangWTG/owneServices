using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Common.Business;

namespace Enterprise.Freight.LocalCartage.Business.Testing
{
	public class CommonCartageBehaviorStrategyTest : TestCaseWithFactory
	{
		public void TestFindOrCreateMainDocAddress_NullElementInRequirements()
		{
			var dummyCartageType = Factory.New<CommonCartageType>();
			dummyCartageType.E3_JobType = "IAFA";
			var orgType = dummyCartageType.CommonCartageOrganisations.AddNew();
			orgType.E5_OrgType = "BKD";
			var cartageLeg = dummyCartageType.ContainerizedCartageLegTypes.AddNew();
			cartageLeg.E4_E5_FromOrg = orgType.PK;
			cartageLeg.E4_E5_ToOrg = orgType.PK;
			var cartage = Factory.New<CartageForTest>();
			cartage.JJ_E3_NKJobType = "IAFA";
			var strategy = new CommonCartageBehaviorStrategy();
			AssertNull(strategy.FindOrCreateMainDocAddress(cartage, 1));
		}
	}
}
