using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(AsycudaBillPackedItemCollection))]
	sealed class AsycudaBillPackedItemCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestDefaultValue()
		{
			var bill = Factory.New<AsycudaManifestHeader>().Bills.AddNew();
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			var newPackedItem = bill.PackedItems.AddNew();
			CombineAssertions(() =>
			{
				AssertEquals("API_RX_NKGoodsValueCurrency should be", "USD", newPackedItem.API_RX_NKGoodsValueCurrency);
				AssertEquals("API_NetWeightUQ  should be", "KG", newPackedItem.API_NetWeightUQ);
			});

			bill.ABL_RX_NKGoodsValueCurrency = "TWD";
			newPackedItem = bill.PackedItems.AddNew();
			AssertEquals("TWD", newPackedItem.API_RX_NKGoodsValueCurrency);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			return (BusinessObjectCollection)header.Bills.AddNew().PackedItems;
		}
	}
}
