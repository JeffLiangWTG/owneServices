using CargoWise.EntityFramework.Testing;

namespace Enterprise.Customs.ZA.Manifest.Business.Testing
{
	class AsycudaManifestHeaderDocWrapperTest : TestCaseWithFactory
	{
		public void TestZAEntryNumbers()
		{
			var manifest = Factory.New<AsycudaManifestHeader>();
			manifest.FillWithValidTestData();
			manifest.AMA_TransportMode = Core.Constants.TransportModes.Road;
			var bill = manifest.Bills.AddNew();
			bill.CustomsEntryNumber = "ABC123";
			var entryNum1 = bill.CustomsEntryNumbers[0];
			var entryNum2 = bill.CustomsEntryNumbers.AddNew();
			entryNum2.CE_EntryNum = "ABC124";
			Factory.Save();
			var zaEntryNumbers = manifest.GetDocWrapper().ZAEntryNumbers;
			AssertEquals(2, zaEntryNumbers.Count);
			AssertCollectionContains(entryNum1, zaEntryNumbers);
			AssertCollectionContains(entryNum2, zaEntryNumbers);
		}
	}
}
