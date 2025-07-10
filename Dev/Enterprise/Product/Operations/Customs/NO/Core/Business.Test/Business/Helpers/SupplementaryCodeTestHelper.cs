using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.NO.Business.Testing;

static class SupplementaryCodeTestHelper
{
	public static void SetupTariffAndCusCodeList(BusinessObjectFactory factory)
	{
		var refDataHelper = new UniversalReferenceTestDataHelper(factory);
		SetupCusCodeList(refDataHelper, factory);
		SetupTariffAndRate(refDataHelper, factory);
	}

	static void SetupTariffAndRate(UniversalReferenceTestDataHelper refDataHelper, BusinessObjectFactory factory)
	{
		var startDate = new ZDate(2010, 12, 10);
		var endDate = new ZDate(2079, 06, 06);
		const string dataGrouping = Core.Constants.CountryCodes.Norway;

		var tradeGroupStandard = refDataHelper.LoadOrCreateTradeGroup(dataGrouping, "STANDARD", startDate, endDate);
		refDataHelper.AddCountry(tradeGroupStandard, Core.Constants.CountryCodes.Botswana, startDate, endDate);

		var tariffTestHelper = new RefCusTariffTestHelper(factory);
		var cusTariff = tariffTestHelper.CreateImportTariff("DUMMYTRF");

		var rateCodeEXC = tariffTestHelper.GetOrCreateRateCode(Universal.Constants.RateTypes.Excise, "RC1", "Excise");
		var testRateEXC = refDataHelper.CreateRate(cusTariff, rateCodeEXC.PK, startDate, endDate, "0");

		refDataHelper.CreateCusApplicability(testRateEXC, tradeGroupStandard, startDate, endDate, "FA400");

		refDataHelper.CreateCusApplicability(testRateEXC, tradeGroupStandard, startDate, endDate, "MA400");
		refDataHelper.CreateCusApplicability(testRateEXC, tradeGroupStandard, startDate, endDate, "MB400");
		refDataHelper.CreateCusApplicability(testRateEXC, tradeGroupStandard, startDate, endDate, "MP400");

		refDataHelper.CreateCusApplicability(testRateEXC, tradeGroupStandard, startDate, endDate, "GA400");
		refDataHelper.CreateCusApplicability(testRateEXC, tradeGroupStandard, startDate, endDate, "GB400");
		refDataHelper.CreateCusApplicability(testRateEXC, tradeGroupStandard, startDate, endDate, "GP400");

		var rateCodeDTY = tariffTestHelper.GetOrCreateRateCode(Universal.Constants.RateTypes.Duty, "RC2", "Duty");
		var testRateDTY = refDataHelper.CreateRate(cusTariff, rateCodeDTY.PK, startDate, endDate, "0");

		refDataHelper.CreateCusApplicability(testRateDTY, tradeGroupStandard, startDate, endDate, "DTY01");
		refDataHelper.CreateCusApplicability(testRateDTY, tradeGroupStandard, startDate, endDate, "DTY02");

		factory.Save();
	}

	static void SetupCusCodeList(UniversalReferenceTestDataHelper refDataHelper, BusinessObjectFactory factory)
	{
		const string additionalCodes = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalCodes;
		var currentCountry = Core.Constants.CountryCodes.Norway;
		var startDate = ZDateTime.Today.AddDays(-1);
		var endDate = ZDateTime.Today.AddDays(1);

		refDataHelper.CreateNewOrGetExistingDataGrouping(currentCountry);
		refDataHelper.CreateCusCodeType(additionalCodes, "Additional Codes", currentCountry);
		factory.Save();
		refDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "FA400", "FA400 Descriptions", startDate, endDate);
		refDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "MA400", "MA400 Descriptions", startDate, endDate);
		refDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "MB400", "MB400 Descriptions", startDate, endDate);
		refDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "MP400", "MP400 Descriptions", startDate, endDate);
		refDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "GA400", "GA400 Descriptions", startDate, endDate);
		refDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "GB400", "GB400 Descriptions", startDate, endDate);
		refDataHelper.CreateCusCodeList(currentCountry, additionalCodes, "GP400", "GP400 Descriptions", startDate, endDate);
		factory.Save();
	}
}
