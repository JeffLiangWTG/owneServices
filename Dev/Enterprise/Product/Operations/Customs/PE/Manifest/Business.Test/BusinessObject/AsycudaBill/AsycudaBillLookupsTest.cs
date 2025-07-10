using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	sealed class AsycudaBillLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestCargoNatureList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Bills.AddNew().Lookups.CargoNatureList.GetAllCodes();

			AssertEquals(20, list.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14", "17", "18", "19", "20", "21", "22" }, list);
		}

		public void TestCargoConditionList()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var list = header.Bills.AddNew().Lookups.CargoConditionList.GetAllCodes();

			AssertEquals(14, list.Length);
			AssertContainsExactElementsInAnyOrder(new[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14" }, list);
		}
	}
}
