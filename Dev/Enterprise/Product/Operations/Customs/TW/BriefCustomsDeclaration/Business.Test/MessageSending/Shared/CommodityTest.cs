using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(Commodity))]
	sealed class CommodityTest : TestCaseWithFactory
	{
		public void TestCommodityData()
		{
			var (header, bill) = N5135TestHelper.GetAsycudaBill(Factory);
			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_LineNo = 1;
			var pack1ItemTariff = $"{packedItem.API_LineNo:0000}1234";

			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTA = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTA, rateType.PK);
			var preference = universalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = universalTestHelper.CreateTradeGroup("TW", "JP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTA = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, pack1ItemTariff, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTA = universalTestHelper.CreateRate(tariffDTA, rateCodeDTA.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "0.15 * VFD", preference.PK, "0.15", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTA, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			Factory.Save();

			var prefix = $"PK{packedItem.API_LineNo}";
			packedItem.API_Model = prefix + "Model";
			packedItem.API_GoodsDescription = prefix + "GoodsDescription";
			packedItem.API_Brand = prefix + "BrandName";
			packedItem.API_Tariff = pack1ItemTariff;
			packedItem.API_RN_NKGoodsOrigin = "JP";
			packedItem.API_Preference = "PR1";
			packedItem.API_Remarks = prefix + ": Additional information not including in the goods description node.";
			ICommodity commodity = new Commodity(packedItem);
			CombineAssertions(() =>
			{
				AssertEquals("CommercialCategorizationID is not expected", "PK1Model", commodity.CommercialCategorizationID);
				AssertEquals("Description is not expected", "PK1GoodsDescription", commodity.Description);
				AssertEquals("Name is not expected", "PK1BrandName", commodity.Name);
				AssertEquals("Constituent.ElementDescription is not expected", "PK1: Additional information not including in the goods description node.", commodity.Constituent.ElementDescription);
				AssertEquals("Classification.ID is not expected", "00011234", commodity.Classification.ID);
				AssertEquals("GovernmentProcedure.CurrentCode is not expected", "31", commodity.GovernmentProcedure.CurrentCode);
				AssertType<GovernmentProcedureWrapper>(commodity.GovernmentProcedure);
				AssertType<DutyTaxFeeQuantity>(commodity.DutyTaxFeeQuantity);
			});

			CombineAssertions("DutyTaxFee", () =>
			{
				packedItem.API_CustomsValue = 1m;
				packedItem.API_CustomsQty2 = 2m;
				var dutyTaxFee = commodity.DutyTaxFee;
				AssertEquals("DutyTaxFee.AdValoremTaxBaseAmount", 1m, dutyTaxFee.AdValoremTaxBaseAmount);
				AssertEquals("DutyTaxFee.SpecificTaxBaseQuantity", 2m, dutyTaxFee.SpecificTaxBaseQuantity);
			});

			CombineAssertions("DutyTaxFeeAmount", () =>
			{
				var dutyTaxFeeAmount = commodity.DutyTaxFeeAmount;
				AssertEquals("DutyTaxFeeAmount.TaxRateNumeric", 0.15m, dutyTaxFeeAmount.TaxRateNumeric);
				Assert("DutyTaxFeeAmount.PercentageNumeric do not populate", dutyTaxFeeAmount.PercentageNumeric.IsEmpty);
			});

			CombineAssertions("InvoiceLine", () =>
			{
				packedItem.API_CustomsValue = 220m;
				var invoiceLine = commodity.InvoiceLine;
				AssertEquals("ItemChargeAmount", 220m, invoiceLine.ItemChargeAmount);
			});

			CombineAssertions("CommodityNumbers", () =>
			{
				header.AMA_TransportMode = Core.Constants.TransportModes.Sea;
				packedItem.API_CustomsBuyerPartNo = "B01";
				var commodityNumbers = commodity.CommodityNumbers;
				AssertEquals(1, commodityNumbers.Count());
				AssertEquals("Single: ID", "B01", commodityNumbers.Single().ID);
				AssertEquals("Single: IdentifierTypeCode", "BA", commodityNumbers.Single().IdentifierTypeCode);

				packedItem.API_CustomsSupplierPartNo = "S01";
				commodityNumbers = commodity.CommodityNumbers;
				AssertEquals(2, commodityNumbers.Count());
				AssertEquals("Buyer: ID", "B01", commodityNumbers.First().ID);
				AssertEquals("Buyer: IdentifierTypeCode", "BA", commodityNumbers.First().IdentifierTypeCode);
				AssertEquals("Seller: ID", "S01", commodityNumbers.ElementAt(1).ID);
				AssertEquals("Seller: IdentifierTypeCode", "SA", commodityNumbers.ElementAt(1).IdentifierTypeCode);

				header.AMA_TransportMode = Core.Constants.TransportModes.Air;
				AssertEquals("CommodityNumbers count", 0, commodity.CommodityNumbers.Count());
			});
		}
	}
}
