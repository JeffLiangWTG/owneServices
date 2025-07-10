using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPackedItemCollection))]
	class AsycudaPackedItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestLoadPackedItemsWhenPackIsNotInDatabase()
		{
			var pack = Factory.New<AsycudaPack>();
			pack.APA_ClusterKey = 1;

			var packedItem = Factory.New<AsycudaPackedItem>();
			packedItem.API_ClusterKey = 1;

			var pivot = Factory.New<ManifestBase.AsycudaPackPackedItemPivot>();
			pivot.APP_APA_Pack = pack.PK;
			pivot.APP_API_Item = packedItem.PK;
			pivot.APP_ClusterKey = 1;

			AssertEquals("Collection should load items correctly when data is not yet in database", 1, pack.PackedItemsForBinding.Count);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var bill = Factory.New<AsycudaBill>();
			var pack = Factory.New<AsycudaPack>();
			pack.APA_ABL_Bill = bill.PK;

			return new AsycudaPackedItemCollection(pack);
		}
	}
}
