using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.TW.Business;
using Enterprise.Customs.TW.Messaging;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.BriefCustomsDeclaration.Business.Testing
{
	[TestedType(typeof(DutyTaxFeeQuantity))]
	sealed class DutyTaxFeeQuantityTest : TestCaseWithFactory
	{
		public void TestDutyTaxFeeQuantityData()
		{
			var (header, bill) = N5135TestHelper.GetAsycudaBill(Factory);
			var universalTestHelper = new UniversalReferenceTestDataHelper(Factory);
			var hsnTariffType = universalTestHelper.CreateNewOrGetExistingTariffType(Core.Constants.CountryCodes.Taiwan, "HSN");
			var rateType = universalTestHelper.CreateNewOrGetExistingRateType(Core.Constants.CountryCodes.Taiwan, "DTY", "Duty");
			var rateCodeDTS = universalTestHelper.LoadOrCreateNewCusRateCode(Factory, UniversalReferenceConstants.RefCusRateCodes.DTS, rateType.PK);
			var preference = universalTestHelper.CreatePreferenceForCountry("PR1", "PR1", Core.Constants.CountryCodes.Taiwan);
			var tradeGroup = universalTestHelper.CreateTradeGroup("TW", "JP", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			universalTestHelper.AddCountry(tradeGroup, "JP", ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);

			var tariffDTS = universalTestHelper.CreateTariff(Core.Constants.CountryCodes.Taiwan, hsnTariffType.PK, "00010123", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime);
			var rateDTS = universalTestHelper.CreateRate(tariffDTS, rateCodeDTS.PK, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, "160 * [LTR]", preference.PK, "160/LTR", Core.Constants.CountryCodes.Taiwan);
			universalTestHelper.CreateCusApplicability(rateDTS, tradeGroup, ZDateTime.MinSmallDateTimeValue.Date, ZDateTime.MaxSmallDateTime.Date);
			Factory.Save();

			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_LineNo = 1;
			packedItem.API_Tariff = "00010123";
			packedItem.API_RN_NKGoodsOrigin = "JP";
			packedItem.API_Preference = "PR1";
			packedItem.API_CustomsUQ2 = "KGA";
			IDutyTaxFeeQuantity dutyTaxFeeQuantity = new DutyTaxFeeQuantity(packedItem);

			CombineAssertions(() =>
			{
				AssertEquals("DutyTaxFeeQuantity.DutyUnitCode", "KGA", dutyTaxFeeQuantity.DutyUnitCode);
				Assert("DutyTaxFeeQuantity.PercentageNumeric do not populate", dutyTaxFeeQuantity.PercentageNumeric.IsEmpty);
				AssertEquals("DutyTaxFeeQuantity.TaxRateNumeric", 160m, dutyTaxFeeQuantity.TaxRateNumeric);
			});
		}
	}
}
