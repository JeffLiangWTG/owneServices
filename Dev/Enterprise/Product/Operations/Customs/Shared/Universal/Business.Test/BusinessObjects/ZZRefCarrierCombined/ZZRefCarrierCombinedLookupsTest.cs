using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.Universal.Testing
{
	internal class ZZRefCarrierCombinedLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCountryOrGroupingList()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var parentGrouping = helper.CreateNewOrGetExistingDataGrouping("EUN", "EuropeanUnion");
			helper.CreateNewOrGetExistingDataGrouping("IT", "Italy", parentGrouping);
			Factory.Save();
			var carrier = Factory.New<ZZRefCarrierCombined>();
			AssertEquals(2, carrier.Lookups.CountryOrGroupingList.Count);
		}
	}
}
