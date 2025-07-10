using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItem))]
	sealed class AsycudaPackedItemTest : EnterpriseBusinessObjectTestCase
	{
		public void TestHeader()
		{
			var item = (AsycudaPackedItem)GetNewBusinessObject();
			AssertType<AsycudaManifestHeader>(item.Header);
		}

		public void TestPack()
		{
			var item = (AsycudaPackedItem)GetNewBusinessObject();
			AssertType<AsycudaPack>(item.Pack);
		}

		public void TestValidation()
		{
			var item = (AsycudaPackedItem)GetNewBusinessObject();
			AssertType<AsycudaPackedItemValidation>(item.Validation);
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var header = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			var packedItem = pack.PackedItems.AddNewPackedItem();
			return packedItem;
		}
	}
}
