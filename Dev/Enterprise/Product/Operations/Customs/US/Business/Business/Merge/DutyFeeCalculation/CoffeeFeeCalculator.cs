using CargoWise.Types;

namespace Enterprise.Customs.US.Business
{
	public class CoffeeFeeCalculator : ILineFeeCalculator
	{
		public CoffeeFeeCalculator(bool ignoreTIBExemptionCondition)
		{
			this.ignoreTIBExemptionCondition = ignoreTIBExemptionCondition;
		}
		readonly ZBool ignoreTIBExemptionCondition;

		public FeeResult CalculateFee(IFeeCalculationDataProvider dataProvider)
		{
			ZDecimal result = ZDecimal.Zero;
			if (dataProvider.IsClearedInPR)
			{
				return new TaxFeeCalculator(Core.Constants.USCustoms.FeeCodes.Coffee, ignoreTIBExemptionCondition).CalculateFee(dataProvider);
			}
			return new FeeResult(result);
		}

		public static bool IsFeeApplicable(IFeeCalculationDataProvider line, USCTariff importTariff)
		{
			return !line.IsSetVLine && importTariff != null && importTariff.IsFeeApplicable(Core.Constants.USCustoms.FeeCodes.Coffee);
		}
	}
}
