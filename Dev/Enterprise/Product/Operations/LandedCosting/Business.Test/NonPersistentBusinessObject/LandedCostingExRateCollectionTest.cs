using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.LandedCosting.Business.Testing
{
	[TestedType(typeof(LandedCostingExRateCollection))]
	sealed class LandedCostingExRateCollectionTest : NonPersistentBusinessObjectCollectionTestCase<LandedCostingExRateCollection>
	{
		public void TestAllowNew()
		{
			AssertEquals("This collection does not allow new", false, ExRates.AllowNew);
			AssertEquals("This collection does not allow Remove", false, ExRates.AllowRemove);
		}

		public void TestLoadFromLCHeaderHost()
		{
			DummyHost.ExchangeRateHoldersExposed = new ILandedCostExchangeRateHolder[] { ExRateHolder };
			ExRates.LoadFromLCHeaderHost();
			AssertEquals("THere should be only one item in the collection", 1, ExRates.Count);

			DummyExchangeRateHolder exRateHolder2 = Factory.New<DummyExchangeRateHolder>();
			DummyHost.ExchangeRateHoldersExposed = new ILandedCostExchangeRateHolder[] { ExRateHolder, exRateHolder2 };
			ExRates.LoadFromLCHeaderHost();
			AssertEquals("THere should be only two items in the collection", 2, ExRates.Count);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection() => new LandedCostingExRate(ExRateHolder);

		protected override LandedCostingExRateCollection GetCollectionToTest() => ExRates;

		LandedCostingExRateCollection exRates;
		LandedCostingExRateCollection ExRates => exRates ?? (exRates = new LandedCostingExRateCollection(DummyHost));

		DummyLandedCostHeader dummyHost;
		DummyLandedCostHeader DummyHost => dummyHost ?? (dummyHost = Factory.New<DummyLandedCostHeader>());

		DummyExchangeRateHolder exRateHolder;
		DummyExchangeRateHolder ExRateHolder => exRateHolder ?? (exRateHolder = Factory.New<DummyExchangeRateHolder>());
	}
}
