using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(GovernmentAgencyGoodsItem))]
	sealed class GovernmentAgencyGoodsItemTest : TestCaseWithFactory
	{
		public void TestGovernmentAgencyGoodsItemData()
		{
			(var header, _, var packedItem) = SetupData();
			header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
			header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Import23;
			packedItem.API_PreviousEntryNo = "L1";
			packedItem.API_PreviousEntryLineNo = 2;
			packedItem.API_RN_NKGoodsOrigin = "AU";
			IGovernmentAgencyGoodsItem goodsItem = new GovernmentAgencyGoodsItem(packedItem, 1);
			CombineAssertions(() =>
			{
				AssertEquals("GovernmentProcedure.CurrentCode", "31", goodsItem.GovernmentProcedure.CurrentCode);
				AssertEquals("PreBondedDocument.ID", "L1", goodsItem.PreBondedDocument.ID);
				AssertEquals("PreBondedDocument.LineNumeric", 2, goodsItem.PreBondedDocument.LineNumeric);
				AssertEquals("Origin.CountryCode", "AU", goodsItem.Origin.CountryCode);
				AssertType<Commodity>("Commodity type", goodsItem.Commodity);
				AssertEquals("SequenceNumeric", 1, goodsItem.SequenceNumeric);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertNull("PreBondedDocument should be null when AIR", goodsItem.PreBondedDocument);
				AssertEquals("Origin.CountryCode", "AU", goodsItem.Origin.CountryCode);

				header.AMA_Nature = Universal.Helper.ShipmentTypeList.Codes.Export22;
				AssertEquals("Origin.CountryCode should be empty", ZString.Empty, goodsItem.Origin.CountryCode);
			});
		}

		public void TestGoodsMeasure()
		{
			(_, _, var packedItem) = SetupData();
			packedItem.API_NetWeight = 10000m;
			packedItem.API_NetWeightUQ = Core.Constants.Weight.Grams;
			packedItem.API_CustomsQty = 100m;
			packedItem.API_CustomsUQ = "G";
			IGovernmentAgencyGoodsItem goodsItem = new GovernmentAgencyGoodsItem(packedItem, 1);
			var goodsMeasure = goodsItem.GoodsMeasure;
			CombineAssertions(() =>
			{
				AssertEquals("NetWeightMeasure", 10m, goodsMeasure.NetWeightMeasure);
				AssertEquals("TariffQuantity", 100m, goodsMeasure.TariffQuantity);
				AssertEquals("UnitCode", Core.Constants.Weight.Grams, goodsMeasure.UnitCode);
			});
		}

		(AsycudaManifestHeader header, AsycudaBill bill, AsycudaPackedItem packedItem) SetupData()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			header.AMA_RN_NKCountry = Core.Constants.CountryCodes.Taiwan;
			header.AMA_ApplicationCode = ManifestBase.ApplicationCodeTypeList.Codes.TWBriefCustomsDeclaration;
			var bill = header.Bills.AddNew();
			var packedItem = bill.PackedItems.AddNew();
			return (header, bill, packedItem);
		}
	}
}
