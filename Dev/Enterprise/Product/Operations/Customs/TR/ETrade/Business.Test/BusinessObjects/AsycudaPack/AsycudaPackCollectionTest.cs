using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TR.ETrade.Business.Testing
{
	[TestedType(typeof(AsycudaPackCollection))]
	public class AsycudaPackCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestSetDefaultsForNewChild()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsValue = 100m;
			bill.ABL_RX_NKGoodsValueCurrency = "TRY";

			var pack1 = bill.Packs.AddNew();
			AssertEquals(100m, pack1.PackedItem.API_GoodsValue);
			AssertEquals("TRY", pack1.PackedItem.API_RX_NKGoodsValueCurrency);

			pack1.PackedItem.API_GoodsValue = 60m;
			var pack2 = bill.Packs.AddNew();
			AssertEquals(40m, pack2.PackedItem.API_GoodsValue);
			AssertEquals("TRY", pack2.PackedItem.API_RX_NKGoodsValueCurrency);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			return bill.Packs;
		}

		protected override Type GetExpectedCollectionType()
		{
			return typeof(AsycudaPackCollection);
		}
	}
}
