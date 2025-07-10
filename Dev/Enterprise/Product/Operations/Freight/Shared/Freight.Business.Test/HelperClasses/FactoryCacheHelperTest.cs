using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Freight.Business.Testing
{
	sealed class FactoryCacheHelperTest : TestCaseWithFactory
	{
		public void TestSetOrGetIsViewingFromPortTransportLegPlannerFactoryCache()
		{
			var factory = new BusinessObjectFactory();
			AssertEquals(false, FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(factory));

			FactoryCacheHelper.SetIsViewingFromPortTransportLegPlanner(factory);
			AssertEquals(true, FactoryCacheHelper.GetIsViewingFromPortTransportLegPlanner(factory));
		}
	}
}
