using CargoWise.Types;

namespace Enterprise.Customs.US.Business.Testing
{
	static class DairyTariffHelper
	{
		public static void SetUpTestDataForDairyFeeWithXComputationCode(this USCTariff importTariff)
		{
			var dutyRateForDairy = importTariff.DutyRates.GetRateForTaxFeeClassCode(Core.Constants.USCustoms.FeeCodes.DairyFee);

			if (dutyRateForDairy == null)
			{
				dutyRateForDairy = importTariff.DutyRates.AddNew();
				dutyRateForDairy.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DairyFee;
				dutyRateForDairy.UD_TaxFeeFlag = "1";
				dutyRateForDairy.UD_TaxFeeComputationCode = ZString.Empty;
				dutyRateForDairy.UD_TaxFeeSpecificRate = 0.01327m;
				dutyRateForDairy.UD_TaxFeeAdvalorem = 9999.9999999m;
			}
		}

		public static void SetUpTestDataForDairyFeeWith2ComputationCode(this USCTariff importTariff)
		{
			var dutyRateForDairy = importTariff.DutyRates.GetRateForTaxFeeClassCode(Core.Constants.USCustoms.FeeCodes.DairyFee);
			importTariff.UE_Unit2 = ABIUnitOfMeasureList.Codes.ContentKilogram;

			if (dutyRateForDairy == null)
			{
				dutyRateForDairy = importTariff.DutyRates.AddNew();
				dutyRateForDairy.UD_TaxFeeClassCode = Core.Constants.USCustoms.FeeCodes.DairyFee;
				dutyRateForDairy.UD_TaxFeeFlag = "1";
				dutyRateForDairy.UD_TaxFeeComputationCode = ComputationCodeList.Codes.SpecificRateSecondQuantity;
				dutyRateForDairy.UD_TaxFeeSpecificRate = 0.01327m;
				dutyRateForDairy.UD_TaxFeeAdvalorem = 9999.9999999m;
			}
		}
	}
}
