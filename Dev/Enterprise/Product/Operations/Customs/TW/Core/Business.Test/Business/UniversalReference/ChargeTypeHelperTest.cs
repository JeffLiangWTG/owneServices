using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.TW.Business.Testing
{
	sealed class ChargeTypeHelperTest : TestCaseWithFactory
	{
		[TestDate(2019, 07, 04)]
		public void TestGetChargeTypes()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			var refCusRateTypeTW = helper.CreateCusRateType(Core.Constants.CountryCodes.Taiwan, "DTY");
			var refCusRateCode1 = helper.LoadOrCreateNewCusRateCode(Factory, "DTA", refCusRateTypeTW.PK, description: "Business Tax");
			var refCusRateCode2 = helper.LoadOrCreateNewCusRateCode(Factory, "CTS", refCusRateTypeTW.PK, description: "Trade promotion fee for exported goods");
			var startDate = ZDate.Today.AddMonths(-3);
			var endDate = ZDate.Today.AddMonths(3);
			var refCusTaxOrFeeTw = helper.CreateTaxOrFee("TPF", 0.00040000m, Core.Constants.CountryCodes.Taiwan, startDate: startDate, endDate: endDate, description: "Trade Promotion Fee");
			var refCusTaxOrFeeZa = helper.CreateTaxOrFee("VAT", 0.15000000m, Core.Constants.CountryCodes.SouthAfrica, startDate: startDate, endDate: endDate, description: "VAT Normal");
			Factory.Save();
			var chargeTypeList = ChargeTypeHelper.GetChargeTypes(Factory, new ZDateTime(2019, 06, 27));
			AssertEquals(7, chargeTypeList.Count());
			AssertNotNull(chargeTypeList.SingleOrDefault(chargeType => chargeType.RateCode == "DTA"));
			AssertNotNull(chargeTypeList.SingleOrDefault(chargeType => chargeType.RateCode == "CTS"));
			AssertNotNull(chargeTypeList.SingleOrDefault(chargeType => chargeType.RateCode == "TPF"));
			AssertNull(chargeTypeList.SingleOrDefault(chargeType => chargeType.RateCode == "VAT"));
			AssertNotNull(chargeTypeList.SingleOrDefault(chargeType => chargeType.RateCode == "ADD"));
			AssertNotNull(chargeTypeList.SingleOrDefault(chargeType => chargeType.RateCode == "CVD"));
			AssertNotNull(chargeTypeList.SingleOrDefault(chargeType => chargeType.RateCode == "ADT"));
			AssertNotNull(chargeTypeList.SingleOrDefault(chargeType => chargeType.RateCode == "RTD"));
			var header = Factory.New<CusEntryHeader>();
			var chargeTypeCodeList = ChargeTypeHelper.GetEntryHeaderChargeTypes(Factory, header);
			AssertEquals(7, chargeTypeCodeList.Count);
			AssertEquals("Trade Promotion Fee", chargeTypeCodeList.GetDescriptionFromCode("TPF"));
			AssertEquals("Business Tax", chargeTypeCodeList.GetDescriptionFromCode("DTA"));
			AssertEquals("Trade promotion fee for exported goods", chargeTypeCodeList.GetDescriptionFromCode("CTS"));
			AssertEquals(SpecialDutyRateCodeList.Descriptions.AntiDumpingDuty, chargeTypeCodeList.GetDescriptionFromCode("ADD"));
			AssertEquals(SpecialDutyRateCodeList.Descriptions.CountervailingDuty, chargeTypeCodeList.GetDescriptionFromCode("CVD"));
			AssertEquals(SpecialDutyRateCodeList.Descriptions.AdditionalDuty, chargeTypeCodeList.GetDescriptionFromCode("ADT"));
			AssertEquals(SpecialDutyRateCodeList.Descriptions.RetaliatoryDuty, chargeTypeCodeList.GetDescriptionFromCode("RTD"));
		}
	}
}
