using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class ABLEntryNumLookupsTest : BusinessObjectLookupsTestCase
	{
		readonly string[] expectedEntryNumberTypes = new[] { "ABT", "AFM" };

		public void TestABLEntryNumLRNTypeList()
		{
			var bill = Factory.NewWithValidTestData<AsycudaManifestHeader>().Bills.AddNew();
			var entryNumber = bill.CustomsEntryNumbers.AddNew();
			AssertContainsExactElementsInAnyOrder(expectedEntryNumberTypes, entryNumber.Lookups.CustomsEntryNumberTypes.GetAllCodes());
		}
	}
}
