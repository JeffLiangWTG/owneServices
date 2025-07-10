using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Business.Testing
{
	class CusSealLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestUnloadingState()
		{
			var lookup = Factory.GetCachedValue<UnloadingStates>();

			AssertEquals("Should contain 5 items", 5, lookup.Count);
			AssertCodeDescriptionPairList(lookup, ("DAM", "Damaged"), ("DEC", "As Declared"), ("DIF", "Differences to Declared"), ("MIS", "Missing"), ("NEW", "New"));

			var seal = Factory.New<CusSeal>();
			AssertSame("UnloadingStatesList is cached", lookup, seal.Lookups.UnloadingStatesList);
		}
	}
}
