using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.PE.Manifest.Business.Testing
{
	[TestedType(typeof(AsycudaPack))]
	sealed class AsycudaPackTest : EnterpriseBusinessObjectTestCase
	{
		public void TestIAsycudaPack()
		{
			var bizObj = GetNewBusinessObject();
			Factory.Save();
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<Integration.Customs.ASYCUDA.PEManifest.IAsycudaPack>(bizObj.PK).GetType());
			AssertEquals(bizObj.GetType(), new BusinessObjectFactory().Load<AsycudaPack>(bizObj.PK).GetType());
		}

		public void TestBill()
		{
			var pack = (AsycudaPack)GetNewBusinessObject();
			AssertType<AsycudaBill>(pack.Bill);
		}

		public void TestLinePriceCurrency()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertNullOrEmpty("LinePriceCurrency", pack.LinePriceCurrency);

			var packedItem = pack.PackedItem;
			packedItem.API_RX_NKGoodsValueCurrency = "USD";
			AssertEquals("LinePriceCurrency", "USD", pack.LinePriceCurrency);

			pack.LinePriceCurrency = "EUR";
			AssertEquals("API_RX_NKGoodsValueCurrency should be", "EUR", packedItem.API_RX_NKGoodsValueCurrency);
		}

		public void TestPackedItemRelationship()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();

			Assert("Header.IsNonePackedItemRelationship", !header.IsNonePackedItemRelationship);
			Assert("Header.IsOnePackedItemRelationship", header.IsOnePackedItemRelationship);
			Assert("Header.IsManyPackedItemRelationship", !header.IsManyPackedItemRelationship);

			Assert("Bill.IsNonePackedItemRelationship", !bill.IsNonePackedItemRelationship);
			Assert("Bill.IsNonePackedItemRelationship", bill.IsOnePackedItemRelationship);
			Assert("Bill.IsNonePackedItemRelationship", !bill.IsManyPackedItemRelationship);

			Assert("Pack.IsNonePackedItemRelationship", !pack.IsNonePackedItemRelationship);
			Assert("Pack.IsNonePackedItemRelationship", pack.IsOnePackedItemRelationship);
			Assert("Pack.IsNonePackedItemRelationship", !pack.IsManyPackedItemRelationship);
		}

		public void TestLinePrice()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill = header.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			AssertEquals("LinePrice", 0m, pack.LinePrice);

			var packedItem = pack.PackedItem;
			packedItem.API_GoodsValue = 10m;
			AssertEquals("LinePrice", 10m, pack.LinePrice);

			pack.LinePrice = 20m;
			AssertEquals("API_GoodsValue should be", 20m, pack.PackedItem.API_GoodsValue);
		}

		public void TestPackUQIsNotConverted()
		{
			var pack1 = Factory.New<CusRefPacks>();
			pack1.RP_CustomsPack = "BT";
			pack1.RP_Type = "GMB";
			pack1.RP_CustomsCountry = "PE";
			pack1.RP_ConversionFactor = 2;
			pack1.RP_CommercialPack = "PCS";

			var shipment = Factory.New<ForwardingShipment>();
			shipment.JS_TransportMode = Core.Constants.TransportModes.Sea;

			var consol = shipment.Consols.AddNew();
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;

			var line1 = shipment.OuterPackLines.AddNew();
			line1.JL_PackageCount = 10;
			line1.JL_F3_NKPackType = "PCS";

			Factory.Save();

			var manifestHeader = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			manifestHeader.SetParent(consol);
			manifestHeader.Synchroniser.SetEnabled(true, false);
			manifestHeader.Synchroniser.Synchronise();

			var bill = manifestHeader.Bills[0];
			var pack = bill.Packs[0];

			CombineAssertions(() =>
			{
				AssertEquals("APA_PackUQ is not converted", 10, pack.APA_PackQty);
				AssertEquals("APA_PackUQ is not converted", "PCS", pack.APA_PackUQ);
			});
		}

		protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObject() => GetNewBusinessObjectForDeleteTest(Factory);

		protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
		{
			var manifestHeader = factory.NewWithValidTestData<AsycudaManifestHeader>();
			var bill = manifestHeader.Bills.AddNew();
			var pack = bill.Packs.AddNew();
			return pack;
		}
	}
}
