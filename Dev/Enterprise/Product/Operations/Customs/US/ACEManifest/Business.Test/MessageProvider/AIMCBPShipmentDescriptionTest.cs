using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.US.ACEManifest.Business.Testing
{
	class AIMCBPShipmentDescriptionTest : TestCaseWithFactory
	{
		public void TestProperties()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = "US";
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsValue = 12m;
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			bill.ABL_RL_NKOrigin = "USCHI";
			bill.ABL_Tariff = "6203223015";
			var aimCBPShipmentDescription = AIMCBPShipmentDescription.CreateIfHasGoodsValue(bill);
			AssertEquals(12m, aimCBPShipmentDescription.DeclaredValue);
			AssertEquals("USD", aimCBPShipmentDescription.ISOCurrencyCode);
			AssertEquals("US", aimCBPShipmentDescription.OriginOfGoods);
			AssertEquals("6203.22.3015", aimCBPShipmentDescription.HarmonizedCommodityCode);

			var packedItem = bill.Packs.Cast<AsycudaPack>().FirstOrDefault()?.PackedItem;
			AssertNotNull(packedItem);
			packedItem.API_RN_NKGoodsOrigin = "CN";
			AssertEquals("CN", aimCBPShipmentDescription.OriginOfGoods);

			packedItem.API_RN_NKGoodsOrigin = "";
			bill.ABL_RL_NKOrigin = "";
			bill.ABL_Tariff = "";
			AssertEquals(ZString.Empty, aimCBPShipmentDescription.OriginOfGoods);
			AssertEquals(ZString.Empty, aimCBPShipmentDescription.HarmonizedCommodityCode);
		}

		public void TestCreateIfHasGoodsValue()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			AssertNull(AIMCBPShipmentDescription.CreateIfHasGoodsValue(bill));
			bill.ABL_GoodsValue = 12m;
			bill.ABL_RX_NKGoodsValueCurrency = "";
			AssertNull(AIMCBPShipmentDescription.CreateIfHasGoodsValue(bill));
			bill.ABL_GoodsValue = 0m;
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			AssertNull(AIMCBPShipmentDescription.CreateIfHasGoodsValue(bill));
			bill.ABL_GoodsValue = 12m;
			bill.ABL_RX_NKGoodsValueCurrency = "USD";
			var aimCBPShipmentDescription = AIMCBPShipmentDescription.CreateIfHasGoodsValue(bill);
			AssertEquals(12m, aimCBPShipmentDescription.DeclaredValue);
			AssertEquals("USD", aimCBPShipmentDescription.ISOCurrencyCode);
			AssertEquals(ZString.Empty, aimCBPShipmentDescription.OriginOfGoods);
			AssertEquals(ZString.Empty, aimCBPShipmentDescription.HarmonizedCommodityCode);
		}

		public void TestGoodsValueOfUnknownCurrency()
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			bill.ABL_GoodsValue = 12m;
			bill.ABL_RX_NKGoodsValueCurrency = "XXX";
			AssertEquals(0m, bill.GoodsValueInUSD);
			var aimCBPShipmentDescription = AIMCBPShipmentDescription.CreateIfHasGoodsValue(bill);
			AssertEquals(12m, aimCBPShipmentDescription.DeclaredValue);
			AssertEquals("XXX", aimCBPShipmentDescription.ISOCurrencyCode);
			AssertEquals(ZString.Empty, aimCBPShipmentDescription.OriginOfGoods);
			AssertEquals(ZString.Empty, aimCBPShipmentDescription.HarmonizedCommodityCode);
		}
	}
}
