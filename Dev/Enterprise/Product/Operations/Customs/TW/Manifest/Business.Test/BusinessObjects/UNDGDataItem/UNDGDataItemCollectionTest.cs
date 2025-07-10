using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Manifest.Business.Testing
{
	[TestedType(typeof(UNDGDataItemCollection))]
	sealed class UNDGDataItemCollectionTest : ActiveBusinessObjectCollectionTestCase<UNDGDataItemCollection>
	{
		protected override UNDGDataItemCollection GetCollectionToTest()
		{
			return new UNDGDataItemCollection(Factory.New<AsycudaPack>());
		}

		public override void TestAddNew()
		{
			base.TestAddNew();
			var collection = new UNDGDataItemCollection(Factory.New<AsycudaPack>());
			var item1 = collection.AddNew();
			AssertType<UNDGDataItem>(item1);
		}

		public void TestIndexer()
		{
			var collection = new UNDGDataItemCollection(Factory.New<AsycudaPack>());
			var item1 = collection.AddNew();
			var item2 = collection.AddNew();
			AssertContainsExactElementsInAnyOrder(new[] { item1, item2 }, new[] { collection[0], collection[1] });
		}
	}
}
