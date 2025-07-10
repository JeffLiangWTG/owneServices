using CargoWise.EntityFramework.Testing;
using WTG.ProductionRules.Core;

namespace Enterprise.TransportConsignment.ProductionRulesEngine.Testing
{
	public class LandTransportFactLoaderProviderTest : TestCaseWithFactory
	{
		public void TestGetFactLoader()
		{
			var contextType = RulesContextType.LandTransportControllingBranchDefaulting;
			var factLoaderProvider = new LandTransportFactLoaderProvider();
			var factLoader = factLoaderProvider.GetFactLoader(contextType);

			AssertNotNull(factLoader);
			AssertType<ConsignmentFactLoader>(factLoader);
		}

		public void TestGetFactLoader_WithUnsupportedType_ReturnsNull()
		{
			var factLoaderProvider = new LandTransportFactLoaderProvider();
			var factLoader = factLoaderProvider.GetFactLoader(RulesContextType.DummyForTesting);

			AssertNull(factLoader);
		}
	}
}
